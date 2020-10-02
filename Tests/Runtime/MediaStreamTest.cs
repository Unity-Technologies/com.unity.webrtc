using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;

namespace Unity.WebRTC.RuntimeTest
{
    class MediaStreamTest
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
        public void CreateAndDeleteMediaStream()
        {
            var stream = new MediaStream();
            Assert.NotNull(stream);
            stream.Dispose();
        }

        [Test]
        public void RegisterDelegate()
        {
            var stream = new MediaStream();
            stream.OnAddTrack = e => {};
            stream.OnRemoveTrack = e => {};
            stream.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        [Category("MediaStream")]
        [Ignore("TODO::Crash on windows standalone")]
        public IEnumerator VideoStreamAddTrackAndRemoveTrack()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var stream = new MediaStream();
            var track = new VideoStreamTrack("video", rt);

            // wait for the end of the initialization for encoder on the render thread.
            yield return 0;

            Assert.AreEqual(TrackKind.Video, track.Kind);
            Assert.AreEqual(0, stream.GetVideoTracks().Count());
            Assert.True(stream.AddTrack(track));
            Assert.AreEqual(1, stream.GetVideoTracks().Count());
            Assert.NotNull(stream.GetVideoTracks().First());
            Assert.True(stream.RemoveTrack(track));
            Assert.AreEqual(0, stream.GetVideoTracks().Count());
            track.Dispose();
            // wait for disposing video track.
            yield return 0;

            stream.Dispose();
            Object.DestroyImmediate(rt);
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

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator CameraCaptureStream()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(1, videoStream.GetVideoTracks().Count());
            Assert.AreEqual(0, videoStream.GetAudioTracks().Count());
            Assert.AreEqual(1, videoStream.GetTracks().Count());
            foreach (var track in videoStream.GetTracks())
            {
                track.Dispose();
            }
            // wait for disposing video track.
            yield return 0;

            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        [Test]
        public void AddAndRemoveAudioStream()
        {
            var audioStream = Audio.CaptureStream();
            Assert.AreEqual(1, audioStream.GetAudioTracks().Count());
            Assert.AreEqual(0, audioStream.GetVideoTracks().Count());
            Assert.AreEqual(1, audioStream.GetTracks().Count());
            foreach (var track in audioStream.GetTracks())
            {
                track.Dispose();
            }
            audioStream.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator AddAndRemoveAudioMediaTrack()
        {
            RTCConfiguration config = default;
            config.iceServers = new[]
            {
                new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}
            };
            var audioStream = Audio.CaptureStream();
            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.SetStream(audioStream);
            yield return test;
            test.component.Dispose();
            foreach (var track in audioStream.GetTracks())
            {
                track.Dispose();
            }
            audioStream.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator CaptureStream()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.SetStream(videoStream);
            yield return test;
            yield return new WaitForSeconds(0.1f);
            test.component.Dispose();
            foreach (var track in videoStream.GetTracks())
            {
                track.Dispose();
            }
            // wait for disposing video track.
            yield return 0;

            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SenderGetStats()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);
            var op = test.component.GetSenderStats(0, 0);
            yield return op;
            Assert.True(op.IsDone);
            Assert.IsNotEmpty(op.Value.Stats);
            Assert.Greater(op.Value.Stats.Count, 0);

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.NotNull(stats);
                Assert.Greater(stats.Timestamp, 0);
                Assert.IsNotEmpty(stats.Id);
                foreach (var pair in stats.Dict)
                {
                    Assert.IsNotEmpty(pair.Key);
                }
                StatsCheck.Test(stats);
            }

            op.Value.Dispose();
            test.component.Dispose();
            foreach (var track in videoStream.GetTracks())
            {
                track.Dispose();
            }
            // wait for disposing video track.
            yield return 0;

            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ReceiverGetStats()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);
            var op = test.component.GetReceiverStats(1, 0);
            yield return op;
            Assert.True(op.IsDone);
            Assert.IsNotEmpty(op.Value.Stats);
            Assert.Greater(op.Value.Stats.Count, 0);

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.NotNull(stats);
                Assert.Greater(stats.Timestamp, 0);
                Assert.IsNotEmpty(stats.Id);
                foreach (var pair in stats.Dict)
                {
                    Assert.IsNotEmpty(pair.Key);
                }
                StatsCheck.Test(stats);
            }
            test.component.Dispose();
            foreach (var track in videoStream.GetTracks())
            {
                track.Dispose();
            }
            // wait for disposing video track.
            yield return 0;

            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SetParametersReturnNoError()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);

            var senders = test.component.GetPeerSenders(0);
            Assert.IsNotEmpty(senders);

            foreach(var sender in senders)
            {
                var parameters = sender.GetParameters();
                Assert.IsNotEmpty(parameters.Encodings);
                const uint framerate = 20;
                parameters.Encodings[0].maxFramerate = framerate;
                RTCErrorType error = sender.SetParameters(parameters);
                Assert.AreEqual(RTCErrorType.None, error);
                var parameters2 = sender.GetParameters();
                Assert.AreEqual(framerate, parameters2.Encodings[0].maxFramerate);
            }

            test.component.Dispose();
            foreach (var track in videoStream.GetTracks())
            {
                track.Dispose();
            }
            // wait for disposing video track.
            yield return 0;

            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        // todo::(kazuki) Test execution timed out on linux standalone
        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        public IEnumerator OnAddTrackDelegatesWithEvent()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);

            bool isCalledOnAddTrack = false;
            bool isCalledOnRemoveTrack = false;

            videoStream.OnAddTrack = e =>
            {
                Assert.NotNull(e.Track);
                isCalledOnAddTrack = true;
            };
            videoStream.OnRemoveTrack = e =>
            {
                Assert.NotNull(e.Track);
                isCalledOnRemoveTrack = true;
            };

            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var track2 = new VideoStreamTrack("video2", rt);

            videoStream.AddTrack(track2);
            var op1 = new WaitUntilWithTimeout(() => isCalledOnAddTrack, 5000);
            yield return op1;
            videoStream.RemoveTrack(track2);
            var op2 = new WaitUntilWithTimeout(() => isCalledOnRemoveTrack, 5000);
            yield return op2;

            test.component.Dispose();
            track2.Dispose();
            // wait for disposing video track.
            yield return 0;

            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
            Object.DestroyImmediate(rt);
        }
    }
}
