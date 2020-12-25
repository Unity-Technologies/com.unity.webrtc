using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class MediaSourceGraphView
    {
        private GraphView widthGraph = new GraphView("width");
        private GraphView heightGraph = new GraphView("height");
        private GraphView framesGraph = new GraphView("frames");
        private GraphView framesPerSecondGraph = new GraphView("framesPerSecond");

        public void AddInput(RTCMediaSourceStats input)
        {
            var timestamp = input.UtcTimeStamp;
            widthGraph.AddInput(timestamp, input.width);
            heightGraph.AddInput(timestamp, input.height);
            framesGraph.AddInput(timestamp, input.frames);
            framesPerSecondGraph.AddInput(timestamp, input.framesPerSecond);
        }

        public VisualElement Create()
        {
            var container = new VisualElement {style = {flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap}};
            container.Add(widthGraph.Create());
            container.Add(heightGraph.Create());
            container.Add(framesGraph.Create());
            container.Add(framesPerSecondGraph.Create());
            return container;
        }
    }
}
