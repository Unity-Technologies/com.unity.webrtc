using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine.Assertions;
using UnityEngine.UI;

class PerfectNegotiationSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button politeSwapButton;
    [SerializeField] private Button impoliteSwapButton;
    [SerializeField] private Button swapPoliteFirstButton;
    [SerializeField] private Button swapImpoliteFirstButton;
    [SerializeField] private Camera politeSourceCamera1;
    [SerializeField] private Camera politeSourceCamera2;
    [SerializeField] private Camera impoliteSourceCamera1;
    [SerializeField] private Camera impoliteSourceCamera2;
    [SerializeField] private RawImage politeSourceImage1;
    [SerializeField] private RawImage politeSourceImage2;
    [SerializeField] private RawImage impoliteSourceImage1;
    [SerializeField] private RawImage impoliteSourceImage2;
    [SerializeField] private RawImage politeReceiveImage;
    [SerializeField] private RawImage impoliteReceiveImage;
#pragma warning restore 0649

    private Peer politePeer, impolitePeer;
    private readonly Color red = Color.red;
    private readonly Color magenta = Color.magenta;
    private readonly Color blue = Color.blue;
    private readonly Color cyan = Color.cyan;
    private readonly Color green = Color.green;
    private readonly Color yellow = Color.yellow;
    private int count = 0;
    private const int MAX = 120;
    private float lerp = 0.0f;

    private void Awake()
    {
        WebRTC.Initialize(WebRTCSettings.EncoderType, WebRTCSettings.LimitTextureSize);
    }

    private void OnDestroy()
    {
        politePeer?.Dispose();
        impolitePeer?.Dispose();
        WebRTC.Dispose();
    }

    private void Start()
    {
        politePeer = new Peer(this, true, politeSourceCamera1, politeSourceCamera2, politeReceiveImage);
        impolitePeer = new Peer(this, false, impoliteSourceCamera1, impoliteSourceCamera2, impoliteReceiveImage);

        politeSourceImage1.texture = politeSourceCamera1.targetTexture;
        politeSourceImage2.texture = politeSourceCamera2.targetTexture;
        impoliteSourceImage1.texture = impoliteSourceCamera1.targetTexture;
        impoliteSourceImage2.texture = impoliteSourceCamera2.targetTexture;

        politeSwapButton.onClick.AddListener(politePeer.SwapTransceivers);
        impoliteSwapButton.onClick.AddListener(impolitePeer.SwapTransceivers);
        swapPoliteFirstButton.onClick.AddListener(() =>
        {
            politePeer.SwapTransceivers();
            impolitePeer.SwapTransceivers();
        });
        swapImpoliteFirstButton.onClick.AddListener(() =>
        {
            impolitePeer.SwapTransceivers();
            politePeer.SwapTransceivers();
        });

        StartCoroutine(WebRTC.Update());
    }

    private void Update()
    {
        count++;
        count %= MAX;
        lerp = (float) count / MAX;
        politeSourceCamera1.backgroundColor = Color.LerpUnclamped(red, magenta, lerp);
        politeSourceCamera2.backgroundColor = Color.LerpUnclamped(magenta, yellow, lerp);
        impoliteSourceCamera1.backgroundColor = Color.LerpUnclamped(blue, cyan, lerp);
        impoliteSourceCamera2.backgroundColor = Color.LerpUnclamped(cyan, green, lerp);
    }

    public void PostMessage(Peer from, Message message)
    {
        var other = from == politePeer ? impolitePeer : politePeer;
        other.OnMessage(message);
    }
}

class Message
{
    public RTCSessionDescription description;
    public RTCIceCandidate candidate;
}

class Peer : IDisposable
{
    private readonly PerfectNegotiationSample parent;
    private readonly bool polite;
    private readonly RTCPeerConnection pc;
    private readonly VideoStreamTrack sourceVideoTrack1;
    private readonly VideoStreamTrack sourceVideoTrack2;
    private readonly List<RTCRtpTransceiver> sendingTransceiverList = new List<RTCRtpTransceiver>();

    private bool makingOffer;
    private bool ignoreOffer;
    private bool srdAnswerPending;
    private bool sldGetBackStable;

    private const int width = 128;
    private const int height = 128;

    public Peer(
        PerfectNegotiationSample parent,
        bool polite,
        Camera source1,
        Camera source2,
        RawImage receive)
    {
        this.parent = parent;
        this.polite = polite;


        var config = GetSelectedSdpSemantics();
        pc = new RTCPeerConnection(ref config);

        pc.OnTrack = e =>
        {
            Debug.Log($"{this} OnTrack");
            if (e.Track is VideoStreamTrack video)
            {
                if (video.IsDecoderInitialized)
                {
                    receive.texture = video.Texture;
                    return;
                }

                receive.texture = video.InitializeReceiver(width, height);
            }
        };

        pc.OnIceCandidate = candidate =>
        {
            var message = new Message {candidate = candidate};
            this.parent.PostMessage(this, message);
        };

        pc.OnNegotiationNeeded = () =>
        {
            this.parent.StartCoroutine(NegotiationProcess());
        };

        sourceVideoTrack1 = source1.CaptureStreamTrack(width, height, 0);
        sourceVideoTrack2 = source2.CaptureStreamTrack(width, height, 0);
    }

