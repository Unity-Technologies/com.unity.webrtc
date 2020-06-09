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
            //var list = WebRTC.PeerList;
            var list = new List<RTCPeerConnection>
            {
                new RTCPeerConnection(IntPtr.Zero),
                new RTCPeerConnection(IntPtr.Zero),
                new RTCPeerConnection(IntPtr.Zero),
                new RTCPeerConnection(IntPtr.Zero),
                new RTCPeerConnection(IntPtr.Zero),
                new RTCPeerConnection(IntPtr.Zero),
            };

            if (list == null)
            {
                return;
            }

            OnRefreshPeerList?.Invoke(list);
        }

        public VisualElement Create()
        {
            var root = new VisualElement();

            foreach (var peer in Enumerable.Range(0, 5))
            {
                root.Add(new Label($"peer list {peer}"));
            }

            var container = new VisualElement();
            root.Add(container);

            OnRefreshPeerList += peerList =>
            {
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
