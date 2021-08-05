using NUnit.Framework;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class WebRTCTest
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
        public void InitializeTwiceThrowException()
        {
            Assert.That(() => WebRTC.Initialize(), Throws.InvalidOperationException);
        }

        [Test]
        public void GraphicsFormat()
        {
            var graphicsFormat = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var renderTextureFormat = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var textureFormat = WebRTC.GetSupportedTextureFormat(SystemInfo.graphicsDeviceType);

            var rt = new RenderTexture(10, 10, 0, renderTextureFormat);
            rt.Create();
            Assert.That(rt.format, Is.EqualTo(renderTextureFormat),
                $"RenderTexture.format:{rt.format} not equal to supportedFormat:{renderTextureFormat}");
            Assert.That(rt.graphicsFormat, Is.EqualTo(graphicsFormat),
                $"RenderTexture.graphicsFormat:{rt.graphicsFormat} not equal to supportedFormat:{graphicsFormat}");

            var tx = new Texture2D(10, 10, textureFormat, false);
            Assert.That(tx.format, Is.EqualTo(textureFormat),
                $"RenderTexture.format:{tx.format} not equal to supportedFormat:{textureFormat}");
            Assert.That(tx.graphicsFormat, Is.EqualTo(graphicsFormat),
                $"RenderTexture.graphicsFormat:{tx.format} not equal to supportedFormat:{graphicsFormat}");

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tx);
        }

#if WEBRTC_TEST_PROJECT
        [Test]
        [UnityPlatform(exclude = new[] {RuntimePlatform.Android})]
        public void WebCamTextureFormat()
        {
            var webCam = new WebCamTexture(10, 10);
            Assert.That(() => WebRTC.ValidateGraphicsFormat(webCam.graphicsFormat), Throws.Nothing);
            Object.DestroyImmediate(webCam);
        }
#endif

        [Test]
        [TestCase(256, 256)]
        [TestCase(640, 360)]
        [TestCase(1280, 720)]
        [TestCase(1920, 1080)]
        public void ValidateTextureSize(int width, int height)
        {
            var encoderType = WebRTC.GetEncoderType();
            var platform = Application.platform;
            Assert.That(() => WebRTC.ValidateTextureSize(width, height, platform, encoderType), Throws.Nothing);
        }

        [Test]
        public void ValidateGraphicsFormat()
        {
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            Assert.That(() => WebRTC.ValidateGraphicsFormat(format), Throws.Nothing);
        }

        [Test]
        [TestCase((GraphicsFormat)87)] //LegacyARGB32_sRGB
        [TestCase((GraphicsFormat)88)] //LegacyARGB32_UNorm
        public void ValidateLegacyGraphicsFormat(GraphicsFormat format)
        {
            Assert.That(() => WebRTC.ValidateGraphicsFormat(format), Throws.Nothing);
        }
    }
}
