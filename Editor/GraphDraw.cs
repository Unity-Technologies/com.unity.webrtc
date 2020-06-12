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
}
