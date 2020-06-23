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
            }){text = "jsontest"});

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
        private List<RTCStatsReport> m_statsReportList;

        public PeerConnectionRecord()
        {
            m_statsReportList = new List<RTCStatsReport>();
        }

        public void Update(RTCStatsReport report)
        {
            if (m_statsReportList.Count > MAX_BUFFER_SIZE)
            {
                m_statsReportList.RemoveAt(0);
            }

            m_statsReportList.Add(report);
        }

        public string ToJson()
        {
            var map = new Dictionary<(RTCStatsType, string), List<RTCStats>>();
            foreach (var element in m_statsReportList.SelectMany(report => report.Stats))
            {
                if (!map.ContainsKey(element.Key))
                {
                    map[element.Key] = new List<RTCStats>();
                }

                map[element.Key].Add(element.Value);
            }

            var hoge = map.SelectMany(x =>
            {
                var prefix = $"{x.Key.Item1}_{x.Key.Item2}";
                var sorted = x.Value.OrderByDescending(y => y.Timestamp);
                var start = sorted.First().Timestamp;
                var end = sorted.Last().Timestamp;

                var memberMap = new Dictionary<string, List<object>>();

                foreach (var element in sorted.SelectMany(y => y.Dict))
                {
                    if (!memberMap.ContainsKey(element.Key))
                    {
                        memberMap[element.Key] = new List<object>();
                    }

                    memberMap[element.Key].Add(element.Value);
                }

                return memberMap.Select(y => $"{prefix}-{y.Key}:{{\"startTime\":\"{start}\", \"endTime\":\"{end}\", \"values\":\"[{string.Join(",", y.Value)}]\"}}");
            });



            return $"\"stats\":{{{string.Join(",", hoge)}}}";
        }
    }
}
