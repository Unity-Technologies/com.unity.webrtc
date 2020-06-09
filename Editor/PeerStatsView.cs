using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Unity.WebRTC.Editor
{
    public class PeerStatsView
    {
        private RTCPeerConnection m_peerConnection;
        private RTCStatsReport m_statsReport;
        private Thread m_getStatsThred;

        //demo
        private List<float> m_data = Enumerable.Range(0, 100).Select(x => (float)Random.Range(0, 100)).ToList();

        public PeerStatsView(RTCPeerConnection peer)
        {
            m_peerConnection = peer;
            // m_getStatsThred = new Thread(GetStats);
            // m_getStatsThred.Start();
        }

        private void GetStats()
        {
            while (true)
            {
                var op = m_peerConnection.GetStats();
                while (op.MoveNext())
                {
                }

                if (!op.IsError)
                {
                    m_statsReport = op.Value;
                }

                Thread.Sleep(1000);
            }
        }

        public VisualElement Create()
        {
            var view = new VisualElement();
            var list = Enum.GetValues(typeof(RTCStatsType)).Cast<RTCStatsType>().ToList();
            var popup = new PopupField<RTCStatsType>(list, 0);
            view.Add(popup);

            var container = new VisualElement();
            container.Add(new Label($"{RTCStatsType.Codec}"));
            view.Add(container);

            popup.RegisterValueChangedCallback(e =>
            {
                Debug.Log($"new choose stats type is {e.newValue}");
                // var stats = m_statsReport.Stats[e.newValue];
                // type 別に status をより具体のクラスに変換
                // container クラス別のViewを生成してAddする
                container.Clear();
                container.Add(new Label($"{e.newValue}"));

                container.Add(new IMGUIContainer(() =>
                    GraphDraw.Draw(GUILayoutUtility.GetRect(Screen.width / 2, 200), m_data)));
            });

            return view;
        }
    }
}
