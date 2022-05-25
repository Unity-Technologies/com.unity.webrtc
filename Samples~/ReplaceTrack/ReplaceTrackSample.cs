using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine.Assertions;
using UnityEngine.UI;

class ReplaceTrackSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button switchButton;
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;
    [SerializeField] private RawImage sourceImage1;
    [SerializeField] private RawImage sourceImage2;
    [SerializeField] private RawImage receiveImage;
    [SerializeField] private Text textSourceImage1;
    [SerializeField] private Text textSourceImage2;
    [SerializeField] private Text textReceiveImage;
    [SerializeField] private Transform[] rotateObjects;
#pragma warning restore 0649

    private Peer peer1, peer2;

    private void Awake()
    {
        WebRTC.Initialize(WebRTCSettings.LimitTextureSize);
    }

    private void OnDestroy()
    {
        peer1?.Dispose();
        peer2?.Dispose();
        WebRTC.Dispose();
    }

    private void Start()
    {
        peer1 = new Peer(this, true, camera1, camera2, null);
        peer2 = new Peer(this, false, null, null, receiveImage);

        sourceImage1.texture = camera1.targetTexture;
        sourceImage2.texture = camera2.targetTexture;

        sourceImage1.color = Color.white;
        sourceImage2.color = Color.white;
        receiveImage.color = Color.white;

        switchButton.onClick.AddListener(peer1.SwitchTrack);

        StartCoroutine(WebRTC.Update());
    }

    private void Update()
    {
        if (rotateObjects != null)
        {
            foreach(var rotateObject in rotateObjects)
            {
                float t = Time.deltaTime;
                rotateObject.Rotate(100 * t, 200 * t, 300 * t);
            }
        }

        string TextureResolutionText(RawImage image)
        {
            if(image != null && image.texture != null)
                return string.Format($"{image.texture.width}x{image.texture.height}");
            return string.Empty;
        }

        textSourceImage1.text = TextureResolutionText(sourceImage1);
        textSourceImage2.text = TextureResolutionText(sourceImage2);
        textReceiveImage.text = TextureResolutionText(receiveImage);
    }

    void PostMessage(Peer from, Message message)
    {
        var other = from == peer1 ? peer2 : peer1;
        other.OnMessage(message);
    }

    class Message
    {
        public RTCSessionDescription description;
        public RTCIceCandidate candidate;
    }

    class Peer : IDisposable
    {
        private readonly ReplaceTrackSample parent;
        private readonly bool polite;
        private readonly RTCPeerConnection pc;
        private readonly VideoStreamTrack sourceVideoTrack1;
        private readonly VideoStreamTrack sourceVideoTrack2;
        RTCRtpTransceiver sendingTransceiver = null;

        private bool makingOffer;
        private bool ignoreOffer;
        private bool srdAnswerPending;
        private bool sldGetBackStable;

        private const int width = 256;
        private const int height = 256;

        public Peer(
            ReplaceTrackSample parent,
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
                if (e.Track is VideoStreamTrack video)
                {
                    video.OnVideoReceived += tex =>
                    {
                        receive.texture = tex;
                    };
                }
            };

            pc.OnIceCandidate = candidate =>
            {
                var message = new Message { candidate = candidate };
                this.parent.PostMessage(this, message);
            };

            pc.OnNegotiationNeeded = () =>
            {
                this.parent.StartCoroutine(NegotiationProcess());
            };

            sourceVideoTrack1 = source1?.CaptureStreamTrack(width, height, 0);

            int width2 = width * 2;
            int height2 = height * 2;
            sourceVideoTrack2 = source2?.CaptureStreamTrack(width2, height2, 0);
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

            var offer = new Message { description = pc.LocalDescription };
            parent.PostMessage(this, offer);
        }

        public void Dispose()
        {
            //sendingTransceiverList.Clear();
            sourceVideoTrack1?.Dispose();
            sourceVideoTrack2?.Dispose();
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
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };

            return config;
        }

        public void OnMessage(Message message)
        {
            if (message.candidate != null)
            {
                if (!pc.AddIceCandidate(message.candidate) && !ignoreOffer)
                {
                    Debug.LogError($"{this} this candidate can't accept current signaling state {pc.SignalingState}, ignoreOffer {ignoreOffer}.");
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

                var answer = new Message { description = pc.LocalDescription };
                parent.PostMessage(this, answer);
            }
            else
            {
                Assert.AreEqual(pc.RemoteDescription.type, RTCSdpType.Answer, $"{this} Answer was set");
                Assert.AreEqual(pc.SignalingState, RTCSignalingState.Stable, $"{this} answered");
            }
        }

        public void SwitchTrack()
        {
            if (sendingTransceiver == null)
            {
                var transceiver = pc.AddTransceiver(sourceVideoTrack1);
                transceiver.Direction = RTCRtpTransceiverDirection.SendRecv;

                if (WebRTCSettings.UseVideoCodec != null)
                {
                    var codecs = new[] { WebRTCSettings.UseVideoCodec };
                    transceiver.SetCodecPreferences(codecs);
                }

                sendingTransceiver = transceiver;
                return;
            }

            var nextTrack = sendingTransceiver.Sender.Track == sourceVideoTrack1 ? sourceVideoTrack2 : sourceVideoTrack1;
            sendingTransceiver.Sender.ReplaceTrack(nextTrack);
        }
    }
}
