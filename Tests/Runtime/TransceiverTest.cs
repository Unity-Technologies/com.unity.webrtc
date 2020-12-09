using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    class TransceiverTest
    {
        [SetUp]
        public void SetUp()
        {
            var value = NativeMethods.GetHardwareEncoderSupport();
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
        public void TransceiverSetCodecPreferences()
        {
            var peer = new RTCPeerConnection();
            var capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            var error = transceiver.SetCodecPreferences(capabilities.codecs);
            Assert.AreEqual(RTCErrorType.None, error);
        }
    }
}
