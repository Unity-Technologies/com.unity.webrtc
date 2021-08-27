using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    //ToDo: decoder is not supported H.264 codec on Windows/Linux.
    [TestFixture]
    [ConditionalIgnore(ConditionalIgnore.UnsupportedReceiveVideoOnHardware, "Not supported hardware decoder")]
    class VideoReceiveTestWithHardwareEncoder : VideoReceiveTestWithSoftwareEncoder
    {
        [OneTimeSetUp]
        public new void OneTimeInit()
        {
            encoderType = EncoderType.Hardware;
        }
    }

    class VideoReceiveTestWithSoftwareEncoder
    {
        protected EncoderType encoderType;

        [OneTimeSetUp]
        public void OneTimeInit()
        {
            encoderType = EncoderType.Software;
        }

        [SetUp]
        public void SetUp()
        {
            var type = TestHelper.HardwareCodecSupport() ? EncoderType.Hardware : EncoderType.Software;
            WebRTC.Initialize(type: type, limitTextureSize: true, forTest: true);
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator InitializeReceiver()
        {
            const int width = 256;
            const int height = 256;

            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.That(transceiver, Is.Not.Null);
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.That(receiver, Is.Not.Null);
            MediaStreamTrack track = receiver.Track;
            Assert.That(track, Is.Not.Null);
            Assert.AreEqual(TrackKind.Video, track.Kind);
            Assert.That(track.Kind, Is.EqualTo(TrackKind.Video));
            var videoTrack = track as VideoStreamTrack;
            Assert.That(videoTrack, Is.Not.Null);
            var rt = videoTrack.InitializeReceiver(width, height);
            Assert.That(videoTrack.IsDecoderInitialized, Is.True);
            videoTrack.Dispose();
            // wait for disposing video track.
            yield return 0;

            peer.Dispose();
            Object.DestroyImmediate(rt);
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
        };

        internal static int[] range = Enumerable.Range(0, 6).ToArray();

        // not supported TestCase attribute on UnityTest
        // refer to https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-tests-parameterized.html
        [UnityTest]
        [Timeout(10000)]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformVideoDecoder,
            "VideoStreamTrack.UpdateReceiveTexture is not supported on Direct3D12")]
        public IEnumerator VideoReceive([ValueSource(nameof(range))]int index)
        {
            if(SystemInfo.processorType == "Apple M1")
                Assert.Ignore("todo:: This test will hang up on Apple M1");

            var value = testValues[index];
            var test = new MonoBehaviourTest<VideoReceivePeers>();
            test.component.SetResolution(value.width, value.height);
            yield return test;
            test.component.CoroutineUpdate();

            IEnumerator VideoReceive()
            {
                test.component.CreatePeers();
                test.component.CreateVideoStreamTrack();
                test.component.AddTrack();

                yield return test.component.Signaling();

                var receiveVideoTrack = test.component.RecvVideoTrack;
                yield return new WaitUntilWithTimeout(() => receiveVideoTrack != null && receiveVideoTrack.IsDecoderInitialized, 5000);
                Assert.That(test.component.RecvTexture, Is.Not.Null);

                yield return new WaitForSeconds(0.1f);

                test.component.Clear();
            }

            for (int i = 0; i < value.count; i++)
            {
                yield return VideoReceive();
                yield return new WaitForSeconds(1);
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

        void Start()
        {
            camObj = new GameObject("Camera");
            cam = camObj.AddComponent<Camera>();

            IsTestFinished = true;
        }

        public void SetResolution(int width, int height)
        {
            this.width = width;
            this.height = height;
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
                if (e.Track is VideoStreamTrack track && !track.IsDecoderInitialized)
                {
                    RecvVideoTrack = track;
                    RecvTexture = track.InitializeReceiver(width, height);
                }
            };
        }
        public void CreateVideoStreamTrack()
        {
            SendVideoTrack = cam.CaptureStreamTrack(width, height, 1000000);
            SendTexture = cam.targetTexture;
        }

        public void AddTrack()
        {
            sender = offerPc.AddTrack(SendVideoTrack);
        }

        public void RemoveTrack()
        {
            offerPc.RemoveTrack(sender);
        }

        public Coroutine CoroutineUpdate()
        {
            return StartCoroutine(WebRTC.Update());
        }

        public void Clear()
        {
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
