using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

class ChangeCodecsSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Dropdown codecSelector;
    [SerializeField] private Button startButton;
    [SerializeField] private Button callButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Text actualCodecText;

    [SerializeField] private Camera cam;
    [SerializeField] private RawImage sourceImage;
    [SerializeField] private RawImage receiveImage;
    [SerializeField] private Transform rotateObject;
#pragma warning restore 0649

    private RTCPeerConnection _pc1, _pc2;
    private List<RTCRtpSender> pc1Senders;
    private MediaStream videoStream, receiveStream;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
    private DelegateOnIceCandidate pc1OnIceCandidate;
    private DelegateOnIceCandidate pc2OnIceCandidate;
    private DelegateOnTrack pc2Ontrack;
    private DelegateOnNegotiationNeeded pc1OnNegotiationNeeded;
    private bool videoUpdateStarted;

    private readonly string[] excludeCodecMimeType = { "video/red", "video/ulpfec", "video/rtx" };
    private RTCRtpCodecCapability[] availableCodecs;

    private const int width = 1280;
    private const int height = 720;

    private void Awake()
    {
        WebRTC.Initialize(WebRTCSettings.EncoderType, WebRTCSettings.LimitTextureSize);
        startButton.onClick.AddListener(OnStart);
        callButton.onClick.AddListener(Call);
        hangUpButton.onClick.AddListener(HangUp);
        receiveStream = new MediaStream();
    }

    private void OnDestroy()
    {
        WebRTC.Dispose();
    }

    private void Start()
    {
        pc1Senders = new List<RTCRtpSender>();
        callButton.interactable = true;
        hangUpButton.interactable = false;
        codecSelector.interactable = false;

        pc1OnIceConnectionChange = state => { OnIceConnectionChange(_pc1, state); };
        pc2OnIceConnectionChange = state => { OnIceConnectionChange(_pc2, state); };
        pc1OnIceCandidate = candidate => { OnIceCandidate(_pc1, candidate); };
        pc2OnIceCandidate = candidate => { OnIceCandidate(_pc2, candidate); };
        pc2Ontrack = e =>
        {
            receiveStream.AddTrack(e.Track);
        };
        pc1OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc1)); };

        receiveStream.OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                receiveImage.texture = track.InitializeReceiver(width, height);
                receiveImage.color = Color.white;
            }
        };
    }

    private void OnStart()
    {
        var capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
        availableCodecs =
            capabilities.codecs.Where(codec => !excludeCodecMimeType.Contains(codec.mimeType)).ToArray();
        var list = availableCodecs
            .Select(codec => new Dropdown.OptionData { text = codec.mimeType + " " + codec.sdpFmtpLine })
            .ToList();
        codecSelector.options.AddRange(list);
        codecSelector.interactable = true;
        startButton.interactable = false;
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
        switch (state)
        {
            case RTCIceConnectionState.New:
                Debug.Log($"{GetName(pc)} IceConnectionState: New");
                break;
            case RTCIceConnectionState.Checking:
                Debug.Log($"{GetName(pc)} IceConnectionState: Checking");
                break;
            case RTCIceConnectionState.Closed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Closed");
                break;
            case RTCIceConnectionState.Completed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Completed");
                break;
            case RTCIceConnectionState.Connected:
                Debug.Log($"{GetName(pc)} IceConnectionState: Connected");
                break;
            case RTCIceConnectionState.Disconnected:
                Debug.Log($"{GetName(pc)} IceConnectionState: Disconnected");
                break;
            case RTCIceConnectionState.Failed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Failed");
                break;
            case RTCIceConnectionState.Max:
                Debug.Log($"{GetName(pc)} IceConnectionState: Max");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
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
        foreach (var track in videoStream.GetTracks())
        {
            pc1Senders.Add(_pc1.AddTrack(track, videoStream));
        }

        if (!videoUpdateStarted)
        {
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
        }

        RTCRtpCodecCapability[] codecs = null;
        if (codecSelector.value == 0)
        {
            codecs = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs;
        }
        else
        {
            RTCRtpCodecCapability preferredCodec = availableCodecs[codecSelector.value-1];
            codecs = new[] { preferredCodec };
        }
        RTCRtpTransceiver transceiver = _pc1.GetTransceivers().First();
        RTCErrorType error = transceiver.SetCodecPreferences(codecs);
        if (error != RTCErrorType.None)
        {
            Debug.LogErrorFormat("RTCRtpTransceiver.SetCodecPreferences failed. {0}", error);
        }
    }

    private void RemoveTracks()
    {
        foreach (var sender in pc1Senders)
        {
            _pc1.RemoveTrack(sender);
        }
        pc1Senders.Clear();

        MediaStreamTrack[] tracks = receiveStream.GetTracks().ToArray();
        foreach (var track in tracks)
        {
            receiveStream.RemoveTrack(track);
            track.Dispose();
        }
    }

    private void Call()
    {
        callButton.interactable = false;
        hangUpButton.interactable = true;
        codecSelector.interactable = false;

        var configuration = GetSelectedSdpSemantics();
        _pc1 = new RTCPeerConnection(ref configuration);
        _pc1.OnIceCandidate = pc1OnIceCandidate;
        _pc1.OnIceConnectionChange = pc1OnIceConnectionChange;
        _pc1.OnNegotiationNeeded = pc1OnNegotiationNeeded;
        _pc2 = new RTCPeerConnection(ref configuration);
        _pc2.OnIceCandidate = pc2OnIceCandidate;
        _pc2.OnIceConnectionChange = pc2OnIceConnectionChange;
        _pc2.OnTrack = pc2Ontrack;

        if (videoStream == null)
        {
            videoStream = cam.CaptureStream(width, height, 1000000);
        }
        sourceImage.texture = cam.targetTexture;
        sourceImage.color = Color.white;

        AddTracks();
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
        hangUpButton.interactable = false;
        codecSelector.interactable = true;
        codecSelector.value = 0;
        actualCodecText.text = string.Empty;

        sourceImage.color = Color.black;
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

    static void OnSetSessionDescriptionError(ref RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
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

            StartCoroutine(CheckActualCodec());
        }
        else
        {
            var error = op2.Error;
            OnSetSessionDescriptionError(ref error);
        }
    }

    // Display the video codec that is actually used.
    IEnumerator CheckActualCodec()
    {
        yield return new WaitForSeconds(1f);
        if (_pc1 == null)
            yield break;

        var op = _pc1.GetStats();
        yield return op;
        if (op.IsError)
        {
            Debug.LogErrorFormat("RTCPeerConnection.GetStats failed: {0}", op.Error);
            yield break;
        }

        RTCStatsReport report = op.Value;

        IEnumerable<RTCOutboundRTPStreamStats> outBoundStatsList = report.Stats.Values.Where(
            stats => stats.Type == RTCStatsType.OutboundRtp).Cast<RTCOutboundRTPStreamStats>();

        RTCOutboundRTPStreamStats outBoundStats = outBoundStatsList.First(stats => stats.kind == "video");
        string codecId = outBoundStats.codecId;

        List<RTCCodecStats> codecStatsList = report.Stats.Values.Where(
            stats => stats.Type == RTCStatsType.Codec).Cast<RTCCodecStats>().ToList();

        RTCCodecStats codecStats =
            codecStatsList.First(stats => stats.Id == codecId);

        actualCodecText.text = string.Format("Using {0} {1}, payloadType={2}.",
            codecStats.mimeType,
            codecStats.sdpFmtpLine,
            codecStats.payloadType
            );
    }

    private static void OnCreateSessionDescriptionError(RTCError error)
    {
        Debug.LogError($"Error Detail Type: {error.message}");
    }
}
