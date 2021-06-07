using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine.UI;
using Random = UnityEngine.Random;

class MultiVideoReceiveSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private InputField widthInput;
    [SerializeField] private InputField heightInput;
    [SerializeField] private Button callButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Button addVideoObjectButton;
    [SerializeField] private Button addTracksButton;
    [SerializeField] private Transform cameraParent;
    [SerializeField] private Transform sourceImageParent;
    [SerializeField] private Transform receiveImageParent;
    [SerializeField] private List<Camera> cameras;
    [SerializeField] private List<RawImage> sourceImages;
    [SerializeField] private List<RawImage> receiveImages;
    [SerializeField] private Transform rotateObject;
#pragma warning restore 0649

    private RTCPeerConnection _pc1, _pc2;
    private List<VideoStreamTrack> videoStreamTrackList;
    private List<RTCRtpSender> sendingSenderList;
    private DelegateOnIceConnectionChange pc1OnIceConnectionChange;
    private DelegateOnIceConnectionChange pc2OnIceConnectionChange;
    private DelegateOnIceCandidate pc1OnIceCandidate;
    private DelegateOnIceCandidate pc2OnIceCandidate;
    private DelegateOnTrack pc2Ontrack;
    private DelegateOnNegotiationNeeded pc1OnNegotiationNeeded;
    private DelegateOnNegotiationNeeded pc2OnNegotiationNeeded;
    private bool videoUpdateStarted;
    private int objectIndex = 0;
    private int videoIndex = 0;
    private const int DefaultWidth = 128;
    private const int DefaultHeight = 128;
    private int width = DefaultWidth;
    private int height = DefaultHeight;

    private void Awake()
    {
        WebRTC.Initialize(WebRTCSettings.EncoderType, WebRTCSettings.LimitTextureSize);
        callButton.onClick.AddListener(Call);
        hangUpButton.onClick.AddListener(HangUp);
        addVideoObjectButton.onClick.AddListener(AddVideoObject);
        addTracksButton.onClick.AddListener(AddTracks);
        widthInput.onValueChanged.AddListener(w =>
        {
            if (!int.TryParse(w, out width))
                width = DefaultWidth;
        });
        heightInput.onValueChanged.AddListener(h =>
        {
            if (!int.TryParse(h, out height))
                height = DefaultHeight;
        });
    }

    private void OnDestroy()
    {
        WebRTC.Dispose();
    }

    private void Start()
    {
        videoStreamTrackList = new List<VideoStreamTrack>();
        sendingSenderList = new List<RTCRtpSender>();
        callButton.interactable = true;
        hangUpButton.interactable = false;

        pc1OnIceConnectionChange = state => { OnIceConnectionChange(_pc1, state); };
        pc2OnIceConnectionChange = state => { OnIceConnectionChange(_pc2, state); };
        pc1OnIceCandidate = candidate => { OnIceCandidate(_pc1, candidate); };
        pc2OnIceCandidate = candidate => { OnIceCandidate(_pc2, candidate); };
        pc2Ontrack = e =>
        {
            if (e.Track is VideoStreamTrack track && !track.IsDecoderInitialized)
            {
                receiveImages[videoIndex].texture = track.InitializeReceiver(width, height);
                videoIndex++;
            }
        };

        pc1OnNegotiationNeeded = () => { StartCoroutine(PeerNegotiationNeeded(_pc1)); };
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

    private void AddVideoObject()
    {
        var newCam = new GameObject($"Camera{objectIndex}").AddComponent<Camera>();
        newCam.backgroundColor = Random.ColorHSV();
        newCam.transform.SetParent(cameraParent);
        cameras.Add(newCam);
        var newSource = new GameObject($"SourceImage{objectIndex}").AddComponent<RawImage>();
        newSource.transform.SetParent(sourceImageParent);
        sourceImages.Add(newSource);
        var newReceive = new GameObject($"ReceiveImage{objectIndex}").AddComponent<RawImage>();
        newReceive.transform.SetParent(receiveImageParent);
        receiveImages.Add(newReceive);

        try
        {
            videoStreamTrackList.Add(newCam.CaptureStreamTrack(width, height, 0));
            newSource.texture = newCam.targetTexture;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            HangUp();
            return;
        }

        objectIndex++;
        addTracksButton.interactable = true;
    }

    private void Call()
    {
        widthInput.interactable = false;
        heightInput.interactable = false;
        callButton.interactable = false;
        hangUpButton.interactable = true;
        addVideoObjectButton.interactable = true;
        addTracksButton.interactable = false;

        Debug.Log("GetSelectedSdpSemantics");
        var configuration = GetSelectedSdpSemantics();
        _pc1 = new RTCPeerConnection(ref configuration);
        Debug.Log("Created local peer connection object pc1");
        _pc1.OnIceCandidate = pc1OnIceCandidate;
        _pc1.OnIceConnectionChange = pc1OnIceConnectionChange;
        _pc1.OnNegotiationNeeded = pc1OnNegotiationNeeded;

        _pc2 = new RTCPeerConnection(ref configuration);
        Debug.Log("Created remote peer connection object pc2");
        _pc2.OnIceCandidate = pc2OnIceCandidate;
        _pc2.OnIceConnectionChange = pc2OnIceConnectionChange;
        _pc2.OnTrack = pc2Ontrack;
        _pc2.OnNegotiationNeeded = pc2OnNegotiationNeeded;

        if (!videoUpdateStarted)
        {
            StartCoroutine(WebRTC.Update());
            videoUpdateStarted = true;
        }
    }

    private void AddTracks()
    {
        Debug.Log("Add not added tracks");

        foreach (var track in videoStreamTrackList.Where(x =>
            !sendingSenderList.Exists(y => y.Track.Id == x.Id)))
        {
            var sender = _pc1.AddTrack(track);
            sendingSenderList.Add(sender);
        }
    }

    private void HangUp()
    {
        foreach (var image in receiveImages.Concat(sourceImages))
        {
            image.texture = null;
            DestroyImmediate(image.gameObject);
        }

        receiveImages.Clear();
        sourceImages.Clear();

        foreach (var cam in cameras)
        {
            DestroyImmediate(cam.gameObject);
        }

        cameras.Clear();

        foreach (var track in videoStreamTrackList)
        {
            track.Dispose();
        }

        videoStreamTrackList.Clear();
        sendingSenderList.Clear();

        _pc1.Close();
        _pc2.Close();
        Debug.Log("Close local/remote peer connection");
        _pc1.Dispose();
        _pc2.Dispose();
        _pc1 = null;
        _pc2 = null;
        videoIndex = 0;
        objectIndex = 0;
        widthInput.interactable = true;
        heightInput.interactable = true;
        callButton.interactable = true;
        hangUpButton.interactable = false;
        addVideoObjectButton.interactable = false;
        addTracksButton.interactable = false;
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
