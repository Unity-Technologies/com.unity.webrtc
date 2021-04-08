using System;
using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.Assertions;
using UnityEngine.UI;

class PerfectNegotiationSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button callButton;
    [SerializeField] private Button hangUpButton;
    [SerializeField] private Camera cam1;
    [SerializeField] private Camera cam2;
    [SerializeField] private RawImage sourceImage1;
    [SerializeField] private RawImage sourceImage2;
    [SerializeField] private RawImage receiveImage1;
    [SerializeField] private RawImage receiveImage2;
    [SerializeField] private Transform rotateObject;
#pragma warning restore 0649

    private Peer politePeer, imPolitePeer;

    private void Awake()
    {
        WebRTC.Initialize(EncoderType.Software);
        politePeer = new Peer(this, true);
        imPolitePeer = new Peer(this, false);
    }

    private void OnDestroy()
    {
        politePeer.Dispose();
        imPolitePeer.Dispose();
        WebRTC.Dispose();
    }

    private void Start()
    {
        callButton.interactable = true;
        hangUpButton.interactable = false;
    }

    private void Update()
    {
        if (rotateObject != null)
        {
            rotateObject.Rotate(1, 2, 3);
        }
    }

    public void PostMessage(Peer from, Message message)
    {
        var other = from == politePeer ? imPolitePeer : politePeer;
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
    private readonly bool polite;
    private PerfectNegotiationSample parent;
    private RTCPeerConnection pc;

    private bool makingOffer;
    private bool ignoreOffer;
    private bool srdAnswerPending;

    public Peer(PerfectNegotiationSample parent, bool polite)
    {
        this.parent = parent;
        this.polite = polite;
        var config = GetSelectedSdpSemantics();
        pc = new RTCPeerConnection(ref config);

        pc.OnTrack = e =>
        {
            Debug.Log($"{this} OnTrack");
            // remoteVideo.srcObject = new MediaStream();
            // remoteVideo.srcObject.addTrack(e.Track);
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
    }

    private IEnumerator NegotiationProcess()
    {
        Debug.Log("SLD due to negotiationneeded");
        Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable,
            "negotiationneeded always fires in stable state");
        Assert.AreEqual(makingOffer, false, "negotiationneeded not already in progress");

        makingOffer = true;
        var op = pc.SetLocalDescription();
        yield return op;

        if (op.IsError)
        {
            makingOffer = false;
            yield break;
        }

        Assert.AreEqual(pc.SignalingState, RTCSignalingState.HaveLocalOffer,
            "negotiationneeded always fires in stable state");
        Assert.AreEqual(pc.LocalDescription.type, RTCSdpType.Offer, "negotiationneeded SLD worked");

        var offer = new Message {description = pc.LocalDescription};
        parent.PostMessage(this, offer);
        makingOffer = false;
    }

    public void Dispose()
    {
        pc.Dispose();
        parent = null;
    }

    public override string ToString()
    {
        var str = polite ? "polite" : "impolite";
        return $"{str} {base.ToString()}";
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
            //ToDO: run command
            return;
        }

        if (message.candidate != null)
        {
            if (!pc.AddIceCandidate(message.candidate) && ! ignoreOffer)
            {
                throw new ArgumentException("this candidate can not accept.");
            }
            return;
        }

        parent.StartCoroutine(OfferAnswerProcess(message));
    }

    private IEnumerator OfferAnswerProcess(Message message)
    {
        var description = message.description;
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
        Debug.Log($"SRD {description.type}");
        var op1 = pc.SetRemoteDescription(ref description);
        yield return op1;

        srdAnswerPending = false;
        if (description.type == RTCSdpType.Offer)
        {
            Assert.AreEqual(pc.SignalingState, RTCSignalingState.HaveRemoteOffer, "Remote offer");
            Assert.AreEqual(pc.RemoteDescription.type, RTCSdpType.Offer, "SRD worked");
            Debug.Log("SLD to get back to stable");

            var op2 = pc.SetLocalDescription();
            yield return op2;

            Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable, "onmessage not racing with negotiationneeded");
            Assert.AreEqual(pc.LocalDescription.type, RTCSdpType.Answer, "onmessage SLD worked");

            var answer = new Message {description = pc.LocalDescription};
            parent.PostMessage(this, answer);
        }
        else
        {
            Assert.AreEqual(pc.RemoteDescription.type, RTCSdpType.Answer, "Answer was set");
            Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable, "answered");
        }
    }
}
