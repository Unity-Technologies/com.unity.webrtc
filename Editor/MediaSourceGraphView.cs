using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class MediaSourceGraphView
    {
        //Video
        private GraphView widthGraph = new GraphView("width");
        private GraphView heightGraph = new GraphView("height");
        private GraphView framesGraph = new GraphView("frames");
        private GraphView framesPerSecondGraph = new GraphView("framesPerSecond");

        //Audio
        private GraphView audioLevelGraph = new GraphView("audioLevel");
        private GraphView totalAudioEnergyGraph = new GraphView("totalAudioEnergy");
        private GraphView totalSamplesDurationGraph = new GraphView("totalSamplesDuration");

        public void AddInput(RTCVideoSourceStats input)
        {
            var timestamp = input.UtcTimeStamp;
            widthGraph.AddInput(timestamp, input.width);
            heightGraph.AddInput(timestamp, input.height);
            framesGraph.AddInput(timestamp, input.frames);
            framesPerSecondGraph.AddInput(timestamp, (float)input.framesPerSecond);
        }

        public void AddInput(RTCAudioSourceStats input)
        {
            var timestamp = input.UtcTimeStamp;
            audioLevelGraph.AddInput(timestamp, (float)input.audioLevel);
            totalAudioEnergyGraph.AddInput(timestamp, (float)input.totalAudioEnergy);
            totalSamplesDurationGraph.AddInput(timestamp, (float)input.totalSamplesDuration);
        }

        public VisualElement Create()
        {
            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
            container.Add(widthGraph.Create());
            container.Add(heightGraph.Create());
            container.Add(framesGraph.Create());
            container.Add(framesPerSecondGraph.Create());
            container.Add(audioLevelGraph.Create());
            container.Add(totalAudioEnergyGraph.Create());
            container.Add(totalSamplesDurationGraph.Create());
            return container;
        }
    }
}
