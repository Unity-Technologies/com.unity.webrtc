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
        [Category("PeerConnection")]
        public void SenderGetTrackReturnsNull()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);
            RTCRtpSender sender = transceiver.Sender;
            Assert.That(sender, Is.Not.Null);
            Assert.That(sender.Track, Is.Null);
            Assert.That(peer.GetTransceivers(), Has.Count.EqualTo(1));
            Assert.That(peer.GetTransceivers(), Has.All.Not.Null);
            peer.Dispose();
        }
    }
}
