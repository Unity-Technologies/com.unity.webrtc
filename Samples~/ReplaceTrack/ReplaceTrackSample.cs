using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using Unity.WebRTC.Samples;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

class ReplaceTrackSample : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button switchButton;
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;
    [SerializeField] private RawImage sourceImage1;
    [SerializeField] private RawImage sourceImage2;
    [SerializeField] private RawImage receiveImage;
    [SerializeField] private Dropdown dropdown1;
    [SerializeField] private Dropdown dropdown2;
    [SerializeField] private Text textSourceImage1;
    [SerializeField] private Text textSourceImage2;
    [SerializeField] private Text textReceiveImage;
    [SerializeField] private Transform[] rotateObjects;
#pragma warning restore 0649

    List<Vector2Int> streamSizeList = new List<Vector2Int>()
    {
            new Vector2Int(640, 360),
            new Vector2Int(1280, 720),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
            new Vector2Int(3840, 2160),
    };

    private Peer peer1, peer2;
    private Vector2Int size1, size2;

    private void OnDestroy()
    {
        peer1?.Dispose();
        peer2?.Dispose();
    }

    private void Start()
    {
        size1 = streamSizeList[dropdown1.value];
        size2 = streamSizeList[dropdown2.value];

        dropdown1.options = streamSizeList.Select(size => new Dropdown.OptionData($" {size.x} x {size.y} ")).ToList();
        dropdown2.options = streamSizeList.Select(size => new Dropdown.OptionData($" {size.x} x {size.y} ")).ToList();
        dropdown1.onValueChanged.AddListener(value => size1 = streamSizeList[value]);
        dropdown2.onValueChanged.AddListener(value => size2 = streamSizeList[value]);
        dropdown1.value = 0;
        dropdown2.value = 1;

        startButton.onClick.AddListener(OnStart);
        startButton.gameObject.SetActive(true);
        stopButton.onClick.AddListener(OnStop);
        stopButton.gameObject.SetActive(false);
        switchButton.onClick.AddListener(OnSwitchTrack);
        switchButton.interactable = false;

        StartCoroutine(WebRTC.Update());
    }

    private void OnStart()
    {
        peer1 = new Peer(this, true, camera1, size1, camera2, size2);
        peer2 = new Peer(this, false, receiveImage);

        sourceImage1.texture = camera1.targetTexture;
        sourceImage2.texture = camera2.targetTexture;

        sourceImage1.color = Color.white;
        sourceImage2.color = Color.white;
        receiveImage.color = Color.white;

        dropdown1.interactable = false;
        dropdown2.interactable = false;

        startButton.gameObject.SetActive(false);
        stopButton.gameObject.SetActive(true);
        switchButton.interactable = true;
    }

    private void OnStop()
    {
        peer1.Dispose();
        peer2.Dispose();

        sourceImage1.color = Color.black;
        sourceImage2.color = Color.black;
        receiveImage.color = Color.black;

        dropdown1.interactable = true;
        dropdown2.interactable = true;

        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
        switchButton.interactable = false;
    }

    private void OnSwitchTrack()
    {
        peer1.SwitchTrack();
    }

    private void Update()
    {
        if (rotateObjects != null)
        {
            foreach (var rotateObject in rotateObjects)
            {
                float t = Time.deltaTime;
                rotateObject.Rotate(100 * t, 200 * t, 300 * t);
            }
        }

        string TextureResolutionText(RawImage image)
        {
            if (image != null && image.texture != null)
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

        Peer(ReplaceTrackSample parent, bool polite)
        {
            this.parent = parent;
            this.polite = polite;

            var config = GetSelectedSdpSemantics();
            pc = new RTCPeerConnection(ref config);
            pc.OnIceCandidate = candidate =>
            {
                var message = new Message { candidate = candidate };
                this.parent.PostMessage(this, message);
            };

            pc.OnNegotiationNeeded = () =>
            {
                this.parent.StartCoroutine(NegotiationProcess());
            };
        }

        public Peer(ReplaceTrackSample parent, bool polite, RawImage receive)
            : this(parent, polite)
        {
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
        }

        public Peer(
            ReplaceTrackSample parent,
            bool polite,
            Camera source1,
            Vector2Int size1,
            Camera source2,
            Vector2Int size2)
            : this(parent, polite)
        {
            sourceVideoTrack1 = source1?.CaptureStreamTrack(size1.x, size1.y);
            sourceVideoTrack2 = source2?.CaptureStreamTrack(size2.x, size2.y);
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
