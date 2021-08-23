using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;

namespace Unity.WebRTC.RuntimeTest
{
    class TransceiverTest
    {
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

        [Test]
        [Category("RTCRtpSender")]
        public void SenderGetVideoCapabilities()
        {
            RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
            Assert.NotNull(capabilities);
            Assert.NotNull(capabilities.codecs);
            Assert.IsNotEmpty(capabilities.codecs);
            Assert.NotNull(capabilities.headerExtensions);
            Assert.IsNotEmpty(capabilities.headerExtensions);

            foreach (var codec in capabilities.codecs)
            {
                Assert.NotNull(codec);
                Assert.IsNotEmpty(codec.mimeType);
            }
            foreach (var extensions in capabilities.headerExtensions)
            {
                Assert.NotNull(extensions);
                Assert.IsNotEmpty(extensions.uri);
            }
        }

        [Test]
        [Category("RTCRtpSender")]
        public void SenderGetAudioCapabilities()
        {
            RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Audio);
            Assert.NotNull(capabilities);
            Assert.NotNull(capabilities.codecs);
            Assert.IsNotEmpty(capabilities.codecs);
            Assert.NotNull(capabilities.headerExtensions);
            Assert.IsNotEmpty(capabilities.headerExtensions);

            foreach (var codec in capabilities.codecs)
            {
                Assert.NotNull(codec);
                Assert.IsNotEmpty(codec.mimeType);
            }
            foreach (var extensions in capabilities.headerExtensions)
            {
                Assert.NotNull(extensions);
                Assert.IsNotEmpty(extensions.uri);
            }
        }

        [Test]
        [Category("RTCRtpReceiver")]
        public void ReceiverGetVideoCapabilities()
        {
            RTCRtpCapabilities capabilities = RTCRtpReceiver.GetCapabilities(TrackKind.Video);
            Assert.NotNull(capabilities);
            Assert.NotNull(capabilities.codecs);
            Assert.IsNotEmpty(capabilities.codecs);
            Assert.NotNull(capabilities.headerExtensions);
            Assert.IsNotEmpty(capabilities.headerExtensions);

            foreach (var codec in capabilities.codecs)
            {
                Assert.NotNull(codec);
                Assert.IsNotEmpty(codec.mimeType);
            }
            foreach (var extensions in capabilities.headerExtensions)
            {
                Assert.NotNull(extensions);
                Assert.IsNotEmpty(extensions.uri);
            }
        }

        [Test]
        [Category("RTCRtpReceiver")]
        public void ReceiverGetAudioCapabilities()
        {
            RTCRtpCapabilities capabilities = RTCRtpReceiver.GetCapabilities(TrackKind.Audio);
            Assert.NotNull(capabilities);
            Assert.NotNull(capabilities.codecs);
            Assert.IsNotEmpty(capabilities.codecs);
            Assert.NotNull(capabilities.headerExtensions);
            Assert.IsNotEmpty(capabilities.headerExtensions);

            foreach (var codec in capabilities.codecs)
            {
                Assert.NotNull(codec);
                Assert.IsNotEmpty(codec.mimeType);
            }
            foreach (var extensions in capabilities.headerExtensions)
            {
                Assert.NotNull(extensions);
                Assert.IsNotEmpty(extensions.uri);
            }
        }

        [Test]
        [Category("RTCRtpTransceiver")]
        public void TransceiverSetVideoCodecPreferences()
        {
            var peer = new RTCPeerConnection();
            var capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            var error = transceiver.SetCodecPreferences(capabilities.codecs);
            Assert.AreEqual(RTCErrorType.None, error);
        }

        [Test]
        [Category("RTCRtpTransceiver")]
        public void TransceiverSetAudioCodecPreferences()
        {
            var peer = new RTCPeerConnection();
            var capabilities = RTCRtpSender.GetCapabilities(TrackKind.Audio);
            var transceiver = peer.AddTransceiver(TrackKind.Audio);
            var error = transceiver.SetCodecPreferences(capabilities.codecs);
            Assert.AreEqual(RTCErrorType.None, error);
        }

        [Test]
        [Category("RTCRtpReceiver")]
        public void ReceiverGetTrackReturnsVideoTrack()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);

            // The receiver has a video track
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.That(receiver, Is.Not.Null);
            Assert.That(receiver.Track, Is.Not.Null);
            Assert.That(receiver.Track, Is.TypeOf<VideoStreamTrack>());

            // The receiver has no track
            RTCRtpSender sender = transceiver.Sender;
            Assert.That(sender, Is.Not.Null);
            Assert.That(sender.Track, Is.Null);

            peer.Dispose();
        }

        [Test]
        [Category("RTCRtpReceiver")]
        public void ReceiverGetTrackReturnsAudioTrack()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Audio);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);

            // The receiver has a audio track
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.That(receiver, Is.Not.Null);
            Assert.That(receiver.Track, Is.Not.Null);
            Assert.That(receiver.Track, Is.TypeOf<AudioStreamTrack>());

            // The receiver has no track
            RTCRtpSender sender = transceiver.Sender;
            Assert.That(sender, Is.Not.Null);
            Assert.That(sender.Track, Is.Null);

            peer.Dispose();
        }


        [UnityTest]
        [Timeout(5000)]
        public IEnumerator TransceiverStop()
        {
            if (SystemInfo.processorType == "Apple M1")
                Assert.Ignore("todo:: This test will hang up on Apple M1");

            var go = new GameObject("Test");
            var cam = go.AddComponent<Camera>();

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddTransceiver(0, cam.CaptureStreamTrack(1280, 720, 0));
            yield return test;
            test.component.CoroutineUpdate();

            var senderTransceivers = test.component.GetPeerTransceivers(0);
            Assert.That(senderTransceivers.Count(), Is.EqualTo(1));
            var transceiver1 = senderTransceivers.First();

            var receiverTransceivers = test.component.GetPeerTransceivers(1);
            Assert.That(receiverTransceivers.Count(), Is.EqualTo(1));
            var transceiver2 = receiverTransceivers.First();

            Assert.That(transceiver1.Stop(), Is.EqualTo(RTCErrorType.None));

            // wait for OnNegotiationNeeded callback in SignalingPeers class
            yield return 0;
            yield return new WaitUntil(() => test.component.NegotiationCompleted());

            Assert.That(transceiver1.Direction, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));
            Assert.That(transceiver1.CurrentDirection, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));
            Assert.That(transceiver2.Direction, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));
            Assert.That(transceiver2.CurrentDirection, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(test.gameObject);
        }
    }
}
