﻿using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public delegate void OnStatsReportHandler(RTCPeerConnection peer, RTCStatsReport statsReport);

    public class WebRTCInternals : EditorWindow
    {
        [MenuItem("Window/WebRTCInternals")]
        public static void Show()
        {
            WebRTCInternals wnd = GetWindow<WebRTCInternals>();
            wnd.titleContent = new GUIContent("WebRTCInternals");
        }

        private const int UpdateStatsInterval = 1;

        public event OnStatsReportHandler OnStats;

        private EditorCoroutine m_editorCoroutine;

        private void OnEnable()
        {
            var root = this.rootVisualElement;
            root.Add(CreateStatsView());

            EditorApplication.playModeStateChanged += change =>
            {
                switch (change)
                {
                    case PlayModeStateChange.EnteredPlayMode:
                        m_editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(GetStatsPolling());
                        break;
                    case PlayModeStateChange.ExitingPlayMode:
                        EditorCoroutineUtility.StopCoroutine(m_editorCoroutine);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(change), change, null);
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
                    foreach (var peer in peerList)
                    {
                        var op = peer.GetStats();
                        yield return op;

                        if (!op.IsError)
                        {
                            OnStats?.Invoke(peer, op.Value);
                        }
                    }
                }

                yield return new WaitForSeconds(UpdateStatsInterval);
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
            var peerListView = new PeerListView();

            var refreshButton = new Button(() =>
            {
                peerListView.Refresh();
            }) {text = "Refresh Peer List", style = { }};
            sideView.Add(refreshButton);


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
}
