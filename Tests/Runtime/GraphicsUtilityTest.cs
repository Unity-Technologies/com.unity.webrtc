using System;
using NUnit.Framework; 
using UnityEngine;

namespace Unity.WebRTC.RuntimeTest
{
    class GraphicUtilityTest
    {
        [Test]
        public void CreateExternalTexture()
        {
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var texture = GraphicUtility.CreateExternalTexture(256, 256, format);
            Assert.IsNotNull(texture);
            UnityEngine.Object.DestroyImmediate(texture);
        }

        [Test]
        public void CreateVideoFrameBuffer()
        {
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var texture = GraphicUtility.CreateExternalTexture(256, 256, format);
            Assert.IsNotNull(texture);
            VideoFrameBuffer buffer = texture.CreateVideoFrameBuffer();
            Assert.That(buffer.State, Is.EqualTo(VideoFrameBufferState.Unused));
            Assert.IsNotNull(buffer);
            buffer.Dispose();
            Assert.That(buffer.State, Is.EqualTo(VideoFrameBufferState.Unknown));
            UnityEngine.Object.DestroyImmediate(texture);
        }

        [Test]
        public void ReserveVideoFrameBuffer()
        {
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var texture = GraphicUtility.CreateExternalTexture(256, 256, format);
            Assert.IsNotNull(texture);
            VideoFrameBuffer buffer = texture.CreateVideoFrameBuffer();
            Assert.That(buffer.Reserve(), Is.True);
            Assert.That(buffer.State, Is.EqualTo(VideoFrameBufferState.Reserved));

            // already reserved.
            Assert.That(buffer.Reserve(), Is.False);
            Assert.That(buffer.State, Is.EqualTo(VideoFrameBufferState.Reserved));

            buffer.Dispose();
            UnityEngine.Object.DestroyImmediate(texture);
        }

        [Test]
        public void VideoFrameBufferPool()
        {
            var pool = new VideoFrameBufferPool();
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var buffer1 = pool.Create(256, 256, format);
            Assert.That(buffer1, Is.Not.Null);

            // return unused buffer.
            var buffer2 = pool.Create(256, 256, format);
            Assert.That(buffer1, Is.EqualTo(buffer2));

            // reserve buffer1
            Assert.That(buffer1.Reserve(), Is.True);

            // failed multiple reserve.
            Assert.That(buffer2.Reserve(), Is.False);

            // create new one.
            var buffer3 = pool.Create(256, 256, format);
            Assert.That(buffer1, Is.Not.EqualTo(buffer3));
        }
    }
}
