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
        RTCPeerConnection[] peers = new RTCPeerConnection[2];
        RTCDataChannel dataChannel;

        public bool IsTestFinished { get; private set; }

        public void SetStream(MediaStream stream)
        {
            m_stream = stream;
        }

        public RTCStatsReportAsyncOperation GetPeerStats(int indexPeer)
        {
            return peers[indexPeer].GetStats();
        }

        public RTCStatsReportAsyncOperation GetSenderStats(int indexPeer, int indexSender)
        {
            return GetPeerSenders(indexPeer).ElementAt(indexSender).GetStats();
        }

        public RTCStatsReportAsyncOperation GetReceiverStats(int indexPeer, int indexReceiver)
        {
            return GetReceivers(indexPeer).ElementAt(indexReceiver).GetStats();
        }

        public IEnumerable<RTCRtpSender> GetPeerSenders(int indexPeer)
        {
            return peers[indexPeer].GetSenders();
        }

        public IEnumerable<RTCRtpReceiver> GetReceivers(int indexPeer)
        {
            return peers[indexPeer].GetReceivers();
        }

        public Coroutine CoroutineUpdate()
        {
            return StartCoroutine(WebRTC.Update());
        }

        IEnumerator Start()
        {
            RTCConfiguration config = default;
            config.iceServers = new[]
            {
                    new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}
                };

            peers[0] = new RTCPeerConnection(ref config);
            peers[1] = new RTCPeerConnection(ref config);
            RTCDataChannelInit conf = new RTCDataChannelInit(true);
            dataChannel = peers[0].CreateDataChannel("data", ref conf);

            peers[0].OnIceCandidate = candidate =>
            {
                Assert.NotNull(candidate);
                Assert.NotNull(candidate.candidate);
                peers[1].AddIceCandidate(ref candidate);
            };
            peers[1].OnIceCandidate = candidate =>
            {
                Assert.NotNull(candidate);
                Assert.NotNull(candidate.candidate);
                peers[0].AddIceCandidate(ref candidate);
            };
            peers[1].OnTrack = e =>
            {
                Assert.NotNull(e);
                Assert.NotNull(e.Track);
                Assert.NotNull(e.Receiver);
                Assert.NotNull(e.Transceiver);
                peers[1].AddTrack(e.Track);
            };

            foreach (var track in m_stream.GetTracks())
            {
                peers[0].AddTrack(track, m_stream);
            }

            RTCOfferOptions options1 = default;
            RTCAnswerOptions options2 = default;
            var op1 = peers[0].CreateOffer(ref options1);
            yield return op1;
            Assert.False(op1.IsError);
            var desc = op1.Desc;
            var op2 = peers[0].SetLocalDescription(ref desc);
            yield return op2;
            Assert.False(op2.IsError);
            var op3 = peers[1].SetRemoteDescription(ref desc);
            yield return op3;
            Assert.False(op3.IsError);
            var op4 = peers[1].CreateAnswer(ref options2);
            yield return op4;
            Assert.False(op4.IsError);
            desc = op4.Desc;
            var op5 = peers[1].SetLocalDescription(ref desc);
            yield return op5;
            Assert.False(op5.IsError);
            var op6 = peers[0].SetRemoteDescription(ref desc);
            yield return op6;
            Assert.False(op6.IsError);

            var op7 = new WaitUntilWithTimeout(() =>
                peers[0].IceConnectionState == RTCIceConnectionState.Connected ||
                peers[0].IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op7;
            Assert.True(op7.IsCompleted);

            var op8 = new WaitUntilWithTimeout(() =>
                peers[1].IceConnectionState == RTCIceConnectionState.Connected ||
                peers[1].IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op8;
            Assert.True(op8.IsCompleted);

            var op9 = new WaitUntilWithTimeout(() => GetPeerSenders(0).Any(), 5000);
            yield return op9;
            Assert.True(op9.IsCompleted);

            IsTestFinished = true;
        }

        public void Dispose()
        {
            dataChannel.Dispose();
            peers[0].Close();
            peers[1].Close();
        }
    }
}
