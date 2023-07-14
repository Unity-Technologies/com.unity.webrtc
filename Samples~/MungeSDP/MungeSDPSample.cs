using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.UI;

class MungeSDPSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button startButton;
    [SerializeField] private Button callButton;
    [SerializeField] private Button createOfferButton;
    [SerializeField] private Button setOfferButton;
    [SerializeField] private Button createAnswerButton;
    [SerializeField] private Button setAnswerButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Camera cam;
    [SerializeField] private RawImage sourceImage;
    [SerializeField] private RawImage receiveImage;
    [SerializeField] private InputField offerSdpInput;
    [SerializeField] private InputField answerSdpInput;
    [SerializeField] private Transform rotateObject;
#pragma warning restore 0649

    private RTCConfiguration configuration = new RTCConfiguration
    {
        iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } }
    };

    private RTCPeerConnection pcLocal, pcRemote;
    private MediaStream sourceVideoStream, receiveVideoStream;
    private Coroutine updateCoroutine;

    private void Start()
    {
        startButton.onClick.AddListener(Setup);
        callButton.onClick.AddListener(Call);
        createOfferButton.onClick.AddListener(() => StartCoroutine(CreateOffer()));
        setOfferButton.onClick.AddListener(() => StartCoroutine(SetOffer()));
        createAnswerButton.onClick.AddListener(() => StartCoroutine(CreateAnswer()));
        setAnswerButton.onClick.AddListener(() => StartCoroutine(SetAnswer()));
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

        sourceVideoStream = cam.CaptureStream(WebRTCSettings.StreamSize.x, WebRTCSettings.StreamSize.y);
        sourceImage.texture = cam.targetTexture;
        updateCoroutine = StartCoroutine(WebRTC.Update());

        receiveVideoStream = new MediaStream();
        receiveVideoStream.OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                track.OnVideoReceived += tex =>
                {
                    receiveImage.texture = tex;
                };
            }
        };

        startButton.interactable = false;
        callButton.interactable = true;
    }

    private void Call()
    {
        Debug.Log("Starting calls");

        pcLocal = new RTCPeerConnection(ref configuration);
        pcRemote = new RTCPeerConnection(ref configuration);
        pcRemote.OnTrack = e => receiveVideoStream.AddTrack(e.Track);
        pcLocal.OnIceCandidate = candidate => pcRemote.AddIceCandidate(candidate);
        pcRemote.OnIceCandidate = candidate => pcLocal.AddIceCandidate(candidate);
        Debug.Log("pc1: created local and remote peer connection object");

        var senders = new List<RTCRtpSender>();
        foreach (var track in sourceVideoStream.GetTracks())
        {
            senders.Add(pcLocal.AddTrack(track, sourceVideoStream));
        }

        if (WebRTCSettings.UseVideoCodec != null)
        {
            var codecs = new[] { WebRTCSettings.UseVideoCodec };
            foreach (var transceiver in pcLocal.GetTransceivers())
            {
                if (senders.Contains(transceiver.Sender))
                {
                    transceiver.SetCodecPreferences(codecs);
                }
            }
        }

        Debug.Log("Adding local stream to pcLocal");

        callButton.interactable = false;
        createOfferButton.interactable = true;
        createAnswerButton.interactable = true;
        setOfferButton.interactable = true;
        setAnswerButton.interactable = true;
        hangUpButton.interactable = true;
    }

    private IEnumerator CreateOffer()
    {
        var op = pcLocal.CreateOffer();
        yield return op;

        if (op.IsError)
        {
            OnCreateSessionDescriptionError(op.Error);
            yield break;
        }

        offerSdpInput.text = op.Desc.sdp;
        offerSdpInput.interactable = true;
    }

    private IEnumerator SetOffer()
    {
        var offer = new RTCSessionDescription { type = RTCSdpType.Offer, sdp = offerSdpInput.text };
        Debug.Log($"Modified Offer from LocalPeerConnection\n{offer.sdp}");

        var opLocal = pcLocal.SetLocalDescription(ref offer);
        yield return opLocal;

        if (opLocal.IsError)
        {
            OnSetSessionDescriptionError(opLocal.Error);
            yield break;
        }

        Debug.Log("Set Local session description success on LocalPeerConnection");

        var opRemote = pcRemote.SetRemoteDescription(ref offer);
        yield return opRemote;

        if (opRemote.IsError)
        {
            OnSetSessionDescriptionError(opRemote.Error);
            yield break;
        }

        Debug.Log("Set Remote session description success on RemotePeerConnection");
    }

    private IEnumerator CreateAnswer()
    {
        var op = pcRemote.CreateAnswer();
        yield return op;

        if (op.IsError)
        {
            OnCreateSessionDescriptionError(op.Error);
            yield break;
        }

        answerSdpInput.text = op.Desc.sdp;
        answerSdpInput.interactable = true;
    }

    private IEnumerator SetAnswer()
    {
        var answer = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = answerSdpInput.text };
        Debug.Log($"Modified Answer from RemotePeerConnection\n{answer.sdp}");

        var opLocal = pcRemote.SetLocalDescription(ref answer);
        yield return opLocal;

        if (opLocal.IsError)
        {
            OnSetSessionDescriptionError(opLocal.Error);
            yield break;
        }

        Debug.Log("Set Local session description success on RemotePeerConnection");

        var opRemote = pcLocal.SetRemoteDescription(ref answer);
        yield return opRemote;

        if (opRemote.IsError)
        {
            OnSetSessionDescriptionError(opRemote.Error);
            yield break;
        }

        Debug.Log("Set Remote session description success on LocalPeerConnection");
    }

    private void HangUp()
    {
        StopCoroutine(updateCoroutine);
        updateCoroutine = null;
        sourceVideoStream.Dispose();
        sourceVideoStream = null;
        sourceImage.texture = null;
        receiveVideoStream.Dispose();
        receiveVideoStream = null;
        receiveImage.texture = null;

        offerSdpInput.text = string.Empty;
        answerSdpInput.text = string.Empty;

        pcLocal.Close();
        pcRemote.Close();
        pcLocal.Dispose();
        pcRemote.Dispose();
        pcLocal = null;
        pcRemote = null;

        startButton.interactable = true;
        callButton.interactable = false;
        createOfferButton.interactable = false;
        createAnswerButton.interactable = false;
        setOfferButton.interactable = false;
        setAnswerButton.interactable = false;
        hangUpButton.interactable = false;
        offerSdpInput.interactable = false;
        answerSdpInput.interactable = false;
    }

    private static void OnCreateSessionDescriptionError(RTCError error)
    {
        Debug.LogError($"Failed to create session description: {error.message}");
    }

    private static void OnSetSessionDescriptionError(RTCError error)
    {
        Debug.LogError($"Failed to set session description: {error.message}");
    }
}
