using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class MediaStreamTrackGraphView
    {
        private List<float> jitterBufferDelayData;
        private List<float> jitterBufferEmittedCountData;
        private List<float> frameWidthData;
        private List<float> frameHeightData;
        private List<float> framesPerSecondData;
        private List<float> framesSentData;
        private List<float> hugeFramesSentData;
        private List<float> framesReceivedData;
        private List<float> framesDecodedData;
        private List<float> framesDroppedData;
        private List<float> framesCorruptedData;
        private List<float> partialFramesLostData;
        private List<float> fullFramesLostData;
        private List<float> audioLevelData;
        private List<float> totalAudioEnergyData;
        private List<float> echoReturnLossData;
        private List<float> echoReturnLossEnhancementData;
        private List<float> totalSamplesReceivedData;
        private List<float> totalSamplesDurationData;
        private List<float> concealedSamplesData;
        private List<float> silentConcealedSamplesData;
        private List<float> concealmentEventsData;
        private List<float> insertedSamplesForDecelerationData;
        private List<float> removedSamplesForAccelerationData;
        private List<float> jitterBufferFlushesData;
        private List<float> delayedPacketOutageSamplesData;
        private List<float> relativePacketArrivalDelayData;
        private List<float> interruptionCountData;
        private List<float> totalInterruptionDurationData;
        private List<float> freezeCountData;
        private List<float> pauseCountData;
        private List<float> totalFreezesDurationData;
        private List<float> totalPausesDurationData;
        private List<float> totalFramesDurationData;
        private List<float> sumOfSquaredFramesDurationData;


        public MediaStreamTrackGraphView()
        {
            jitterBufferDelayData = Enumerable.Repeat(0f, 300).ToList();
            jitterBufferEmittedCountData = Enumerable.Repeat(0f, 300).ToList();
            frameWidthData = Enumerable.Repeat(0f, 300).ToList();
            frameHeightData = Enumerable.Repeat(0f, 300).ToList();
            framesPerSecondData = Enumerable.Repeat(0f, 300).ToList();
            framesSentData = Enumerable.Repeat(0f, 300).ToList();
            hugeFramesSentData = Enumerable.Repeat(0f, 300).ToList();
            framesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            framesDecodedData = Enumerable.Repeat(0f, 300).ToList();
            framesDroppedData = Enumerable.Repeat(0f, 300).ToList();
            framesCorruptedData = Enumerable.Repeat(0f, 300).ToList();
            partialFramesLostData = Enumerable.Repeat(0f, 300).ToList();
            fullFramesLostData = Enumerable.Repeat(0f, 300).ToList();
            audioLevelData = Enumerable.Repeat(0f, 300).ToList();
            totalAudioEnergyData = Enumerable.Repeat(0f, 300).ToList();
            echoReturnLossData = Enumerable.Repeat(0f, 300).ToList();
            echoReturnLossEnhancementData = Enumerable.Repeat(0f, 300).ToList();
            totalSamplesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            totalSamplesDurationData = Enumerable.Repeat(0f, 300).ToList();
            concealedSamplesData = Enumerable.Repeat(0f, 300).ToList();
            silentConcealedSamplesData = Enumerable.Repeat(0f, 300).ToList();
            concealmentEventsData = Enumerable.Repeat(0f, 300).ToList();
            insertedSamplesForDecelerationData = Enumerable.Repeat(0f, 300).ToList();
            removedSamplesForAccelerationData = Enumerable.Repeat(0f, 300).ToList();
            jitterBufferFlushesData = Enumerable.Repeat(0f, 300).ToList();
            delayedPacketOutageSamplesData = Enumerable.Repeat(0f, 300).ToList();
            relativePacketArrivalDelayData = Enumerable.Repeat(0f, 300).ToList();
            interruptionCountData = Enumerable.Repeat(0f, 300).ToList();
            totalInterruptionDurationData = Enumerable.Repeat(0f, 300).ToList();
            freezeCountData = Enumerable.Repeat(0f, 300).ToList();
            pauseCountData = Enumerable.Repeat(0f, 300).ToList();
            totalFreezesDurationData = Enumerable.Repeat(0f, 300).ToList();
            totalPausesDurationData = Enumerable.Repeat(0f, 300).ToList();
            totalFramesDurationData = Enumerable.Repeat(0f, 300).ToList();
            sumOfSquaredFramesDurationData = Enumerable.Repeat(0f, 300).ToList();
        }

        public void AddInput(RTCMediaStreamTrackStats input)
        {
            jitterBufferDelayData.RemoveAt(0);
            jitterBufferDelayData.Add((float) input.jitterBufferDelay);

            jitterBufferEmittedCountData.RemoveAt(0);
            jitterBufferEmittedCountData.Add(input.jitterBufferEmittedCount);

            frameWidthData.RemoveAt(0);
            frameWidthData.Add(input.frameWidth);

            frameHeightData.RemoveAt(0);
            frameHeightData.Add(input.frameHeight);

            framesPerSecondData.RemoveAt(0);
            framesPerSecondData.Add((float)input.framesPerSecond);

            framesSentData.RemoveAt(0);
            framesSentData.Add(input.framesSent);

            hugeFramesSentData.RemoveAt(0);
            hugeFramesSentData.Add(input.hugeFramesSent);

            framesReceivedData.RemoveAt(0);
            framesReceivedData.Add(input.framesReceived);

            framesDecodedData.RemoveAt(0);
            framesDecodedData.Add(input.framesDecoded);

            framesDroppedData.RemoveAt(0);
            framesDroppedData.Add(input.framesDropped);

            framesCorruptedData.RemoveAt(0);
            framesCorruptedData.Add(input.framesCorrupted);

            partialFramesLostData.RemoveAt(0);
            partialFramesLostData.Add(input.partialFramesLost);

            fullFramesLostData.RemoveAt(0);
            fullFramesLostData.Add(input.fullFramesLost);

            audioLevelData.RemoveAt(0);
            audioLevelData.Add((float) input.audioLevel);

            totalAudioEnergyData.RemoveAt(0);
            totalAudioEnergyData.Add((float) input.totalAudioEnergy);

            echoReturnLossData.RemoveAt(0);
            echoReturnLossData.Add((float) input.echoReturnLoss);

            echoReturnLossEnhancementData.RemoveAt(0);
            echoReturnLossEnhancementData.Add((float) input.echoReturnLossEnhancement);

            totalSamplesReceivedData.RemoveAt(0);
            totalSamplesReceivedData.Add(input.totalSamplesReceived);

            totalSamplesDurationData.RemoveAt(0);
            totalSamplesDurationData.Add((float) input.totalSamplesDuration);

            concealedSamplesData.RemoveAt(0);
            concealedSamplesData.Add(input.concealedSamples);

            silentConcealedSamplesData.RemoveAt(0);
            silentConcealedSamplesData.Add(input.silentConcealedSamples);

            concealmentEventsData.RemoveAt(0);
            concealmentEventsData.Add(input.concealmentEvents);

            insertedSamplesForDecelerationData.RemoveAt(0);
            insertedSamplesForDecelerationData.Add(input.insertedSamplesForDeceleration);

            removedSamplesForAccelerationData.RemoveAt(0);
            removedSamplesForAccelerationData.Add(input.removedSamplesForAcceleration);

            jitterBufferFlushesData.RemoveAt(0);
            jitterBufferFlushesData.Add(input.jitterBufferFlushes);

            delayedPacketOutageSamplesData.RemoveAt(0);
            delayedPacketOutageSamplesData.Add(input.delayedPacketOutageSamples);

            relativePacketArrivalDelayData.RemoveAt(0);
            relativePacketArrivalDelayData.Add((float) input.relativePacketArrivalDelay);

            interruptionCountData.RemoveAt(0);
            interruptionCountData.Add(input.interruptionCount);

            totalInterruptionDurationData.RemoveAt(0);
            totalInterruptionDurationData.Add((float) input.totalInterruptionDuration);

            freezeCountData.RemoveAt(0);
            freezeCountData.Add(input.freezeCount);

            pauseCountData.RemoveAt(0);
            pauseCountData.Add(input.pauseCount);

            totalFreezesDurationData.RemoveAt(0);
            totalFreezesDurationData.Add((float) input.totalFreezesDuration);

            totalPausesDurationData.RemoveAt(0);
            totalPausesDurationData.Add((float) input.totalPausesDuration);

            totalFramesDurationData.RemoveAt(0);
            totalFramesDurationData.Add((float) input.totalFramesDuration);

            sumOfSquaredFramesDurationData.RemoveAt(0);
            sumOfSquaredFramesDurationData.Add((float) input.sumOfSquaredFramesDuration);

        }

        public VisualElement Create()
        {
            var container = new VisualElement();
            container.Add(new IMGUIContainer(() =>
            {
                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref jitterBufferDelayData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref jitterBufferEmittedCountData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref frameWidthData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref frameHeightData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesPerSecondData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref hugeFramesSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesReceivedData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesDecodedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesDroppedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesCorruptedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref partialFramesLostData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref fullFramesLostData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref audioLevelData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalAudioEnergyData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref echoReturnLossData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref echoReturnLossEnhancementData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalSamplesReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalSamplesDurationData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref concealedSamplesData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref silentConcealedSamplesData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref concealmentEventsData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref insertedSamplesForDecelerationData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref removedSamplesForAccelerationData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref jitterBufferFlushesData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref delayedPacketOutageSamplesData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref relativePacketArrivalDelayData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref interruptionCountData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalInterruptionDurationData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref freezeCountData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref pauseCountData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalFreezesDurationData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalPausesDurationData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalFramesDurationData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref sumOfSquaredFramesDurationData);
                }

            }));
            return container;
        }
    }
}
