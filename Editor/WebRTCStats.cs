using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal delegate void OnPeerListHandler(IEnumerable<WeakReference<RTCPeerConnection>> peerList);

    internal delegate void OnStatsReportHandler(RTCPeerConnection peer, RTCStatsReport statsReport);

    internal class WebRTCStats : EditorWindow
    {
        [MenuItem("Window/Analysis/WebRTC Stats")]
        public static void Init()
        {
            WebRTCStats wnd = GetWindow<WebRTCStats>();
            wnd.titleContent = new GUIContent("WebRTC Stats");
        }

        private const int UpdateStatsInterval = 1;
        private static readonly Color BackgroundColorInProSkin = new Color(45 / 255f, 45 / 255f, 45 / 255f);

        public event OnPeerListHandler OnPeerList;
        public event OnStatsReportHandler OnStats;

        private EditorCoroutine m_editorCoroutine;

        private Dictionary<int, PeerConnectionRecord> m_peerConnenctionDataStore =
            new Dictionary<int, PeerConnectionRecord>();

        private void OnEnable()
        {
            var root = this.rootVisualElement;
            root.style.backgroundColor = EditorGUIUtility.isProSkin ? BackgroundColorInProSkin : Color.white;

            var toolbar = new Toolbar {style = {alignItems = Align.FlexEnd}};
            root.Add(toolbar);

            toolbar.Add(new ToolbarSpacer {flex = true});

            var buttonContainer = new VisualElement
            {
                tooltip = "Save current webrtc stats information to a json file",
            };
            toolbar.Add(buttonContainer);

            var dumpButton = new ToolbarButton(() =>
            {
                if (!m_peerConnenctionDataStore.Any())
                {
                    return;
                }

                var filePath = EditorUtility.SaveFilePanel("Save", "Assets", "dump", "json");

                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                var peerRecord = string.Join(",",
                    m_peerConnenctionDataStore.Select(record => $"\"{record.Key}\":{{{record.Value.ToJson()}}}"));
                var json =
                    $"{{\"getUserMedia\":[], \"PeerConnections\":{{{peerRecord}}}, \"UserAgent\":\"UnityEditor\"}}";
                File.WriteAllText(filePath, json);

            })
            { text = "Save"};
            buttonContainer.Add(dumpButton);

            root.Add(CreateStatsView());

            EditorApplication.update += () =>
            {
                dumpButton.SetEnabled(m_peerConnenctionDataStore.Any());
            };

            EditorApplication.playModeStateChanged += change =>
            {
                switch (change)
                {
                    case PlayModeStateChange.EnteredPlayMode:
                        m_peerConnenctionDataStore.Clear();
                        m_editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(GetStatsPolling());
                        break;
                    case PlayModeStateChange.ExitingPlayMode:
                        EditorCoroutineUtility.StopCoroutine(m_editorCoroutine);
                        break;
                }
            };
        }

        private void OnDisable()
        {
            if (m_editorCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(m_editorCoroutine);
            }

            m_peerConnenctionDataStore.Clear();
        }

        IEnumerator GetStatsPolling()
        {
            while (true)
            {
                var peerList = WebRTC.PeerList;

                if (peerList != null)
                {
                    OnPeerList?.Invoke(peerList);

                    foreach (var weakReference in peerList)
                    {
                        if (!weakReference.TryGetTarget(out var peer))
                        {
                            continue;
                        }

                        var op = peer.GetStats();
                        yield return op;

                        if (!op.IsError)
                        {
                            OnStats?.Invoke(peer, op.Value);

                            var peerId = peer.GetHashCode();
                            if (!m_peerConnenctionDataStore.ContainsKey(peerId))
                            {
                                m_peerConnenctionDataStore[peerId] = new PeerConnectionRecord(peer.GetConfiguration());
                            }

                            m_peerConnenctionDataStore[peerId].Update(op.Value);
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
                style = {borderRightColor = Color.gray, borderRightWidth = 1, width = 250,}
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

            mainView.Add(new Label("Statistics are displayed when in play mode"));

            return container;
        }
    }

    internal class PeerConnectionRecord
    {
        private readonly RTCConfiguration m_config;
        private readonly Dictionary<string, StatsRecord> m_statsRecordMap;

        public PeerConnectionRecord(RTCConfiguration config)
        {
            m_config = config;
            m_statsRecordMap = new Dictionary<string, StatsRecord>();
        }

        public void Update(RTCStatsReport report)
        {
            foreach (var element in report.Stats)
            {
                if (!m_statsRecordMap.ContainsKey(element.Key))
                {
                    m_statsRecordMap[element.Key] = new StatsRecord(element.Value.Id);
                }

                m_statsRecordMap[element.Key].Update(element.Value.Timestamp, element.Value.Dict);
            }
        }

        public string ToJson()
        {
            var constraintsJson = "\"constraints\": \"\"";
            var configJson = $"\"rtcConfiguration\":{JsonUtility.ToJson(m_config)}";
            var statsJson = $"\"stats\":{{{string.Join(",", m_statsRecordMap.Select(x => x.Value.ToJson()))}}}";
            var url = "\"url\":\"\"";
            var updateLog = "\"updateLog\":[]";
            return string.Join(",", constraintsJson, configJson, statsJson, url, updateLog);
        }
    }

    internal class StatsRecord
    {
        private const int MAX_BUFFER_SIZE = 1000;
        private readonly Dictionary<string, List<(long timeStamp, object value)>> m_memberRecord;
        private readonly string m_id;

        public StatsRecord(string id)
        {
            m_id = id;
            m_memberRecord = new Dictionary<string, List<(long, object)>>();
        }

        public void Update(long timeStamp, IDictionary<string, object> record)
        {
            foreach (var pair in record)
            {
                if (!m_memberRecord.ContainsKey((pair.Key)))
                {
                    m_memberRecord[pair.Key] = new List<(long, object)>();
                }

                var target = m_memberRecord[pair.Key];
                if (target.Count > MAX_BUFFER_SIZE)
                {
                    target.RemoveAt(0);
                }

                target.Add((timeStamp, pair.Value));
            }
        }

        public string ToJson()
        {
            return string.Join(",", m_memberRecord.Select(x =>
            {
                var start = DateTimeOffset.FromUnixTimeMilliseconds(x.Value.Min(y => y.timeStamp)/1000).DateTime.ToUniversalTime().ToString("O");
                var end = DateTimeOffset.FromUnixTimeMilliseconds(x.Value.Max(y => y.timeStamp)/1000).DateTime.ToUniversalTime().ToString("O");
                var values = string.Join(",", x.Value.Select(y =>
                {
                    if (y.value is string z && !string.IsNullOrEmpty(z))
                    {
                        return $"\\\"{z}\\\"";
                    }

                    if (y.value is bool b)
                    {
                        return b.ToString().ToLower();
                    }

                    return y.value;
                }).Where(y =>
                {
                    if (y is string z)
                    {
                        return !string.IsNullOrEmpty(z);
                    }

                    return y != null;
                }));

                return string.IsNullOrEmpty(values) ? "" : $"\"{m_id}-{x.Key}\":{{\"startTime\":\"{start}\", \"endTime\":\"{end}\", \"values\":\"[{values}]\"}}";
            }).Where(x => !string.IsNullOrEmpty(x)));
        }
    }
}
