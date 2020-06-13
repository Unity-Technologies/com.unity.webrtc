using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class InboundRTPStreamGraphView
    {
        private List<float> firCountData;
        private List<float> pliCountData;
        private List<float> nackCountData;
        private List<float> sliCountData;
        private List<float> qpSumData;
        private List<float> packetsReceivedData;
        private List<float> bytesReceivedData;
        private List<float> headerBytesReceivedData;
        private List<float> packetsLostData;
        private List<float> jitterData;
        private List<float> packetsDiscardedData;
        private List<float> packetsRepairedData;
        private List<float> burstPacketsLostData;
        private List<float> burstPacketsDiscardedData;
        private List<float> burstLossCountData;
        private List<float> burstDiscardCountData;
        private List<float> burstLossRateData;
        private List<float> burstDiscardRateData;
        private List<float> gapLossRateData;
        private List<float> gapDiscardRateData;
        private List<float> framesDecodedData;
        private List<float> keyFramesDecodedData;


        public InboundRTPStreamGraphView()
        {
            firCountData = Enumerable.Repeat(0f, 300).ToList();
            pliCountData = Enumerable.Repeat(0f, 300).ToList();
            nackCountData = Enumerable.Repeat(0f, 300).ToList();
            sliCountData = Enumerable.Repeat(0f, 300).ToList();
            qpSumData = Enumerable.Repeat(0f, 300).ToList();
            packetsReceivedData = Enumerable.Repeat(0f, 300).ToList();
            bytesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            headerBytesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            packetsLostData = Enumerable.Repeat(0f, 300).ToList();
            jitterData = Enumerable.Repeat(0f, 300).ToList();
            packetsDiscardedData = Enumerable.Repeat(0f, 300).ToList();
            packetsRepairedData = Enumerable.Repeat(0f, 300).ToList();
            burstPacketsLostData = Enumerable.Repeat(0f, 300).ToList();
            burstPacketsDiscardedData = Enumerable.Repeat(0f, 300).ToList();
            burstLossCountData = Enumerable.Repeat(0f, 300).ToList();
            burstDiscardCountData = Enumerable.Repeat(0f, 300).ToList();
            burstLossRateData = Enumerable.Repeat(0f, 300).ToList();
            burstDiscardRateData = Enumerable.Repeat(0f, 300).ToList();
            gapLossRateData = Enumerable.Repeat(0f, 300).ToList();
            gapDiscardRateData = Enumerable.Repeat(0f, 300).ToList();
            framesDecodedData = Enumerable.Repeat(0f, 300).ToList();
            keyFramesDecodedData = Enumerable.Repeat(0f, 300).ToList();
        }

        public void AddInput(RTCInboundRTPStreamStats input)
        {
            firCountData.RemoveAt(0);
            firCountData.Add(input.firCount);

            pliCountData.RemoveAt(0);
            pliCountData.Add(input.pliCount);

            nackCountData.RemoveAt(0);
            nackCountData.Add(input.nackCount);

            sliCountData.RemoveAt(0);
            sliCountData.Add(input.sliCount);

            qpSumData.RemoveAt(0);
            qpSumData.Add(input.qpSum);

            packetsReceivedData.RemoveAt(0);
            packetsReceivedData.Add(input.packetsReceived);

            bytesReceivedData.RemoveAt(0);
            bytesReceivedData.Add(input.bytesReceived);

            headerBytesReceivedData.RemoveAt(0);
            headerBytesReceivedData.Add(input.headerBytesReceived);

            packetsLostData.RemoveAt(0);
            packetsLostData.Add(input.packetsLost);

            jitterData.RemoveAt(0);
            jitterData.Add((float) input.jitter);

            packetsDiscardedData.RemoveAt(0);
            packetsDiscardedData.Add(input.packetsDiscarded);

            packetsRepairedData.RemoveAt(0);
            packetsRepairedData.Add(input.packetsRepaired);

            burstPacketsLostData.RemoveAt(0);
            burstPacketsLostData.Add(input.burstPacketsLost);

            burstPacketsDiscardedData.RemoveAt(0);
            burstPacketsDiscardedData.Add(input.burstPacketsDiscarded);

            burstLossCountData.RemoveAt(0);
            burstLossCountData.Add(input.burstLossCount);

            burstDiscardCountData.RemoveAt(0);
            burstDiscardCountData.Add(input.burstDiscardCount);

            burstLossRateData.RemoveAt(0);
            burstLossRateData.Add((float)input.burstLossRate);

            burstDiscardRateData.RemoveAt(0);
            burstDiscardRateData.Add((float) input.burstDiscardRate);

            gapLossRateData.RemoveAt(0);
            gapLossRateData.Add((float)input.gapLossRate);

            gapDiscardRateData.RemoveAt(0);
            gapDiscardRateData.Add((float) input.gapDiscardRate);

            framesDecodedData.RemoveAt(0);
            framesDecodedData.Add(input.framesDecoded);

            keyFramesDecodedData.RemoveAt(0);
            keyFramesDecodedData.Add(input.keyFramesDecoded);
        }

        public VisualElement Create()
        {
            var container = new VisualElement();
            container.Add(new IMGUIContainer(() =>
            {
                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref firCountData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref pliCountData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref nackCountData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref sliCountData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref qpSumData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref packetsReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref bytesReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref headerBytesReceivedData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref packetsLostData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref jitterData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref packetsDiscardedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref packetsRepairedData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref burstPacketsLostData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref burstPacketsDiscardedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref burstLossCountData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref burstDiscardCountData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref burstLossRateData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref burstDiscardRateData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref gapLossRateData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref gapDiscardRateData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesDecodedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref keyFramesDecodedData);
                }
            }));
            return container;
        }
    }
}
