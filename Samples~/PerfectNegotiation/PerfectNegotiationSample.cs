using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Random = UnityEngine.Random;

class PerfectNegotiationSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button politeSwapButton;
    [SerializeField] private Button impoliteSwapButton;
    [SerializeField] private Button swapPoliteFirstButton;
    [SerializeField] private Button swapImpoliteFirstButton;
    [SerializeField] private RawImage politeSourceImage1;
    [SerializeField] private RawImage politeSourceImage2;
    [SerializeField] private RawImage impoliteSourceImage1;
    [SerializeField] private RawImage impoliteSourceImage2;
    [SerializeField] private RawImage politeReceiveImage;
    [SerializeField] private RawImage impoliteReceiveImage;
#pragma warning restore 0649

    private Peer politePeer, impolitePeer;

    private void Awake()
    {
        WebRTC.Initialize(EncoderType.Software);
        politeSourceImage1.texture = CreateTexture();
        politeSourceImage2.texture = CreateTexture();
        impoliteSourceImage1.texture = CreateTexture();
        impoliteSourceImage2.texture = CreateTexture();
    }

    private static Texture CreateTexture()
    {
        var format = WebRTC.GetSupportedTextureFormat(SystemInfo.graphicsDeviceType);
        var tex = new Texture2D(100, 100, format, false);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();
        return tex;
    }

    private void OnDestroy()
    {
        politePeer.Dispose();
        impolitePeer.Dispose();
        WebRTC.Dispose();
    }

    private void Start()
    {
        politePeer = new Peer(this, true, politeSourceImage1, politeSourceImage2, politeReceiveImage);
        impolitePeer = new Peer(this, false, impoliteSourceImage1, impoliteSourceImage2, impoliteReceiveImage);

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
        UpdateRed(politeSourceImage1);
        UpdateRed(politeSourceImage2);
        UpdateBlue(impoliteSourceImage1);
        UpdateBlue(impoliteSourceImage2);
    }

    private void UpdateRed(RawImage source)
    {
        var current = source.color;
        source.color = new Color(Random.Range(0f, 1f), current.g, current.b);
    }

    private void UpdateBlue(RawImage source)
    {
        var current = source.color;
        source.color = new Color(current.r, current.g, Random.Range(0f, 1f));
    }

    public void PostMessage(Peer from, Message message)
    {
        var other = from == politePeer ? impolitePeer : politePeer;
        other.OnMessage(message);
    }
}

[Serializable]
class Message
{
    public RTCSessionDescription description;
    public RTCIceCandidate candidate;
    public bool runCommand;
}

class Peer : IDisposable
{
    private readonly PerfectNegotiationSample parent;
    private readonly RawImage receive;
    private readonly bool polite;
    private readonly RTCPeerConnection pc;
    private readonly VideoStreamTrack sourceVideoTrack1;
    private readonly VideoStreamTrack sourceVideoTrack2;

    private bool makingOffer;
    private bool ignoreOffer;
    private bool srdAnswerPending;
    private List<RTCRtpTransceiver> sendingTransceiverList = new List<RTCRtpTransceiver>();

    public Peer(
        PerfectNegotiationSample parent,
        bool polite,
        RawImage source1,
        RawImage source2,
        RawImage receive)
    {
        this.parent = parent;
        this.polite = polite;
        this.receive = receive;

        var config = GetSelectedSdpSemantics();
        pc = new RTCPeerConnection(ref config);

        pc.OnTrack = e =>
        {
            Debug.Log($"{this} OnTrack");
            if (e.Track is VideoStreamTrack video)
            {
                this.receive.texture = video.InitializeReceiver(100, 100);
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

        sourceVideoTrack1 = new VideoStreamTrack($"{this}1", source1.texture);
        sourceVideoTrack2 = new VideoStreamTrack($"{this}2", source2.texture);
    }

    private IEnumerator NegotiationProcess()
    {
        Debug.Log($"{this} SLD due to negotiationneeded");
        Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable,
            $"{this} negotiationneeded always fires in stable state");
        Assert.AreEqual(makingOffer, false, $"{this} negotiationneeded not already in progress");

        makingOffer = true;
        var op = pc.SetLocalDescription();
        yield return op;

        if (op.IsError)
        {
            makingOffer = false;
            yield break;
        }

        Assert.AreEqual(pc.SignalingState, RTCSignalingState.HaveLocalOffer,
            $"{this} negotiationneeded always fires in stable state");
        Assert.AreEqual(pc.LocalDescription.type, RTCSdpType.Offer, $"{this} negotiationneeded SLD worked");

        var offer = new Message {description = pc.LocalDescription};
        parent.PostMessage(this, offer);
        makingOffer = false;
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
        return $"{str}-{base.ToString()}";
    }

    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};

        return config;
    }

    public void OnMessage(Message message)
    {
        if (message.runCommand)
        {
            SwapTransceivers();
            return;
        }

        if (message.candidate != null)
        {
            if (!pc.AddIceCandidate(message.candidate) && !ignoreOffer)
            {
                throw new ArgumentException($"{this} this candidate can not accept.");
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

        srdAnswerPending = description.type == RTCSdpType.Answer;
        Debug.Log($"{this} SRD {description.type}");
        var op1 = pc.SetRemoteDescription(ref description);
        yield return op1;

        srdAnswerPending = false;
        if (description.type == RTCSdpType.Offer)
        {
            Assert.AreEqual(pc.SignalingState, RTCSignalingState.HaveRemoteOffer, $"{this} Remote offer");
            Assert.AreEqual(pc.RemoteDescription.type, RTCSdpType.Offer, $"{this} SRD worked");
            Debug.Log($"{this} SLD to get back to stable");

            var op2 = pc.SetLocalDescription();
            yield return op2;

            Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable, $"{this} onmessage not racing with negotiationneeded");
            Assert.AreEqual(pc.LocalDescription.type, RTCSdpType.Answer, $"{this} onmessage SLD worked");

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
            // This is the first time swapTransceivers is called.
            // Add the initial transceivers, which are remembered for future swaps.

            var transceiver1 = pc.AddTransceiver(sourceVideoTrack1);
            transceiver1.Direction = RTCRtpTransceiverDirection.SendOnly;
            var transceiver2 = pc.AddTransceiver(sourceVideoTrack2);
            transceiver2.Direction = RTCRtpTransceiverDirection.Inactive;

            sendingTransceiverList.Add(transceiver1);
            sendingTransceiverList.Add(transceiver2);
            return;
        }

        // We have sent before. Swap which transceiver is the sending one.
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
