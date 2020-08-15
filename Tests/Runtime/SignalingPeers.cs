using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Unity.WebRTC.RuntimeTest
{
    class SignalingPeers : MonoBehaviour, IMonoBehaviourTest
    {
        private MediaStream m_stream;
        RTCPeerConnection peer1;
        RTCPeerConnection peer2;
        RTCDataChannel dataChannel;

        public bool IsTestFinished { get; private set; }

        public void SetStream(MediaStream stream)
        {
            m_stream = stream;
        }

        public RTCStatsReportAsyncOperation GetPeerStats()
        {
            return peer1.GetStats();
        }

        public RTCStatsReportAsyncOperation GetSenderStats(int index)
        {
            return GetPeer1Senders().ElementAt(index).GetStats();
        }

        public RTCStatsReportAsyncOperation GetReceiverStats(int index)
        {
            return GetPeer1Receivers().ElementAt(index).GetStats();
        }

        IEnumerator Start()
        {
            RTCConfiguration config = default;
            config.iceServers = new[]
            {
                    new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}
                };

            peer1 = new RTCPeerConnection(ref config);
            peer2 = new RTCPeerConnection(ref config);
            RTCDataChannelInit conf = new RTCDataChannelInit(true);
            dataChannel = peer1.CreateDataChannel("data", ref conf);

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
                peer2.AddTrack(e.Track);
            };

            foreach (var track in m_stream.GetTracks())
            {
                peer1.AddTrack(track, m_stream);
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

            var op9 = new WaitUntilWithTimeout(() => GetPeer1Senders().Any(), 5000);
            yield return op9;
            Assert.True(op9.IsCompleted);

            IsTestFinished = true;
        }

        public Coroutine CoroutineUpdate()
        {
            return StartCoroutine(WebRTC.Update());
        }

        public IEnumerable<RTCRtpSender> GetPeer1Senders()
        {
            return peer1.GetSenders();
        }

        public IEnumerable<RTCRtpReceiver> GetPeer1Receivers()
        {
            return peer2.GetReceivers();
        }

        public void Dispose()
        {
            dataChannel.Dispose();
            peer1.Close();
            peer2.Close();
        }
    }
}
