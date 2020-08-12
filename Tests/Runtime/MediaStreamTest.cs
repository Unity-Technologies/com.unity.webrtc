using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace Unity.WebRTC.RuntimeTest
{
    class Ignore
    {
        static public void Pass<T>(T val)
        {
        }
    }

    class MediaStreamTest
    {
        void StatsTest(RTCStats stats)
        {
            switch (stats.Type)
            {
                case RTCStatsType.CandidatePair:
                    var iceCandidatePairStats = stats as RTCIceCandidatePairStats;
                    Assert.NotNull(iceCandidatePairStats);
                    Assert.IsNotEmpty(iceCandidatePairStats.transportId);
                    Assert.IsNotEmpty(iceCandidatePairStats.localCandidateId);
                    Assert.IsNotEmpty(iceCandidatePairStats.remoteCandidateId);
                    Assert.IsNotEmpty(iceCandidatePairStats.state);
                    Ignore.Pass(iceCandidatePairStats.nominated);
                    Ignore.Pass(iceCandidatePairStats.writable);
                    Ignore.Pass(iceCandidatePairStats.readable);
                    Ignore.Pass(iceCandidatePairStats.bytesSent);
                    Ignore.Pass(iceCandidatePairStats.bytesReceived);
                    Ignore.Pass(iceCandidatePairStats.totalRoundTripTime);
                    Ignore.Pass(iceCandidatePairStats.availableIncomingBitrate);
                    Ignore.Pass(iceCandidatePairStats.availableOutgoingBitrate);
                    Ignore.Pass(iceCandidatePairStats.consentRequestsReceived);
                    Ignore.Pass(iceCandidatePairStats.consentRequestsSent);
                    Ignore.Pass(iceCandidatePairStats.currentRoundTripTime);
                    Ignore.Pass(iceCandidatePairStats.priority);
                    Ignore.Pass(iceCandidatePairStats.requestsReceived);
                    Ignore.Pass(iceCandidatePairStats.requestsSent);
                    Ignore.Pass(iceCandidatePairStats.responsesReceived);
                    Ignore.Pass(iceCandidatePairStats.responsesSent);
                    Ignore.Pass(iceCandidatePairStats.retransmissionsReceived);
                    Ignore.Pass(iceCandidatePairStats.retransmissionsSent);
                    Ignore.Pass(iceCandidatePairStats.consentResponsesReceived);
                    Ignore.Pass(iceCandidatePairStats.consentResponsesSent);
                    break;
                case RTCStatsType.DataChannel:
                    var dataChannelStats = stats as RTCDataChannelStats;
                    Assert.NotNull(dataChannelStats);
                    Assert.IsNotEmpty(dataChannelStats.label);
                    Assert.IsNotEmpty(dataChannelStats.state);
                    Ignore.Pass(dataChannelStats.protocol);
                    Ignore.Pass(dataChannelStats.messagesSent);
                    Ignore.Pass(dataChannelStats.messagesReceived);
                    Ignore.Pass(dataChannelStats.datachannelid);
                    Ignore.Pass(dataChannelStats.bytesSent);
                    Ignore.Pass(dataChannelStats.bytesReceived);
                    break;
                case RTCStatsType.LocalCandidate:
                case RTCStatsType.RemoteCandidate:
                    var candidateStats = stats as RTCIceCandidateStats;
                    Assert.NotNull(candidateStats);
                    Assert.IsNotEmpty(candidateStats.protocol);
                    Assert.IsNotEmpty(candidateStats.candidateType);
                    Assert.IsNotEmpty(candidateStats.ip);
                    Assert.IsNotEmpty(candidateStats.transportId);
                    Ignore.Pass(candidateStats.url);
                    Ignore.Pass(candidateStats.relayProtocol);
                    Ignore.Pass(candidateStats.networkType);
                    Ignore.Pass(candidateStats.priority);
                    Ignore.Pass(candidateStats.deleted);
                    Ignore.Pass(candidateStats.isRemote);
                    Ignore.Pass(candidateStats.port);
                    break;
                case RTCStatsType.Certificate:
                    var certificateStats = stats as RTCCertificateStats;
                    Assert.NotNull(certificateStats);
                    Assert.IsNotEmpty(certificateStats.fingerprint);
                    Assert.IsNotEmpty(certificateStats.fingerprintAlgorithm);
                    Assert.IsNotEmpty(certificateStats.base64Certificate);
                    Ignore.Pass(certificateStats.issuerCertificateId);
                    break;
                case RTCStatsType.Codec:
                    var codecStats = stats as RTCCodecStats;
                    Assert.NotNull(codecStats);
                    Assert.IsNotEmpty(codecStats.mimeType);
                    Ignore.Pass(codecStats.sdpFmtpLine);
                    Ignore.Pass(codecStats.payloadType);
                    Ignore.Pass(codecStats.clockRate);
                    Ignore.Pass(codecStats.channels);
                    break;
                case RTCStatsType.RemoteInboundRtp:
                case RTCStatsType.InboundRtp:
                    var inboundRtpStreamStats = stats as RTCInboundRTPStreamStats;
                    Assert.NotNull(inboundRtpStreamStats);
                    Ignore.Pass(inboundRtpStreamStats.bytesReceived);
                    Ignore.Pass(inboundRtpStreamStats.burstDiscardCount);
                    Ignore.Pass(inboundRtpStreamStats.burstLossCount);
                    Ignore.Pass(inboundRtpStreamStats.burstDiscardRate);
                    Ignore.Pass(inboundRtpStreamStats.burstLossRate);
                    Ignore.Pass(inboundRtpStreamStats.burstPacketsDiscarded);
                    Ignore.Pass(inboundRtpStreamStats.burstPacketsLost);
                    Ignore.Pass(inboundRtpStreamStats.contentType);
                    Ignore.Pass(inboundRtpStreamStats.decoderImplementation);
                    Ignore.Pass(inboundRtpStreamStats.framesDecoded);
                    Ignore.Pass(inboundRtpStreamStats.gapDiscardRate);
                    Ignore.Pass(inboundRtpStreamStats.gapLossRate);
                    Ignore.Pass(inboundRtpStreamStats.contentType);
                    Ignore.Pass(inboundRtpStreamStats.jitter);
                    Ignore.Pass(inboundRtpStreamStats.headerBytesReceived);
                    Ignore.Pass(inboundRtpStreamStats.packetsLost);
                    Ignore.Pass(inboundRtpStreamStats.lastPacketReceivedTimestamp);
                    Ignore.Pass(inboundRtpStreamStats.totalDecodeTime);
                    Ignore.Pass(inboundRtpStreamStats.keyFramesDecoded);
                    Ignore.Pass(inboundRtpStreamStats.packetsDiscarded);
                    Ignore.Pass(inboundRtpStreamStats.packetsReceived);
                    Ignore.Pass(inboundRtpStreamStats.packetsRepaired);
                    break;
                case RTCStatsType.Transport:
                    var transportStats = stats as RTCTransportStats;
                    Assert.NotNull(transportStats);
                    Ignore.Pass(transportStats.bytesSent);
                    Ignore.Pass(transportStats.bytesReceived);
                    Ignore.Pass(transportStats.rtcpTransportStatsId);
                    Ignore.Pass(transportStats.dtlsState);
                    Ignore.Pass(transportStats.selectedCandidatePairId);
                    Ignore.Pass(transportStats.localCertificateId);
                    Ignore.Pass(transportStats.remoteCertificateId);
                    Ignore.Pass(transportStats.selectedCandidatePairChanges);
                    break;
                case RTCStatsType.RemoteOutboundRtp:
                case RTCStatsType.OutboundRtp:
                    var outboundRtpStreamStats = stats as RTCOutboundRTPStreamStats;
                    Assert.NotNull(outboundRtpStreamStats);
                    Ignore.Pass(outboundRtpStreamStats.mediaSourceId);
                    Ignore.Pass(outboundRtpStreamStats.packetsSent);
                    Ignore.Pass(outboundRtpStreamStats.retransmittedPacketsSent);
                    Ignore.Pass(outboundRtpStreamStats.bytesSent);
                    Ignore.Pass(outboundRtpStreamStats.headerBytesSent);
                    Ignore.Pass(outboundRtpStreamStats.retransmittedBytesSent);
                    Ignore.Pass(outboundRtpStreamStats.targetBitrate);
                    Ignore.Pass(outboundRtpStreamStats.framesEncoded);
                    Ignore.Pass(outboundRtpStreamStats.keyFramesEncoded);
                    Ignore.Pass(outboundRtpStreamStats.totalEncodeTime);
                    Ignore.Pass(outboundRtpStreamStats.totalEncodedBytesTarget);
                    Ignore.Pass(outboundRtpStreamStats.totalPacketSendDelay);
                    Ignore.Pass(outboundRtpStreamStats.qualityLimitationReason);
                    Ignore.Pass(outboundRtpStreamStats.qualityLimitationResolutionChanges);
                    Ignore.Pass(outboundRtpStreamStats.contentType);
                    Ignore.Pass(outboundRtpStreamStats.encoderImplementation);
                    break;
                case RTCStatsType.MediaSource:
                    var mediaSourceStats = stats as RTCMediaSourceStats;
                    Assert.NotNull(mediaSourceStats);
                    Assert.IsNotEmpty(mediaSourceStats.trackIdentifier);
                    Assert.IsNotEmpty(mediaSourceStats.kind);
                    Ignore.Pass(mediaSourceStats.width);
                    Ignore.Pass(mediaSourceStats.height);
                    Ignore.Pass(mediaSourceStats.frames);
                    Ignore.Pass(mediaSourceStats.framesPerSecond);

                    break;
                case RTCStatsType.Track:
                    var mediaStreamTrackStats = stats as RTCMediaStreamTrackStats;
                    Assert.NotNull(mediaStreamTrackStats);
                    Ignore.Pass(mediaStreamTrackStats.trackIdentifier);
                    Ignore.Pass(mediaStreamTrackStats.mediaSourceId);
                    Ignore.Pass(mediaStreamTrackStats.remoteSource);
                    Ignore.Pass(mediaStreamTrackStats.ended);
                    Ignore.Pass(mediaStreamTrackStats.detached);
                    Ignore.Pass(mediaStreamTrackStats.kind);
                    Ignore.Pass(mediaStreamTrackStats.jitterBufferDelay);
                    Ignore.Pass(mediaStreamTrackStats.jitterBufferEmittedCount);
                    Ignore.Pass(mediaStreamTrackStats.frameWidth);
                    Ignore.Pass(mediaStreamTrackStats.frameHeight);
                    Ignore.Pass(mediaStreamTrackStats.framesPerSecond);
                    Ignore.Pass(mediaStreamTrackStats.framesSent);
                    Ignore.Pass(mediaStreamTrackStats.hugeFramesSent);
                    Ignore.Pass(mediaStreamTrackStats.framesReceived);
                    Ignore.Pass(mediaStreamTrackStats.framesDecoded);
                    Ignore.Pass(mediaStreamTrackStats.framesDropped);
                    Ignore.Pass(mediaStreamTrackStats.framesCorrupted);
                    Ignore.Pass(mediaStreamTrackStats.partialFramesLost);
                    Ignore.Pass(mediaStreamTrackStats.fullFramesLost);
                    Ignore.Pass(mediaStreamTrackStats.audioLevel);
                    Ignore.Pass(mediaStreamTrackStats.totalAudioEnergy);
                    Ignore.Pass(mediaStreamTrackStats.echoReturnLoss);
                    Ignore.Pass(mediaStreamTrackStats.echoReturnLossEnhancement);
                    Ignore.Pass(mediaStreamTrackStats.totalSamplesReceived);
                    Ignore.Pass(mediaStreamTrackStats.totalSamplesDuration);
                    Ignore.Pass(mediaStreamTrackStats.concealedSamples);
                    Ignore.Pass(mediaStreamTrackStats.silentConcealedSamples);
                    Ignore.Pass(mediaStreamTrackStats.concealmentEvents);
                    Ignore.Pass(mediaStreamTrackStats.insertedSamplesForDeceleration);
                    Ignore.Pass(mediaStreamTrackStats.removedSamplesForAcceleration);
                    Ignore.Pass(mediaStreamTrackStats.jitterBufferFlushes);
                    Ignore.Pass(mediaStreamTrackStats.delayedPacketOutageSamples);
                    Ignore.Pass(mediaStreamTrackStats.relativePacketArrivalDelay);
                    Ignore.Pass(mediaStreamTrackStats.interruptionCount);
                    Ignore.Pass(mediaStreamTrackStats.totalInterruptionDuration);
                    Ignore.Pass(mediaStreamTrackStats.freezeCount);
                    Ignore.Pass(mediaStreamTrackStats.pauseCount);
                    Ignore.Pass(mediaStreamTrackStats.totalFreezesDuration);
                    Ignore.Pass(mediaStreamTrackStats.totalPausesDuration);
                    Ignore.Pass(mediaStreamTrackStats.totalFramesDuration);
                    Ignore.Pass(mediaStreamTrackStats.sumOfSquaredFramesDuration);
                    Assert.NotNull(mediaStreamTrackStats);
                    break;
                case RTCStatsType.Stream:
                    var mediaStreamStats = stats as RTCMediaStreamStats;
                    Assert.NotNull(mediaStreamStats);
                    Ignore.Pass(mediaStreamStats.streamIdentifier);
                    Ignore.Pass(mediaStreamStats.trackIds);
                    break;
            }
        }



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
            yield return new WaitForSeconds(0.1f);
            test.component.Dispose();
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        /// <todo>
        /// This unittest failed standalone mono 2019.3 on linux
        /// </todo>
        [UnityTest]
        [Timeout(5000)]
        public IEnumerator PeerConnectionGetStats()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeersTest>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineWebRTCUpdate();
            yield return new WaitForSeconds(0.1f);
            var op = test.component.GetPeerStats();
            yield return op;
            Assert.True(op.IsDone);
            Assert.IsNotEmpty(op.Value.Stats);
            Assert.IsNotEmpty(op.Value.Stats.Keys);
            Assert.IsNotEmpty(op.Value.Stats.Values);
            Assert.Greater(op.Value.Stats.Count, 0);

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.NotNull(stats);
                Assert.Greater(stats.Timestamp, 0);
                Assert.IsNotEmpty(stats.Id);
                foreach(var pair in stats.Dict)
                {
                    Assert.IsNotEmpty(pair.Key);
                    Assert.NotNull(pair.Value);
                }
                StatsTest(stats);
            }

            test.component.Dispose();
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SenderGetStats()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeersTest>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineWebRTCUpdate();
            yield return new WaitForSeconds(0.1f);
            var op = test.component.GetSenderStats(0);
            yield return op;
            Assert.True(op.IsDone);
            Assert.IsNotEmpty(op.Value.Stats);
            Assert.Greater(op.Value.Stats.Count, 0);

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.NotNull(stats);
                Assert.Greater(stats.Timestamp, 0);
                Assert.IsNotEmpty(stats.Id);
                foreach (var pair in stats.Dict)
                {
                    Assert.IsNotEmpty(pair.Key);
                    Assert.NotNull(pair.Value);
                }
                StatsTest(stats);
            }

            test.component.Dispose();
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ReceiverGetStats()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeersTest>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineWebRTCUpdate();
            yield return new WaitForSeconds(0.1f);
            var op = test.component.GetReceiverStats(0);
            yield return op;
            Assert.True(op.IsDone);
            Assert.IsNotEmpty(op.Value.Stats);
            Assert.Greater(op.Value.Stats.Count, 0);

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.NotNull(stats);
                Assert.Greater(stats.Timestamp, 0);
                Assert.IsNotEmpty(stats.Id);
                foreach (var pair in stats.Dict)
                {
                    Assert.IsNotEmpty(pair.Key);
                    Assert.NotNull(pair.Value);
                }
                StatsTest(stats);
            }
            test.component.Dispose();
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        /// <todo>
        /// This unittest failed standalone mono 2019.3 on linux
        /// </todo>
        [UnityTest]
        [Timeout(5000)]
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

        /// <todo>
        /// This unittest failed standalone mono 2019.3 on linux
        /// </todo>
        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SetParametersReturnNoError()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeersTest>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineWebRTCUpdate();
            yield return new WaitForSeconds(0.1f);

            var senders = test.component.GetPeer1Senders();
            Assert.IsNotEmpty(senders);

            foreach(var sender in senders)
            {
                var parameters = sender.GetParameters();
                Assert.IsNotEmpty(parameters.Encodings);
                const uint framerate = 20;
                parameters.Encodings[0].maxFramerate = framerate;
                RTCErrorType error = sender.SetParameters(parameters);
                Assert.AreEqual(RTCErrorType.None, error);
                var parameters2 = sender.GetParameters();
                Assert.AreEqual(framerate, parameters2.Encodings[0].maxFramerate);
            }

            test.component.Dispose();
            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }

        public class SignalingPeersTest : MonoBehaviour, IMonoBehaviourTest
        {
            private bool m_isFinished;
            private MediaStream m_stream;
            RTCPeerConnection peer1;
            RTCPeerConnection peer2;
            RTCDataChannel dataChannel;

            public bool IsTestFinished
            {
                get { return m_isFinished; }
            }

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

                m_isFinished = true;
            }

            public Coroutine CoroutineWebRTCUpdate()
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
}
