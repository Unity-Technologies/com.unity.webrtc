using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class DataChannelGraphView
    {
        private List<float> messagesSentData;
        private List<float> bytesSentData;
        private List<float> messagesReceivedData;
        private List<float> bytesReceivedData;

        public DataChannelGraphView()
        {
            messagesSentData = Enumerable.Repeat(0f, 300).ToList();
            bytesSentData = Enumerable.Repeat(0f, 300).ToList();
            messagesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            bytesReceivedData = Enumerable.Repeat(0f, 300).ToList();
        }

        public void AddInput(RTCDataChannelStats input)
        {
            messagesSentData.RemoveAt(0);
            messagesSentData.Add(input.messagesSent);

            bytesSentData.RemoveAt(0);
            bytesSentData.Add(input.bytesSent);

            messagesReceivedData.RemoveAt(0);
            messagesReceivedData.Add(input.messagesReceived);

            bytesReceivedData.RemoveAt(0);
            bytesReceivedData.Add(input.bytesReceived);
        }

        public VisualElement Create()
        {
            var container = new VisualElement();
            container.Add(new IMGUIContainer(() =>
            {
                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref messagesSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref bytesSentData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref messagesReceivedData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref bytesReceivedData);
                }
            }));
            return container;
        }
    }
}
