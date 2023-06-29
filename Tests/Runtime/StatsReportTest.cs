using System;
using NUnit.Framework;
using UnityEngine;

namespace Unity.WebRTC.RuntimeTest
{
    class StatsReportTest
    {
        [Test]
        public void ConstructorThrowsException()
        {
            Assert.That(() => new RTCStatsReport(IntPtr.Zero), Throws.ArgumentException);
        }
    }



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
                    // Obsolete: Ignore.Pass(iceCandidatePairStats.priority);
                    Ignore.Pass(iceCandidatePairStats.nominated);
                    // Does not exist in the spec: Ignore.Pass(iceCandidatePairStats.writable);
                    Ignore.Pass(iceCandidatePairStats.packetsSent);
                    Ignore.Pass(iceCandidatePairStats.packetsReceived);
                    Ignore.Pass(iceCandidatePairStats.bytesSent);
                    Ignore.Pass(iceCandidatePairStats.bytesReceived);
                    Ignore.Pass(iceCandidatePairStats.lastPacketSentTimestamp);
                    Ignore.Pass(iceCandidatePairStats.lastPacketReceivedTimestamp);
                    Ignore.Pass(iceCandidatePairStats.totalRoundTripTime);
                    Ignore.Pass(iceCandidatePairStats.currentRoundTripTime);
                    Ignore.Pass(iceCandidatePairStats.availableOutgoingBitrate);
                    Ignore.Pass(iceCandidatePairStats.availableIncomingBitrate);
                    Ignore.Pass(iceCandidatePairStats.requestsReceived);
                    Ignore.Pass(iceCandidatePairStats.requestsSent);
                    Ignore.Pass(iceCandidatePairStats.responsesReceived);
                    Ignore.Pass(iceCandidatePairStats.responsesSent);
                    Ignore.Pass(iceCandidatePairStats.consentRequestsSent);
                    Ignore.Pass(iceCandidatePairStats.packetsDiscardedOnSend);
                    Ignore.Pass(iceCandidatePairStats.bytesDiscardedOnSend);
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
                    Assert.AreEqual(18, candidateStats.Dict.Count);
                    Assert.IsNotEmpty(candidateStats.transportId);
                    // Obsolete: Ignore.Pass(candidateStats.isRemote);
                    Ignore.Pass(candidateStats.networkType);
                    Ignore.Pass(candidateStats.ip);
                    Ignore.Pass(candidateStats.address);
                    Ignore.Pass(candidateStats.port);
                    Assert.IsNotEmpty(candidateStats.protocol);
                    Ignore.Pass(candidateStats.relayProtocol);
                    Assert.IsNotEmpty(candidateStats.candidateType);
                    Ignore.Pass(candidateStats.priority);
                    Ignore.Pass(candidateStats.url);
                    Ignore.Pass(candidateStats.foundation);
                    Ignore.Pass(candidateStats.relatedAddress);
                    Ignore.Pass(candidateStats.relatedPort);
                    Ignore.Pass(candidateStats.usernameFragment);
                    Ignore.Pass(candidateStats.tcpType);
                    Ignore.Pass(candidateStats.vpn);
                    Ignore.Pass(candidateStats.networkAdapterType);
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
                    Assert.AreEqual(6, codecStats.Dict.Count);
                    Assert.IsNotEmpty(codecStats.mimeType);
                    Assert.IsNotEmpty(codecStats.transportId);
                    Ignore.Pass(codecStats.sdpFmtpLine);
                    Ignore.Pass(codecStats.payloadType);
                    Ignore.Pass(codecStats.clockRate);
                    Ignore.Pass(codecStats.channels);
                    break;
                case RTCStatsType.InboundRtp:
                    var inboundRtpStreamStats = stats as RTCInboundRTPStreamStats;
                    Assert.NotNull(inboundRtpStreamStats);
                    Assert.AreEqual(64, inboundRtpStreamStats.Dict.Count);
                    Ignore.Pass(inboundRtpStreamStats.ssrc);
                    Assert.IsNotEmpty(inboundRtpStreamStats.kind);
                    // Obsolete:  Ignore.Pass(inboundRtpStreamStats.trackId);
                    Assert.IsNotEmpty(inboundRtpStreamStats.transportId);
                    Ignore.Pass(inboundRtpStreamStats.codecId);
                    // Obsolete: Assert.IsNotEmpty(inboundRtpStreamStats.mediaType);
                    Ignore.Pass(inboundRtpStreamStats.jitter);
                    Ignore.Pass(inboundRtpStreamStats.packetsLost);
                    Ignore.Pass(inboundRtpStreamStats.playoutId);
                    Ignore.Pass(inboundRtpStreamStats.trackIdentifier);
                    Ignore.Pass(inboundRtpStreamStats.mid);
                    Ignore.Pass(inboundRtpStreamStats.remoteId);
                    Ignore.Pass(inboundRtpStreamStats.packetsReceived);
                    Ignore.Pass(inboundRtpStreamStats.packetsDiscarded);
                    Ignore.Pass(inboundRtpStreamStats.fecPacketsReceived);
                    Ignore.Pass(inboundRtpStreamStats.fecPacketsDiscarded);
                    Ignore.Pass(inboundRtpStreamStats.bytesReceived);
                    Ignore.Pass(inboundRtpStreamStats.headerBytesReceived);
                    Ignore.Pass(inboundRtpStreamStats.lastPacketReceivedTimestamp);
                    Ignore.Pass(inboundRtpStreamStats.jitterBufferDelay);
                    Ignore.Pass(inboundRtpStreamStats.jitterBufferTargetDelay);
                    Ignore.Pass(inboundRtpStreamStats.jitterBufferMinimumDelay);
                    Ignore.Pass(inboundRtpStreamStats.jitterBufferEmittedCount);
                    Ignore.Pass(inboundRtpStreamStats.totalSamplesReceived);
                    Ignore.Pass(inboundRtpStreamStats.concealedSamples);
                    Ignore.Pass(inboundRtpStreamStats.silentConcealedSamples);
                    Ignore.Pass(inboundRtpStreamStats.concealmentEvents);
                    Ignore.Pass(inboundRtpStreamStats.insertedSamplesForDeceleration);
                    Ignore.Pass(inboundRtpStreamStats.removedSamplesForAcceleration);
                    Ignore.Pass(inboundRtpStreamStats.audioLevel);
                    Ignore.Pass(inboundRtpStreamStats.totalAudioEnergy);
                    Ignore.Pass(inboundRtpStreamStats.totalSamplesDuration);
                    Ignore.Pass(inboundRtpStreamStats.framesReceived);
                    Ignore.Pass(inboundRtpStreamStats.frameWidth);
                    Ignore.Pass(inboundRtpStreamStats.frameHeight);
                    Ignore.Pass(inboundRtpStreamStats.framesPerSecond);
                    Ignore.Pass(inboundRtpStreamStats.framesDecoded);
                    Ignore.Pass(inboundRtpStreamStats.keyFramesDecoded);
                    Ignore.Pass(inboundRtpStreamStats.framesDropped);
                    Ignore.Pass(inboundRtpStreamStats.totalDecodeTime);
                    Ignore.Pass(inboundRtpStreamStats.totalProcessingDelay);
                    Ignore.Pass(inboundRtpStreamStats.totalAssemblyTime);
                    Ignore.Pass(inboundRtpStreamStats.framesAssembledFromMultiplePackets);
                    Ignore.Pass(inboundRtpStreamStats.totalInterFrameDelay);
                    Ignore.Pass(inboundRtpStreamStats.totalSquaredInterFrameDelay);
                    Ignore.Pass(inboundRtpStreamStats.pauseCount);
                    Ignore.Pass(inboundRtpStreamStats.totalPausesDuration);
                    Ignore.Pass(inboundRtpStreamStats.freezeCount);
                    Ignore.Pass(inboundRtpStreamStats.totalFreezesDuration);
                    Ignore.Pass(inboundRtpStreamStats.contentType);
                    Ignore.Pass(inboundRtpStreamStats.estimatedPlayoutTimestamp);
                    Ignore.Pass(inboundRtpStreamStats.decoderImplementation);
                    Ignore.Pass(inboundRtpStreamStats.firCount);
                    Ignore.Pass(inboundRtpStreamStats.pliCount);
                    Ignore.Pass(inboundRtpStreamStats.nackCount);
                    Ignore.Pass(inboundRtpStreamStats.qpSum);
                    break;
                case RTCStatsType.RemoteInboundRtp:
                    var remoteInboundRtpStreamStats = stats as RTCRemoteInboundRtpStreamStats;
                    Assert.NotNull(remoteInboundRtpStreamStats);
                    Assert.AreEqual(13, remoteInboundRtpStreamStats.Dict.Count);
                    Ignore.Pass(remoteInboundRtpStreamStats.ssrc);
                    Assert.IsNotEmpty(remoteInboundRtpStreamStats.kind);
                    Assert.IsNotEmpty(remoteInboundRtpStreamStats.transportId);
                    Assert.IsNotEmpty(remoteInboundRtpStreamStats.codecId);
                    Ignore.Pass(remoteInboundRtpStreamStats.packetsLost);
                    Ignore.Pass(remoteInboundRtpStreamStats.jitter);
                    Assert.IsNotEmpty(remoteInboundRtpStreamStats.localId);
                    Ignore.Pass(remoteInboundRtpStreamStats.roundTripTime);
                    Ignore.Pass(remoteInboundRtpStreamStats.fractionLost);
                    Ignore.Pass(remoteInboundRtpStreamStats.totalRoundTripTime);
                    Ignore.Pass(remoteInboundRtpStreamStats.roundTripTimeMeasurements);
                    // Obsolete: Assert.IsNotEmpty(remoteInboundRtpStreamStats.trackId);
                    // Obsolete: Assert.IsNotEmpty(remoteInboundRtpStreamStats.mediaType);
                    break;
                case RTCStatsType.RemoteOutboundRtp:
                    var remoteOutboundRtpStreamStats = stats as RTCRemoteOutboundRtpStreamStats;
                    Assert.NotNull(remoteOutboundRtpStreamStats);
                    Assert.AreEqual(11, remoteOutboundRtpStreamStats.Dict.Count);
                    Assert.IsNotEmpty(remoteOutboundRtpStreamStats.localId);
                    Ignore.Pass(remoteOutboundRtpStreamStats.remoteTimestamp);
                    Ignore.Pass(remoteOutboundRtpStreamStats.reportsSent);
                    Ignore.Pass(remoteOutboundRtpStreamStats.packetsSent);
                    Ignore.Pass(remoteOutboundRtpStreamStats.bytesSent);
                    Ignore.Pass(remoteOutboundRtpStreamStats.ssrc);
                    Assert.IsNotEmpty(remoteOutboundRtpStreamStats.kind);
                    Assert.IsNotEmpty(remoteOutboundRtpStreamStats.transportId);
                    Assert.IsNotEmpty(remoteOutboundRtpStreamStats.codecId);
                    // Obsolete: Assert.IsNotEmpty(remoteOutboundRtpStreamStats.trackId);
                    // Obsolete: Assert.IsNotEmpty(remoteOutboundRtpStreamStats.mediaType);
                    break;
                case RTCStatsType.Transport:
                    var transportStats = stats as RTCTransportStats;
                    Assert.NotNull(transportStats);
                    Assert.AreEqual(17, transportStats.Dict.Count);
                    Ignore.Pass(transportStats.bytesSent);
                    Ignore.Pass(transportStats.bytesReceived);
                    Ignore.Pass(transportStats.packetsSent);
                    Ignore.Pass(transportStats.packetsReceived);
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
                case RTCStatsType.OutboundRtp:
                    var outboundRtpStreamStats = stats as RTCOutboundRTPStreamStats;
                    Assert.NotNull(outboundRtpStreamStats);
                    Assert.AreEqual(38, outboundRtpStreamStats.Dict.Count);
                    Ignore.Pass(outboundRtpStreamStats.ssrc);
                    // Obsolete: Assert.IsNotEmpty(outboundRtpStreamStats.mediaType);
                    Assert.IsNotEmpty(outboundRtpStreamStats.kind);
                    // Obsolete: Ignore.Pass(outboundRtpStreamStats.trackId);
                    Assert.IsNotEmpty(outboundRtpStreamStats.transportId);
                    Ignore.Pass(outboundRtpStreamStats.codecId);
                    Ignore.Pass(outboundRtpStreamStats.mediaSourceId);
                    Ignore.Pass(outboundRtpStreamStats.mid);
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
                    Ignore.Pass(outboundRtpStreamStats.firCount);
                    Ignore.Pass(outboundRtpStreamStats.pliCount);
                    Ignore.Pass(outboundRtpStreamStats.nackCount);
                    Ignore.Pass(outboundRtpStreamStats.qpSum);
                    Ignore.Pass(outboundRtpStreamStats.active);
                    break;
                case RTCStatsType.MediaSource:
                    var mediaSourceStats = stats as RTCMediaSourceStats;
                    Assert.NotNull(mediaSourceStats);
                    Assert.IsNotEmpty(mediaSourceStats.trackIdentifier);
                    Assert.IsNotEmpty(mediaSourceStats.kind);
                    if (mediaSourceStats is RTCVideoSourceStats videoSourceStats)
                    {
                        Assert.AreEqual(6, videoSourceStats.Dict.Count);
                        Ignore.Pass(videoSourceStats.width);
                        Ignore.Pass(videoSourceStats.height);
                        Ignore.Pass(videoSourceStats.frames);
                        Ignore.Pass(videoSourceStats.framesPerSecond);
                    }
                    if (mediaSourceStats is RTCAudioSourceStats audioSourceStats)
                    {
                        Assert.AreEqual(5, audioSourceStats.Dict.Count);
                        Ignore.Pass(audioSourceStats.audioLevel);
                        Ignore.Pass(audioSourceStats.totalAudioEnergy);
                        Ignore.Pass(audioSourceStats.totalSamplesDuration);
                    }
                    break;
                case RTCStatsType.PeerConnection:
                    var peerConnectionStats = stats as RTCPeerConnectionStats;
                    Assert.NotNull(peerConnectionStats);
                    Assert.AreEqual(2, peerConnectionStats.Dict.Count);
                    Ignore.Pass(peerConnectionStats.dataChannelsOpened);
                    Ignore.Pass(peerConnectionStats.dataChannelsClosed);
                    break;
                case RTCStatsType.ReceivedRtp:
                    var receivedRtpStats = stats as RTCReceivedRtpStats;
                    Assert.NotNull(receivedRtpStats);
                    Assert.AreEqual(2, receivedRtpStats.Dict.Count);
                    Ignore.Pass(receivedRtpStats.jitter);
                    Ignore.Pass(receivedRtpStats.packetsLost);
                    break;
                case RTCStatsType.SentRtp:
                    var sentRtpStats = stats as RTCSentRtpStats;
                    Assert.NotNull(sentRtpStats);
                    Assert.AreEqual(2, sentRtpStats.Dict.Count);
                    Ignore.Pass(sentRtpStats.packetsSent);
                    Ignore.Pass(sentRtpStats.bytesSent);
                    break;
            }
        }
    }
}
