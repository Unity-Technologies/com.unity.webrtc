using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class PeerStatsView
    {
        private readonly WebRTCStats m_parent;
        private readonly RTCPeerConnection m_peerConnection;
        private int m_lastUpdateStatsCount = 0;

        public PeerStatsView(RTCPeerConnection peer, WebRTCStats parent)
        {
            m_peerConnection = peer;
            m_parent = parent;
        }


        public VisualElement Create()
        {
            var root = new ScrollView();

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection || report.Stats.Keys.Count == m_lastUpdateStatsCount)
                {
                    return;
                }

                m_lastUpdateStatsCount = report.Stats.Count;
                root.Clear();

                var container = new VisualElement();

                var popup = new PopupField<int>(Enumerable.Range(0, report.Stats.Count).ToList(), 0,
                    index => $"{report.Stats.ElementAt(index).Value.Type}:{report.Stats.ElementAt(index).Value.Id}",
                    index => $"{report.Stats.ElementAt(index).Value.Type}:{report.Stats.ElementAt(index).Value.Id}");

                root.Add(popup);
                root.Add(container);

                popup.RegisterValueChangedCallback(e =>
                {
                    container.Clear();
                    var id = report.Stats.ElementAt(e.newValue).Key;
                    var type = report.Get(id).Type;
                    switch (type)
                    {
                        case RTCStatsType.Codec:
                            container.Add(CreateCodecView(id));
                            break;
                        case RTCStatsType.InboundRtp:
                            container.Add(CreateInboundRtpView(id));
                            break;
                        case RTCStatsType.OutboundRtp:
                            container.Add(CreateOutboundRtpView(id));
                            break;
                        case RTCStatsType.RemoteInboundRtp:
                            container.Add(CreateRemoteInboundRtpView(id));
                            break;
                        case RTCStatsType.RemoteOutboundRtp:
                            container.Add(CreateRemoteOutboundRtpView(id));
                            break;
                        case RTCStatsType.MediaSource:
                            container.Add(CreateMediaSourceView(id));
                            break;
                        case RTCStatsType.Csrc:
                            container.Add(CreateCsrcView(id));
                            break;
                        case RTCStatsType.PeerConnection:
                            container.Add(CreatePeerConnectionView(id));
                            break;
                        case RTCStatsType.DataChannel:
                            container.Add(CreateDataChannelView(id));
                            break;
#pragma warning disable 0612
                        case RTCStatsType.Stream:
                            container.Add(CreateStreamView(id));
                            break;
                        case RTCStatsType.Track:
                            container.Add(CreateTrackView(id));
                            break;
#pragma warning restore 0612
                        case RTCStatsType.Transceiver:
                            container.Add(CreateTransceiverView(id));
                            break;
                        case RTCStatsType.Sender:
                            container.Add(CreateSenderView(id));
                            break;
                        case RTCStatsType.Receiver:
                            container.Add(CreateReceiverView(id));
                            break;
                        case RTCStatsType.Transport:
                            container.Add(CreateTransportView(id));
                            break;
                        case RTCStatsType.SctpTransport:
                            container.Add(CreateSctpTransportView(id));
                            break;
                        case RTCStatsType.CandidatePair:
                            container.Add(CreateCandidatePairView(id));
                            break;
                        case RTCStatsType.LocalCandidate:
                            container.Add(CreateLocalCandidateView(id));
                            break;
                        case RTCStatsType.RemoteCandidate:
                            container.Add(CreateRemoteCandidateView(id));
                            break;
                        case RTCStatsType.Certificate:
                            container.Add(CreateCertificateView(id));
                            break;
                        case RTCStatsType.IceServer:
                            container.Add(CreateIceServerView(id));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException($"this type is not supported : {type}");
                    }
                });
            };

            return root;
        }

        private VisualElement CreateCodecView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

        private VisualElement CreateInboundRtpView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            var inboundGraph = new InboundRTPStreamGraphView();

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCInboundRTPStreamStats inboundStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.InboundRtp}"));
                    return;
                }

                container.Add(new Label($"{nameof(inboundStats.Id)}: {inboundStats.Id}"));
                container.Add(new Label($"{nameof(inboundStats.Timestamp)}: {inboundStats.Timestamp}"));
                container.Add(new Label($"{nameof(inboundStats.ssrc)}: {inboundStats.ssrc}"));
                container.Add(new Label($"{nameof(inboundStats.estimatedPlayoutTimestamp)}: {inboundStats.estimatedPlayoutTimestamp}"));
                container.Add(new Label($"{nameof(inboundStats.kind)}: {inboundStats.kind}"));
                container.Add(new Label($"{nameof(inboundStats.transportId)}: {inboundStats.transportId}"));
                container.Add(new Label($"{nameof(inboundStats.codecId)}: {inboundStats.codecId}"));
                container.Add(new Label($"{nameof(inboundStats.firCount)}: {inboundStats.firCount}"));
                container.Add(new Label($"{nameof(inboundStats.pliCount)}: {inboundStats.pliCount}"));
                container.Add(new Label($"{nameof(inboundStats.nackCount)}: {inboundStats.nackCount}"));
                container.Add(new Label($"{nameof(inboundStats.qpSum)}: {inboundStats.qpSum}"));
                container.Add(new Label($"{nameof(inboundStats.packetsReceived)}: {inboundStats.packetsReceived}"));
                container.Add(new Label($"{nameof(inboundStats.bytesReceived)}: {inboundStats.bytesReceived}"));
                container.Add(
                    new Label($"{nameof(inboundStats.headerBytesReceived)}: {inboundStats.headerBytesReceived}"));
                container.Add(new Label($"{nameof(inboundStats.packetsLost)}: {inboundStats.packetsLost}"));
                container.Add(new Label(
                    $"{nameof(inboundStats.lastPacketReceivedTimestamp)}: {inboundStats.lastPacketReceivedTimestamp}"));
                container.Add(new Label($"{nameof(inboundStats.jitter)}: {inboundStats.jitter}"));
                container.Add(new Label($"{nameof(inboundStats.packetsDiscarded)}: {inboundStats.packetsDiscarded}"));
                container.Add(new Label($"{nameof(inboundStats.framesDecoded)}: {inboundStats.framesDecoded}"));
                container.Add(new Label($"{nameof(inboundStats.keyFramesDecoded)}: {inboundStats.keyFramesDecoded}"));
                container.Add(new Label($"{nameof(inboundStats.totalDecodeTime)}: {inboundStats.totalDecodeTime}"));
                container.Add(new Label($"{nameof(inboundStats.contentType)}: {inboundStats.contentType}"));
                container.Add(
                    new Label($"{nameof(inboundStats.decoderImplementation)}: {inboundStats.decoderImplementation}"));

                inboundGraph.AddInput(inboundStats);
            };

            root.Add(inboundGraph.Create());

            return root;
        }

        private VisualElement CreateOutboundRtpView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            var outboundGraph = new OutboundRTPStreamGraphView();

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCOutboundRTPStreamStats outboundStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.OutboundRtp}"));
                    return;
                }

                container.Add(new Label($"{nameof(outboundStats.Id)}: {outboundStats.Id}"));
                container.Add(new Label($"{nameof(outboundStats.Timestamp)}: {outboundStats.Timestamp}"));
                container.Add(new Label($"{nameof(outboundStats.ssrc)}: {outboundStats.ssrc}"));
                container.Add(new Label($"{nameof(outboundStats.kind)}: {outboundStats.kind}"));
                container.Add(new Label($"{nameof(outboundStats.transportId)}: {outboundStats.transportId}"));
                container.Add(new Label($"{nameof(outboundStats.codecId)}: {outboundStats.codecId}"));
                container.Add(new Label($"{nameof(outboundStats.firCount)}: {outboundStats.firCount}"));
                container.Add(new Label($"{nameof(outboundStats.pliCount)}: {outboundStats.pliCount}"));
                container.Add(new Label($"{nameof(outboundStats.nackCount)}: {outboundStats.nackCount}"));
                container.Add(new Label($"{nameof(outboundStats.qpSum)}: {outboundStats.qpSum}"));
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

                outboundGraph.AddInput(outboundStats);
            };

            root.Add(outboundGraph.Create());

            return root;
        }

        private VisualElement CreateRemoteInboundRtpView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

        private VisualElement CreateRemoteOutboundRtpView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

        private VisualElement CreateMediaSourceView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            var sourceGraph = new MediaSourceGraphView();
            var graphView = sourceGraph.Create();

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

                if (mediaSourceStats is RTCVideoSourceStats videoSourceStats)
                {
                    container.Add(new Label($"{nameof(videoSourceStats.width)}: {videoSourceStats.width}"));
                    container.Add(new Label($"{nameof(videoSourceStats.height)}: {videoSourceStats.height}"));
                    container.Add(new Label($"{nameof(videoSourceStats.frames)}: {videoSourceStats.frames}"));
                    container.Add(
                        new Label($"{nameof(videoSourceStats.framesPerSecond)}: {videoSourceStats.framesPerSecond}"));
                    sourceGraph.AddInput(videoSourceStats);
                    graphView.visible = true;
                }

                if (mediaSourceStats is RTCAudioSourceStats audioSourceStats)
                {
                    container.Add(new Label($"{nameof(audioSourceStats.audioLevel)}: {audioSourceStats.audioLevel}"));
                    container.Add(new Label($"{nameof(audioSourceStats.totalAudioEnergy)}: {audioSourceStats.totalAudioEnergy}"));
                    container.Add(new Label($"{nameof(audioSourceStats.totalSamplesDuration)}: {audioSourceStats.totalSamplesDuration}"));
                    sourceGraph.AddInput(audioSourceStats);
                    graphView.visible = true;
                }
            };

            root.Add(graphView);

            return root;
        }

        private VisualElement CreateCsrcView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

        private VisualElement CreatePeerConnectionView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

        private VisualElement CreateDataChannelView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            var dataChannelGraph = new DataChannelGraphView();

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCDataChannelStats dataChannelStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.DataChannel}"));
                    return;
                }

                container.Add(new Label($"{nameof(dataChannelStats.Id)}: {dataChannelStats.Id}"));
                container.Add(new Label($"{nameof(dataChannelStats.Timestamp)}: {dataChannelStats.Timestamp}"));
                container.Add(new Label($"{nameof(dataChannelStats.label)}: {dataChannelStats.label}"));
                container.Add(new Label($"{nameof(dataChannelStats.protocol)}: {dataChannelStats.protocol}"));
                container.Add(new Label($"{nameof(dataChannelStats.dataChannelIdentifier)}: {dataChannelStats.dataChannelIdentifier}"));
                container.Add(new Label($"{nameof(dataChannelStats.state)}: {dataChannelStats.state}"));
                container.Add(new Label($"{nameof(dataChannelStats.messagesSent)}: {dataChannelStats.messagesSent}"));
                container.Add(new Label($"{nameof(dataChannelStats.bytesSent)}: {dataChannelStats.bytesSent}"));
                container.Add(
                    new Label($"{nameof(dataChannelStats.messagesReceived)}: {dataChannelStats.messagesReceived}"));
                container.Add(new Label($"{nameof(dataChannelStats.bytesReceived)}: {dataChannelStats.bytesReceived}"));

                dataChannelGraph.AddInput(dataChannelStats);
            };

            root.Add(dataChannelGraph.Create());

            return root;
        }

        private VisualElement CreateStreamView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();

