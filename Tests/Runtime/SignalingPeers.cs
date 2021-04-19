using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

            desc.sdp = ReplaceOfferSdpForHardwareEncodeTest(desc.sdp);
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

            desc.sdp = ReplaceAnswerSdpForHardwareEncodeTest(desc.sdp);

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

        private static string ReplaceAnswerSdpForHardwareEncodeTest(string originalSdp)
        {
            if (WebRTC.GetEncoderType() == EncoderType.Software)
            {
                return originalSdp;
            }

            return originalSdp
                .Replace("m=video 9 UDP/TLS/RTP/SAVPF 96 98 100 104 105 106",
                    "m=video 9 UDP/TLS/RTP/SAVPF 96 98 100 102 104 105 106")
                .Replace("a=rtpmap:104 red/90000", @"a=rtpmap:102 H264/90000
a=rtcp-fb:102 goog-remb
a=rtcp-fb:102 transport-cc
a=rtcp-fb:102 ccm fir
a=rtcp-fb:102 nack
a=rtcp-fb:102 nack pli
a=fmtp:102 level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=42e033
a=rtpmap:104 red/90000");
        }

        private static string ReplaceOfferSdpForHardwareEncodeTest(string originalSdp)
        {
            if (WebRTC.GetEncoderType() == EncoderType.Software)
            {
                return originalSdp;
            }

            var result = originalSdp
                .Replace("m=video 9 UDP/TLS/RTP/SAVPF 96 97 98 99 100",
                    "m=video 9 UDP/TLS/RTP/SAVPF 96 97 98 99 100 101 102 103 104 105 106");

            var matchedObject = Regex.Match(originalSdp, @"(a=rtpmap:96.*)a=ssrc-group", RegexOptions.Singleline);
            if (!string.IsNullOrEmpty(matchedObject.Value))
            {
                result = result.Replace(matchedObject.Value, @"a=rtpmap:96 VP8/90000
a=rtcp-fb:96 goog-remb
a=rtcp-fb:96 transport-cc
a=rtcp-fb:96 ccm fir
a=rtcp-fb:96 nack
a=rtcp-fb:96 nack pli
a=rtpmap:97 rtx/90000
a=fmtp:97 apt=96
a=rtpmap:98 VP9/90000
a=rtcp-fb:98 goog-remb
a=rtcp-fb:98 transport-cc
a=rtcp-fb:98 ccm fir
a=rtcp-fb:98 nack
a=rtcp-fb:98 nack pli
a=fmtp:98 profile-id=0
a=rtpmap:99 rtx/90000
a=fmtp:99 apt=98
a=rtpmap:100 VP9/90000
a=rtcp-fb:100 goog-remb
a=rtcp-fb:100 transport-cc
a=rtcp-fb:100 ccm fir
a=rtcp-fb:100 nack
a=rtcp-fb:100 nack pli
a=fmtp:100 profile-id=2
a=rtpmap:101 rtx/90000
a=fmtp:101 apt=100
a=rtpmap:102 H264/90000
a=rtcp-fb:102 goog-remb
a=rtcp-fb:102 transport-cc
a=rtcp-fb:102 ccm fir
a=rtcp-fb:102 nack
a=rtcp-fb:102 nack pli
a=fmtp:102 level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=42e033
a=rtpmap:103 rtx/90000
a=fmtp:103 apt=102
a=rtpmap:104 red/90000
a=rtpmap:105 rtx/90000
a=fmtp:105 apt=104
a=rtpmap:106 ulpfec/90000
a=ssrc-group");
            }

            return result;
        }

        public void Dispose()
        {
            dataChannels.Clear();
            peers[0].Close();
            peers[1].Close();
        }
    }
}
