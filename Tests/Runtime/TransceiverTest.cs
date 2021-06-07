using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    class TransceiverTest
    {
        [SetUp]
        public void SetUp()
        {
            var value = TestHelper.HardwareCodecSupport();
            WebRTC.Initialize(value ? EncoderType.Hardware : EncoderType.Software);
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
    }
}
