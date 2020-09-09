using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;

namespace Unity.WebRTC.RuntimeTest
{
    class MediaStreamTrackTest
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

        [UnityTest]
        [Timeout(5000)]
        [Category("VideoStreamTrack")]
        [Ignore("TODO::Crash on windows standalone")]
        public IEnumerator VideoStreamTrackEnabled()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack("video", rt);
            Assert.NotNull(track);

            // wait for the end of the initialization for encoder on the render thread.
            yield return 0;

            // todo:: returns always false.
            // Assert.True(track.IsInitialized);

            // Enabled property
            Assert.True(track.Enabled);
            track.Enabled = false;
            Assert.False(track.Enabled);

            // ReadyState property
            Assert.AreEqual(track.ReadyState, TrackState.Live);

            track.Dispose();

            // wait for disposing video track.
            yield return 0;

            Object.DestroyImmediate(rt);
        }

        // todo::(kazuki) Test execution timed out on linux standalone
        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        public IEnumerator CaptureStreamTrack()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var track = cam.CaptureStreamTrack(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);
            track.Dispose();
            // wait for disposing video track.
            yield return 0;

            Object.DestroyImmediate(camObj);
        }

        [Test]
        public void AddAndRemoveAudioStreamTrack()
        {
            var stream = new MediaStream();
            var track = new AudioStreamTrack("audio");
            Assert.AreEqual(TrackKind.Audio, track.Kind);
            Assert.AreEqual(0, stream.GetAudioTracks().Count());
            Assert.True(stream.AddTrack(track));
            Assert.AreEqual(1, stream.GetAudioTracks().Count());
            Assert.NotNull(stream.GetAudioTracks().First());
            Assert.True(stream.RemoveTrack(track));
            Assert.AreEqual(0, stream.GetAudioTracks().Count());
            track.Dispose();
            stream.Dispose();
        }

        [Test]
        [Category("MediaStream")]
        [Ignore("TODO:: This test occurs crash already")]
        public void VideoStreamTrackDisposeImmediately()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack("video", rt);

            track.Dispose();
            Object.DestroyImmediate(rt);
        }

        [UnityTest]
        [Timeout(5000)]
        [Category("MediaStream")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        public IEnumerator VideoStreamTrackInstantiateMultiple()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt1 = new UnityEngine.RenderTexture(width, height, 0, format);
            rt1.Create();
            var track1 = new VideoStreamTrack("video1", rt1);

            var rt2 = new UnityEngine.RenderTexture(width, height, 0, format);
            rt2.Create();
            var track2 = new VideoStreamTrack("video2", rt2);

            // wait for initialization encoder on render thread.
            yield return new WaitForSeconds(0.1f);

            track1.Dispose();
            track2.Dispose();
            Object.DestroyImmediate(rt1);
            Object.DestroyImmediate(rt2);
        }
    }
}
