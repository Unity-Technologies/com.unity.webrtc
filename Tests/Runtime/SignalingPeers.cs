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
        Dictionary<RTCPeerConnection, List<RTCDataChannel>> dataChannels
            = new Dictionary<RTCPeerConnection, List<RTCDataChannel>>();

        public bool IsTestFinished { get; private set; }

        public void SetStream(MediaStream stream)
        {
            m_stream = stream;
        }

        public RTCDataChannel CreateDataChannel(int indexPeer, string label, RTCDataChannelInit option = null)
        {
            RTCDataChannel channel =  peers[indexPeer].CreateDataChannel(label, option);
            dataChannels[peers[indexPeer]].Add(channel);
            return channel;
        }

        public RTCDataChannel GetDataChannel(int indexPeer, int indexDataChannel)
        {
            return dataChannels[peers[indexPeer]][indexDataChannel];
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
            dataChannels[peers[0]] = new List<RTCDataChannel>();
            dataChannels[peers[1]] = new List<RTCDataChannel>();

            RTCDataChannel channel = peers[0].CreateDataChannel("data");
            dataChannels[peers[0]].Add(channel);

            peers[0].OnIceCandidate = candidate =>
            {
                Assert.That(candidate, Is.Not.Null);
                Assert.That(candidate, Is.Not.Null);
                peers[1].AddIceCandidate(candidate);
            };
            peers[1].OnIceCandidate = candidate =>
            {
                Assert.NotNull(candidate);
                Assert.NotNull(candidate.Candidate);
                peers[0].AddIceCandidate(candidate);
            };
            peers[1].OnTrack = e =>
            {
                Assert.NotNull(e);
                Assert.NotNull(e.Track);
                Assert.NotNull(e.Receiver);
                Assert.NotNull(e.Transceiver);
                peers[1].AddTrack(e.Track);
            };
            peers[0].OnDataChannel = e =>
            {
                if (peers[0].ConnectionState == RTCPeerConnectionState.Connected)
                    dataChannels[peers[0]].Add(e);
            };
            peers[1].OnDataChannel = e =>
            {
                if(peers[1].ConnectionState == RTCPeerConnectionState.Connected)
                    dataChannels[peers[1]].Add(e);
            };

            if (m_stream != null)
            {
                foreach (var track in m_stream.GetTracks())
                {
                    peers[0].AddTrack(track, m_stream);
                }
            }

            // Because some platform can't accept H264 codec for receive.
            foreach (var transceiver in peers[0].GetTransceivers())
            {
                transceiver.Direction = RTCRtpTransceiverDirection.SendOnly;
            }

            var op1 = peers[0].CreateOffer();
            yield return op1;
            Assert.That(op1.IsError, Is.False, op1.Error.message);
            var desc = op1.Desc;
            var op2 = peers[0].SetLocalDescription(ref desc);
            yield return op2;
            Assert.That(op2.IsError, Is.False, op2.Error.message);

            var op3 = peers[1].SetRemoteDescription(ref desc);
            yield return op3;
            Assert.That(op3.IsError, Is.False, op3.Error.message);
            var op4 = peers[1].CreateAnswer();
            yield return op4;
            Assert.That(op4.IsError, Is.False, op4.Error.message);
            desc = op4.Desc;
            var op5 = peers[1].SetLocalDescription(ref desc);
            yield return op5;
            Assert.That(op5.IsError, Is.False, op5.Error.message);

            var op6 = peers[0].SetRemoteDescription(ref desc);
            yield return op6;
            Assert.That(op6.IsError, Is.False, op6.Error.message);

            var op7 = new WaitUntilWithTimeout(() =>
                peers[0].IceConnectionState == RTCIceConnectionState.Connected ||
                peers[0].IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op7;
            Assert.That(op7.IsCompleted, Is.True);

            var op8 = new WaitUntilWithTimeout(() =>
                peers[1].IceConnectionState == RTCIceConnectionState.Connected ||
                peers[1].IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op8;
            Assert.That(op8.IsCompleted, Is.True);

            if (m_stream != null)
            {
                var op9 = new WaitUntilWithTimeout(() => GetPeerSenders(0).Any(), 5000);
                yield return op9;
                Assert.That(op9.IsCompleted, Is.True);
            }
            IsTestFinished = true;
        }

        public void Dispose()
        {
            dataChannels.Clear();
            peers[0].Close();
            peers[1].Close();
        }
    }
}
