using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public delegate void OnPeerListHandler(IEnumerable<RTCPeerConnection> peerList);

    public delegate void OnStatsReportHandler(RTCPeerConnection peer, RTCStatsReport statsReport);

    public class WebRTCInternals : EditorWindow
    {
        [MenuItem("Window/Analysis/WebRTCInternals")]
        public static void Show()
        {
            WebRTCInternals wnd = GetWindow<WebRTCInternals>();
            wnd.titleContent = new GUIContent("WebRTCInternals");
        }

        private const int UpdateStatsInterval = 1;

        public event OnPeerListHandler OnPeerList;
        public event OnStatsReportHandler OnStats;

        private EditorCoroutine m_editorCoroutine;

        private Dictionary<int, PeerConnectionRecord> m_peerConenctionDataStore =
            new Dictionary<int, PeerConnectionRecord>();

        private void OnEnable()
        {
            var root = this.rootVisualElement;
            root.Add(CreateStatsView());
            root.Add(new Button(() =>
            {
                foreach (var record in m_peerConenctionDataStore)
                {
                    Debug.Log(record.Value.ToJson());
                }
            }) {text = "jsontest"});

            EditorApplication.playModeStateChanged += change =>
            {
                switch (change)
                {
                    case PlayModeStateChange.EnteredPlayMode:
                        m_editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(GetStatsPolling());
                        break;
                    case PlayModeStateChange.ExitingPlayMode:
                        m_peerConenctionDataStore.Clear();
                        EditorCoroutineUtility.StopCoroutine(m_editorCoroutine);
                        break;
                }
            };
        }

        private void OnDisable()
        {
            EditorCoroutineUtility.StopCoroutine(m_editorCoroutine);
        }

        IEnumerator GetStatsPolling()
        {
            while (true)
            {
                var peerList = WebRTC.PeerList;

                if (peerList != null)
                {
                    OnPeerList?.Invoke(peerList);

                    foreach (var peer in peerList)
                    {
                        var op = peer.GetStats();
                        yield return op;

                        if (!op.IsError)
                        {
                            OnStats?.Invoke(peer, op.Value);

                            var peerId = peer.GetHashCode();
                            if (!m_peerConenctionDataStore.ContainsKey(peerId))
                            {
                                m_peerConenctionDataStore[peerId] = new PeerConnectionRecord();
                            }

                            m_peerConenctionDataStore[peerId].Update(op.Value);
                        }
                    }
                }

                yield return new EditorWaitForSeconds(UpdateStatsInterval);
            }
        }

        private VisualElement CreateStatsView()
        {
            var container = new VisualElement {style = {flexDirection = FlexDirection.Row, flexGrow = 1,}};

            var sideView = new VisualElement
            {
                style = {borderColor = new StyleColor(Color.gray), borderRightWidth = 1, width = 250,}
            };
            var mainView = new VisualElement {style = {flexGrow = 1}};

            container.Add(sideView);
            container.Add(mainView);

            // peer connection list view
            var peerListView = new PeerListView(this);

            sideView.Add(peerListView.Create());

            peerListView.OnChangePeer += newPeer =>
            {
                mainView.Clear();

                // main stats view
                var statsView = new PeerStatsView(newPeer, this);
                mainView.Add(statsView.Create());
            };

            return container;
        }
    }

    public class PeerConnectionRecord
    {
        private const int MAX_BUFFER_SIZE = 1000;
        private Dictionary<(RTCStatsType, string), Dictionary<string, List<object>>> m_report;

        public PeerConnectionRecord()
        {
            m_report = new Dictionary<(RTCStatsType, string), Dictionary<string, List<object>>>();
        }

        public void Update(RTCStatsReport report)
        {
            foreach (var element in report.Stats)
            {
                if (!m_report.ContainsKey(element.Key))
                {
                    m_report[element.Key] = new Dictionary<string, List<object>>();
                }

                foreach (var pair in element.Value.Dict)
                {
                    var map = m_report[element.Key];
                    if (!map.ContainsKey(pair.Key))
                    {
                        map[pair.Key] = new List<object>();
                    }

                    var target = map[pair.Key];
                    if (target.Count > MAX_BUFFER_SIZE)
                    {
                        target.RemoveAt(0);
                    }
                    target.Add(pair.Value);
                }
            }
        }

        public string ToJson()
        {
            var values = m_report.SelectMany(x =>
            {
                return x.Value.Select(y =>
                    $"\"{x.Key.Item2}-{y.Key}\":{{\"startTime\":\"{DateTime.Now}\", \"endTime\":\"{DateTime.Now}\", \"values\":\"[{string.Join(",", y.Value)}]\"}}");
            });


            return $"\"stats\":{{{string.Join(",", values)}}}";
        }
    }
}
