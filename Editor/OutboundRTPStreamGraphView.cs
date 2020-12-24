using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class OutboundRTPStreamGraphView
    {
        private GraphView firCountGraph = new GraphView("firCount");
        private GraphView pliCountGraph = new GraphView("pliCount");
        private GraphView nackCountGraph = new GraphView("nackCount");
        private GraphView sliCountGraph = new GraphView("sliCount");
        private GraphView qpSumGraph = new GraphView("qpSum");
        private GraphView packetsSentGraph = new GraphView("packetsSent");
        private GraphView retransmittedPacketsSentGraph = new GraphView("retransmittedPacketsSent");
        private GraphView bytesSentGraph = new GraphView("bytesSent");
        private GraphView headerBytesSentGraph = new GraphView("headerBytesSent");
        private GraphView retransmittedBytesSentGraph = new GraphView("retransmittedBytesSent");
        private GraphView framesDecodedGraph = new GraphView("framesDecoded");
        private GraphView keyFramesDecodedGraph = new GraphView("keyFramesDecoded");
        private GraphView targetBitrateGraph = new GraphView("targetBitrate");
        private GraphView totalEncodeTimeGraph = new GraphView("totalEncodeTime");
        private GraphView totalEncodedBytesTargetGraph = new GraphView("totalEncodedBytesTarget");
        private GraphView totalPacketSendDelayGraph = new GraphView("totalPacketSendDelay");

        public void AddInput(RTCOutboundRTPStreamStats input)
        {
            var timestamp = input.UtcTimeStamp;
            firCountGraph.AddInput(timestamp, input.firCount);
            pliCountGraph.AddInput(timestamp, input.pliCount);
            nackCountGraph.AddInput(timestamp, input.nackCount);
            sliCountGraph.AddInput(timestamp, input.sliCount);
            qpSumGraph.AddInput(timestamp, input.qpSum);
            packetsSentGraph.AddInput(timestamp, input.packetsSent);
            retransmittedPacketsSentGraph.AddInput(timestamp, input.retransmittedPacketsSent);
            bytesSentGraph.AddInput(timestamp, input.bytesSent);
            headerBytesSentGraph.AddInput(timestamp, input.headerBytesSent);
            retransmittedBytesSentGraph.AddInput(timestamp, input.retransmittedBytesSent);
            targetBitrateGraph.AddInput(timestamp, (float)input.targetBitrate);
            framesDecodedGraph.AddInput(timestamp, input.framesEncoded);
            keyFramesDecodedGraph.AddInput(timestamp, input.keyFramesEncoded);
            totalEncodeTimeGraph.AddInput(timestamp, (float)input.totalEncodeTime);
            totalEncodedBytesTargetGraph.AddInput(timestamp, input.totalEncodedBytesTarget);
            totalPacketSendDelayGraph.AddInput(timestamp, (float)input.totalPacketSendDelay);
        }

        public VisualElement Create()
        {
            var container = new VisualElement();
            container.Add(firCountGraph.Create());
            container.Add(pliCountGraph.Create());
            container.Add(nackCountGraph.Create());
            container.Add(sliCountGraph.Create());
            container.Add(qpSumGraph.Create());
            container.Add(packetsSentGraph.Create());
            container.Add(retransmittedPacketsSentGraph.Create());
            container.Add(bytesSentGraph.Create());
            container.Add(headerBytesSentGraph.Create());
            container.Add(retransmittedBytesSentGraph.Create());
            container.Add(framesDecodedGraph.Create());
            container.Add(keyFramesDecodedGraph.Create());
            container.Add(targetBitrateGraph.Create());
            container.Add(totalEncodeTimeGraph.Create());
            container.Add(totalEncodedBytesTargetGraph.Create());
            container.Add(totalPacketSendDelayGraph.Create());
            return container;
        }
    }
}
