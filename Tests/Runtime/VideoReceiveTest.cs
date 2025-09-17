using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    [TestFixture]
    [UnityPlatform(exclude = new[] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.OSXPlayer, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxPlayer })]
    [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
    class VideoReceiveTestWithH264Codec : VideoReceiveTestBase
    {
        protected override void SetUpCodecCapability()
        {
            var mimetype = "H264";
            videoCodec = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs.FirstOrDefault(c => c.mimeType.Contains(mimetype));
            if (videoCodec == null)
                Assert.Ignore($"{mimetype} codec is not supported this platform.");
        }
    }

    [TestFixture]
    [UnityPlatform(exclude = new[] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.OSXPlayer, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxPlayer })]
    [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
    class VideoReceiveTestWithVP8Codec : VideoReceiveTestBase
    {
        protected override void SetUpCodecCapability()
        {
            var mimetype = "VP8";
            videoCodec = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs.FirstOrDefault(c => c.mimeType.Contains(mimetype));
            if (videoCodec == null)
                Assert.Ignore($"{mimetype} codec is not supported this platform.");
        }
    }

    abstract class VideoReceiveTestBase
    {
        protected RTCRtpCodecCapability videoCodec;

        protected abstract void SetUpCodecCapability();

        [SetUp]
        public void SetUp()
        {
            SetUpCodecCapability();
        }

        internal class TestValue
        {
            public int width;
            public int height;
            public int count;
        }

        internal static TestValue[] testValues = new TestValue[]
        {
            new TestValue{width = 256, height = 256, count = 1},
            new TestValue{width = 256, height = 256, count = 2},
            new TestValue{width = 256, height = 256, count = 3},
            new TestValue{width = 1280, height = 720, count = 1},
            new TestValue{width = 1280, height = 720, count = 2},
            new TestValue{width = 1280, height = 720, count = 3},
            new TestValue{width = 1920, height = 1080, count = 1},
            new TestValue{width = 1920, height = 1080, count = 2},
            new TestValue{width = 1920, height = 1080, count = 3},
        };

        internal static int[] range = Enumerable.Range(0, 9).ToArray();

        // not supported TestCase attribute on UnityTest
        // refer to https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-tests-parameterized.html
        [UnityTest, LongRunning]
        [Timeout(15000)]
        [ConditionalIgnoreMultiple(ConditionalIgnore.UnsupportedPlatformVideoDecoder,
            "VideoStreamTrack.UpdateTexture is not supported on Direct3D12 for decoder")]
        [ConditionalIgnoreMultiple(ConditionalIgnore.UnsupportedPlatformOpenGL,
            "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator VideoReceive([ValueSource(nameof(range))] int index)
        {
            if (SystemInfo.processorType == "Apple M1")
                Assert.Ignore("todo:: This test will hang up on Apple M1");

            if (ConditionalIgnore.IsUnstableOnGraphicsDevice(Application.platform, SystemInfo.graphicsDeviceName))
            {
                Assert.Ignore($"This test is not stable on {SystemInfo.graphicsDeviceName}");
            }

            var value = testValues[index];
            var test = new MonoBehaviourTest<VideoReceivePeers>();
            test.component.SetResolution(value.width, value.height);
            test.component.SetCodec(videoCodec);
            yield return test;

            IEnumerator VideoReceive()
            {
                test.component.CreatePeers();
                test.component.CreateVideoStreamTrack();
                test.component.AddTrack();

                yield return test.component.Signaling();

                var receiveVideoTrack = test.component.RecvVideoTrack;
                Assert.That(receiveVideoTrack, Is.Not.Null);

                yield return new WaitUntilWithTimeout(() => receiveVideoTrack.Texture != null, 15000);
                Assert.That(receiveVideoTrack.Texture, Is.Not.Null);

                test.component.Clear();
            }

            for (int i = 0; i < value.count; i++)
            {
                yield return VideoReceive();
            }

            Object.DestroyImmediate(test.gameObject);
        }
    }

    class VideoReceivePeers : MonoBehaviour, IMonoBehaviourTest
    {
        public bool IsTestFinished { get; private set; }

        public VideoStreamTrack SendVideoTrack { get; private set; }
        public VideoStreamTrack RecvVideoTrack { get; private set; }
        public Texture SendTexture { get; private set; }
        public Texture RecvTexture { get; private set; }

        RTCPeerConnection offerPc;
        RTCPeerConnection answerPc;
        RTCRtpSender sender;
        GameObject camObj;
        Camera cam;
        int width;
        int height;
        RTCRtpCodecCapability videoCodec;
        Coroutine coroutine;

        void Start()
        {
            camObj = new GameObject("Camera");
            cam = camObj.AddComponent<Camera>();

            IsTestFinished = true;

            coroutine = StartCoroutine(WebRTC.Update());
        }



        public void SetResolution(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public void SetCodec(RTCRtpCodecCapability codec)
        {
            this.videoCodec = codec;
        }

        public void CreatePeers()
        {
            var config = new RTCConfiguration
            {
                iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } }
            };
            offerPc = new RTCPeerConnection(ref config);
            answerPc = new RTCPeerConnection(ref config);

            answerPc.OnTrack = e =>
            {
                if (e.Track is VideoStreamTrack track)
                {
                    RecvVideoTrack = track;
                    track.OnVideoReceived += tex => RecvTexture = tex;
                }
            };
        }
        public void CreateVideoStreamTrack()
        {
            SendVideoTrack = cam.CaptureStreamTrack(width, height);
            SendTexture = cam.targetTexture;
        }

        public void AddTrack()
        {
            sender = offerPc.AddTrack(SendVideoTrack);
            if (videoCodec != null)
            {
                var transceiver = offerPc.GetTransceivers().First(t => t.Sender == sender);
                transceiver.SetCodecPreferences(new[] { videoCodec });
            }
        }

        public void RemoveTrack()
        {
            offerPc.RemoveTrack(sender);
        }

        public void Clear()
        {
            RenderTexture.active = null;
            SendVideoTrack?.Dispose();
            SendVideoTrack = null;

            offerPc?.Close();
            offerPc?.Dispose();
            offerPc = null;
            answerPc?.Close();
            answerPc?.Dispose();
            answerPc = null;
            SendTexture = null;
            RecvTexture = null;
        }

        void OnDestroy()
        {
            Clear();
            DestroyImmediate(camObj);
            camObj = null;
            StopCoroutine(coroutine);
        }

        private void Update()
        {
        }

        public IEnumerator Signaling()
        {
            offerPc.OnIceCandidate = candidate => answerPc.AddIceCandidate(candidate);
            answerPc.OnIceCandidate = candidate => offerPc.AddIceCandidate(candidate);

            var pc1CreateOffer = offerPc.CreateOffer();
            yield return pc1CreateOffer;
            Assert.That(pc1CreateOffer.IsError, Is.False, () => $"Failed {nameof(pc1CreateOffer)}, error:{pc1CreateOffer.Error.message}");
            var offerDesc = pc1CreateOffer.Desc;

            var pc1SetLocalDescription = offerPc.SetLocalDescription(ref offerDesc);
            yield return pc1SetLocalDescription;
            Assert.That(pc1SetLocalDescription.IsError, Is.False, () => $"Failed {nameof(pc1SetLocalDescription)}, error:{pc1SetLocalDescription.Error.message}");

            var pc2SetRemoteDescription = answerPc.SetRemoteDescription(ref offerDesc);
            yield return pc2SetRemoteDescription;
            Assert.That(pc2SetRemoteDescription.IsError, Is.False, () => $"Failed {nameof(pc2SetRemoteDescription)}, error:{pc2SetRemoteDescription.Error.message}");

            var pc2CreateAnswer = answerPc.CreateAnswer();
            yield return pc2CreateAnswer;
            Assert.That(pc2CreateAnswer.IsError, Is.False, () => $"Failed {nameof(pc2CreateAnswer)}, error:{pc2CreateAnswer.Error.message}");
            var answerDesc = pc2CreateAnswer.Desc;

            var pc2SetLocalDescription = answerPc.SetLocalDescription(ref answerDesc);
            yield return pc2SetLocalDescription;
            Assert.That(pc2SetLocalDescription.IsError, Is.False, () => $"Failed {nameof(pc2SetLocalDescription)}, error:{pc2SetLocalDescription.Error.message}");

            var pc1SetRemoteDescription = offerPc.SetRemoteDescription(ref answerDesc);
            yield return pc1SetRemoteDescription;
            Assert.That(pc1SetRemoteDescription.IsError, Is.False, () => $"Failed {nameof(pc1SetRemoteDescription)}, error:{pc1SetRemoteDescription.Error.message}");

            var waitConnectOfferPc = new WaitUntilWithTimeout(() =>
                offerPc.IceConnectionState == RTCIceConnectionState.Connected ||
                offerPc.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return waitConnectOfferPc;
            Assert.That(waitConnectOfferPc.IsCompleted, Is.True);

            var waitConnectAnswerPc = new WaitUntilWithTimeout(() =>
                answerPc.IceConnectionState == RTCIceConnectionState.Connected ||
                answerPc.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return waitConnectAnswerPc;
            Assert.That(waitConnectAnswerPc.IsCompleted, Is.True);

            var checkSenders = new WaitUntilWithTimeout(() => offerPc.GetSenders().Any(), 5000);
            yield return checkSenders;
            Assert.That(checkSenders.IsCompleted, Is.True);

            var checkReceivers = new WaitUntilWithTimeout(() => answerPc.GetReceivers().Any(), 5000);
            yield return checkReceivers;
            Assert.That(checkReceivers.IsCompleted, Is.True);
        }
    }
}
