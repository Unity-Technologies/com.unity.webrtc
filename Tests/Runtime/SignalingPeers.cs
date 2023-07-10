using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class SignalingPeers : MonoBehaviour, IMonoBehaviourTest
    {
        RTCPeerConnection[] peers = new RTCPeerConnection[2];
        Dictionary<RTCPeerConnection, List<RTCDataChannel>> dataChannels
            = new Dictionary<RTCPeerConnection, List<RTCDataChannel>>();

        public bool IsTestFinished { get; private set; }

        bool negotiating = false;

        public void AddStream(int indexPeer, MediaStream stream)
        {
            foreach (var track in stream.GetTracks())
            {
                peers[indexPeer].AddTrack(track, stream);
            }
        }

        public RTCRtpTransceiver AddTransceiver(int indexPeer, MediaStreamTrack track)
        {
            return peers[indexPeer].AddTransceiver(track);
        }

        public RTCRtpTransceiver AddTransceiver(int indexPeer, TrackKind kind, RTCRtpTransceiverInit init = null)
        {
            return peers[indexPeer].AddTransceiver(kind, init);
        }

        public RTCRtpSender AddTrack(int indexPeer, MediaStreamTrack track)
        {
            return peers[indexPeer].AddTrack(track);
        }

        public RTCErrorType RemoveTrack(int indexPeer, RTCRtpSender sender)
        {
            return peers[indexPeer].RemoveTrack(sender);
        }

        public RTCDataChannel CreateDataChannel(int indexPeer, string label, RTCDataChannelInit option = null)
        {
            RTCDataChannel channel = peers[indexPeer].CreateDataChannel(label, option);
            dataChannels[peers[indexPeer]].Add(channel);
            return channel;
        }

        public List<RTCDataChannel> GetDataChannelList(int indexPeer)
        {
            return dataChannels[peers[indexPeer]];
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
            return GetPeerReceivers(indexPeer).ElementAt(indexReceiver).GetStats();
        }

        public IEnumerable<RTCRtpSender> GetPeerSenders(int indexPeer)
        {
            return peers[indexPeer].GetSenders();
        }

        public IEnumerable<RTCRtpReceiver> GetPeerReceivers(int indexPeer)
        {
            return peers[indexPeer].GetReceivers();
        }

        public IEnumerable<RTCRtpTransceiver> GetPeerTransceivers(int indexPeer)
        {
            return peers[indexPeer].GetTransceivers();
        }


        public Coroutine CoroutineUpdate()
        {
            return StartCoroutine(WebRTC.Update());
        }

        void Awake()
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

            peers[0].OnIceCandidate = candidate =>
            {
                Assert.That(candidate, Is.Not.Null);
                Assert.That(candidate, Is.Not.Null);
                peers[1].AddIceCandidate(candidate);
            };
            peers[1].OnIceCandidate = candidate =>
            {
                Assert.That(candidate, Is.Not.Null);
                Assert.That(candidate.Candidate, Is.Not.Null);
                peers[0].AddIceCandidate(candidate);
            };
            peers[1].OnTrack = e =>
            {
                Assert.That(e, Is.Not.Null);
                Assert.That(e.Track, Is.Not.Null);
                Assert.That(e.Receiver, Is.Not.Null);
                Assert.That(e.Transceiver, Is.Not.Null);
            };
            peers[0].OnDataChannel = e =>
            {
                if (peers[0].ConnectionState == RTCPeerConnectionState.Connected)
                    dataChannels[peers[0]].Add(e);
            };
            peers[1].OnDataChannel = e =>
            {
                if (peers[1].ConnectionState == RTCPeerConnectionState.Connected)
                    dataChannels[peers[1]].Add(e);
            };
            peers[0].OnNegotiationNeeded = () =>
            {
                IsTestFinished = false;
                StartCoroutine(Negotiate(peers[0], peers[1]));
            };

            peers[1].OnNegotiationNeeded = () =>
            {
                IsTestFinished = false;
                StartCoroutine(Negotiate(peers[1], peers[0]));
            };
        }

        IEnumerator Negotiate(RTCPeerConnection peer1, RTCPeerConnection peer2)
        {
            if (negotiating)
            {
                yield break;
            }
            negotiating = true;
            var op1 = peer1.CreateOffer();
            yield return op1;
            Assert.That(op1.IsError, Is.False, op1.Error.message);
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            Assert.That(op2.IsError, Is.False, op2.Error.message);

            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            Assert.That(op3.IsError, Is.False, op3.Error.message);
            var op4 = peer2.CreateAnswer();
            yield return op4;
            Assert.That(op4.IsError, Is.False, op4.Error.message);
            desc = op4.Desc;
            var op5 = peer2.SetLocalDescription(ref desc);
            yield return op5;
            Assert.That(op5.IsError, Is.False, op5.Error.message);

            var op6 = peer1.SetRemoteDescription(ref desc);
            yield return op6;
            Assert.That(op6.IsError, Is.False, op6.Error.message);

            var op7 = new WaitUntilWithTimeout(() =>
                peers[0].SignalingState == RTCSignalingState.Stable &&
                peers[1].SignalingState == RTCSignalingState.Stable, 5000);
            yield return op7;
            Assert.That(op7.IsCompleted, Is.True);

            IsTestFinished = true;
            negotiating = false;
        }

        public bool NegotiationCompleted()
        {
            return !negotiating &&
                peers[0].SignalingState == RTCSignalingState.Stable && peers[1].SignalingState == RTCSignalingState.Stable;
        }

        public void Dispose()
        {
            dataChannels.Clear();
            peers[0].Close();
            peers[1].Close();
        }
    }
}