#pragma warning disable 0612
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCMediaStreamStats streamStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Stream}"));
                    return;
                }
#pragma warning restore 0612

                container.Add(new Label($"{nameof(streamStats.Id)}: {streamStats.Id}"));
                container.Add(new Label($"{nameof(streamStats.Timestamp)}: {streamStats.Timestamp}"));
                container.Add(new Label($"{nameof(streamStats.streamIdentifier)}: {streamStats.streamIdentifier}"));
                container.Add(
                    new Label($"{nameof(streamStats.trackIds)}: {string.Join(",", streamStats.trackIds)}"));
            };
            return root;
        }

        private VisualElement CreateTrackView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

#pragma warning disable 0612
            var trackGraph = new MediaStreamTrackGraphView();
#pragma warning restore 0612

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
#pragma warning disable 0612
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCMediaStreamTrackStats trackStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Track}"));
                    return;
                }
#pragma warning restore 0612

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

                trackGraph.AddInput(trackStats);
            };

            root.Add(trackGraph.Create());

            return root;
        }

        private VisualElement CreateTransceiverView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

        private VisualElement CreateSenderView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

        private VisualElement CreateReceiverView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCReceiverStats receiverStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Receiver}"));
                    return;
                }

                container.Add(new Label($"{nameof(receiverStats.Id)}: {receiverStats.Id}"));
                container.Add(new Label($"{nameof(receiverStats.Timestamp)}: {receiverStats.Timestamp}"));
            };
            return root;
        }

        private VisualElement CreateTransportView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            var transportGraph = new TransportGraphView();

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

                transportGraph.AddInput(transportStats);
            };

            root.Add(transportGraph.Create());

            return root;
        }

        private VisualElement CreateSctpTransportView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            var transportGraph = new TransportGraphView();

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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

                transportGraph.AddInput(transportStats);
            };

            root.Add(transportGraph.Create());

            return root;
        }

        private VisualElement CreateCandidatePairView(string id)
        {
            var root = new VisualElement();
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
                if (!report.TryGetValue(id, out var stats) ||
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
                container.Add(new Label($"{nameof(candidatePairStats.nominated)}: {candidatePairStats.nominated}"));
                container.Add(new Label($"{nameof(candidatePairStats.bytesSent)}: {candidatePairStats.bytesSent}"));
                container.Add(
                    new Label($"{nameof(candidatePairStats.bytesReceived)}: {candidatePairStats.bytesReceived}"));
                container.Add(
                    new Label($"{nameof(candidatePairStats.lastPacketSentTimestamp)}: {candidatePairStats.lastPacketSentTimestamp}"));
                container.Add(
                    new Label($"{nameof(candidatePairStats.lastPacketReceivedTimestamp)}: {candidatePairStats.lastPacketReceivedTimestamp}"));
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
                    $"{nameof(candidatePairStats.consentRequestsSent)}: {candidatePairStats.consentRequestsSent}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.packetsDiscardedOnSend)}: {candidatePairStats.packetsDiscardedOnSend}"));
                container.Add(new Label(
                    $"{nameof(candidatePairStats.bytesDiscardedOnSend)}: {candidatePairStats.bytesDiscardedOnSend}"));

                graphView.AddInput(candidatePairStats);
            };

            root.Add(graphView.Create());

            return root;
        }

        private VisualElement CreateLocalCandidateView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCIceCandidateStats candidateStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.LocalCandidate}"));
                    return;
                }

                container.Add(new Label($"{nameof(candidateStats.Id)}: {candidateStats.Id}"));
                container.Add(new Label($"{nameof(candidateStats.Timestamp)}: {candidateStats.Timestamp}"));
                container.Add(new Label($"{nameof(candidateStats.transportId)}: {candidateStats.transportId}"));
                container.Add(new Label($"{nameof(candidateStats.networkType)}: {candidateStats.networkType}"));
                container.Add(new Label($"{nameof(candidateStats.ip)}: {candidateStats.ip}"));
                container.Add(new Label($"{nameof(candidateStats.address)}: {candidateStats.address}"));
                container.Add(new Label($"{nameof(candidateStats.port)}: {candidateStats.port}"));
                container.Add(new Label($"{nameof(candidateStats.protocol)}: {candidateStats.protocol}"));
                container.Add(new Label($"{nameof(candidateStats.relayProtocol)}: {candidateStats.relayProtocol}"));
                container.Add(new Label($"{nameof(candidateStats.candidateType)}: {candidateStats.candidateType}"));
                container.Add(new Label($"{nameof(candidateStats.priority)}: {candidateStats.priority}"));
                container.Add(new Label($"{nameof(candidateStats.url)}: {candidateStats.url}"));
                container.Add(new Label($"{nameof(candidateStats.foundation)}: {candidateStats.foundation}"));
                container.Add(new Label($"{nameof(candidateStats.relatedAddress)}: {candidateStats.relatedAddress}"));
                container.Add(new Label($"{nameof(candidateStats.relatedPort)}: {candidateStats.relatedPort}"));
                container.Add(new Label($"{nameof(candidateStats.usernameFragment)}: {candidateStats.usernameFragment}"));
                container.Add(new Label($"{nameof(candidateStats.tcpType)}: {candidateStats.tcpType}"));
                container.Add(new Label($"{nameof(candidateStats.vpn)}: {candidateStats.vpn}"));
                container.Add(new Label($"{nameof(candidateStats.networkAdapterType)}: {candidateStats.networkAdapterType}"));
            };
            return root;
        }

        private VisualElement CreateRemoteCandidateView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCIceCandidateStats candidateStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.RemoteCandidate}"));
                    return;
                }

                container.Add(new Label($"{nameof(candidateStats.Id)}: {candidateStats.Id}"));
                container.Add(new Label($"{nameof(candidateStats.Timestamp)}: {candidateStats.Timestamp}"));
                container.Add(new Label($"{nameof(candidateStats.transportId)}: {candidateStats.transportId}"));
                container.Add(new Label($"{nameof(candidateStats.networkType)}: {candidateStats.networkType}"));
                container.Add(new Label($"{nameof(candidateStats.ip)}: {candidateStats.ip}"));
                container.Add(new Label($"{nameof(candidateStats.address)}: {candidateStats.address}"));
                container.Add(new Label($"{nameof(candidateStats.port)}: {candidateStats.port}"));
                container.Add(new Label($"{nameof(candidateStats.protocol)}: {candidateStats.protocol}"));
                container.Add(new Label($"{nameof(candidateStats.relayProtocol)}: {candidateStats.relayProtocol}"));
                container.Add(new Label($"{nameof(candidateStats.candidateType)}: {candidateStats.candidateType}"));
                container.Add(new Label($"{nameof(candidateStats.priority)}: {candidateStats.priority}"));
                container.Add(new Label($"{nameof(candidateStats.url)}: {candidateStats.url}"));
                container.Add(new Label($"{nameof(candidateStats.foundation)}: {candidateStats.foundation}"));
                container.Add(new Label($"{nameof(candidateStats.relatedAddress)}: {candidateStats.relatedAddress}"));
                container.Add(new Label($"{nameof(candidateStats.relatedPort)}: {candidateStats.relatedPort}"));
                container.Add(new Label($"{nameof(candidateStats.usernameFragment)}: {candidateStats.usernameFragment}"));
                container.Add(new Label($"{nameof(candidateStats.tcpType)}: {candidateStats.tcpType}"));
                container.Add(new Label($"{nameof(candidateStats.vpn)}: {candidateStats.vpn}"));
                container.Add(new Label($"{nameof(candidateStats.networkAdapterType)}: {candidateStats.networkAdapterType}"));
            };
            return root;
        }

        private VisualElement CreateCertificateView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
                    !(stats is RTCCertificateStats certificateStats))
                {
                    container.Add(new Label($"no stats report about {RTCStatsType.Certificate}"));
                    return;
                }

                container.Add(new Label($"{nameof(certificateStats.Id)}: {certificateStats.Id}"));
                container.Add(new Label($"{nameof(certificateStats.Timestamp)}: {certificateStats.Timestamp}"));
                container.Add(new Label($"{nameof(certificateStats.fingerprint)}: {certificateStats.fingerprint}"));
                container.Add(new Label(
                    $"{nameof(certificateStats.fingerprintAlgorithm)}: {certificateStats.fingerprintAlgorithm}"));
                container.Add(
                    new Label($"{nameof(certificateStats.base64Certificate)}: {certificateStats.base64Certificate}"));
                container.Add(new Label(
                    $"{nameof(certificateStats.issuerCertificateId)}: {certificateStats.issuerCertificateId}"));
            };
            return root;
        }

        private VisualElement CreateIceServerView(string id)
        {
            var root = new VisualElement();
            var container = new VisualElement();
            root.Add(container);

            m_parent.OnStats += (peer, report) =>
            {
                if (peer != m_peerConnection)
                {
                    return;
                }

                container.Clear();
                if (!report.TryGetValue(id, out var stats) ||
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
