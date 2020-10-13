using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;

public class MultiVideoReceiveSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button callButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Button addTracksButton;
    [SerializeField] private Camera cam1;
    [SerializeField] private Camera cam2;
    [SerializeField] private RawImage sourceImage1;
    [SerializeField] private RawImage sourceImage2;
    [SerializeField] private RawImage receiveImage1;
    [SerializeField] private RawImage receiveImage2;
    [SerializeField] private Transform rotateObject;
#pragma warning restore 0649

    private RTCPeerConnection _pc1, _pc2;
    private List<VideoStreamTrack> videoStreamTrackList;
    private List<MediaStream> receiveStreamList;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
    private DelegateOnIceCandidate pc1OnIceCandidate;
    private DelegateOnIceCandidate pc2OnIceCandidate;
    private DelegateOnTrack pc2Ontrack;
    private DelegateOnNegotiationNeeded pc2OnNegotiationNeeded;
    private bool videoUpdateStarted;
    private int videoIndex = 0;
    private const int MaxVideoIndexLength = 2;

    private RTCOfferOptions _offerOptions = new RTCOfferOptions
    {
        iceRestart = false, offerToReceiveAudio = false, offerToReceiveVideo = true
    };

    private RTCAnswerOptions _answerOptions = new RTCAnswerOptions {iceRestart = false,};

    private void Awake()
    {
        WebRTC.Initialize(EncoderType.Software);
        callButton.onClick.AddListener(Call);
        hangUpButton.onClick.AddListener(HangUp);
        addTracksButton.onClick.AddListener(AddTransceiver);
    }

    private void OnDestroy()
    {
        WebRTC.Dispose();
    }

    private void Start()
    {
        receiveStreamList = new List<MediaStream>();
        videoStreamTrackList = new List<VideoStreamTrack>();
        callButton.interactable = true;
        hangUpButton.interactable = false;

        pc1OnIceConnectionChange = state => { OnIceConnectionChange(_pc1, state); };
        pc2OnIceConnectionChange = state => { OnIceConnectionChange(_pc2, state); };
        pc1OnIceCandidate = candidate => { OnIceCandidate(_pc1, candidate); };
        pc2OnIceCandidate = candidate => { OnIceCandidate(_pc2, candidate); };
        pc2Ontrack = e =>
        {
            if (e.Track.Kind == TrackKind.Video)
            {
                receiveStreamList[videoIndex % MaxVideoIndexLength].AddTrack(e.Track);
                videoIndex++;
            }

            if (videoIndex >= MaxVideoIndexLength)
            {
                addTracksButton.interactable = false;
            }
        };

        pc2OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc2)); };
    }

    private void Update()
    {
        if (rotateObject != null)
        {
            rotateObject.Rotate(1, 2, 3);
        }
    }

    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};

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
        Debug.Log($"{GetName(pc)} createOffer start");
        var op = pc.CreateOffer(ref _offerOptions);
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

    private void AddTransceiver()
    {
        _pc2.AddTransceiver(TrackKind.Video);
    }

    private void Call()
    {
        callButton.interactable = false;
        hangUpButton.interactable = true;
        addTracksButton.interactable = true;

        Debug.Log("GetSelectedSdpSemantics");
        var configuration = GetSelectedSdpSemantics();
        _pc1 = new RTCPeerConnection(ref configuration);
        Debug.Log("Created local peer connection object pc1");
        _pc1.OnIceCandidate = pc1OnIceCandidate;
        _pc1.OnIceConnectionChange = pc1OnIceConnectionChange;
        _pc2 = new RTCPeerConnection(ref configuration);
        Debug.Log("Created remote peer connection object pc2");
        _pc2.OnIceCandidate = pc2OnIceCandidate;
        _pc2.OnIceConnectionChange = pc2OnIceConnectionChange;
        _pc2.OnTrack = pc2Ontrack;
        _pc2.OnNegotiationNeeded = pc2OnNegotiationNeeded;

        videoStreamTrackList.Add(cam1.CaptureStreamTrack(1280, 720, 1000000));
        sourceImage1.texture = cam1.targetTexture;
        videoStreamTrackList.Add(cam2.CaptureStreamTrack(1280, 720, 1000000));
        sourceImage2.texture = cam2.targetTexture;

        receiveStreamList.Add(new MediaStream());
        receiveStreamList.Add(new MediaStream());

        receiveStreamList[0].OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                receiveImage1.texture = track.InitializeReceiver();
            }
        };
        receiveStreamList[1].OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                receiveImage2.texture = track.InitializeReceiver();
            }
        };

        foreach (var track in videoStreamTrackList)
        {
            _pc1.AddTrack(track);
        }

        if (!videoUpdateStarted)
        {
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
        }
    }

    private void HangUp()
    {
        foreach (var stream in receiveStreamList)
        {
            stream.Dispose();
        }
        receiveStreamList.Clear();

        receiveImage1.texture = null;
        receiveImage2.texture = null;

        foreach (var track in videoStreamTrackList)
        {
            track.Dispose();
        }
        videoStreamTrackList.Clear();

        _pc1.Close();
        _pc2.Close();
        Debug.Log("Close local/remote peer connection");
        _pc1.Dispose();
        _pc2.Dispose();
        _pc1 = null;
        _pc2 = null;
        videoIndex = 0;
        callButton.interactable = true;
        hangUpButton.interactable = false;
        addTracksButton.interactable = false;
    }

    private void OnIceCandidate(RTCPeerConnection pc, RTCIceCandidate candidate)
    {
        GetOtherPc(pc).AddIceCandidate(ref candidate);
        Debug.Log($"{GetName(pc)} ICE candidate:\n {candidate.candidate}");
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
        var op3 = otherPc.CreateAnswer(ref _answerOptions);
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
