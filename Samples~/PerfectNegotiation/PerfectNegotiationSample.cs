using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;

delegate void OnMessageHandler(Peer from, Message message);

class PerfectNegotiationSample : MonoBehaviour
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
    public RTCSdpType type;
    public string sdp;
    public string candidate;
    public string sdpMid;
    public int? sdpMLineIndex;
}

class Peer : IDisposable
{
    private readonly PerfectNegotiationSample parent;
    private readonly bool polite;
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
        pc.OnTrack = e => {
            Debug.Log($"{this} OnTrack");
            // remoteVideo.srcObject = new MediaStream();
            // remoteVideo.srcObject.addTrack(e.Track);
        };
        pc.OnIceCandidate = candidate =>
        {
            var message = new Message
            {
                candidate = candidate.Candidate, sdpMid = candidate.SdpMid, sdpMLineIndex = candidate.SdpMLineIndex
            };
            parent.PostMessage(this, message);
        };

        pc.OnNegotiationNeeded = () =>
        {
            try
            {
                // log('SLD due to negotiationneeded');
                // assert_equals(pc.signalingState, 'stable', 'negotiationneeded always fires in stable state');
                // assert_equals(makingOffer, false, 'negotiationneeded not already in progress');
                makingOffer = true;
                var op = pc.SetLocalDescription();
                // assert_equals(pc.signalingState, 'have-local-offer', 'negotiationneeded not racing with onmessage');
                // assert_equals(pc.localDescription.type, 'offer', 'negotiationneeded SLD worked');

            }
            catch (Exception)
            {
                // fail(e);
            }
            finally
            {
                makingOffer = false;
            }
        };
    }

    public void Dispose()
    {
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
        // try {
        //     if (description) {
        //         // If we have a setRemoteDescription() answer operation pending, then
        //         // we will be "stable" by the time the next setRemoteDescription() is
        //         // executed, so we count this being stable when deciding whether to
        //         // ignore the offer.
        //         const isStable =
        //         pc.signalingState == 'stable' ||
        //             (pc.signalingState == 'have-local-offer' && srdAnswerPending);
        //         ignoreOffer =
        //             description.type == 'offer' && !polite && (makingOffer || !isStable);
        //         if (ignoreOffer) {
        //             log('glare - ignoring offer');
        //             return;
        //         }
        //         srdAnswerPending = description.type == 'answer';
        //         log(`SRD(${description.type})`);
        //         await pc.setRemoteDescription(description);
        //         srdAnswerPending = false;
        //         if (description.type == 'offer') {
        //             assert_equals(pc.signalingState, 'have-remote-offer', 'Remote offer');
        //             assert_equals(pc.remoteDescription.type, 'offer', 'SRD worked');
        //             log('SLD to get back to stable');
        //             await pc.setLocalDescription();
        //             assert_equals(pc.signalingState, 'stable', 'onmessage not racing with negotiationneeded');
        //             assert_equals(pc.localDescription.type, 'answer', 'onmessage SLD worked');
        //             send(other, {description: pc.localDescription});
        //         } else {
        //             assert_equals(pc.remoteDescription.type, 'answer', 'Answer was set');
        //             assert_equals(pc.signalingState, 'stable', 'answered');
        //             pc.dispatchEvent(new Event('negotiated'));
        //         }
        //     } else if (candidate) {
        //         try {
        //             await pc.addIceCandidate(candidate);
        //         } catch (e) {
        //             if (!ignoreOffer) throw e;
        //         }
        //     } else if (run) {
        //         send(window.parent, {[run.id]: await commands[run.cmd]() || 0});
        //     }
        // } catch (e) {
        //     fail(e);
        // }
    }
}
