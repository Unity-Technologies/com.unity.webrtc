using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public class GraphView
    {
        private const float GraphWidth = 200f;
        private const float GraphHeight = 100f;
        private const float TimeStampLabelHeight = 20f;
        private const float LineWidth = 5f;
        private const uint GraphGridDivide = 5;

        private List<KeyValuePair<long, float>> data;
        private string label;

        public GraphView(string label)
        {
            data = Enumerable.Repeat(new KeyValuePair<long, float>(0, 0f), 300).ToList();
            this.label = label;
        }

        public void AddInput(long timeStamp, float input)
        {
            data.RemoveAt(0);
            data.Add(new KeyValuePair<long, float>(timeStamp, input));
        }

        public VisualElement Create()
        {
            var root = new VisualElement {style = {flexDirection = FlexDirection.Row}};
            var container = new VisualElement {style = {width = 200, maxWidth = 250}};

            container.Add(new Label(label));
            container.Add(new IMGUIContainer(() => Draw(GUILayoutUtility.GetRect(GraphWidth, GraphHeight + TimeStampLabelHeight), ref data)));
            container.Add(new VisualElement {style = {height = 15}});

            root.Add(new VisualElement {style = {width = 15}});
            root.Add(container);
            root.Add(new VisualElement {style = {width = 15}});

            return root;
        }

        private static void Draw(Rect area, ref List<KeyValuePair<long, float>> data)
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

            // grid
            Handles.color = new Color(1f, 1f, 1f, 0.5f);
            for (int i = 1; i < GraphGridDivide; ++i)
            {
                float x = graphWidth / GraphGridDivide * i;
                float y = graphHeight / GraphGridDivide * i;
                Handles.DrawLine(
                    new Vector2(area.x, area.y + y),
                    new Vector2(area.xMax, area.y + y));
                Handles.Label(new Vector2(area.x, area.y + y), "value sample");
                Handles.Label(new Vector2(area.x + x, area.yMax - TimeStampLabelHeight), "timestamp");
            }

            // data
            Handles.color = Color.red;
            if (data.Count > 0)
            {
                var points = new List<Vector3>();
                var max = data.Max(x => x.Value);
                var dx = graphWidth / data.Count;
                var dy = graphHeight / max;

                for (var i = 0; i < data.Count; ++i)
                {
                    var x = area.x + dx * i;
                    var y = area.yMax - TimeStampLabelHeight - dy * data[i].Value;
                    points.Add(new Vector2(x, y));
                }

                Handles.DrawAAPolyLine(LineWidth, points.ToArray());
            }

            Handles.color = Color.white;
        }
    }
}
