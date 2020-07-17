﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
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
            root.Add(new Button(() =>
            {
                if (!m_peerConenctionDataStore.Any())
                {
                    return;
                }

                var filePath = EditorUtility.SaveFilePanel("Save", "Assets", "dump", "json");

                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                var peerRecord = string.Join(",",
                    m_peerConenctionDataStore.Select(record => $"\"{record.Key}\":{{{record.Value.ToJson()}}}"));
                var json = $"{{\"getUserMedia\":[], \"PeerConnections\":{{{peerRecord}}}, \"UserAgent\":\"UnityEditor\"}}";
                File.WriteAllText(filePath, json);

            }) {text = "DumpExport"});

            root.Add(CreateStatsView());

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
                                m_peerConenctionDataStore[peerId] = new PeerConnectionRecord(peer.GetConfiguration());
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
                style = {borderRightColor = new StyleColor(Color.gray), borderRightWidth = 1, width = 250,}
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
        private RTCConfiguration m_config;
        private Dictionary<(RTCStatsType, string), StatsRecord> m_statsRecordMap;

        public PeerConnectionRecord(RTCConfiguration config)
        {
            m_config = config;
            m_statsRecordMap = new Dictionary<(RTCStatsType, string), StatsRecord>();
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

    public class StatsRecord
    {
        private const int MAX_BUFFER_SIZE = 1000;
        private Dictionary<string, List<(long timeStamp, object value)>> m_memberRecord;
        private string m_id;

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
