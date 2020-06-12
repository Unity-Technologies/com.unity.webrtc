using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    public delegate void OnRefreshPeerListHandler(IEnumerable<RTCPeerConnection> peerList);
    public delegate void OnChangeSelectPeerHandler(RTCPeerConnection peer);

    public class PeerListView
    {
        public event OnRefreshPeerListHandler OnRefreshPeerList;
        public event OnChangeSelectPeerHandler OnChangePeer;

        public void Refresh()
        {
            var list = WebRTC.PeerList;

            if (list == null)
            {
                return;
            }

            OnRefreshPeerList?.Invoke(list);
        }

        public VisualElement Create()
        {
            var root = new ScrollView();

            var container = new VisualElement();
            root.Add(container);

            OnRefreshPeerList += peerList =>
            {
                container.Clear();
                foreach (var peerConnection in peerList)
                {
                    var button = new Button(() =>
                    {
                        OnChangePeer?.Invoke(peerConnection);
                    }) {text = $"peer {peerConnection.GetHashCode()}"};

                    container.Add(button);
                }
            };

            return root;
        }
    }
}
