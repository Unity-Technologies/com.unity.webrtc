using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using Object = UnityEngine.Object;

namespace Unity.WebRTC.RuntimeTest
{
    class MediaStreamTrackTest
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
        public void Construct()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack(rt);
            Assert.That(track, Is.Not.Null);
            track.Dispose();
            Object.DestroyImmediate(rt);
        }

        [Test]
        public void EqualIdWithAudioTrack()
        {
            var guid = Guid.NewGuid().ToString();
            var track = new AudioStreamTrack(WebRTC.Context.CreateAudioTrack(guid));
            Assert.That(track, Is.Not.Null);
            Assert.That(track.Id, Is.EqualTo(guid));
            track.Dispose();
        }

        [Test]
        public void EqualIdWithVideoTrack()
        {
            var guid = Guid.NewGuid().ToString();
            var track = new VideoStreamTrack(WebRTC.Context.CreateVideoTrack(guid));
            Assert.That(track, Is.Not.Null);
            Assert.That(track.Id, Is.EqualTo(guid));
            track.Dispose();
        }

        [Test]
        public void AccessAfterDisposed()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack(rt);
            Assert.That(track, Is.Not.Null);
            track.Dispose();
            Assert.That(() => { var id = track.Id; }, Throws.TypeOf<InvalidOperationException>());
            Object.DestroyImmediate(rt);
        }

        [Test]
        public void ConstructorThrowsExceptionWhenInvalidGraphicsFormat()
        {
            var width = 256;
            var height = 256;
            var format = RenderTextureFormat.R8;
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();

            Assert.That(() => { new VideoStreamTrack(rt); }, Throws.TypeOf<ArgumentException>());
            Object.DestroyImmediate(rt);
        }

        [Test]
        [UnityPlatform(RuntimePlatform.Android)]
        public void ConstructThrowsExceptionWhenSmallTexture()
        {
            var width = 50;
            var height = 50;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();

            Assert.That(() => { new VideoStreamTrack(rt); }, Throws.TypeOf<ArgumentException>());

            Object.DestroyImmediate(rt);
        }

        // todo(kazuki): Crash on windows standalone player
        [UnityTest]
        [Timeout(5000)]
        [Category("MediaStreamTrack")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer, RuntimePlatform.WindowsPlayer })]
        public IEnumerator VideoStreamTrackEnabled()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack(rt);
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
        [Category("MediaStreamTrack")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        public IEnumerator CaptureStreamTrack()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var track = cam.CaptureStreamTrack(1280, 720, 1000000);
            Assert.That(track, Is.Not.Null);
            yield return new WaitForSeconds(0.1f);
            track.Dispose();
            // wait for disposing video track.
            yield return 0;

            Object.DestroyImmediate(camObj);
        }

        [Test]
        [Category("MediaStreamTrack")]
        public void CaptureStreamTrackThrowExeption()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            Assert.That(() => cam.CaptureStreamTrack(0, 0, 1000000), Throws.ArgumentException);

            Object.DestroyImmediate(camObj);
        }


        [Test]
        [Category("MediaStreamTrack")]
        public void AddAndRemoveAudioStreamTrack()
        {
            var stream = new MediaStream();
            var track = new AudioStreamTrack();
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
        [Category("MediaStreamTrack")]
        public void VideoStreamTrackDisposeImmediately()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack(rt);

            track.Dispose();
            Object.DestroyImmediate(rt);
        }

        [UnityTest]
        [Timeout(5000)]
        [Category("MediaStreamTrack")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        public IEnumerator VideoStreamTrackInstantiateMultiple()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt1 = new UnityEngine.RenderTexture(width, height, 0, format);
            rt1.Create();
            var track1 = new VideoStreamTrack(rt1);

            var rt2 = new UnityEngine.RenderTexture(width, height, 0, format);
            rt2.Create();
            var track2 = new VideoStreamTrack(rt2);

            // wait for initialization encoder on render thread.
            yield return new WaitForSeconds(0.1f);

            track1.Dispose();
            track2.Dispose();
            Object.DestroyImmediate(rt1);
            Object.DestroyImmediate(rt2);
        }
    }
}
