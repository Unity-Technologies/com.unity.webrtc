using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class GraphDraw
    {
        /// <summary>
        /// example: container.Add(new IMGUIContainer(() => GraphDraw.Draw(GUILayoutUtility.GetRect(Screen.width / 2, 200), m_data)));
        /// </summary>
        /// <param name="area"></param>
        /// <param name="data"></param>
        public static void Draw(Rect area, ref List<float> data)
        {
            // axis
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[]
                {
                    new Vector2(area.x, area.y), new Vector2(area.xMax, area.y), new Vector2(area.xMax, area.yMax),
                    new Vector2(area.x, area.yMax)
                }, new Color(0, 0, 0, 0), Color.white);

            // grid
            Handles.color = new Color(1f, 1f, 1f, 0.5f);
            const int div = 10;
            for (int i = 1; i < div; ++i)
            {
                float y = area.height / div * i;
                float x = area.width / div * i;
                Handles.DrawLine(
                    new Vector2(area.x, area.y + y),
                    new Vector2(area.xMax, area.y + y));
                Handles.DrawLine(
                    new Vector2(area.x + x, area.y),
                    new Vector2(area.x + x, area.yMax));
            }

            // data
            Handles.color = Color.red;
            if (data.Count > 0)
            {
                var points = new List<Vector3>();
                var max = data.Max();
                var dx = area.width / data.Count;
                var dy = area.height / max;
                for (var i = 0; i < data.Count; ++i)
                {
                    var x = area.x + dx * i;
                    var y = area.yMax - dy * data[i];
                    points.Add(new Vector2(x, y));
                }

                Handles.DrawAAPolyLine(5f, points.ToArray());
            }

            Handles.color = Color.white;
        }
    }

    public class CandidatePairGraphView
    {
        private List<float> bytesSentData;
        private List<float> bytesReceivedData;
        private List<float> totalRoundTripTimeData;
        private List<float> currentRoundTripTimeData;
        private List<float> availableOutgoingBitrateData;
        private List<float> availableIncomingBitrateData;
        private List<float> requestsReceivedData;
        private List<float> requestsSentData;
        private List<float> responsesReceivedData;
        private List<float> responsesSentData;
        private List<float> retransmissionsReceivedData;
        private List<float> retransmissionsSentData;
        private List<float> consentRequestsReceivedData;
        private List<float> consentRequestsSentData;
        private List<float> consentResponsesReceivedData;
        private List<float> consentResponsesSentData;

        public CandidatePairGraphView()
        {
            bytesSentData = Enumerable.Repeat(0f, 300).ToList();
            bytesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            totalRoundTripTimeData = Enumerable.Repeat(0f, 300).ToList();
            currentRoundTripTimeData = Enumerable.Repeat(0f, 300).ToList();
            availableOutgoingBitrateData = Enumerable.Repeat(0f, 300).ToList();
            availableIncomingBitrateData = Enumerable.Repeat(0f, 300).ToList();
            requestsReceivedData = Enumerable.Repeat(0f, 300).ToList();
            requestsSentData = Enumerable.Repeat(0f, 300).ToList();
            responsesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            responsesSentData = Enumerable.Repeat(0f, 300).ToList();
            retransmissionsReceivedData = Enumerable.Repeat(0f, 300).ToList();
            retransmissionsSentData = Enumerable.Repeat(0f, 300).ToList();
            consentRequestsReceivedData = Enumerable.Repeat(0f, 300).ToList();
            consentRequestsSentData = Enumerable.Repeat(0f, 300).ToList();
            consentResponsesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            consentResponsesSentData = Enumerable.Repeat(0f, 300).ToList();
        }

        public void AddInput(RTCIceCandidatePairStats input)
        {
            bytesSentData.RemoveAt(0);
            bytesSentData.Add(input.bytesSent);

            bytesReceivedData.RemoveAt(0);
            bytesReceivedData.Add(input.bytesReceived);

            totalRoundTripTimeData.RemoveAt(0);
            totalRoundTripTimeData.Add((float)input.totalRoundTripTime);

            currentRoundTripTimeData.RemoveAt(0);
            currentRoundTripTimeData.Add((float)input.currentRoundTripTime);

            availableOutgoingBitrateData.RemoveAt(0);
            availableOutgoingBitrateData.Add((float)input.availableOutgoingBitrate);

            availableIncomingBitrateData.RemoveAt(0);
            availableIncomingBitrateData.Add((float)input.availableIncomingBitrate);

            requestsReceivedData.RemoveAt(0);
            requestsReceivedData.Add(input.requestsReceived);

            requestsSentData.RemoveAt(0);
            requestsSentData.Add(input.requestsSent);

            responsesReceivedData.RemoveAt(0);
            responsesReceivedData.Add(input.responsesReceived);

            responsesSentData.RemoveAt(0);
            responsesSentData.Add(input.responsesSent);

            retransmissionsReceivedData.RemoveAt(0);
            retransmissionsReceivedData.Add(input.retransmissionsReceived);

            retransmissionsSentData.RemoveAt(0);
            retransmissionsSentData.Add(input.retransmissionsSent);

            consentRequestsReceivedData.RemoveAt(0);
            consentRequestsReceivedData.Add(input.consentRequestsReceived);

            consentRequestsSentData.RemoveAt(0);
            consentRequestsSentData.Add(input.consentRequestsSent);

            consentResponsesReceivedData.RemoveAt(0);
            consentResponsesReceivedData.Add(input.consentResponsesReceived);

            consentResponsesSentData.RemoveAt(0);
            consentResponsesSentData.Add(input.consentResponsesSent);
        }

        public VisualElement Create()
        {
            var container = new VisualElement();
            container.Add(new IMGUIContainer(() =>
            {
                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref bytesSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref bytesReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref totalRoundTripTimeData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref currentRoundTripTimeData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref availableOutgoingBitrateData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref availableIncomingBitrateData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref requestsReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref requestsSentData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref responsesReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref responsesSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref retransmissionsReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref retransmissionsSentData);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref consentRequestsReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref consentRequestsSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref consentResponsesReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref consentResponsesSentData);
                }

            }));
            return container;
        }
    }
}