    private IEnumerator NegotiationProcess()
    {
        Debug.Log($"{this} SLD due to negotiationneeded");

        yield return new WaitWhile(() => sldGetBackStable);

        Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable,
            $"{this} negotiationneeded always fires in stable state");
        Assert.AreEqual(makingOffer, false, $"{this} negotiationneeded not already in progress");

        makingOffer = true;
        var op = pc.SetLocalDescription();
        yield return op;

        if (op.IsError)
        {
            Debug.LogError($"{this} {op.Error.message}");
            makingOffer = false;
            yield break;
        }

        Assert.AreEqual(pc.SignalingState, RTCSignalingState.HaveLocalOffer,
            $"{this} negotiationneeded always fires in stable state");
        Assert.AreEqual(pc.LocalDescription.type, RTCSdpType.Offer, $"{this} negotiationneeded SLD worked");
        makingOffer = false;

        var offer = new Message {description = pc.LocalDescription};
        parent.PostMessage(this, offer);
    }

    public void Dispose()
    {
        sendingTransceiverList.Clear();
        sourceVideoTrack1.Dispose();
        sourceVideoTrack2.Dispose();
        pc.Dispose();
    }

    public override string ToString()
    {
        var str = polite ? "polite" : "impolite";
        return $"[{str}-{base.ToString()}]";
    }

    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};

        return config;
    }

    public void OnMessage(Message message)
    {
        if (message.candidate != null)
        {
            if (!pc.AddIceCandidate(message.candidate) && !ignoreOffer)
            {
                Debug.LogError($"{this} this candidate can't accept current signaling state {pc.SignalingState}.");
            }

            return;
        }

        parent.StartCoroutine(OfferAnswerProcess(message.description));
    }

    private IEnumerator OfferAnswerProcess(RTCSessionDescription description)
    {
        var isStable =
            pc.SignalingState == RTCSignalingState.Stable ||
            (pc.SignalingState == RTCSignalingState.HaveLocalOffer && srdAnswerPending);
        ignoreOffer =
            description.type == RTCSdpType.Offer && !polite && (makingOffer || !isStable);
        if (ignoreOffer)
        {
            Debug.Log($"{this} glare - ignoring offer");
            yield break;
        }

        yield return new WaitWhile(() => makingOffer);

        srdAnswerPending = description.type == RTCSdpType.Answer;
        Debug.Log($"{this} SRD {description.type} SignalingState {pc.SignalingState}");
        var op1 = pc.SetRemoteDescription(ref description);
        yield return op1;
        Assert.IsFalse(op1.IsError, $"{this} {op1.Error.message}");

        srdAnswerPending = false;
        if (description.type == RTCSdpType.Offer)
        {
            Assert.AreEqual(pc.RemoteDescription.type, RTCSdpType.Offer, $"{this} SRD worked");
            Assert.AreEqual(pc.SignalingState, RTCSignalingState.HaveRemoteOffer, $"{this} Remote offer");
            Debug.Log($"{this} SLD to get back to stable");
            sldGetBackStable = true;

            var op2 = pc.SetLocalDescription();
            yield return op2;
            Assert.IsFalse(op2.IsError, $"{this} {op2.Error.message}");

            Assert.AreEqual(pc.LocalDescription.type, RTCSdpType.Answer, $"{this} onmessage SLD worked");
            Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable,
                $"{this} onmessage not racing with negotiationneeded");
            sldGetBackStable = false;

            var answer = new Message {description = pc.LocalDescription};
            parent.PostMessage(this, answer);
        }
        else
        {
            Assert.AreEqual(pc.RemoteDescription.type, RTCSdpType.Answer, $"{this} Answer was set");
            Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable, $"{this} answered");
        }
    }

    public void SwapTransceivers()
    {
        Debug.Log($"{this} swapTransceivers");
        if (sendingTransceiverList.Count == 0)
        {
            var transceiver1 = pc.AddTransceiver(sourceVideoTrack1);
            transceiver1.Direction = RTCRtpTransceiverDirection.SendOnly;
            var transceiver2 = pc.AddTransceiver(sourceVideoTrack2);
            transceiver2.Direction = RTCRtpTransceiverDirection.Inactive;

            sendingTransceiverList.Add(transceiver1);
            sendingTransceiverList.Add(transceiver2);
            return;
        }

        if (sendingTransceiverList[0].CurrentDirection == RTCRtpTransceiverDirection.SendOnly)
        {
            sendingTransceiverList[0].Direction = RTCRtpTransceiverDirection.Inactive;
            sendingTransceiverList[0].Sender.ReplaceTrack(null);
            sendingTransceiverList[1].Direction = RTCRtpTransceiverDirection.SendOnly;
            sendingTransceiverList[1].Sender.ReplaceTrack(sourceVideoTrack2);
        }
        else
        {
            sendingTransceiverList[1].Direction = RTCRtpTransceiverDirection.Inactive;
            sendingTransceiverList[1].Sender.ReplaceTrack(null);
            sendingTransceiverList[0].Direction = RTCRtpTransceiverDirection.SendOnly;
            sendingTransceiverList[0].Sender.ReplaceTrack(sourceVideoTrack1);
        }
    }
}
