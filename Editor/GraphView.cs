using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal class GraphView
    {
        private const float GraphWidth = 200f;
        private const float GraphHeight = 100f;
        private const float TimeStampLabelHeight = 20f;
        private const float LineWidth = 5f;
        private const uint GraphGridDivide = 5;
        private static readonly string[] unitStr = { "", "k", "M", "G" };

        private List<KeyValuePair<DateTime, float>> data;
        private string label;

        public GraphView(string label)
        {
            data = Enumerable.Repeat(new KeyValuePair<DateTime, float>(DateTime.UtcNow, 0f), 300).ToList();
            this.label = label;
        }

        public void AddInput(DateTime timeStamp, float input)
        {
            data.RemoveAt(0);
            data.Add(new KeyValuePair<DateTime, float>(timeStamp, input));
        }

        public VisualElement Create()
        {
            var root = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var container = new VisualElement { style = { width = 200, maxWidth = 250 } };

            container.Add(new Label(label));
            container.Add(new IMGUIContainer(() => Draw(GUILayoutUtility.GetRect(GraphWidth, GraphHeight + TimeStampLabelHeight), ref data)));
            container.Add(new VisualElement { style = { height = 15 } });

            root.Add(new VisualElement { style = { width = 15 } });
            root.Add(container);
            root.Add(new VisualElement { style = { width = 15 } });

            return root;
        }

        private static void Draw(Rect area, ref List<KeyValuePair<DateTime, float>> data)
        {
            // axis
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[]
                {
                    new Vector2(area.x, area.y), new Vector2(area.xMax, area.y),
                    new Vector2(area.xMax, area.yMax - TimeStampLabelHeight),
                    new Vector2(area.x, area.yMax - TimeStampLabelHeight)
                }, new Color(0, 0, 0, 0), Color.white);


            var graphWidth = area.width;
            var graphHeight = (area.yMax - TimeStampLabelHeight - area.yMin);
            var maxValue = data.Max(x => x.Value);

            // grid
            Handles.color = new Color(1f, 1f, 1f, 0.5f);


            var (unitCount, unitName) = DecideUnit(maxValue);

            for (uint i = 1; i < GraphGridDivide; ++i)
            {
                float x = graphWidth / GraphGridDivide * i;
                float y = graphHeight / GraphGridDivide * i;
                int dataPoint = (int)(data.Count / GraphGridDivide * i);
                Handles.DrawLine(
                    new Vector2(area.x, area.y + y),
                    new Vector2(area.xMax, area.y + y));

                Handles.Label(new Vector2(area.x, area.y + y), $"{maxValue / GraphGridDivide / unitCount * (GraphGridDivide - i):0.00}{unitName}");
                var guiSkin = GUI.skin.label;
                guiSkin.fontSize = 10;
                Handles.Label(new Vector2(area.x + x, area.yMax - TimeStampLabelHeight), data[dataPoint].Key.ToLocalTime().ToShortTimeString(), guiSkin);
            }

            // data
            Handles.color = Color.red;

            var points = new List<Vector3>();
            var dx = graphWidth / data.Count;
            var dy = graphHeight / maxValue;

            for (var i = 0; i < data.Count; ++i)
            {
                var x = area.x + dx * i;
                var y = area.yMax - TimeStampLabelHeight - dy * data[i].Value;
                points.Add(new Vector2(x, y));
            }

            Handles.DrawAAPolyLine(LineWidth, points.ToArray());

            Handles.color = Color.white;
        }

        private static (int unitCount, string unitName) DecideUnit(float maxValue)
        {
            var unitCount = 1;
            var unitIndex = 0;
            var current = maxValue;

            while (current >= 1000)
            {
                current /= 1000;
                unitCount *= 1000;
                unitIndex++;
            }

            if (unitIndex > unitStr.Length - 1)
            {
                unitIndex = unitStr.Length - 1;
            }

            return (unitCount, unitStr[unitIndex]);
        }
    }
}
