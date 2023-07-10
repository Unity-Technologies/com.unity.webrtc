using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class TransportGraphView
    {
        private GraphView bytesSentGraph = new GraphView("bytesSent");
        private GraphView bytesReceivedGraph = new GraphView("bytesReceived");
        private GraphView selectedCandidatePairChangesGraph = new GraphView("selectedCandidatePairChanges");

        public void AddInput(RTCTransportStats input)
        {
            var timestamp = input.UtcTimeStamp;
            bytesSentGraph.AddInput(timestamp, input.bytesSent);
            bytesReceivedGraph.AddInput(timestamp, input.bytesReceived);
            selectedCandidatePairChangesGraph.AddInput(timestamp, input.selectedCandidatePairChanges);
        }

        public VisualElement Create()
        {
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            container.Add(bytesSentGraph.Create());
            container.Add(bytesReceivedGraph.Create());
            container.Add(selectedCandidatePairChangesGraph.Create());
            return container;
        }
    }
}
