using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class MediaStreamTrackGraphView
    {
        private GraphView jitterBufferDelayGraph = new GraphView("jitterBufferDelay");
        private GraphView jitterBufferEmittedCountGraph = new GraphView("jitterBufferEmittedCount");
        private GraphView frameWidthGraph = new GraphView("frameWidth");
        private GraphView frameHeightGraph = new GraphView("frameHeight");
        private GraphView framesPerSecondGraph = new GraphView("framesPerSecond");
        private GraphView framesSentGraph = new GraphView("framesSent");
        private GraphView hugeFramesSentGraph = new GraphView("hugeFramesSent");
        private GraphView framesReceivedGraph = new GraphView("framesReceived");
        private GraphView framesDecodedGraph = new GraphView("framesDecoded");
        private GraphView framesDroppedGraph = new GraphView("framesDropped");
        private GraphView framesCorruptedGraph = new GraphView("framesCorrupted");
        private GraphView partialFramesLostGraph = new GraphView("partialFramesLost");
        private GraphView fullFramesLostGraph = new GraphView("fullFramesLost");
        private GraphView audioLevelGraph = new GraphView("audioLevel");
        private GraphView totalAudioEnergyGraph = new GraphView("totalAudioEnergy");
        private GraphView echoReturnLossGraph = new GraphView("echoReturnLoss");
        private GraphView echoReturnLossEnhancementGraph = new GraphView("echoReturnLossEnhancement");
        private GraphView totalSamplesReceivedGraph = new GraphView("totalSamplesReceived");
        private GraphView totalSamplesDurationGraph = new GraphView("totalSamplesDuration");
        private GraphView concealedSamplesGraph = new GraphView("concealedSamples");
        private GraphView silentConcealedSamplesGraph = new GraphView("silentConcealedSamples");
        private GraphView concealmentEventsGraph = new GraphView("concealmentEvents");
        private GraphView insertedSamplesForDecelerationGraph = new GraphView("insertedSamplesForDeceleration");
        private GraphView removedSamplesForAccelerationGraph = new GraphView("removedSamplesForAcceleration");
        private GraphView jitterBufferFlushesGraph = new GraphView("jitterBufferFlushes");
        private GraphView delayedPacketOutageSamplesGraph = new GraphView("delayedPacketOutageSamples");
        private GraphView relativePacketArrivalDelayGraph = new GraphView("relativePacketArrivalDelay");
        private GraphView interruptionCountGraph = new GraphView("interruptionCount");
        private GraphView totalInterruptionDurationGraph = new GraphView("totalInterruptionDuration");
        private GraphView freezeCountGraph = new GraphView("freezeCount");
        private GraphView pauseCountGraph = new GraphView("pauseCount");
        private GraphView totalFreezesDurationGraph = new GraphView("totalFreezesDuration");
        private GraphView totalPausesDurationGraph = new GraphView("totalPausesDuration");
        private GraphView totalFramesDurationGraph = new GraphView("totalFramesDuration");
        private GraphView sumOfSquaredFramesDurationGraph = new GraphView("sumOfSquaredFramesDuration");

        public void AddInput(RTCMediaStreamTrackStats input)
        {
            var timestamp = input.UtcTimeStamp;
            jitterBufferDelayGraph.AddInput(timestamp, (float)input.jitterBufferDelay);
            jitterBufferEmittedCountGraph.AddInput(timestamp, input.jitterBufferEmittedCount);
            frameWidthGraph.AddInput(timestamp, input.frameWidth);
            frameHeightGraph.AddInput(timestamp, input.frameHeight);
            framesPerSecondGraph.AddInput(timestamp, (float)input.framesPerSecond);
            framesSentGraph.AddInput(timestamp, input.framesSent);
            hugeFramesSentGraph.AddInput(timestamp, input.hugeFramesSent);
            framesReceivedGraph.AddInput(timestamp, input.framesReceived);
            framesDecodedGraph.AddInput(timestamp, input.framesDecoded);
            framesDroppedGraph.AddInput(timestamp, input.framesDropped);
            framesCorruptedGraph.AddInput(timestamp, input.framesCorrupted);
            partialFramesLostGraph.AddInput(timestamp, input.partialFramesLost);
            fullFramesLostGraph.AddInput(timestamp, input.fullFramesLost);
            audioLevelGraph.AddInput(timestamp, (float)input.audioLevel);
            totalAudioEnergyGraph.AddInput(timestamp, (float)input.totalAudioEnergy);
            echoReturnLossGraph.AddInput(timestamp, (float)input.echoReturnLoss);
            echoReturnLossEnhancementGraph.AddInput(timestamp, (float)input.echoReturnLossEnhancement);
            totalSamplesReceivedGraph.AddInput(timestamp, input.totalSamplesReceived);
            totalSamplesDurationGraph.AddInput(timestamp, (float)input.totalSamplesDuration);
            concealedSamplesGraph.AddInput(timestamp, input.concealedSamples);
            silentConcealedSamplesGraph.AddInput(timestamp, input.silentConcealedSamples);
            concealmentEventsGraph.AddInput(timestamp, input.concealmentEvents);
            insertedSamplesForDecelerationGraph.AddInput(timestamp, input.insertedSamplesForDeceleration);
            removedSamplesForAccelerationGraph.AddInput(timestamp, input.removedSamplesForAcceleration);
            jitterBufferFlushesGraph.AddInput(timestamp, input.jitterBufferFlushes);
            delayedPacketOutageSamplesGraph.AddInput(timestamp, input.delayedPacketOutageSamples);
            relativePacketArrivalDelayGraph.AddInput(timestamp, (float)input.relativePacketArrivalDelay);
            interruptionCountGraph.AddInput(timestamp, input.interruptionCount);
            totalInterruptionDurationGraph.AddInput(timestamp, (float)input.totalInterruptionDuration);
            freezeCountGraph.AddInput(timestamp, input.freezeCount);
            pauseCountGraph.AddInput(timestamp, input.pauseCount);
            totalFreezesDurationGraph.AddInput(timestamp, (float)input.totalFreezesDuration);
            totalPausesDurationGraph.AddInput(timestamp, (float)input.totalPausesDuration);
            totalFramesDurationGraph.AddInput(timestamp, (float)input.totalFramesDuration);
            sumOfSquaredFramesDurationGraph.AddInput(timestamp, (float)input.sumOfSquaredFramesDuration);
        }

        public VisualElement Create()
        {
            var container = new VisualElement {style = {flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap}};
            container.Add(jitterBufferDelayGraph.Create());
            container.Add(jitterBufferEmittedCountGraph.Create());
            container.Add(frameWidthGraph.Create());
            container.Add(frameHeightGraph.Create());
            container.Add(framesPerSecondGraph.Create());
            container.Add(framesSentGraph.Create());
            container.Add(hugeFramesSentGraph.Create());
            container.Add(framesReceivedGraph.Create());
            container.Add(framesDecodedGraph.Create());
            container.Add(framesDroppedGraph.Create());
            container.Add(framesCorruptedGraph.Create());
            container.Add(partialFramesLostGraph.Create());
            container.Add(fullFramesLostGraph.Create());
            container.Add(audioLevelGraph.Create());
            container.Add(totalAudioEnergyGraph.Create());
            container.Add(echoReturnLossGraph.Create());
            container.Add(echoReturnLossEnhancementGraph.Create());
            container.Add(totalSamplesReceivedGraph.Create());
            container.Add(totalSamplesDurationGraph.Create());
            container.Add(concealedSamplesGraph.Create());
            container.Add(silentConcealedSamplesGraph.Create());
            container.Add(concealmentEventsGraph.Create());
            container.Add(insertedSamplesForDecelerationGraph.Create());
            container.Add(removedSamplesForAccelerationGraph.Create());
            container.Add(jitterBufferFlushesGraph.Create());
            container.Add(delayedPacketOutageSamplesGraph.Create());
            container.Add(relativePacketArrivalDelayGraph.Create());
            container.Add(interruptionCountGraph.Create());
            container.Add(totalInterruptionDurationGraph.Create());
            container.Add(freezeCountGraph.Create());
            container.Add(pauseCountGraph.Create());
            container.Add(totalFreezesDurationGraph.Create());
            container.Add(totalPausesDurationGraph.Create());
            container.Add(totalFramesDurationGraph.Create());
            container.Add(sumOfSquaredFramesDurationGraph.Create());
            return container;
        }
    }
}
