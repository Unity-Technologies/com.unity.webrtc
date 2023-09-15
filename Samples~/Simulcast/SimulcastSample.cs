using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

[Serializable]
class EncoderParameters
{
    [SerializeField] public Dropdown optionMaxBitrate;
    [SerializeField] public Dropdown optionMinBitrate;
    [SerializeField] public Dropdown optionScaleResolution;
}

class SimulcastSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button startButton;
    [SerializeField] private Button callButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Camera cam;
    [SerializeField] private RawImage sourceImage;
    [SerializeField] private RawImage receiveImage;
    [SerializeField] private Transform rotateObject;
    [SerializeField] private EncoderParameters[] encoderParameters = new EncoderParameters[3];
#pragma warning restore 0649

    private RTCPeerConnection _pc1, _pc2;
    private MediaStream videoStream, receiveStream;
    private RTCRtpTransceiver _pc1Transceiver;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
    private DelegateOnIceCandidate pc1OnIceCandidate;
    private DelegateOnIceCandidate pc2OnIceCandidate;
    private DelegateOnTrack pc2Ontrack;
    private DelegateOnNegotiationNeeded pc1OnNegotiationNeeded;
    private bool videoUpdateStarted;

    private ulong[] optionBitrate = new ulong[] { 100, 150, 200, 300, 400, 600, 800, 1100, 1400 };
    private int[] optionScaleResolution = new[] { 1, 2, 4, 8 };

    private void Awake()
    {
        startButton.onClick.AddListener(OnStart);
        callButton.onClick.AddListener(Call);
        restartButton.onClick.AddListener(RestartIce);
        hangUpButton.onClick.AddListener(HangUp);
        receiveStream = new MediaStream();
    }

    private void Start()
    {
        callButton.interactable = false;
        restartButton.interactable = false;
        hangUpButton.interactable = false;

        pc1OnIceConnectionChange = state => { OnIceConnectionChange(_pc1, state); };
        pc2OnIceConnectionChange = state => { OnIceConnectionChange(_pc2, state); };
        pc1OnIceCandidate = candidate => { OnIceCandidate(_pc1, candidate); };
        pc2OnIceCandidate = candidate => { OnIceCandidate(_pc2, candidate); };

        foreach (var paramters in encoderParameters)
        {
            paramters.optionMaxBitrate.options = optionBitrate.Select(bitrate => new Dropdown.OptionData { text = bitrate.ToString() }).ToList();
            paramters.optionMinBitrate.options = optionBitrate.Select(bitrate => new Dropdown.OptionData { text = bitrate.ToString() }).ToList();
            paramters.optionScaleResolution.options = optionScaleResolution.Select(scaleResolution => new Dropdown.OptionData { text = $"1/{scaleResolution}" }).ToList();
        }

        encoderParameters[0].optionMaxBitrate.value = 1; // 150
        encoderParameters[0].optionMinBitrate.value = 0; // 100
        encoderParameters[0].optionScaleResolution.value = 2; // 1/4
        encoderParameters[1].optionMaxBitrate.value = 4; // 400
        encoderParameters[1].optionMinBitrate.value = 3; // 300
        encoderParameters[1].optionScaleResolution.value = 1; // 1/2
        encoderParameters[2].optionMaxBitrate.value = 7; // 1100
        encoderParameters[2].optionMinBitrate.value = 6; // 800
        encoderParameters[2].optionScaleResolution.value = 0; // 1/1

        pc2Ontrack = e =>
        {
            receiveStream.AddTrack(e.Track);
        };
        pc1OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc1)); };

        receiveStream.OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                track.OnVideoReceived += tex =>
                {
                    receiveImage.texture = tex;
                    receiveImage.color = Color.white;
                };
            }
        };
    }

    private void OnStart()
    {
        startButton.interactable = false;
        callButton.interactable = true;

        if (videoStream == null)
        {
            videoStream = cam.CaptureStream(WebRTCSettings.StreamSize.x, WebRTCSettings.StreamSize.y);
        }

        sourceImage.texture = cam.targetTexture;
        sourceImage.color = Color.white;
    }

    private void Update()
    {
        if (rotateObject != null)
        {
            float t = Time.deltaTime;
            rotateObject.Rotate(100 * t, 200 * t, 300 * t);
        }
    }

    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };

        return config;
    }

    private void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
    {
        Debug.Log($"{GetName(pc)} IceConnectionState: {state}");
    }

    IEnumerator PeerNegotiationNeeded(RTCPeerConnection pc)
    {
        var op = pc.CreateOffer();
        yield return op;

        if (!op.IsError)
        {
            if (pc.SignalingState != RTCSignalingState.Stable)
            {
                Debug.LogError($"{GetName(pc)} signaling state is not stable.");
                yield break;
            }

            yield return StartCoroutine(OnCreateOfferSuccess(pc, op.Desc));
        }
        else
        {
            OnCreateSessionDescriptionError(op.Error);
        }
    }

    private void AddTracks()
    {
        List<RTCRtpEncodingParameters> parameters = new List<RTCRtpEncodingParameters>();
        RTCRtpEncodingParameters encoder = new RTCRtpEncodingParameters();

        encoder = new RTCRtpEncodingParameters();
        encoder.rid = "l";
        encoder.active = true;
        encoder.maxFramerate = 30;
        encoder.maxBitrate = optionBitrate[encoderParameters[0].optionMaxBitrate.value] * 1024;
        encoder.minBitrate = optionBitrate[encoderParameters[0].optionMinBitrate.value] * 1024;
        encoder.scaleResolutionDownBy = optionScaleResolution[encoderParameters[0].optionScaleResolution.value];
        parameters.Add(encoder);

        encoder = new RTCRtpEncodingParameters();
        encoder.rid = "m";
        encoder.active = true;
        encoder.maxFramerate = 30;
        encoder.maxBitrate = optionBitrate[encoderParameters[1].optionMaxBitrate.value] * 1024;
        encoder.minBitrate = optionBitrate[encoderParameters[1].optionMinBitrate.value] * 1024;
        encoder.scaleResolutionDownBy = optionScaleResolution[encoderParameters[1].optionScaleResolution.value];
        parameters.Add(encoder);

        encoder = new RTCRtpEncodingParameters();
        encoder.rid = "h";
        encoder.active = true;
        encoder.maxFramerate = 30;
        encoder.maxBitrate = optionBitrate[encoderParameters[2].optionMaxBitrate.value] * 1024;
        encoder.minBitrate = optionBitrate[encoderParameters[2].optionMinBitrate.value] * 1024;
        encoder.scaleResolutionDownBy = optionScaleResolution[encoderParameters[2].optionScaleResolution.value];
        parameters.Add(encoder);

        RTCRtpTransceiverInit init = new RTCRtpTransceiverInit();
        init.direction = RTCRtpTransceiverDirection.SendOnly;
        init.sendEncodings = parameters.ToArray();

        var track = videoStream.GetTracks().First();
        var transceiver = _pc1.AddTransceiver(track, init);

        if (WebRTCSettings.UseVideoCodec != null)
        {
            var codecs = new[] { WebRTCSettings.UseVideoCodec };
            transceiver.SetCodecPreferences(codecs);
        }
        _pc1Transceiver = transceiver;

        if (!videoUpdateStarted)
        {
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
        }
    }

    private void RemoveTracks()
    {
        _pc1.RemoveTrack(_pc1Transceiver.Sender);

        var tracks = receiveStream.GetTracks().ToArray();
        foreach (var track in tracks)
        {
            receiveStream.RemoveTrack(track);
        }
    }

    private void Call()
    {
        callButton.interactable = false;
        hangUpButton.interactable = true;
        restartButton.interactable = true;

        foreach (var parameters in encoderParameters)
        {
            parameters.optionMaxBitrate.interactable = false;
            parameters.optionMinBitrate.interactable = false;
            parameters.optionScaleResolution.interactable = false;
        }

        var configuration = GetSelectedSdpSemantics();
        _pc1 = new RTCPeerConnection(ref configuration);
        _pc1.OnIceCandidate = pc1OnIceCandidate;
        _pc1.OnIceConnectionChange = pc1OnIceConnectionChange;
        _pc1.OnNegotiationNeeded = pc1OnNegotiationNeeded;
        _pc2 = new RTCPeerConnection(ref configuration);
        _pc2.OnIceCandidate = pc2OnIceCandidate;
        _pc2.OnIceConnectionChange = pc2OnIceConnectionChange;
        _pc2.OnTrack = pc2Ontrack;

        AddTracks();
    }

    private void RestartIce()
    {
        restartButton.interactable = false;

        _pc1.RestartIce();
    }

    private void HangUp()
    {
        RemoveTracks();

        _pc1.Close();
        _pc2.Close();
        _pc1.Dispose();
        _pc2.Dispose();
        _pc1 = null;
        _pc2 = null;

        callButton.interactable = true;
        restartButton.interactable = false;
        hangUpButton.interactable = false;

        foreach (var parameters in encoderParameters)
        {
            parameters.optionMaxBitrate.interactable = true;
            parameters.optionMinBitrate.interactable = true;
            parameters.optionScaleResolution.interactable = true;
        }


        receiveImage.color = Color.black;
    }

    private void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
    {
        GetOtherPc(pc).AddIceCandidate(candidate);
        Debug.Log($"{GetName(pc)} ICE candidate:\n {candidate.Candidate}");
    }

    private string GetName(RTCPeerConnection pc)
    {
        return (pc == _pc1) ? "pc1" : "pc2";
    }

    private RTCPeerConnection GetOtherPc(RTCPeerConnection pc)
    {
        return (pc == _pc1) ? _pc2 : _pc1;
    }

    private IEnumerator OnCreateOfferSuccess(RTCPeerConnection pc, RTCSessionDescription desc)
    {
        Debug.Log($"Offer from {GetName(pc)}\n{desc.sdp}");
        Debug.Log($"{GetName(pc)} setLocalDescription start");
        var op = pc.SetLocalDescription(ref desc);
        yield return op;

        if (!op.IsError)
        {
            OnSetLocalSuccess(pc);
        }
        else
        {
            var error = op.Error;
            OnSetSessionDescriptionError(ref error);
            yield break;
        }

        var otherPc = GetOtherPc(pc);
        Debug.Log($"{GetName(otherPc)} setRemoteDescription start");
        var op2 = otherPc.SetRemoteDescription(ref desc);
        yield return op2;
        if (!op2.IsError)
        {
            OnSetRemoteSuccess(otherPc);
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
            yield break;
        }

        Debug.Log($"{GetName(otherPc)} createAnswer start");
        // Since the 'remote' side has no media stream we need
        // to pass in the right constraints in order for it to
        // accept the incoming offer of audio and video.

        var op3 = otherPc.CreateAnswer();
        yield return op3;
        if (!op3.IsError)
        {
            yield return OnCreateAnswerSuccess(otherPc, op3.Desc);
        }
        else
        {
            OnCreateSessionDescriptionError(op3.Error);
        }
    }

    private void OnSetLocalSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} SetLocalDescription complete");
    }

    void OnSetSessionDescriptionError(ref RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
        HangUp();
    }

    private void OnSetRemoteSuccess(RTCPeerConnection pc)
    {
        Debug.Log($"{GetName(pc)} SetRemoteDescription complete");
    }

    IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription desc)
    {
        Debug.Log($"Answer from {GetName(pc)}:\n{desc.sdp}");
        Debug.Log($"{GetName(pc)} setLocalDescription start");
        var op = pc.SetLocalDescription(ref desc);
        yield return op;

        if (!op.IsError)
        {
            OnSetLocalSuccess(pc);
        }
        else
        {
            var error = op.Error;
            OnSetSessionDescriptionError(ref error);
        }

        var otherPc = GetOtherPc(pc);
        Debug.Log($"{GetName(otherPc)} setRemoteDescription start");

        var op2 = otherPc.SetRemoteDescription(ref desc);
        yield return op2;
        if (!op2.IsError)
        {
            OnSetRemoteSuccess(otherPc);
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
        }
    }

    private static void OnCreateSessionDescriptionError(RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
    }
}
