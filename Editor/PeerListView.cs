using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.WebRTC.Editor
{
    internal delegate void OnChangeSelectPeerHandler(RTCPeerConnection peer);

    internal class PeerListView
    {
        private static readonly Color ButtonBackground = new Color(70 / 255f, 70 / 255f, 70 / 255f);

        public event OnChangeSelectPeerHandler OnChangePeer;

        private WebRTCStats m_parent;

        public PeerListView(WebRTCStats parent)
        {
            m_parent = parent;
        }

        public VisualElement Create()
        {
            var root = new ScrollView();

            var container = new VisualElement();
            root.Add(container);

            m_parent.OnPeerList += peerList =>
            {
                container.Clear();
                foreach (var weakReference in peerList)
                {
                    if (!weakReference.TryGetTarget(out var peerConnection))
                    {
                        continue;
                    }

                    var button = new Button(() =>
                    {
                        OnChangePeer?.Invoke(peerConnection);
                    })
                    { text = $"peer {peerConnection.GetHashCode()}", };

                    if (EditorGUIUtility.isProSkin)
                    {
                        button.style.backgroundColor = ButtonBackground;
                    }

                    container.Add(button);
                }
            };

            return root;
        }
    }
}
