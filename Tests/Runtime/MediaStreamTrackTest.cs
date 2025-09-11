using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Unity.WebRTC.RuntimeTest
{
    class MediaStreamTrackTest
    {
        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void Constructor()
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
        public void ConstructorThrowException()
        {
            if (VideoStreamTrack.IsSupported(Application.platform, SystemInfo.graphicsDeviceType))
                Assert.Ignore();

            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            Assert.That(() => { var track = new VideoStreamTrack(rt); }, Throws.TypeOf<NotSupportedException>());
            Object.DestroyImmediate(rt);
        }


        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void EqualIdWithAudioTrack()
        {
            var guid = Guid.NewGuid().ToString();
            var source = new AudioTrackSource();
            var track = new AudioStreamTrack(WebRTC.Context.CreateAudioTrack(guid, source.self));
            Assert.That(track, Is.Not.Null);
            Assert.That(track.Id, Is.EqualTo(guid));
            track.Dispose();
            source.Dispose();
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void EqualIdWithVideoTrack()
        {
            var guid = Guid.NewGuid().ToString();
            var source = new VideoTrackSource();
            var track = new VideoStreamTrack(WebRTC.Context.CreateVideoTrack(guid, source.self));
            Assert.That(track, Is.Not.Null);
            Assert.That(track.Id, Is.EqualTo(guid));
            track.Dispose();
            source.Dispose();
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
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
            Assert.That(() => { var id = track.Id; }, Throws.TypeOf<ObjectDisposedException>());
            Object.DestroyImmediate(rt);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void ConstructorThrowsExceptionWhenInvalidGraphicsFormat()
        {
            var width = 256;
            var height = 256;
            var format = RenderTextureFormat.R8;
            var rt = new RenderTexture(width, height, 0, format);
            if (rt.format != format)
            {
                Assert.Ignore("RenderTextureFormat.R8 texture format is not supported on this platform.");
            }
            rt.Create();

            Assert.That(() => { new VideoStreamTrack(rt); }, Throws.TypeOf<ArgumentException>());
            Object.DestroyImmediate(rt);
        }

        [Test]
        [UnityPlatform(RuntimePlatform.Android)]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
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
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer, RuntimePlatform.WindowsPlayer })]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]

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
            Assert.That(track.ReadyState, Is.EqualTo(TrackState.Live));

            track.Dispose();

            // wait for disposing video track.
            yield return 0;

            Object.DestroyImmediate(rt);
        }

        // todo::(kazuki) Test execution timed out on linux standalone
        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator CaptureStreamTrack()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var track = cam.CaptureStreamTrack(1280, 720);
            Assert.That(track, Is.Not.Null);
            yield return new WaitForSeconds(0.1f);
            track.Dispose();
            // wait for disposing video track.
            yield return 0;

            Object.DestroyImmediate(camObj);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void CaptureStreamTrackThrowExeption()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            Assert.That(() => cam.CaptureStreamTrack(0, 0), Throws.ArgumentException);

            Object.DestroyImmediate(camObj);
        }


        [Test]
        public void AddAndRemoveAudioStreamTrack()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var stream = new MediaStream();
            var track = new AudioStreamTrack(source);
            Assert.AreEqual(TrackKind.Audio, track.Kind);
            Assert.AreEqual(0, stream.GetAudioTracks().Count());
            Assert.True(stream.AddTrack(track));
            Assert.AreEqual(1, stream.GetAudioTracks().Count());
            Assert.NotNull(stream.GetAudioTracks().First());
            Assert.True(stream.RemoveTrack(track));
            Assert.AreEqual(0, stream.GetAudioTracks().Count());
            track.Dispose();
            stream.Dispose();
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
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

        [UnityTest, LongRunning]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
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
