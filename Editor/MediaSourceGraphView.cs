using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class MediaSourceGraphView
    {
        private List<float> widthData;
        private List<float> heightData;
        private List<float> framesData;
        private List<float> framesPerSecondData;

        public MediaSourceGraphView()
        {
            widthData = Enumerable.Repeat(0f, 300).ToList();
            heightData = Enumerable.Repeat(0f, 300).ToList();
            framesData = Enumerable.Repeat(0f, 300).ToList();
            framesPerSecondData = Enumerable.Repeat(0f, 300).ToList();
        }

        public void AddInput(RTCMediaSourceStats input)
        {
            widthData.RemoveAt(0);
            widthData.Add(input.width);

            heightData.RemoveAt(0);
            heightData.Add(input.height);

            framesData.RemoveAt(0);
            framesData.Add(input.frames);

            framesPerSecondData.RemoveAt(0);
            framesPerSecondData.Add(input.framesPerSecond);
        }

        public VisualElement Create()
        {
            var container = new VisualElement();
            container.Add(new IMGUIContainer(() =>
            {
                using (new GUILayout.HorizontalScope())
                {
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref widthData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref heightData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesData);
                    GraphDraw.Draw(GUILayoutUtility.GetRect(200f, 100f), ref framesPerSecondData);
                }
            }));
            return container;
        }
    }
}
