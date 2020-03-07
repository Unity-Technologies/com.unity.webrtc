using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
            WebRTC.Finalize();
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
        public IEnumerator CreateAndDeleteVideoMediaStreamTrack()
        {
            var width = 256;
            var height = 256;
            var bitrate = 1000000;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack("video", rt.GetNativeTexturePtr(), width, height, bitrate);
            yield return new WaitForSeconds(0.1f);
            Assert.NotNull(track);
            track.Dispose();
            yield return new WaitForSeconds(0.1f);
        }


        [UnityTest]
        [Timeout(5000)]
        public IEnumerator AddAndRemoveVideoStreamTrack()
        {
            var width = 256;
            var height = 256;
            var bitrate = 1000000;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            var stream = new MediaStream();
            var track = new VideoStreamTrack("video", rt.GetNativeTexturePtr(), width, height, bitrate);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(0, stream.GetVideoTracks().Count());
            Assert.True(stream.AddTrack(track));
            Assert.AreEqual(1, stream.GetVideoTracks().Count());
            Assert.NotNull(stream.GetVideoTracks().First());
            Assert.True(stream.RemoveTrack(track));
            Assert.AreEqual(0, stream.GetVideoTracks().Count());
            track.Dispose();
            stream.Dispose();
            Object.DestroyImmediate(rt);
        }

        [Test]
        public void AddAndRemoveAudioStreamTrack()
        {
            var stream = new MediaStream();
            var track = new AudioStreamTrack("audio");
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
            var videoStream = cam.CaptureStream(1280, 720);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(1, videoStream.GetVideoTracks().Count());
            Assert.AreEqual(0, videoStream.GetAudioTracks().Count());
            Assert.AreEqual(1, videoStream.GetTracks().Count());
            yield return new WaitForSeconds(0.1f);
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

            var test = new MonoBehaviourTest<SignalingPeersTest>();
            test.component.SetStream(audioStream);
            yield return test;
            audioStream.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator InitializeAndFinalizeVideoEncoder()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeersTest>();
            test.component.SetStream(videoStream);
            yield return test;
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        public class SignalingPeersTest : MonoBehaviour, IMonoBehaviourTest
        {
            private bool _isFinished = false;
            private MediaStream _stream;

            public bool IsTestFinished
            {
                get { return _isFinished; }
            }

            public void SetStream(MediaStream stream)
            {
                _stream = stream;
            }

            IEnumerator Start()
            {
                RTCConfiguration config = default;
                config.iceServers = new[]
                {
                    new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}
                };
                var pc1Senders = new List<RTCRtpSender>();
                var pc2Senders = new List<RTCRtpSender>();
                var peer1 = new RTCPeerConnection(ref config);
                var peer2 = new RTCPeerConnection(ref config);

                peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(ref candidate); };
                peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(ref candidate); };
                peer2.OnTrack = e => { pc2Senders.Add(peer2.AddTrack(e.Track, _stream)); };

                foreach (var track in _stream.GetTracks())
                {
                    pc1Senders.Add(peer1.AddTrack(track, _stream));
                }

                RTCOfferOptions options1 = default;
                RTCAnswerOptions options2 = default;
                var op1 = peer1.CreateOffer(ref options1);
                yield return op1;
                var desc = op1.Desc;
                var op2 = peer1.SetLocalDescription(ref desc);
                yield return op2;
                var op3 = peer2.SetRemoteDescription(ref desc);
                yield return op3;
                var op4 = peer2.CreateAnswer(ref options2);
                yield return op4;
                desc = op4.Desc;
                var op5 = peer2.SetLocalDescription(ref desc);
                yield return op5;
                var op6 = peer1.SetRemoteDescription(ref desc);
                yield return op6;

                var op7 = new WaitUntilWithTimeout(() =>
                    peer1.IceConnectionState == RTCIceConnectionState.Connected ||
                    peer1.IceConnectionState == RTCIceConnectionState.Completed, 5000);
                yield return op7;
                Assert.True(op7.IsCompleted);

                var op8 = new WaitUntilWithTimeout(() =>
                    peer2.IceConnectionState == RTCIceConnectionState.Connected ||
                    peer2.IceConnectionState == RTCIceConnectionState.Completed, 5000);
                yield return op8;
                Assert.True(op7.IsCompleted);

                var op9 = new WaitUntilWithTimeout(() => pc2Senders.Count > 0, 5000);
                yield return op9;
                Assert.True(op9.IsCompleted);

                foreach (var sender in pc1Senders)
                {
                    peer1.RemoveTrack(sender);
                }

                foreach (var sender in pc2Senders)
                {
                    peer2.RemoveTrack(sender);
                }
                pc1Senders.Clear();
                peer1.Close();
                peer2.Close();

                _isFinished = true;
            }
        }
    }
}
