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
            var type = TestHelper.HardwareCodecSupport() ? EncoderType.Hardware : EncoderType.Software;
            WebRTC.Initialize(type: type, limitTextureSize:true, forTest:true);
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }

        [Test]
        [Category("MediaStream")]
        public void Construct()
        {
            var stream = new MediaStream();
            Assert.That(stream, Is.Not.Null);
            stream.Dispose();
        }

        [Test]
        [Category("MediaStream")]
        public void EqualId()
        {
            var guid = Guid.NewGuid().ToString();
            var stream = new MediaStream(WebRTC.Context.CreateMediaStream(guid));
            Assert.That(stream, Is.Not.Null);
            Assert.That(stream.Id, Is.EqualTo(guid));
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
        [Category("MediaStream")]
        public void RegisterDelegate()
        {
            var stream = new MediaStream();
            stream.OnAddTrack = e => {};
            stream.OnRemoveTrack = e => {};
            stream.Dispose();
        }

        // todo(kazuki): Crash on Android and Linux standalone player
        [UnityTest]
        [Timeout(5000)]
        [Category("MediaStream")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer, RuntimePlatform.Android })]
        public IEnumerator VideoStreamAddTrackAndRemoveTrack()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var stream = new MediaStream();
            var track = new VideoStreamTrack(rt);

            bool isCalledOnAddTrack = false;
            bool isCalledOnRemoveTrack = false;

            stream.OnAddTrack = e =>
            {
                Assert.That(e.Track, Is.EqualTo(track));
                isCalledOnAddTrack = true;
            };
            stream.OnRemoveTrack = e =>
            {
                Assert.That(e.Track, Is.EqualTo(track));
                isCalledOnRemoveTrack = true;
            };

            // wait for the end of the initialization for encoder on the render thread.
            yield return 0;
            Assert.That(track.Kind, Is.EqualTo(TrackKind.Video));
            Assert.That(stream.GetVideoTracks(), Has.Count.EqualTo(0));
            Assert.That(stream.AddTrack(track), Is.True);
            Assert.That(stream.GetVideoTracks(), Has.Count.EqualTo(1));
            Assert.That(stream.GetVideoTracks(), Has.All.Not.Null);
            Assert.That(stream.RemoveTrack(track), Is.True);
            Assert.That(stream.GetVideoTracks(), Has.Count.EqualTo(0));

            var op1 = new WaitUntilWithTimeout(() => isCalledOnAddTrack, 5000);
            yield return op1;
            var op2 = new WaitUntilWithTimeout(() => isCalledOnRemoveTrack, 5000);
            yield return op2;

            track.Dispose();

            stream.Dispose();
            Object.DestroyImmediate(rt);
        }

        [Test]
        public void AddAndRemoveAudioTrack()
        {
            var stream = new MediaStream();
            var track = new AudioStreamTrack();
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

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator CaptureStream()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddStream(0, videoStream);
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
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SenderGetStats()
        {
            if (SystemInfo.processorType == "Apple M1")
                Assert.Ignore("todo:: This test will hang up on Apple M1");

            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddStream(0, videoStream);
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
            Object.DestroyImmediate(test.gameObject);
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
            test.component.AddStream(0, videoStream);
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
            Object.DestroyImmediate(test.gameObject);
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
            test.component.AddStream(0, videoStream);
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
            Object.DestroyImmediate(test.gameObject);
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
            test.component.AddStream(0, videoStream);
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
            var track2 = new VideoStreamTrack(rt);
            yield return 0;

            Assert.That(videoStream.AddTrack(track2), Is.True);
            var op1 = new WaitUntilWithTimeout(() => isCalledOnAddTrack, 5000);
            yield return op1;
            Assert.That(videoStream.RemoveTrack(track2), Is.True);
            var op2 = new WaitUntilWithTimeout(() => isCalledOnRemoveTrack, 5000);
            yield return op2;

            test.component.Dispose();
            track2.Dispose();
            foreach (var track in videoStream.GetTracks())
            {
                track.Dispose();
            }
            // wait for disposing video track.
            yield return 0;

            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ReceiverGetStreams()
        {
            var audioTrack = new AudioStreamTrack();
            var stream = new MediaStream();
            stream.AddTrack(audioTrack);
            yield return 0;

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddStream(0, stream);
            yield return test;

            foreach (var receiver in test.component.GetPeerReceivers(1))
            {
                Assert.That(receiver.Streams, Has.Count.EqualTo(1));
            }

            test.component.Dispose();

            foreach (var track in stream.GetTracks())
            {
                track.Dispose();
            }

            stream.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }
    }
}
