using System.Collections;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

public class VideoReceiveSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button callButton;
    [SerializeField] private Camera senderCam;
    [SerializeField] private RawImage receiveImage;
    private MediaStream videoSendStream;
    private MediaStream videoReceiveStream;
    private RTCPeerConnection senderPc;
    private RTCPeerConnection receiverPc;
#pragma warning restore 0649

    private void Awake()
    {
        WebRTC.Initialize();
        callButton.onClick.AddListener(Call);
    }

    private void OnDestroy()
    {
        WebRTC.Dispose();
    }

    private void Start()
    {
        videoReceiveStream = new MediaStream();
    }

    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};

        return config;
    }

    private void Call()
    {
        callButton.interactable = false;
        Debug.Log("GetSelectedSdpSemantics");
        var configuration = GetSelectedSdpSemantics();

        senderPc = new RTCPeerConnection(ref configuration);
        Debug.Log($"Created local peer connection object {nameof(senderPc)}");

        senderPc.OnIceCandidate = candidate =>
        {
            receiverPc.AddIceCandidate(ref candidate);
            Debug.Log($"{nameof(senderPc)} ICE candidate:\n {candidate.candidate}");
        };

        senderPc.OnIceConnectionChange = state =>
        {
            Debug.Log($"{nameof(senderPc)} iceConnectionState changed: {state}");
        };

        senderPc.OnNegotiationNeeded = () => StartCoroutine(SenderPcOnNegotiationNeeded());

        receiverPc = new RTCPeerConnection(ref configuration);
        Debug.Log($"Created local peer connection object {nameof(receiverPc)}");

        receiverPc.OnIceCandidate = candidate =>
        {
            senderPc.AddIceCandidate(ref candidate);
            Debug.Log($"{nameof(receiverPc)} ICE candidate:\n {candidate.candidate}");
        };

        receiverPc.OnIceConnectionChange = state =>
        {
            Debug.Log($"{nameof(receiverPc)} iceConnectionState changed: {state}");
        };

        receiverPc.OnTrack = e =>
        {
            if (e.Track.Kind == TrackKind.Video)
            {
                videoReceiveStream.AddTrack(e.Track);
            }
        };

        videoReceiveStream.OnAddTrack = e =>
        {
            if (e.Track.Kind == TrackKind.Video)
            {
                var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
                var rt = new RenderTexture(1280, 720, (int)RenderTextureDepth.DEPTH_24, format);
                rt.Create();
                receiveImage.texture = rt;
                // var sink = new VideoSink(rt);
                // var videoTrack = (VideoStreamTrack) e.Track;
                // videoTrack.AddOrUpdateSink(sink)
            }
        };

        videoSendStream = senderCam.CaptureStream(1280, 720, 1000000);
    }

    private IEnumerator SenderPcOnNegotiationNeeded()
    {
        RTCOfferOptions option = default;

        Debug.Log($"{nameof(senderPc)} createOffer start");
        var op = senderPc.CreateOffer(ref option);
        yield return op;

        if (!op.IsError)
        {
            yield return StartCoroutine(OnCreateOfferSuccess(op.Desc));
        }
        else
        {
            OnRTCOperationError(op.Error);
        }
    }

    private IEnumerator OnCreateOfferSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"Offer from {nameof(senderPc)}\n{desc.sdp}");
        Debug.Log($"{nameof(senderPc)} setLocalDescription start");
        var op = senderPc.SetLocalDescription(ref desc);
        yield return op;

        if (!op.IsError)
        {
            Debug.Log($"{nameof(senderPc)} SetLocalDescription complete");
        }
        else
        {
            OnRTCOperationError(op.Error);
        }

        Debug.Log($"{nameof(receiverPc)} setRemoteDescription start");
        var op2 = receiverPc.SetRemoteDescription(ref desc);
        yield return op2;
        if (!op2.IsError)
        {
            Debug.Log($"{nameof(receiverPc)} SetRemoteDescription complete");
        }
        else
        {
            OnRTCOperationError(op2.Error);
        }
        Debug.Log($"{nameof(receiverPc)} createAnswer start");
        // Since the 'remote' side has no media stream we need
        // to pass in the right constraints in order for it to
        // accept the incoming offer of audio and video.

        RTCAnswerOptions option = default;
        var op3 = receiverPc.CreateAnswer(ref option);
        yield return op3;
        if (!op3.IsError)
        {
            yield return OnCreateAnswerSuccess(op3.Desc);
        }
        else
        {
            OnRTCOperationError(op3.Error);
        }
    }

    private IEnumerator OnCreateAnswerSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"Answer from {nameof(receiverPc)}:\n{desc.sdp}");
        Debug.Log($"{nameof(receiverPc)} setLocalDescription start");
        var op = receiverPc.SetLocalDescription(ref desc);
        yield return op;

        if (!op.IsError)
        {
            Debug.Log($"{nameof(receiverPc)} SetLocalDescription complete");
        }
        else
        {
            OnRTCOperationError(op.Error);
        }

        Debug.Log($"{senderPc} setRemoteDescription start");

        var op2 = senderPc.SetRemoteDescription(ref desc);
        yield return op2;
        if (!op2.IsError)
        {
            Debug.Log($"{nameof(senderPc)} SetRemoteDescription complete");
        }
        else
        {
            OnRTCOperationError(op2.Error);
        }
    }

    private static void OnRTCOperationError(RTCError error)
    {
        Debug.LogError($"Error Type: {error.errorType}, {error.message}");
    }
}
