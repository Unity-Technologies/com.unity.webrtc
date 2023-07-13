using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class DataChannelGraphView
    {
        private GraphView messageSentGraph = new GraphView("messageSent");
        private GraphView bytesSentGraph = new GraphView("bytesSent");
        private GraphView messageReceivedGraph = new GraphView("messageReceived");
        private GraphView bytesReceivedGraph = new GraphView("bytesReceived");

        public void AddInput(RTCDataChannelStats input)
        {
            var timestamp = input.UtcTimeStamp;
            messageSentGraph.AddInput(timestamp, input.messagesSent);
            bytesSentGraph.AddInput(timestamp, input.bytesSent);
            messageReceivedGraph.AddInput(timestamp, input.messagesReceived);
            bytesReceivedGraph.AddInput(timestamp, input.bytesReceived);
        }

        public VisualElement Create()
        {
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            container.Add(messageReceivedGraph.Create());
            container.Add(bytesSentGraph.Create());
            container.Add(messageReceivedGraph.Create());
            container.Add(bytesReceivedGraph.Create());
            return container;
        }
    }
}
