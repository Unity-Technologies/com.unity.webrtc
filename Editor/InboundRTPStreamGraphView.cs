using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class InboundRTPStreamGraphView
    {
        private GraphView firCountGraph = new GraphView("firCount");
        private GraphView pliCountGraph = new GraphView("pliCount");
        private GraphView nackCountGraph = new GraphView("nackCount");
        private GraphView sliCountGraph = new GraphView("sliCount");
        private GraphView qpSumGraph = new GraphView("qpSum");
        private GraphView packetsReceivedGraph = new GraphView("packetsReceived");
        private GraphView bytesReceivedGraph = new GraphView("bytesReceived");
        private GraphView headerBytesReceivedGraph = new GraphView("headerBytesReceived");
        private GraphView packetsLostGraph = new GraphView("packetsLost");
        private GraphView jitterGraph = new GraphView("jitter");
        private GraphView packetsDiscardedGraph = new GraphView("packetsDiscarded");
        private GraphView packetsRepairedGraph = new GraphView("packetsRepaired");
        private GraphView burstPacketsLostGraph = new GraphView("burstPacketsLost");
        private GraphView burstPacketsDiscardedGraph = new GraphView("burstPacketsDiscarded");
        private GraphView burstLossCountGraph = new GraphView("burstLossCount");
        private GraphView burstDiscardCountGraph = new GraphView("burstDiscardCount");
        private GraphView burstLossRateGraph = new GraphView("burstLossRate");
        private GraphView burstDiscardRateGraph = new GraphView("burstDiscardRate");
        private GraphView gapLossRateGraph = new GraphView("gapLossRate");
        private GraphView gapDiscardRateGraph = new GraphView("gapDiscardRate");
        private GraphView framesDecodedGraph = new GraphView("framesDecoded");
        private GraphView keyFramesDecodedGraph = new GraphView("keyFramesDecoded");

        public void AddInput(RTCInboundRTPStreamStats input)
        {
            var timestamp = input.UtcTimeStamp;
            firCountGraph.AddInput(timestamp, input.firCount);
            pliCountGraph.AddInput(timestamp, input.pliCount);
            nackCountGraph.AddInput(timestamp, input.nackCount);
            sliCountGraph.AddInput(timestamp, input.sliCount);
            qpSumGraph.AddInput(timestamp, input.qpSum);
            packetsReceivedGraph.AddInput(timestamp, input.packetsReceived);
            bytesReceivedGraph.AddInput(timestamp, input.bytesReceived);
            headerBytesReceivedGraph.AddInput(timestamp, input.headerBytesReceived);
            packetsLostGraph.AddInput(timestamp, input.packetsLost);
            jitterGraph.AddInput(timestamp, (float)input.jitter);
            packetsDiscardedGraph.AddInput(timestamp, input.packetsDiscarded);
            packetsRepairedGraph.AddInput(timestamp, input.packetsRepaired);
            burstPacketsLostGraph.AddInput(timestamp, input.burstPacketsLost);
            burstPacketsDiscardedGraph.AddInput(timestamp, input.burstPacketsDiscarded);
            burstLossCountGraph.AddInput(timestamp, input.burstLossCount);
            burstDiscardCountGraph.AddInput(timestamp, input.burstDiscardCount);
            burstLossRateGraph.AddInput(timestamp, (float)input.burstLossRate);
            burstDiscardRateGraph.AddInput(timestamp, (float)input.burstDiscardRate);
            gapLossRateGraph.AddInput(timestamp, (float)input.gapLossRate);
            gapDiscardRateGraph.AddInput(timestamp, (float)input.gapDiscardRate);
            framesDecodedGraph.AddInput(timestamp, input.framesDecoded);
            keyFramesDecodedGraph.AddInput(timestamp, input.keyFramesDecoded);
        }

        public VisualElement Create()
        {
            var container = new VisualElement {style = {flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap}};
            container.Add(firCountGraph.Create());
            container.Add(pliCountGraph.Create());
            container.Add(nackCountGraph.Create());
            container.Add(sliCountGraph.Create());
            container.Add(qpSumGraph.Create());
            container.Add(packetsReceivedGraph.Create());
            container.Add(bytesReceivedGraph.Create());
            container.Add(headerBytesReceivedGraph.Create());
            container.Add(packetsLostGraph.Create());
            container.Add(jitterGraph.Create());
            container.Add(packetsDiscardedGraph.Create());
            container.Add(packetsRepairedGraph.Create());
            container.Add(burstPacketsLostGraph.Create());
            container.Add(burstPacketsDiscardedGraph.Create());
            container.Add(burstLossCountGraph.Create());
            container.Add(burstDiscardCountGraph.Create());
            container.Add(burstLossRateGraph.Create());
            container.Add(burstDiscardRateGraph.Create());
            container.Add(gapLossRateGraph.Create());
            container.Add(gapDiscardRateGraph.Create());
            container.Add(framesDecodedGraph.Create());
            container.Add(keyFramesDecodedGraph.Create());
            return container;
        }
    }
}
