using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Unity.WebRTC.RuntimeTest
{
    class MediaStreamTest
    {
        [SetUp]
        public void SetUp()
        {
            WebRTC.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Finalize();
        }

        [UnityTest]
        public IEnumerator MediaStreamTest_AddAndRemoveVideoStream()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720);
            yield return new WaitForSeconds(1.0f);
            Assert.AreEqual(1, videoStream.GetVideoTracks().Length);
            Assert.AreEqual(0, videoStream.GetAudioTracks().Length);
            Assert.AreEqual(1, videoStream.GetTracks().Length);
            yield return videoStream.FinalizeEncoder();
            yield return new WaitForSeconds(1.0f);
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        [Test]
        public void MediaStreamTest_AddAndRemoveAudioStream()
        {
            var audioStream = Audio.CaptureStream();
            Assert.AreEqual(1, audioStream.GetAudioTracks().Length);
            Assert.AreEqual(0, audioStream.GetVideoTracks().Length);
            Assert.AreEqual(1, audioStream.GetTracks().Length);
            audioStream.Dispose();
        }


        [UnityTest]
        [Timeout(5000)]
        public IEnumerator MediaStreamTest_AddAndRemoveAudioTrack()
        {
            RTCConfiguration config = default;
            config.iceServers = new[]
            {
                new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}
            };
            var audioStream = Audio.CaptureStream();
            var pc1Senders = new List<RTCRtpSender>();
            var pc2Senders = new List<RTCRtpSender>();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(ref candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(ref candidate); };
            peer2.OnTrack = e => { pc2Senders.Add(peer2.AddTrack(e.Track)); };

            foreach (var track in audioStream.GetTracks())
            {
                pc1Senders.Add(peer1.AddTrack(track));
            }

            RTCOfferOptions options1 = default;
            RTCAnswerOptions options2 = default;
            var op1 = peer1.CreateOffer(ref options1);
            yield return op1;
            var op2 = peer1.SetLocalDescription(ref op1.desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref op1.desc);
            yield return op3;
            var op4 = peer2.CreateAnswer(ref options2);
            yield return op4;
            var op5 = peer2.SetLocalDescription(ref op4.desc);
            yield return op5;
            var op6 = peer1.SetRemoteDescription(ref op4.desc);
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
            audioStream.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator MediaStreamTest_AddAndRemoveVideoTrack()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
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
            peer2.OnTrack = e => { pc2Senders.Add(peer2.AddTrack(e.Track)); };
            var videoStream = cam.CaptureStream(1280, 720);
            yield return new WaitForSeconds(1.0f);

            foreach (var track in videoStream.GetTracks())
            {
                pc1Senders.Add(peer1.AddTrack(track));
            }

            RTCOfferOptions options1 = default;
            RTCAnswerOptions options2 = default;
            var op1 = peer1.CreateOffer(ref options1);
            yield return op1;
            var op2 = peer1.SetLocalDescription(ref op1.desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref op1.desc);
            yield return op3;
            var op4 = peer2.CreateAnswer(ref options2);
            yield return op4;
            var op5 = peer2.SetLocalDescription(ref op4.desc);
            yield return op5;
            var op6 = peer1.SetRemoteDescription(ref op4.desc);
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

            yield return videoStream.FinalizeEncoder();
            yield return new WaitForSeconds(1.0f);
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }
    }
}
