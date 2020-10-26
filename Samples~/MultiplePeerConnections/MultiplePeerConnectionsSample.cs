using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;

public class MultiplePeerConnectionsSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button startButton;
    [SerializeField] private Button callButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Camera cam;
    [SerializeField] private RawImage sourceImage;
    [SerializeField] private RawImage receiveImage1;
    [SerializeField] private RawImage receiveImage2;
    [SerializeField] private Transform rotateObject;
#pragma warning restore 0649

    private static RTCOfferOptions offerOptions = new RTCOfferOptions
    {
        iceRestart = false, offerToReceiveAudio = true, offerToReceiveVideo = true
    };

    private static RTCAnswerOptions answerOptions = new RTCAnswerOptions {iceRestart = false,};

    private static RTCConfiguration configuration = new RTCConfiguration
    {
        iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}}
    };

    private RTCPeerConnection pc1Local, pc1Remote, pc2Local, pc2Remote;
    private MediaStream sourceVideoStream, receiveVideoStream1, receiveVideoStream2;
    private Coroutine updateCoroutine;

    private void Awake()
    {
        WebRTC.Initialize(EncoderType.Software);
    }

    private void OnDestroy()
    {
        WebRTC.Dispose();
    }

    private void Start()
    {
        startButton.onClick.AddListener(Setup);
        callButton.onClick.AddListener(Call);
        hangUpButton.onClick.AddListener(HangUp);


        startButton.interactable = true;
        callButton.interactable = false;
        hangUpButton.interactable = false;
    }

    private void Update()
    {
        if (rotateObject != null)
        {
            rotateObject.Rotate(1, 2, 3);
        }
    }

    private void Setup()
    {
        Debug.Log("Set up source/receive streams");

        sourceVideoStream = cam.CaptureStream(1280, 720, 1000000);
        sourceImage.texture = cam.targetTexture;
        updateCoroutine = StartCoroutine(WebRTC.Update());

        receiveVideoStream1 = new MediaStream();
        receiveVideoStream2 = new MediaStream();

        receiveVideoStream1.OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                receiveImage1.texture = track.InitializeReceiver(1280, 720);
            }
        };
        receiveVideoStream2.OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                receiveImage2.texture = track.InitializeReceiver(1280, 720);
            }
        };

        startButton.interactable = false;
        callButton.interactable = true;
    }

    private void Call()
    {
        Debug.Log("Starting calls");

        pc1Local = new RTCPeerConnection(ref configuration);
        pc1Remote = new RTCPeerConnection(ref configuration);
        pc1Remote.OnTrack = e => receiveVideoStream1.AddTrack(e.Track);
        pc1Local.OnIceCandidate = candidate => pc1Remote.AddIceCandidate(ref candidate);
        pc1Remote.OnIceCandidate = candidate => pc1Local.AddIceCandidate(ref candidate);
        Debug.Log("pc1: created local and remote peer connection object");

        pc2Local = new RTCPeerConnection(ref configuration);
        pc2Remote = new RTCPeerConnection(ref configuration);
        pc2Remote.OnTrack = e => receiveVideoStream2.AddTrack(e.Track);
        pc2Local.OnIceCandidate = candidate => pc2Remote.AddIceCandidate(ref candidate);
        pc2Remote.OnIceCandidate = candidate => pc2Local.AddIceCandidate(ref candidate);
        Debug.Log("pc2: created local and remote peer connection object");

        foreach (var track in sourceVideoStream.GetTracks())
        {
            pc1Local.AddTrack(track, sourceVideoStream);
            pc2Local.AddTrack(track, sourceVideoStream);
        }

        Debug.Log("Adding local stream to pc1Local/pc2Local");

        StartCoroutine(NegotiationPeer(pc1Local, pc1Remote));
        StartCoroutine(NegotiationPeer(pc2Local, pc2Remote));

        callButton.interactable = false;
        hangUpButton.interactable = true;
    }

    private void HangUp()
    {
        StopCoroutine(updateCoroutine);
        updateCoroutine = null;
        sourceVideoStream.Dispose();
        sourceVideoStream = null;
        sourceImage.texture = null;
        receiveVideoStream1.Dispose();
        receiveVideoStream1 = null;
        receiveImage1.texture = null;
        receiveVideoStream2.Dispose();
        receiveVideoStream2 = null;
        receiveImage2.texture = null;

        pc1Local.Close();
        pc1Remote.Close();
        pc2Local.Close();
        pc2Remote.Close();
        pc1Local.Dispose();
        pc1Remote.Dispose();
        pc2Local.Dispose();
        pc2Remote.Dispose();
        pc1Local = null;
        pc1Remote = null;
        pc2Local = null;
        pc2Remote = null;

        startButton.interactable = true;
        callButton.interactable = false;
        hangUpButton.interactable = false;
    }

    private static void OnCreateSessionDescriptionError(RTCError error)
    {
        Debug.LogError($"Failed to create session description: {error.message}");
    }

    private static IEnumerator NegotiationPeer(RTCPeerConnection localPeer, RTCPeerConnection remotePeer)
    {
        var opCreateOffer = localPeer.CreateOffer(ref offerOptions);
        yield return opCreateOffer;

        if (opCreateOffer.IsError)
        {
            OnCreateSessionDescriptionError(opCreateOffer.Error);
            yield break;
        }

        var offerDesc = opCreateOffer.Desc;
        yield return localPeer.SetLocalDescription(ref offerDesc);
        Debug.Log($"Offer from LocalPeer \n {offerDesc.sdp}");
        yield return remotePeer.SetRemoteDescription(ref offerDesc);

        var opCreateAnswer = remotePeer.CreateAnswer(ref answerOptions);
        yield return opCreateAnswer;

        if (opCreateAnswer.IsError)
        {
            OnCreateSessionDescriptionError(opCreateAnswer.Error);
            yield break;
        }

        var answerDesc = opCreateAnswer.Desc;
        yield return remotePeer.SetLocalDescription(ref answerDesc);
        Debug.Log($"Answer from RemotePeer \n {answerDesc.sdp}");
        yield return localPeer.SetRemoteDescription(ref answerDesc);
    }
}
