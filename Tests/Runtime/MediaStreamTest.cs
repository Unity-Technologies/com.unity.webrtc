using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using Object = UnityEngine.Object;

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
        public void Construct()
        {
            var stream = new MediaStream();
            Assert.That(stream, Is.Not.Null);
            stream.Dispose();
        }

        [Test]
        [Category("MediaStream")]
        public void AccessAfterDisposed()
        {
            var stream = new MediaStream();
            stream.Dispose();
            Assert.That(() => { var id = stream.Id; }, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void RegisterDelegate()
        {
            var stream = new MediaStream();
            stream.OnAddTrack = e => {};
            stream.OnRemoveTrack = e => {};
            stream.Dispose();
        }

        // todo(kazuki): Crash on windows standalone player
        [UnityTest]
        [Timeout(5000)]
        [Category("MediaStream")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer, RuntimePlatform.WindowsPlayer })]
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

            Assert.That(track.Kind, Is.EqualTo(TrackKind.Video));
            Assert.That(stream.GetVideoTracks(), Has.Count.EqualTo(0));
            Assert.That(stream.AddTrack(track), Is.True);
            Assert.That(stream.GetVideoTracks(), Has.Count.EqualTo(1));
            Assert.That(stream.GetVideoTracks(), Has.All.Not.Null);
            Assert.That(stream.RemoveTrack(track), Is.True);
            Assert.That(stream.GetVideoTracks(), Has.Count.EqualTo(0));
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
            Assert.That(TrackKind.Audio, Is.EqualTo(track.Kind));
            Assert.That(stream.GetAudioTracks(), Has.Count.EqualTo(0));
            Assert.That(stream.AddTrack(track), Is.True);
            Assert.That(stream.GetAudioTracks(), Has.Count.EqualTo(1));
            Assert.That(stream.GetAudioTracks(), Has.All.Not.Null);
            Assert.That(stream.RemoveTrack(track), Is.True);
            Assert.That(stream.GetAudioTracks(), Has.Count.EqualTo(0));
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
            Assert.That(videoStream.GetVideoTracks(), Has.Count.EqualTo(1));
            Assert.That(videoStream.GetAudioTracks(), Has.Count.EqualTo(0));
            Assert.That(videoStream.GetTracks().ToList(), Has.Count.EqualTo(1).And.All.InstanceOf<VideoStreamTrack>());
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
            Assert.That(audioStream.GetAudioTracks(), Has.Count.EqualTo(1));
            Assert.That(audioStream.GetVideoTracks(), Has.Count.EqualTo(0));
            Assert.That(audioStream.GetTracks().ToList(),
                Has.Count.EqualTo(1).And.All.InstanceOf<AudioStreamTrack>());
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
            Assert.That(audioStream.GetTracks().ToList(),
                Has.Count.EqualTo(1).And.All.InstanceOf<AudioStreamTrack>());
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
            Assert.That(videoStream.GetTracks().ToList(),
                Has.Count.EqualTo(1).And.All.InstanceOf<VideoStreamTrack>());
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
            Assert.That(op.IsDone, Is.True);
            Assert.That(op.Value.Stats, Has.No.Empty.And.Count.GreaterThan(0));

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.That(stats, Is.Not.Null);
                Assert.That(stats.Timestamp, Is.GreaterThan(0));
                Assert.That(stats.Id, Is.Not.Empty);
                foreach (var pair in stats.Dict)
                {
                    Assert.That(pair.Key, Is.Not.Empty);
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
            Assert.That(op.IsDone, Is.True);
            Assert.That(op.Value.Stats, Has.No.Empty.And.Count.GreaterThan(0));

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.That(stats, Is.Not.Null);
                Assert.That(stats.Timestamp, Is.GreaterThan(0));
                Assert.That(stats.Id, Is.Not.Empty);
                foreach (var pair in stats.Dict)
                {
                    Assert.That(pair.Key, Is.Not.Empty);
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
            Assert.That(senders, Has.Count.GreaterThan(0));

            foreach(var sender in senders)
            {
                var parameters = sender.GetParameters();
                Assert.That(parameters.encodings, Has.Length.GreaterThan(0).And.All.Not.Null);
                const uint framerate = 20;
                parameters.encodings[0].maxFramerate = framerate;
                RTCErrorType error = sender.SetParameters(parameters);
                Assert.That(error, Is.EqualTo(RTCErrorType.None));
                var parameters2 = sender.GetParameters();
                Assert.That(parameters2.encodings[0].maxFramerate, Is.EqualTo(framerate));
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
                Assert.That(e.Track, Is.Not.Null);
                isCalledOnAddTrack = true;
            };
            videoStream.OnRemoveTrack = e =>
            {
                Assert.That(e.Track, Is.Not.Null);
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
