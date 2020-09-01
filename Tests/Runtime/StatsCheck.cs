using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Unity.WebRTC.RuntimeTest
{
    class StatsCheck
    {
        class Ignore
        {
            static public void Pass<T>(T val)
            {
            }
        }

        public static void Test(RTCStats stats)
        {
            switch (stats.Type)
            {
                case RTCStatsType.CandidatePair:
                    var iceCandidatePairStats = stats as RTCIceCandidatePairStats;
                    Assert.NotNull(iceCandidatePairStats);
                    Assert.AreEqual(24, iceCandidatePairStats.Dict.Count);
                    Assert.IsNotEmpty(iceCandidatePairStats.transportId);
                    Assert.IsNotEmpty(iceCandidatePairStats.localCandidateId);
                    Assert.IsNotEmpty(iceCandidatePairStats.remoteCandidateId);
                    Assert.IsNotEmpty(iceCandidatePairStats.state);
                    Ignore.Pass(iceCandidatePairStats.priority);
                    Ignore.Pass(iceCandidatePairStats.nominated);
                    Ignore.Pass(iceCandidatePairStats.writable);
                    Ignore.Pass(iceCandidatePairStats.readable);
                    Ignore.Pass(iceCandidatePairStats.bytesSent);
                    Ignore.Pass(iceCandidatePairStats.bytesReceived);
                    Ignore.Pass(iceCandidatePairStats.totalRoundTripTime);
                    Ignore.Pass(iceCandidatePairStats.currentRoundTripTime);
                    Ignore.Pass(iceCandidatePairStats.availableOutgoingBitrate);
                    Ignore.Pass(iceCandidatePairStats.availableIncomingBitrate);
                    Ignore.Pass(iceCandidatePairStats.consentRequestsReceived);
                    Ignore.Pass(iceCandidatePairStats.consentRequestsSent);
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
                    Assert.AreEqual(8, dataChannelStats.Dict.Count);
                    Assert.IsNotEmpty(dataChannelStats.label);
                    Assert.IsNotEmpty(dataChannelStats.state);
                    Ignore.Pass(dataChannelStats.protocol);
                    Ignore.Pass(dataChannelStats.messagesSent);
                    Ignore.Pass(dataChannelStats.messagesReceived);
                    Ignore.Pass(dataChannelStats.dataChannelIdentifier);
                    Ignore.Pass(dataChannelStats.bytesSent);
                    Ignore.Pass(dataChannelStats.bytesReceived);
                    break;
                case RTCStatsType.LocalCandidate:
                case RTCStatsType.RemoteCandidate:
                    var candidateStats = stats as RTCIceCandidateStats;
                    Assert.NotNull(candidateStats);
                    Assert.AreEqual(11, candidateStats.Dict.Count);
                    Assert.IsNotEmpty(candidateStats.protocol);
                    Assert.IsNotEmpty(candidateStats.candidateType);
                    Ignore.Pass(candidateStats.ip);
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
                    Assert.AreEqual(4, certificateStats.Dict.Count);
                    Assert.IsNotEmpty(certificateStats.fingerprint);
                    Assert.IsNotEmpty(certificateStats.fingerprintAlgorithm);
                    Assert.IsNotEmpty(certificateStats.base64Certificate);
                    Ignore.Pass(certificateStats.issuerCertificateId);
                    break;
                case RTCStatsType.Codec:
                    var codecStats = stats as RTCCodecStats;
                    Assert.NotNull(codecStats);
                    Assert.AreEqual(5, codecStats.Dict.Count);
                    Assert.IsNotEmpty(codecStats.mimeType);
                    Ignore.Pass(codecStats.sdpFmtpLine);
                    Ignore.Pass(codecStats.payloadType);
                    Ignore.Pass(codecStats.clockRate);
                    Ignore.Pass(codecStats.channels);
                    break;
                case RTCStatsType.InboundRtp:
                    var inboundRtpStreamStats = stats as RTCInboundRTPStreamStats;
                    Assert.NotNull(inboundRtpStreamStats);
                    Assert.AreEqual(39, inboundRtpStreamStats.Dict.Count);
                    Ignore.Pass(inboundRtpStreamStats.ssrc);
                    Ignore.Pass(inboundRtpStreamStats.isRemote);
                    Assert.IsNotEmpty(inboundRtpStreamStats.mediaType);
                    Assert.IsNotEmpty(inboundRtpStreamStats.kind);
                    Ignore.Pass(inboundRtpStreamStats.trackId);
                    Assert.IsNotEmpty(inboundRtpStreamStats.transportId);
                    Ignore.Pass(inboundRtpStreamStats.codecId);
                    Ignore.Pass(inboundRtpStreamStats.firCount);
                    Ignore.Pass(inboundRtpStreamStats.pliCount);
                    Ignore.Pass(inboundRtpStreamStats.nackCount);
                    Ignore.Pass(inboundRtpStreamStats.sliCount);
                    Ignore.Pass(inboundRtpStreamStats.qpSum);
                    Ignore.Pass(inboundRtpStreamStats.packetsReceived);
                    Ignore.Pass(inboundRtpStreamStats.fecPacketsReceived);
                    Ignore.Pass(inboundRtpStreamStats.fecPacketsDiscarded);
                    Ignore.Pass(inboundRtpStreamStats.bytesReceived);
                    Ignore.Pass(inboundRtpStreamStats.headerBytesReceived);
                    Ignore.Pass(inboundRtpStreamStats.packetsLost);
                    Ignore.Pass(inboundRtpStreamStats.lastPacketReceivedTimestamp);
                    Ignore.Pass(inboundRtpStreamStats.jitter);
                    Ignore.Pass(inboundRtpStreamStats.roundTripTime);
                    Ignore.Pass(inboundRtpStreamStats.packetsDiscarded);
                    Ignore.Pass(inboundRtpStreamStats.packetsRepaired);
                    Ignore.Pass(inboundRtpStreamStats.burstPacketsLost);
                    Ignore.Pass(inboundRtpStreamStats.burstPacketsDiscarded);
                    Ignore.Pass(inboundRtpStreamStats.burstLossCount);
                    Ignore.Pass(inboundRtpStreamStats.burstDiscardCount);
                    Ignore.Pass(inboundRtpStreamStats.burstLossRate);
                    Ignore.Pass(inboundRtpStreamStats.burstDiscardRate);
                    Ignore.Pass(inboundRtpStreamStats.gapLossRate);
                    Ignore.Pass(inboundRtpStreamStats.gapDiscardRate);
                    Ignore.Pass(inboundRtpStreamStats.framesDecoded);
                    Ignore.Pass(inboundRtpStreamStats.keyFramesDecoded);
                    Ignore.Pass(inboundRtpStreamStats.totalDecodeTime);
                    Ignore.Pass(inboundRtpStreamStats.totalInterFrameDelay);
                    Ignore.Pass(inboundRtpStreamStats.totalSquaredInterFrameDelay);
                    Ignore.Pass(inboundRtpStreamStats.contentType);
                    Ignore.Pass(inboundRtpStreamStats.estimatedPlayoutTimestamp);
                    Ignore.Pass(inboundRtpStreamStats.decoderImplementation);
                    break;
                case RTCStatsType.RemoteInboundRtp:
                    var remoteInboundRtpStreamStats = stats as RTCRemoteInboundRtpStreamStats;
                    Assert.NotNull(remoteInboundRtpStreamStats);
                    Assert.AreEqual(8, remoteInboundRtpStreamStats.Dict.Count);
                    Ignore.Pass(remoteInboundRtpStreamStats.ssrc);
                    Assert.IsNotEmpty(remoteInboundRtpStreamStats.kind);
                    Assert.IsNotEmpty(remoteInboundRtpStreamStats.transportId);
                    Assert.IsNotEmpty(remoteInboundRtpStreamStats.codecId);
                    Ignore.Pass(remoteInboundRtpStreamStats.packetsLost);
                    Ignore.Pass(remoteInboundRtpStreamStats.jitter);
                    Assert.IsNotEmpty(remoteInboundRtpStreamStats.localId);
                    Ignore.Pass(remoteInboundRtpStreamStats.roundTripTime);
                    break;
                case RTCStatsType.Transport:
                    var transportStats = stats as RTCTransportStats;
                    Assert.NotNull(transportStats);
                    Assert.AreEqual(11, transportStats.Dict.Count);
                    Ignore.Pass(transportStats.bytesSent);
                    Ignore.Pass(transportStats.bytesReceived);
                    Ignore.Pass(transportStats.rtcpTransportStatsId);
                    Ignore.Pass(transportStats.dtlsState);
                    Ignore.Pass(transportStats.selectedCandidatePairId);
                    Ignore.Pass(transportStats.localCertificateId);
                    Ignore.Pass(transportStats.remoteCertificateId);
                    Ignore.Pass(transportStats.tlsVersion);
                    Ignore.Pass(transportStats.dtlsCipher);
                    Ignore.Pass(transportStats.srtpCipher);
                    Ignore.Pass(transportStats.selectedCandidatePairChanges);
                    break;
                case RTCStatsType.RemoteOutboundRtp:
                case RTCStatsType.OutboundRtp:
                    var outboundRtpStreamStats = stats as RTCOutboundRTPStreamStats;
                    Assert.NotNull(outboundRtpStreamStats);
                    Assert.AreEqual(35, outboundRtpStreamStats.Dict.Count);
                    Ignore.Pass(outboundRtpStreamStats.ssrc);
                    Ignore.Pass(outboundRtpStreamStats.isRemote);
                    Assert.IsNotEmpty(outboundRtpStreamStats.mediaType);
                    Assert.IsNotEmpty(outboundRtpStreamStats.kind);
                    Ignore.Pass(outboundRtpStreamStats.trackId);
                    Assert.IsNotEmpty(outboundRtpStreamStats.transportId);
                    Ignore.Pass(outboundRtpStreamStats.codecId);
                    Ignore.Pass(outboundRtpStreamStats.firCount);
                    Ignore.Pass(outboundRtpStreamStats.pliCount);
                    Ignore.Pass(outboundRtpStreamStats.nackCount);
                    Ignore.Pass(outboundRtpStreamStats.sliCount);
                    Ignore.Pass(outboundRtpStreamStats.qpSum);
                    Ignore.Pass(outboundRtpStreamStats.mediaSourceId);
                    Ignore.Pass(outboundRtpStreamStats.remoteId);
                    Ignore.Pass(outboundRtpStreamStats.rid);
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
                    Ignore.Pass(outboundRtpStreamStats.frameWidth);
                    Ignore.Pass(outboundRtpStreamStats.frameHeight);
                    Ignore.Pass(outboundRtpStreamStats.framesPerSecond);
                    Ignore.Pass(outboundRtpStreamStats.framesSent);
                    Ignore.Pass(outboundRtpStreamStats.hugeFramesSent);
                    Ignore.Pass(outboundRtpStreamStats.totalPacketSendDelay);
                    Ignore.Pass(outboundRtpStreamStats.qualityLimitationReason);
                    Ignore.Pass(outboundRtpStreamStats.qualityLimitationResolutionChanges);
                    Ignore.Pass(outboundRtpStreamStats.contentType);
                    Ignore.Pass(outboundRtpStreamStats.encoderImplementation);
                    break;
                case RTCStatsType.MediaSource:
                    var mediaSourceStats = stats as RTCMediaSourceStats;
                    Assert.NotNull(mediaSourceStats);
                    Assert.AreEqual(6, mediaSourceStats.Dict.Count);
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
                    Assert.AreEqual(42, mediaStreamTrackStats.Dict.Count);
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
                    Ignore.Pass(mediaStreamTrackStats.jitterBufferTargetDelay);
                    Ignore.Pass(mediaStreamTrackStats.interruptionCount);
                    Ignore.Pass(mediaStreamTrackStats.totalInterruptionDuration);
                    Ignore.Pass(mediaStreamTrackStats.freezeCount);
                    Ignore.Pass(mediaStreamTrackStats.pauseCount);
                    Ignore.Pass(mediaStreamTrackStats.totalFreezesDuration);
                    Ignore.Pass(mediaStreamTrackStats.totalPausesDuration);
                    Ignore.Pass(mediaStreamTrackStats.totalFramesDuration);
                    Ignore.Pass(mediaStreamTrackStats.sumOfSquaredFramesDuration);
                    break;
                case RTCStatsType.Stream:
                    var mediaStreamStats = stats as RTCMediaStreamStats;
                    Assert.NotNull(mediaStreamStats);
                    Assert.AreEqual(2, mediaStreamStats.Dict.Count);
                    Ignore.Pass(mediaStreamStats.streamIdentifier);
                    Ignore.Pass(mediaStreamStats.trackIds);
                    break;
                case RTCStatsType.PeerConnection:
                    var peerConnectionStats = stats as RTCPeerConnectionStats;
                    Assert.NotNull(peerConnectionStats);
                    Assert.AreEqual(2, peerConnectionStats.Dict.Count);
                    Ignore.Pass(peerConnectionStats.dataChannelsOpened);
                    Ignore.Pass(peerConnectionStats.dataChannelsClosed);
                    break;
                default:
                    Debug.LogWarning(stats.Type);
                    break;
            }
        }
    }
}
