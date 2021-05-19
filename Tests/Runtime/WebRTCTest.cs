using NUnit.Framework;
using UnityEngine;

namespace Unity.WebRTC.RuntimeTest
{
    public class WebRTCTest
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
        public void GraphicsFormat()
        {
            var graphicsFormat = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var renderTextureFormat = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var textureFormat = WebRTC.GetSupportedTextureFormat(SystemInfo.graphicsDeviceType);

            var rt = new RenderTexture(10, 10, 0, renderTextureFormat);
            rt.Create();
            Assert.That(rt.graphicsFormat, Is.EqualTo(graphicsFormat));

            var tx = new Texture2D(10, 10, textureFormat, false);
            Assert.That(tx.graphicsFormat, Is.EqualTo(graphicsFormat));

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tx);
        }

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
    }
}
