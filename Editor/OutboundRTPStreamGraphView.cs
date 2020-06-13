using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class OutboundRTPStreamGraphView
    {
        private List<float> firCountData;
        private List<float> pliCountData;
        private List<float> nackCountData;
        private List<float> sliCountData;
        private List<float> qpSumData;
        private List<float> packetsSentData;
        private List<float> retransmittedPacketsSentData;
        private List<float> bytesSentData;
        private List<float> headerBytesSentData;
        private List<float> retransmittedBytesSentData;
        private List<float> framesDecodedData;
        private List<float> keyFramesDecodedData;
        private List<float> targetBitrateData;
        private List<float> totalEncodeTimeData;
        private List<float> totalEncodedBytesTargetData;
        private List<float> totalPacketSendDelayData;


        public OutboundRTPStreamGraphView()
        {
            firCountData = Enumerable.Repeat(0f, 300).ToList();
            pliCountData = Enumerable.Repeat(0f, 300).ToList();
            nackCountData = Enumerable.Repeat(0f, 300).ToList();
            sliCountData = Enumerable.Repeat(0f, 300).ToList();
            qpSumData = Enumerable.Repeat(0f, 300).ToList();
            packetsSentData = Enumerable.Repeat(0f, 300).ToList();
            retransmittedPacketsSentData = Enumerable.Repeat(0f, 300).ToList();
            bytesSentData = Enumerable.Repeat(0f, 300).ToList();
            headerBytesSentData = Enumerable.Repeat(0f, 300).ToList();
            retransmittedBytesSentData = Enumerable.Repeat(0f, 300).ToList();
            framesDecodedData = Enumerable.Repeat(0f, 300).ToList();
            keyFramesDecodedData = Enumerable.Repeat(0f, 300).ToList();
            targetBitrateData = Enumerable.Repeat(0f, 300).ToList();
            totalEncodeTimeData = Enumerable.Repeat(0f, 300).ToList();
            totalEncodedBytesTargetData = Enumerable.Repeat(0f, 300).ToList();
            totalPacketSendDelayData = Enumerable.Repeat(0f, 300).ToList();
        }

        public void AddInput(RTCOutboundRTPStreamStats input)
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

            packetsSentData.RemoveAt(0);
            packetsSentData.Add(input.packetsSent);

            retransmittedPacketsSentData.RemoveAt(0);
            retransmittedPacketsSentData.Add(input.retransmittedPacketsSent);

            bytesSentData.RemoveAt(0);
            bytesSentData.Add(input.bytesSent);

            headerBytesSentData.RemoveAt(0);
            headerBytesSentData.Add(input.headerBytesSent);

            retransmittedBytesSentData.RemoveAt(0);
            retransmittedBytesSentData.Add(input.retransmittedBytesSent);

            targetBitrateData.RemoveAt(0);
            targetBitrateData.Add((float) input.targetBitrate);

            framesDecodedData.RemoveAt(0);
            framesDecodedData.Add(input.framesEncoded);

            keyFramesDecodedData.RemoveAt(0);
            keyFramesDecodedData.Add(input.keyFramesEncoded);

            totalEncodeTimeData.RemoveAt(0);
            totalEncodeTimeData.Add((float) input.totalEncodeTime);

            totalEncodedBytesTargetData.RemoveAt(0);
            totalEncodedBytesTargetData.Add(input.totalEncodedBytesTarget);

            totalPacketSendDelayData.RemoveAt(0);
            totalPacketSendDelayData.Add((float) input.totalPacketSendDelay);

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
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref packetsSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref retransmittedPacketsSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref bytesSentData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref headerBytesSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref retransmittedBytesSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref targetBitrateData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalEncodeTimeData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalEncodedBytesTargetData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalPacketSendDelayData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesDecodedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref keyFramesDecodedData);
                }
            }));
            return container;
        }
    }
}
