using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class CandidatePairGraphView
    {
        private GraphView bytesSentGraph = new GraphView("bytesSent");
        private GraphView bytesReceivedGraph = new GraphView("bytesReceived");
        private GraphView totalRoundTripTimeGraph = new GraphView("totalRoundTripTime");
        private GraphView currentRoundTripTimeGraph = new GraphView("currentRoundTripTime");
        private GraphView availableOutgoingBitrateGraph = new GraphView("availableOutgoingBitrate");
        private GraphView availableIncomingBitrateGraph = new GraphView("availableIncomingBitrate");
        private GraphView requestsReceivedGraph = new GraphView("requestsReceived");
        private GraphView requestsSentGraph = new GraphView("requestsSent");
        private GraphView responsesReceivedGraph = new GraphView("responsesReceived");
        private GraphView responsesSentGraph = new GraphView("responsesSent");
        private GraphView retransmissionsReceivedGraph = new GraphView("retransmissionsReceived");
        private GraphView retransmissionsSentGraph = new GraphView("retransmissionsSent");
        private GraphView consentRequestsReceivedGraph = new GraphView("consentRequestsReceived");
        private GraphView consentRequestsSentGraph = new GraphView("consentRequestsSent");
        private GraphView consentResponsesReceivedGraph = new GraphView("consentResponsesReceived");
        private GraphView consentResponsesSentGraph = new GraphView("consentResponsesSent");

        public void AddInput(RTCIceCandidatePairStats input)
        {
            var timestamp = input.UtcTimeStamp;
            bytesSentGraph.AddInput(timestamp, input.bytesSent);
            bytesReceivedGraph.AddInput(timestamp, input.bytesReceived);
            totalRoundTripTimeGraph.AddInput(timestamp, (float)input.totalRoundTripTime);
            currentRoundTripTimeGraph.AddInput(timestamp, (float)input.currentRoundTripTime);
            availableOutgoingBitrateGraph.AddInput(timestamp, (float)input.availableOutgoingBitrate);
            availableIncomingBitrateGraph.AddInput(timestamp, (float)input.availableIncomingBitrate);
            requestsReceivedGraph.AddInput(timestamp, input.requestsReceived);
            requestsSentGraph.AddInput(timestamp, input.requestsSent);
            responsesReceivedGraph.AddInput(timestamp, input.responsesReceived);
            responsesSentGraph.AddInput(timestamp, input.responsesSent);
            retransmissionsReceivedGraph.AddInput(timestamp, input.consentRequestsSent);
            retransmissionsSentGraph.AddInput(timestamp, input.packetsDiscardedOnSend);
            consentRequestsReceivedGraph.AddInput(timestamp, input.bytesDiscardedOnSend);
        }

        public VisualElement Create()
        {
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            container.Add(bytesSentGraph.Create());
            container.Add(bytesReceivedGraph.Create());
            container.Add(totalRoundTripTimeGraph.Create());
            container.Add(currentRoundTripTimeGraph.Create());
            container.Add(availableOutgoingBitrateGraph.Create());
            container.Add(availableIncomingBitrateGraph.Create());
            container.Add(requestsReceivedGraph.Create());
            container.Add(requestsSentGraph.Create());
            container.Add(responsesReceivedGraph.Create());
            container.Add(responsesSentGraph.Create());
            container.Add(retransmissionsReceivedGraph.Create());
            container.Add(retransmissionsSentGraph.Create());
            container.Add(consentRequestsReceivedGraph.Create());
            container.Add(consentRequestsSentGraph.Create());
            container.Add(consentResponsesReceivedGraph.Create());
            container.Add(consentResponsesSentGraph.Create());
            return container;
        }
    }
}
