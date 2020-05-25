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
        [Category("MediaStreamTrack")]
        public IEnumerator MediaStreamTrackEnabled()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack("video", rt);
            Assert.NotNull(track);
            yield return new WaitForSeconds(0.1f);
            Assert.True(track.IsInitialized);

            // Enabled property
            Assert.True(track.Enabled);
            track.Enabled = false;
            Assert.False(track.Enabled);

            // ReadyState property
            Assert.AreEqual(track.ReadyState, TrackState.Live);
            track.Dispose();
            yield return new WaitForSeconds(0.1f);

            Object.DestroyImmediate(rt);
        }

        [UnityTest]
        [Timeout(5000)]
        [Category("MediaStream")]
        public IEnumerator MediaStreamAddTrack()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var stream = new MediaStream();
            var track = new VideoStreamTrack("video", rt);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(TrackKind.Video, track.Kind);
            Assert.AreEqual(0, stream.GetVideoTracks().Count());
            Assert.True(stream.AddTrack(track));
            Assert.AreEqual(1, stream.GetVideoTracks().Count());
            Assert.NotNull(stream.GetVideoTracks().First());
            Assert.True(stream.RemoveTrack(track));
            Assert.AreEqual(0, stream.GetVideoTracks().Count());
            track.Dispose();
            yield return new WaitForSeconds(0.1f);
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

        /// <todo>
        /// This unittest failed standalone mono 2019.3 on linux
        /// </todo>
        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        public IEnumerator CameraCaptureStream()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
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
            test.component.Dispose();
            audioStream.Dispose();
        }

        /// <todo>
        /// This unittest failed standalone mono 2019.3 on linux
        /// </todo>
        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        public IEnumerator CaptureStream()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeersTest>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineWebRTCUpdate();
            yield return 0;
            test.component.Dispose();
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        /// <todo>
        /// This unittest failed standalone mono 2019.3 on linux
        /// </todo>
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
            yield return new WaitForSeconds(0.1f);
            Object.DestroyImmediate(camObj);
        }

        public class SignalingPeersTest : MonoBehaviour, IMonoBehaviourTest
        {
            private bool m_isFinished;
            private MediaStream m_stream;
            List<RTCRtpSender> pc1Senders;
            List<RTCRtpSender> pc2Senders;
            RTCPeerConnection peer1;
            RTCPeerConnection peer2;

            public bool IsTestFinished
            {
                get { return m_isFinished; }
            }

            public void SetStream(MediaStream stream)
            {
                m_stream = stream;
            }

            IEnumerator Start()
            {
                RTCConfiguration config = default;
                config.iceServers = new[]
                {
                    new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}
                };

                pc1Senders = new List<RTCRtpSender>();
                pc2Senders = new List<RTCRtpSender>();
                peer1 = new RTCPeerConnection(ref config);
                peer2 = new RTCPeerConnection(ref config);
                peer1.OnIceCandidate = candidate =>
                {
                    Assert.NotNull(candidate);
                    Assert.NotNull(candidate.candidate);
                    peer2.AddIceCandidate(ref candidate);
                };
                peer2.OnIceCandidate = candidate =>
                {
                    Assert.NotNull(candidate);
                    Assert.NotNull(candidate.candidate);
                    peer1.AddIceCandidate(ref candidate);
                };
                peer2.OnTrack = e =>
                {
                    Assert.NotNull(e);
                    Assert.NotNull(e.Track);
                    Assert.NotNull(e.Receiver);
                    Assert.NotNull(e.Transceiver);
                    pc2Senders.Add(peer2.AddTrack(e.Track, m_stream));
                };

                foreach (var track in m_stream.GetTracks())
                {
                    pc1Senders.Add(peer1.AddTrack(track, m_stream));
                }

                RTCOfferOptions options1 = default;
                RTCAnswerOptions options2 = default;
                var op1 = peer1.CreateOffer(ref options1);
                yield return op1;
                Assert.False(op1.IsError);
                var desc = op1.Desc;
                var op2 = peer1.SetLocalDescription(ref desc);
                yield return op2;
                Assert.False(op2.IsError);
                var op3 = peer2.SetRemoteDescription(ref desc);
                yield return op3;
                Assert.False(op3.IsError);
                var op4 = peer2.CreateAnswer(ref options2);
                yield return op4;
                Assert.False(op4.IsError);
                desc = op4.Desc;
                var op5 = peer2.SetLocalDescription(ref desc);
                yield return op5;
                Assert.False(op5.IsError);
                var op6 = peer1.SetRemoteDescription(ref desc);
                yield return op6;
                Assert.False(op6.IsError);

                var op7 = new WaitUntilWithTimeout(() =>
                    peer1.IceConnectionState == RTCIceConnectionState.Connected ||
                    peer1.IceConnectionState == RTCIceConnectionState.Completed, 5000);
                yield return op7;
                Assert.True(op7.IsCompleted);

                var op8 = new WaitUntilWithTimeout(() =>
                    peer2.IceConnectionState == RTCIceConnectionState.Connected ||
                    peer2.IceConnectionState == RTCIceConnectionState.Completed, 5000);
                yield return op8;
                Assert.True(op8.IsCompleted);

                var op9 = new WaitUntilWithTimeout(() => pc2Senders.Count > 0, 5000);
                yield return op9;
                Assert.True(op9.IsCompleted);

                m_isFinished = true;
            }

            public Coroutine CoroutineWebRTCUpdate()
            {
                return StartCoroutine(WebRTC.Update());
            }

            public void Dispose()
            {
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
            }
        }
    }
}
