using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Unity.WebRTC.RuntimeTest
{
    class MediaStreamTest
    {
        [Test]
        public void Construct()
        {
            var stream = new MediaStream();
            Assert.That(stream, Is.Not.Null);
            stream.Dispose();
        }

        [Test]
        public void EqualId()
        {
            var guid = Guid.NewGuid().ToString();
            var stream = new MediaStream(WebRTC.Context.CreateMediaStream(guid));
            Assert.That(stream, Is.Not.Null);
            Assert.That(stream.Id, Is.EqualTo(guid));
            stream.Dispose();
        }

        [Test]
        public void AccessAfterDisposed()
        {
            var stream = new MediaStream();
            stream.Dispose();
            Assert.That(() => { var id = stream.Id; }, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void RegisterDelegate()
        {
            var stream = new MediaStream();
            stream.OnAddTrack = e => { };
            stream.OnRemoveTrack = e => { };
            stream.Dispose();
        }

        // todo(kazuki): Crash on Android and Linux standalone player
        [UnityTest]
        [Timeout(5000)]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
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

            Assert.That(RenderTexture.active, Is.Null);
        }

        [Test]
        public void AddAndRemoveAudioTrack()
        {
            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var stream = new MediaStream();
            var track = new AudioStreamTrack(source);
            Assert.That(TrackKind.Audio, Is.EqualTo(track.Kind));
            Assert.That(stream.GetAudioTracks(), Has.Count.EqualTo(0));
            Assert.That(stream.AddTrack(track), Is.True);
            Assert.That(stream.GetAudioTracks(), Has.Count.EqualTo(1));
            Assert.That(stream.GetAudioTracks(), Has.All.Not.Null);
            Assert.That(stream.RemoveTrack(track), Is.True);
            Assert.That(stream.GetAudioTracks(), Has.Count.EqualTo(0));
            track.Dispose();
            stream.Dispose();
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(5000)]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator CameraCaptureStream()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);
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
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator CaptureStream()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);
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
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator SenderGetStats()
        {
            if (SystemInfo.processorType == "Apple M1")
                Assert.Ignore("todo:: This test will hang up on Apple M1");

            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);
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
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator ReceiverGetStats()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);

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
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator SetParametersReturnNoError()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddStream(0, videoStream);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);

            var senders = test.component.GetPeerSenders(0);
            Assert.That(senders, Has.Count.GreaterThan(0));

            foreach (var sender in senders)
            {
                var parameters = sender.GetParameters();
                Assert.That(parameters.encodings, Has.Length.GreaterThan(0).And.All.Not.Null);
                const uint framerate = 20;
                parameters.encodings[0].maxFramerate = framerate;
                RTCError error = sender.SetParameters(parameters);
                Assert.That(error.errorType, Is.EqualTo(RTCErrorType.None));
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

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(include = new[] { RuntimePlatform.Android })]
        public IEnumerator SetParametersReturnErrorIfInvalidTextureResolution()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddStream(0, videoStream);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);

            var senders = test.component.GetPeerSenders(0);
            Assert.That(senders, Has.Count.GreaterThan(0));

            foreach (var sender in senders)
            {
                var parameters = sender.GetParameters();
                Assert.That(parameters.encodings, Has.Length.GreaterThan(0).And.All.Not.Null);
                const uint nonErrorScale = 2;
                parameters.encodings[0].scaleResolutionDownBy = nonErrorScale;
                RTCError error = sender.SetParameters(parameters);
                Assert.That(error.errorType, Is.EqualTo(RTCErrorType.None));
                var parameters2 = sender.GetParameters();
                Assert.That(parameters2.encodings[0].scaleResolutionDownBy, Is.EqualTo(nonErrorScale));

                // limit texture size by WebRTC.ValidateTextureSize
                const uint errorScale = 8;
                parameters2.encodings[0].scaleResolutionDownBy = errorScale;
                RTCError error2 = sender.SetParameters(parameters2);
                Assert.That(error2.errorType, Is.EqualTo(RTCErrorType.InvalidRange));
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
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator AddAndRemoveTrack()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var stream1 = cam.CaptureStream(1280, 720);
            var stream2 = new MediaStream();
            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddStream(0, stream1);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitUntil(() => test.component.NegotiationCompleted());

            bool calledOnAddTrack = false;
            stream2.OnAddTrack = e =>
            {
                Assert.That(e.Track, Is.Not.Null);
                calledOnAddTrack = true;
            };
            var receivers = test.component.GetPeerReceivers(1);
            stream2.AddTrack(receivers.First().Track);
            yield return new WaitUntil(() => calledOnAddTrack);

            var tracks = stream2.GetTracks().ToArray();
            foreach (var track in tracks)
            {
                stream2.RemoveTrack(track);
                track.Dispose();
            }
            Object.DestroyImmediate(camObj);
            Object.DestroyImmediate(test.gameObject);
            yield return new WaitForSeconds(0.1f);
        }

        // todo::(kazuki) Test execution timed out on linux standalone
        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator OnAddTrackDelegatesWithEvent()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);

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
            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var audioTrack = new AudioStreamTrack(source);
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
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }
    }
}
