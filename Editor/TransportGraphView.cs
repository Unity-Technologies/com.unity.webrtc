using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class TransportGraphView
    {
        private List<float> bytesSentData;
        private List<float> bytesReceivedData;
        private List<float> selectedCandidatePairChangesData;

        public TransportGraphView()
        {
            bytesSentData = Enumerable.Repeat(0f, 300).ToList();
            bytesReceivedData = Enumerable.Repeat(0f, 300).ToList();
            selectedCandidatePairChangesData = Enumerable.Repeat(0f, 300).ToList();
        }

        public void AddInput(RTCTransportStats input)
        {
            bytesSentData.RemoveAt(0);
            bytesSentData.Add(input.bytesSent);

            bytesReceivedData.RemoveAt(0);
            bytesReceivedData.Add(input.bytesReceived);

            selectedCandidatePairChangesData.RemoveAt(0);
            selectedCandidatePairChangesData.Add(input.selectedCandidatePairChanges);
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
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref selectedCandidatePairChangesData);
                }
            }));
            return container;
        }
    }
}
