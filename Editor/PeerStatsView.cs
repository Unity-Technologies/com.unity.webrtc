using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class PeerStatsView
    {
        private WebRTCInternals m_parent;
        private RTCPeerConnection m_peerConnection;

        public PeerStatsView(RTCPeerConnection peer, WebRTCInternals parent)
        {
            m_peerConnection = peer;
            m_parent = parent;
        }


        public VisualElement Create()
        {
            var root = new VisualElement();
            var list = Enum.GetValues(typeof(RTCStatsType)).Cast<RTCStatsType>().ToList();
            var popup = new PopupField<RTCStatsType>(list, 0);
            root.Add(popup);

            var container = new VisualElement();
            root.Add(container);
            popup.RegisterValueChangedCallback(e =>
            {
                Debug.Log($"new choose stats type is {e.newValue}");
                container.Clear();

                switch (e.newValue)
                {
                    case RTCStatsType.Codec:
                        container.Add(CreateCodecView());
                        break;
                    case RTCStatsType.InboundRtp:
                        container.Add(CreateInboundRtpView());
                        break;
                    case RTCStatsType.OutboundRtp:
                        container.Add(CreateOutboundRtpView());
                        break;
                    case RTCStatsType.RemoteInboundRtp:
                        container.Add(CreateRemoteInboundRtpView());
                        break;
                    case RTCStatsType.RemoteOutboundRtp:
                        container.Add(CreateRemoteOutboundRtpView());
                        break;
                    case RTCStatsType.MediaSource:
                        container.Add(CreateMediaSourceView());
                        break;
                    case RTCStatsType.Csrc:
                        container.Add(CreateCsrcView());
                        break;
                    case RTCStatsType.PeerConnection:
                        container.Add(CreatePeerConnectionView());
                        break;
                    case RTCStatsType.DataChannel:
                        container.Add(CreateDataChannelView());
                        break;
                    case RTCStatsType.Stream:
                        container.Add(CreateStreamView());
                        break;
                    case RTCStatsType.Track:
                        container.Add(CreateTrackView());
                        break;
                    case RTCStatsType.Transceiver:
                        container.Add(CreateTransceiverView());
                        break;
                    case RTCStatsType.Sender:
                        container.Add(CreateSenderView());
                        break;
                    case RTCStatsType.Receiver:
                        container.Add(CreateReceiverView());
                        break;
                    case RTCStatsType.Transport:
                        container.Add(CreateTransportView());
                        break;
                    case RTCStatsType.SctpTransport:
                        container.Add(CreateSctpTransportView());
                        break;
                    case RTCStatsType.CandidatePair:
                        container.Add(CreateCandidatePairView());
                        break;
                    case RTCStatsType.LocalCandidate:
                        container.Add(CreateLocalCandidateView());
                        break;
                    case RTCStatsType.RemoteCandidate:
                        container.Add(CreateRemoteCandidateView());
                        break;
                    case RTCStatsType.Certificate:
                        container.Add(CreateCertificateView());
                        break;
                    case RTCStatsType.IceServer:
                        container.Add(CreateIceServerView());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"this type is not supported : {e.newValue}");
                }
            });

            return root;
        }

        private VisualElement CreateCodecView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Codec}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Codec, out var stats) ||
                    !(stats is RTCCodecStats codecStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Codec}"));
                    return;
                }

                container.Add(new Label($"{nameof(codecStats.Id)}: {codecStats.Id}"));
                container.Add(new Label($"{nameof(codecStats.Timestamp)}: {codecStats.Timestamp}"));
                container.Add(new Label($"{nameof(codecStats.payloadType)}: {codecStats.payloadType}"));
                container.Add(new Label($"{nameof(codecStats.mimeType)}: {codecStats.mimeType}"));
                container.Add(new Label($"{nameof(codecStats.clockRate)}: {codecStats.clockRate}"));
                container.Add(new Label($"{nameof(codecStats.channels)}: {codecStats.channels}"));
                container.Add(new Label($"{nameof(codecStats.sdpFmtpLine)}: {codecStats.sdpFmtpLine}"));
            };
            return root;
        }

        private VisualElement CreateInboundRtpView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.InboundRtp}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.InboundRtp, out var stats) ||
                    !(stats is RTCInboundRTPStreamStats inboundStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.InboundRtp}"));
                    return;
                }

                container.Add(new Label($"{nameof(inboundStats.Id)}: {inboundStats.Id}"));
                container.Add(new Label($"{nameof(inboundStats.Timestamp)}: {inboundStats.Timestamp}"));
                container.Add(new Label($"{nameof(inboundStats.packetsReceived)}: {inboundStats.packetsReceived}"));
                container.Add(new Label($"{nameof(inboundStats.bytesReceived)}: {inboundStats.bytesReceived}"));
                container.Add(
                    new Label($"{nameof(inboundStats.headerBytesReceived)}: {inboundStats.headerBytesReceived}"));
                container.Add(new Label($"{nameof(inboundStats.packetsLost)}: {inboundStats.packetsLost}"));
                container.Add(new Label(
                    $"{nameof(inboundStats.lastPacketReceivedTimestamp)}: {inboundStats.lastPacketReceivedTimestamp}"));
                container.Add(new Label($"{nameof(inboundStats.jitter)}: {inboundStats.jitter}"));
                container.Add(new Label($"{nameof(inboundStats.roundTripTime)}: {inboundStats.roundTripTime}"));
                container.Add(new Label($"{nameof(inboundStats.packetsDiscarded)}: {inboundStats.packetsDiscarded}"));
                container.Add(new Label($"{nameof(inboundStats.packetsRepaired)}: {inboundStats.packetsRepaired}"));
                container.Add(new Label($"{nameof(inboundStats.burstPacketsLost)}: {inboundStats.burstPacketsLost}"));
                container.Add(
                    new Label($"{nameof(inboundStats.burstPacketsDiscarded)}: {inboundStats.burstPacketsDiscarded}"));
                container.Add(new Label($"{nameof(inboundStats.burstLossCount)}: {inboundStats.burstLossCount}"));
                container.Add(new Label($"{nameof(inboundStats.burstDiscardCount)}: {inboundStats.burstDiscardCount}"));
                container.Add(new Label($"{nameof(inboundStats.burstLossRate)}: {inboundStats.burstLossRate}"));
                container.Add(new Label($"{nameof(inboundStats.burstDiscardRate)}: {inboundStats.burstDiscardRate}"));
                container.Add(new Label($"{nameof(inboundStats.gapLossRate)}: {inboundStats.gapLossRate}"));
                container.Add(new Label($"{nameof(inboundStats.gapDiscardRate)}: {inboundStats.gapDiscardRate}"));
                container.Add(new Label($"{nameof(inboundStats.framesDecoded)}: {inboundStats.framesDecoded}"));
                container.Add(new Label($"{nameof(inboundStats.keyFramesDecoded)}: {inboundStats.keyFramesDecoded}"));
                container.Add(new Label($"{nameof(inboundStats.totalDecodeTime)}: {inboundStats.totalDecodeTime}"));
                container.Add(new Label($"{nameof(inboundStats.contentType)}: {inboundStats.contentType}"));
                container.Add(
                    new Label($"{nameof(inboundStats.decoderImplementation)}: {inboundStats.decoderImplementation}"));
            };
            return root;
        }

        private VisualElement CreateOutboundRtpView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.OutboundRtp}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.OutboundRtp, out var stats) ||
                    !(stats is RTCOutboundRTPStreamStats outboundStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.OutboundRtp}"));
                    return;
                }

                container.Add(new Label($"{nameof(outboundStats.Id)}: {outboundStats.Id}"));
                container.Add(new Label($"{nameof(outboundStats.Timestamp)}: {outboundStats.Timestamp}"));
                container.Add(new Label($"{nameof(outboundStats.mediaSourceId)}: {outboundStats.mediaSourceId}"));
                container.Add(new Label($"{nameof(outboundStats.packetsSent)}: {outboundStats.packetsSent}"));
                container.Add(new Label(
                    $"{nameof(outboundStats.retransmittedPacketsSent)}: {outboundStats.retransmittedPacketsSent}"));
                container.Add(new Label($"{nameof(outboundStats.bytesSent)}: {outboundStats.bytesSent}"));
                container.Add(new Label($"{nameof(outboundStats.headerBytesSent)}: {outboundStats.headerBytesSent}"));
                container.Add(new Label(
                    $"{nameof(outboundStats.retransmittedBytesSent)}: {outboundStats.retransmittedBytesSent}"));
                container.Add(new Label($"{nameof(outboundStats.targetBitrate)}: {outboundStats.targetBitrate}"));
                container.Add(new Label($"{nameof(outboundStats.framesEncoded)}: {outboundStats.framesEncoded}"));
                container.Add(new Label($"{nameof(outboundStats.keyFramesEncoded)}: {outboundStats.keyFramesEncoded}"));
                container.Add(new Label($"{nameof(outboundStats.totalEncodeTime)}: {outboundStats.totalEncodeTime}"));
                container.Add(new Label(
                    $"{nameof(outboundStats.totalEncodedBytesTarget)}: {outboundStats.totalEncodedBytesTarget}"));
                container.Add(
                    new Label($"{nameof(outboundStats.totalPacketSendDelay)}: {outboundStats.totalPacketSendDelay}"));
                container.Add(new Label(
                    $"{nameof(outboundStats.qualityLimitationReason)}: {outboundStats.qualityLimitationReason}"));
                container.Add(new Label(
                    $"{nameof(outboundStats.qualityLimitationResolutionChanges)}: {outboundStats.qualityLimitationResolutionChanges}"));
                container.Add(new Label($"{nameof(outboundStats.contentType)}: {outboundStats.contentType}"));
                container.Add(new Label(
                    $"{nameof(outboundStats.encoderImplementation)}: {outboundStats.encoderImplementation}"));
            };
            return root;
        }

        private VisualElement CreateRemoteInboundRtpView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.RemoteInboundRtp}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.RemoteInboundRtp, out var stats) ||
                    !(stats is RTCRemoteInboundRtpStreamStats remoteInboundStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.RemoteInboundRtp}"));
                    return;
                }

                container.Add(new Label($"{nameof(remoteInboundStats.Id)}: {remoteInboundStats.Id}"));
                container.Add(new Label($"{nameof(remoteInboundStats.Timestamp)}: {remoteInboundStats.Timestamp}"));
            };
            return root;
        }

        private VisualElement CreateRemoteOutboundRtpView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.RemoteOutboundRtp}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.RemoteOutboundRtp, out var stats) ||
                    !(stats is RTCRemoteOutboundRtpStreamStats remoteOutboundStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.RemoteOutboundRtp}"));
                    return;
                }

                container.Add(new Label($"{nameof(remoteOutboundStats.Id)}: {remoteOutboundStats.Id}"));
                container.Add(new Label($"{nameof(remoteOutboundStats.Timestamp)}: {remoteOutboundStats.Timestamp}"));
            };
            return root;
        }

        private VisualElement CreateMediaSourceView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.MediaSource}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.MediaSource, out var stats) ||
                    !(stats is RTCMediaSourceStats mediaSourceStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.MediaSource}"));
                    return;
                }

                container.Add(new Label($"{nameof(mediaSourceStats.Id)}: {mediaSourceStats.Id}"));
                container.Add(new Label($"{nameof(mediaSourceStats.Timestamp)}: {mediaSourceStats.Timestamp}"));
                container.Add(
                    new Label($"{nameof(mediaSourceStats.trackIdentifier)}: {mediaSourceStats.trackIdentifier}"));
                container.Add(new Label($"{nameof(mediaSourceStats.kind)}: {mediaSourceStats.kind}"));
                container.Add(new Label($"{nameof(mediaSourceStats.width)}: {mediaSourceStats.width}"));
                container.Add(new Label($"{nameof(mediaSourceStats.height)}: {mediaSourceStats.height}"));
                container.Add(new Label($"{nameof(mediaSourceStats.frames)}: {mediaSourceStats.frames}"));
                container.Add(
                    new Label($"{nameof(mediaSourceStats.framesPerSecond)}: {mediaSourceStats.framesPerSecond}"));
            };
            return root;
        }

        private VisualElement CreateCsrcView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Csrc}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Csrc, out var stats) ||
                    !(stats is RTCCodecStats csrcStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Csrc}"));
                    return;
                }

                container.Add(new Label($"{nameof(csrcStats.Id)}: {csrcStats.Id}"));
                container.Add(new Label($"{nameof(csrcStats.Timestamp)}: {csrcStats.Timestamp}"));
            };
            return root;
        }

        private VisualElement CreatePeerConnectionView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.PeerConnection}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.PeerConnection, out var stats) ||
                    !(stats is RTCPeerConnectionStats peerStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Codec}"));
                    return;
                }

                container.Add(new Label($"{nameof(peerStats.Id)}: {peerStats.Id}"));
                container.Add(new Label($"{nameof(peerStats.Timestamp)}: {peerStats.Timestamp}"));
                container.Add(new Label($"{nameof(peerStats.dataChannelsOpened)}: {peerStats.dataChannelsOpened}"));
                container.Add(new Label($"{nameof(peerStats.dataChannelsClosed)}: {peerStats.dataChannelsClosed}"));
            };
            return root;
        }

        private VisualElement CreateDataChannelView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.DataChannel}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.DataChannel, out var stats) ||
                    !(stats is RTCDataChannelStats dataChannelStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.DataChannel}"));
                    return;
                }

                container.Add(new Label($"{nameof(dataChannelStats.Id)}: {dataChannelStats.Id}"));
                container.Add(new Label($"{nameof(dataChannelStats.Timestamp)}: {dataChannelStats.Timestamp}"));
                container.Add(new Label($"{nameof(dataChannelStats.label)}: {dataChannelStats.label}"));
                container.Add(new Label($"{nameof(dataChannelStats.protocol)}: {dataChannelStats.protocol}"));
                container.Add(new Label($"{nameof(dataChannelStats.datachannelid)}: {dataChannelStats.datachannelid}"));
                container.Add(new Label($"{nameof(dataChannelStats.state)}: {dataChannelStats.state}"));
                container.Add(new Label($"{nameof(dataChannelStats.messagesSent)}: {dataChannelStats.messagesSent}"));
                container.Add(new Label($"{nameof(dataChannelStats.bytesSent)}: {dataChannelStats.bytesSent}"));
                container.Add(
                    new Label($"{nameof(dataChannelStats.messagesReceived)}: {dataChannelStats.messagesReceived}"));
                container.Add(new Label($"{nameof(dataChannelStats.bytesReceived)}: {dataChannelStats.bytesReceived}"));
            };
            return root;
        }

        private VisualElement CreateStreamView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Stream}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Stream, out var stats) ||
                    !(stats is RTCMediaStreamStats streamStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Stream}"));
                    return;
                }

                container.Add(new Label($"{nameof(streamStats.Id)}: {streamStats.Id}"));
                container.Add(new Label($"{nameof(streamStats.Timestamp)}: {streamStats.Timestamp}"));
                container.Add(new Label($"{nameof(streamStats.streamIdentifier)}: {streamStats.streamIdentifier}"));
                container.Add(
                    new Label($"{nameof(streamStats.trackIds)}: {string.Join(",", streamStats.trackIds)}"));
            };
            return root;
        }

        private VisualElement CreateTrackView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Track}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Track, out var stats) ||
                    !(stats is RTCMediaStreamTrackStats trackStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Track}"));
                    return;
                }

                container.Add(new Label($"{nameof(trackStats.Id)}: {trackStats.Id}"));
                container.Add(new Label($"{nameof(trackStats.Timestamp)}: {trackStats.Timestamp}"));
                container.Add(new Label($"{nameof(trackStats.trackIdentifier)}: {trackStats.trackIdentifier}"));
                container.Add(new Label($"{nameof(trackStats.mediaSourceId)}: {trackStats.mediaSourceId}"));
                container.Add(new Label($"{nameof(trackStats.remoteSource)}: {trackStats.remoteSource}"));
                container.Add(new Label($"{nameof(trackStats.ended)}: {trackStats.ended}"));
                container.Add(new Label($"{nameof(trackStats.detached)}: {trackStats.detached}"));
                container.Add(new Label($"{nameof(trackStats.kind)}: {trackStats.kind}"));
                container.Add(new Label($"{nameof(trackStats.jitterBufferDelay)}: {trackStats.jitterBufferDelay}"));
                container.Add(new Label(
                    $"{nameof(trackStats.jitterBufferEmittedCount)}: {trackStats.jitterBufferEmittedCount}"));
                container.Add(new Label($"{nameof(trackStats.frameWidth)}: {trackStats.frameWidth}"));
                container.Add(new Label($"{nameof(trackStats.frameHeight)}: {trackStats.frameHeight}"));
                container.Add(new Label($"{nameof(trackStats.framesPerSecond)}: {trackStats.framesPerSecond}"));
                container.Add(new Label($"{nameof(trackStats.framesSent)}: {trackStats.framesSent}"));
                container.Add(new Label($"{nameof(trackStats.hugeFramesSent)}: {trackStats.hugeFramesSent}"));
                container.Add(new Label($"{nameof(trackStats.framesReceived)}: {trackStats.framesReceived}"));
                container.Add(new Label($"{nameof(trackStats.framesDecoded)}: {trackStats.framesDecoded}"));
                container.Add(new Label($"{nameof(trackStats.framesDropped)}: {trackStats.framesDropped}"));
                container.Add(new Label($"{nameof(trackStats.framesCorrupted)}: {trackStats.framesCorrupted}"));
                container.Add(new Label($"{nameof(trackStats.partialFramesLost)}: {trackStats.partialFramesLost}"));
                container.Add(new Label($"{nameof(trackStats.fullFramesLost)}: {trackStats.fullFramesLost}"));
                container.Add(new Label($"{nameof(trackStats.audioLevel)}: {trackStats.audioLevel}"));
                container.Add(new Label($"{nameof(trackStats.totalAudioEnergy)}: {trackStats.totalAudioEnergy}"));
                container.Add(new Label($"{nameof(trackStats.echoReturnLoss)}: {trackStats.echoReturnLoss}"));
                container.Add(new Label(
                    $"{nameof(trackStats.echoReturnLossEnhancement)}: {trackStats.echoReturnLossEnhancement}"));
                container.Add(
                    new Label($"{nameof(trackStats.totalSamplesReceived)}: {trackStats.totalSamplesReceived}"));
                container.Add(
                    new Label($"{nameof(trackStats.totalSamplesDuration)}: {trackStats.totalSamplesDuration}"));
                container.Add(new Label($"{nameof(trackStats.concealedSamples)}: {trackStats.concealedSamples}"));
                container.Add(
                    new Label($"{nameof(trackStats.silentConcealedSamples)}: {trackStats.silentConcealedSamples}"));
                container.Add(new Label($"{nameof(trackStats.concealmentEvents)}: {trackStats.concealmentEvents}"));
                container.Add(new Label(
                    $"{nameof(trackStats.insertedSamplesForDeceleration)}: {trackStats.insertedSamplesForDeceleration}"));
                container.Add(new Label(
                    $"{nameof(trackStats.removedSamplesForAcceleration)}: {trackStats.removedSamplesForAcceleration}"));
                container.Add(new Label($"{nameof(trackStats.jitterBufferFlushes)}: {trackStats.jitterBufferFlushes}"));
                container.Add(new Label(
                    $"{nameof(trackStats.delayedPacketOutageSamples)}: {trackStats.delayedPacketOutageSamples}"));
                container.Add(new Label(
                    $"{nameof(trackStats.relativePacketArrivalDelay)}: {trackStats.relativePacketArrivalDelay}"));
                container.Add(new Label($"{nameof(trackStats.interruptionCount)}: {trackStats.interruptionCount}"));
                container.Add(new Label(
                    $"{nameof(trackStats.totalInterruptionDuration)}: {trackStats.totalInterruptionDuration}"));
                container.Add(new Label($"{nameof(trackStats.freezeCount)}: {trackStats.freezeCount}"));
                container.Add(new Label($"{nameof(trackStats.pauseCount)}: {trackStats.pauseCount}"));
                container.Add(
                    new Label($"{nameof(trackStats.totalFreezesDuration)}: {trackStats.totalFreezesDuration}"));
                container.Add(new Label($"{nameof(trackStats.totalPausesDuration)}: {trackStats.totalPausesDuration}"));
                container.Add(new Label($"{nameof(trackStats.totalFramesDuration)}: {trackStats.totalFramesDuration}"));
                container.Add(new Label(
                    $"{nameof(trackStats.sumOfSquaredFramesDuration)}: {trackStats.sumOfSquaredFramesDuration}"));
            };
            return root;
        }

        private VisualElement CreateTransceiverView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Transceiver}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Transceiver, out var stats) ||
                    !(stats is RTCTransceiverStats transceiverStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Transceiver}"));
                    return;
                }

                container.Add(new Label($"{nameof(transceiverStats.Id)}: {transceiverStats.Id}"));
                container.Add(new Label($"{nameof(transceiverStats.Timestamp)}: {transceiverStats.Timestamp}"));
            };
            return root;
        }

        private VisualElement CreateSenderView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Sender}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Sender, out var stats) ||
                    !(stats is RTCSenderStats senderStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Sender}"));
                    return;
                }

                container.Add(new Label($"{nameof(senderStats.Id)}: {senderStats.Id}"));
                container.Add(new Label($"{nameof(senderStats.Timestamp)}: {senderStats.Timestamp}"));
            };
            return root;
        }

        private VisualElement CreateReceiverView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Receiver}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Receiver, out var stats) ||
                    !(stats is RTCReceiverStats outboundStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Receiver}"));
                    return;
                }

                container.Add(new Label($"{nameof(outboundStats.Id)}: {outboundStats.Id}"));
                container.Add(new Label($"{nameof(outboundStats.Timestamp)}: {outboundStats.Timestamp}"));
            };
            return root;
        }

        private VisualElement CreateTransportView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Transport}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Transport, out var stats) ||
                    !(stats is RTCTransportStats transportStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Transport}"));
                    return;
                }

                container.Add(new Label($"{nameof(transportStats.Id)}: {transportStats.Id}"));
                container.Add(new Label($"{nameof(transportStats.Timestamp)}: {transportStats.Timestamp}"));
                container.Add(new Label($"{nameof(transportStats.bytesSent)}: {transportStats.bytesSent}"));
                container.Add(new Label($"{nameof(transportStats.bytesReceived)}: {transportStats.bytesReceived}"));
                container.Add(new Label(
                    $"{nameof(transportStats.rtcpTransportStatsId)}: {transportStats.rtcpTransportStatsId}"));
                container.Add(new Label($"{nameof(transportStats.dtlsState)}: {transportStats.dtlsState}"));
                container.Add(new Label(
                    $"{nameof(transportStats.selectedCandidatePairId)}: {transportStats.selectedCandidatePairId}"));
                container.Add(
                    new Label($"{nameof(transportStats.localCertificateId)}: {transportStats.localCertificateId}"));
                container.Add(
                    new Label($"{nameof(transportStats.remoteCertificateId)}: {transportStats.remoteCertificateId}"));
                container.Add(new Label(
                    $"{nameof(transportStats.selectedCandidatePairChanges)}: {transportStats.selectedCandidatePairChanges}"));
            };
            return root;
        }

        private VisualElement CreateSctpTransportView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.SctpTransport}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.SctpTransport, out var stats) ||
                    !(stats is RTCTransportStats transportStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Codec}"));
                    return;
                }

                container.Add(new Label($"{nameof(transportStats.Id)}: {transportStats.Id}"));
                container.Add(new Label($"{nameof(transportStats.Timestamp)}: {transportStats.Timestamp}"));
                container.Add(new Label($"{nameof(transportStats.bytesSent)}: {transportStats.bytesSent}"));
                container.Add(new Label($"{nameof(transportStats.bytesReceived)}: {transportStats.bytesReceived}"));
                container.Add(new Label(
                    $"{nameof(transportStats.rtcpTransportStatsId)}: {transportStats.rtcpTransportStatsId}"));
                container.Add(new Label($"{nameof(transportStats.dtlsState)}: {transportStats.dtlsState}"));
                container.Add(new Label(
                    $"{nameof(transportStats.selectedCandidatePairId)}: {transportStats.selectedCandidatePairId}"));
                container.Add(
                    new Label($"{nameof(transportStats.localCertificateId)}: {transportStats.localCertificateId}"));
                container.Add(
                    new Label($"{nameof(transportStats.remoteCertificateId)}: {transportStats.remoteCertificateId}"));
                container.Add(new Label(
                    $"{nameof(transportStats.selectedCandidatePairChanges)}: {transportStats.selectedCandidatePairChanges}"));
            };
            return root;
        }

        private VisualElement CreateCandidatePairView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.CandidatePair}"));

            var container = new VisualElement();
            root.Add(container);

            var graphView = new CandidatePairGraphView();

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.CandidatePair, out var stats) ||
                    !(stats is RTCIceCandidatePairStats candidatePairStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.CandidatePair}"));
                    return;
                }

                container.Add(new Label($"{nameof(candidatePairStats.Id)}: {candidatePairStats.Id}"));
                container.Add(new Label($"{nameof(candidatePairStats.Timestamp)}: {candidatePairStats.Timestamp}"));
                container.Add(new Label($"{nameof(candidatePairStats.transportId)}: {candidatePairStats.transportId}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.localCandidateId)}: {candidatePairStats.localCandidateId}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.remoteCandidateId)}: {candidatePairStats.remoteCandidateId}"));
                container.Add(new Label($"{nameof(candidatePairStats.state)}: {candidatePairStats.state}"));
                container.Add(new Label($"{nameof(candidatePairStats.priority)}: {candidatePairStats.priority}"));
                container.Add(new Label($"{nameof(candidatePairStats.nominated)}: {candidatePairStats.nominated}"));
                container.Add(new Label($"{nameof(candidatePairStats.writable)}: {candidatePairStats.writable}"));
                container.Add(new Label($"{nameof(candidatePairStats.readable)}: {candidatePairStats.readable}"));
                container.Add(new Label($"{nameof(candidatePairStats.bytesSent)}: {candidatePairStats.bytesSent}"));
                container.Add(
                    new Label($"{nameof(candidatePairStats.bytesReceived)}: {candidatePairStats.bytesReceived}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.totalRoundTripTime)}: {candidatePairStats.totalRoundTripTime}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.currentRoundTripTime)}: {candidatePairStats.currentRoundTripTime}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.availableOutgoingBitrate)}: {candidatePairStats.availableOutgoingBitrate}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.availableIncomingBitrate)}: {candidatePairStats.availableIncomingBitrate}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.requestsReceived)}: {candidatePairStats.requestsReceived}"));
                container.Add(
                    new Label($"{nameof(candidatePairStats.requestsSent)}: {candidatePairStats.requestsSent}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.responsesReceived)}: {candidatePairStats.responsesReceived}"));
                container.Add(
                    new Label($"{nameof(candidatePairStats.responsesSent)}: {candidatePairStats.responsesSent}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.retransmissionsReceived)}: {candidatePairStats.retransmissionsReceived}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.retransmissionsSent)}: {candidatePairStats.retransmissionsSent}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.consentRequestsReceived)}: {candidatePairStats.consentRequestsReceived}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.consentRequestsSent)}: {candidatePairStats.consentRequestsSent}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.consentResponsesReceived)}: {candidatePairStats.consentResponsesReceived}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.consentResponsesSent)}: {candidatePairStats.consentResponsesSent}"));

                graphView.AddInput(candidatePairStats);
            };

            root.Add(graphView.Create());

            return root;
        }

        private VisualElement CreateLocalCandidateView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.LocalCandidate}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.LocalCandidate, out var stats) ||
                    !(stats is RTCIceCandidateStats candidateStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.LocalCandidate}"));
                    return;
                }

                container.Add(new Label($"{nameof(candidateStats.Id)}: {candidateStats.Id}"));
                container.Add(new Label($"{nameof(candidateStats.Timestamp)}: {candidateStats.Timestamp}"));
                container.Add(new Label($"{nameof(candidateStats.transportId)}: {candidateStats.transportId}"));
                container.Add(new Label($"{nameof(candidateStats.isRemote)}: {candidateStats.isRemote}"));
                container.Add(new Label($"{nameof(candidateStats.networkType)}: {candidateStats.networkType}"));
                container.Add(new Label($"{nameof(candidateStats.ip)}: {candidateStats.ip}"));
                container.Add(new Label($"{nameof(candidateStats.port)}: {candidateStats.port}"));
                container.Add(new Label($"{nameof(candidateStats.protocol)}: {candidateStats.protocol}"));
                container.Add(new Label($"{nameof(candidateStats.relayProtocol)}: {candidateStats.relayProtocol}"));
                container.Add(new Label($"{nameof(candidateStats.candidateType)}: {candidateStats.candidateType}"));
                container.Add(new Label($"{nameof(candidateStats.priority)}: {candidateStats.priority}"));
                container.Add(new Label($"{nameof(candidateStats.url)}: {candidateStats.url}"));
                container.Add(new Label($"{nameof(candidateStats.deleted)}: {candidateStats.deleted}"));
            };
            return root;
        }

        private VisualElement CreateRemoteCandidateView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.RemoteCandidate}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.RemoteCandidate, out var stats) ||
                    !(stats is RTCIceCandidateStats candidateStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.RemoteCandidate}"));
                    return;
                }

                container.Add(new Label($"{nameof(candidateStats.Id)}: {candidateStats.Id}"));
                container.Add(new Label($"{nameof(candidateStats.Timestamp)}: {candidateStats.Timestamp}"));
                container.Add(new Label($"{nameof(candidateStats.transportId)}: {candidateStats.transportId}"));
                container.Add(new Label($"{nameof(candidateStats.isRemote)}: {candidateStats.isRemote}"));
                container.Add(new Label($"{nameof(candidateStats.networkType)}: {candidateStats.networkType}"));
                container.Add(new Label($"{nameof(candidateStats.ip)}: {candidateStats.ip}"));
                container.Add(new Label($"{nameof(candidateStats.port)}: {candidateStats.port}"));
                container.Add(new Label($"{nameof(candidateStats.protocol)}: {candidateStats.protocol}"));
                container.Add(new Label($"{nameof(candidateStats.relayProtocol)}: {candidateStats.relayProtocol}"));
                container.Add(new Label($"{nameof(candidateStats.candidateType)}: {candidateStats.candidateType}"));
                container.Add(new Label($"{nameof(candidateStats.priority)}: {candidateStats.priority}"));
                container.Add(new Label($"{nameof(candidateStats.url)}: {candidateStats.url}"));
                container.Add(new Label($"{nameof(candidateStats.deleted)}: {candidateStats.deleted}"));
            };
            return root;
        }

        private VisualElement CreateCertificateView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.Certificate}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.Certificate, out var stats) ||
                    !(stats is RTCCertificateStats certificateStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Certificate}"));
                    return;
                }

                container.Add(new Label($"{nameof(certificateStats.Id)}: {certificateStats.Id}"));
                container.Add(new Label($"{nameof(certificateStats.Timestamp)}: {certificateStats.Timestamp}"));
                container.Add(new Label($"{nameof(certificateStats.fingerprint)}: {certificateStats.fingerprint}"));
                container.Add(new Label($"{nameof(certificateStats.fingerprintAlgorithm)}: {certificateStats.fingerprintAlgorithm}"));
                container.Add(new Label($"{nameof(certificateStats.base64Certificate)}: {certificateStats.base64Certificate}"));
                container.Add(new Label($"{nameof(certificateStats.issuerCertificateId)}: {certificateStats.issuerCertificateId}"));
            };
            return root;
        }

        private VisualElement CreateIceServerView()
        {
            var root = new VisualElement();
            root.Add(new Label($"{RTCStatsType.IceServer}"));

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.Stats.TryGetValue(RTCStatsType.IceServer, out var stats) ||
                    !(stats is RTCCertificateStats outboundStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.IceServer}"));
                    return;
                }

                container.Add(new Label($"{nameof(outboundStats.Id)}: {outboundStats.Id}"));
                container.Add(new Label($"{nameof(outboundStats.Timestamp)}: {outboundStats.Timestamp}"));
            };
            return root;
        }
    }
}
