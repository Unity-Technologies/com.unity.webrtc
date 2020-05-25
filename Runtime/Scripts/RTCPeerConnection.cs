using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace Unity.WebRTC
{
    public delegate void DelegateOnIceCandidate(RTCIceCandidate​ candidate);
    public delegate void DelegateOnIceConnectionChange(RTCIceConnectionState state);
    public delegate void DelegateOnNegotiationNeeded();
    public delegate void DelegateOnTrack(RTCTrackEvent e);
    public delegate void DelegateSetSessionDescSuccess();
    public delegate void DelegateSetSessionDescFailure(RTCError error);

    public class RTCPeerConnection : IDisposable
    {
        public Action<string> OnStatsDelivered = null;

        private int m_id;
        private IntPtr self;
        private DelegateOnIceConnectionChange onIceConnectionChange;
        private DelegateOnIceCandidate onIceCandidate;
        private DelegateOnDataChannel onDataChannel;
        private DelegateOnTrack onTrack;
        private DelegateOnNegotiationNeeded onNegotiationNeeded;
        private DelegateCreateSDSuccess onCreateSDSuccess;
        private DelegateCreateSDFailure onCreateSDFailure;
        private DelegateSetSessionDescSuccess onSetSessionDescSuccess;
        private DelegateSetSessionDescFailure onSetSetSessionDescFailure;
        private DelegateCollectStats m_onStatsDeliveredCallback;

        private RTCSessionDescriptionAsyncOperation m_opSessionDesc;
        private RTCSessionDescriptionAsyncOperation m_opSetRemoteDesc;

        private bool disposed;

        ~RTCPeerConnection()
        {
            this.Dispose();
            WebRTC.Table.Remove(self);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                Close();
                WebRTC.Context.DeletePeerConnection(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public RTCIceConnectionState IceConnectionState
        {
            get
            {
                return NativeMethods.PeerConnectionIceConditionState(self);
            }
        }

        public RTCPeerConnectionState ConnectionState
        {
            get
            {
                return NativeMethods.PeerConnectionState(self);
            }
        }

        public IEnumerable<RTCRtpReceiver> GetReceivers()
        {
            int length = 0;
            var buf = NativeMethods.PeerConnectionGetReceivers(self, ref length);
            return WebRTC.Deserialize(buf, length, ptr => new RTCRtpReceiver(ptr));
        }

        public IEnumerable<RTCRtpSender> GetSenders()
        {
            int length = 0;
            var buf = NativeMethods.PeerConnectionGetSenders(self, ref length);
            return WebRTC.Deserialize(buf, length, ptr => new RTCRtpSender(ptr));
        }

        public IEnumerable<RTCRtpTransceiver> GetTransceivers()
        {
            int length = 0;
            var buf = NativeMethods.PeerConnectionGetTransceivers(self, ref length);
            return WebRTC.Deserialize(buf, length, ptr => new RTCRtpTransceiver(ptr));
        }

        public DelegateOnIceConnectionChange OnIceConnectionChange
        {
            get => onIceConnectionChange;
            set
            {
                onIceConnectionChange = value;
                NativeMethods.PeerConnectionRegisterIceConnectionChange(self, PCOnIceConnectionChange);
            }
        }

        public DelegateOnIceCandidate OnIceCandidate
        {
            get => onIceCandidate;
            set
            {
                onIceCandidate = value;
                NativeMethods.PeerConnectionRegisterOnIceCandidate(self, PCOnIceCandidate);
            }
        }

        public DelegateOnDataChannel OnDataChannel
        {
            get => onDataChannel;
            set
            {
                onDataChannel = value;
                NativeMethods.PeerConnectionRegisterOnDataChannel(self, PCOnDataChannel);
            }
        }

        public DelegateOnNegotiationNeeded OnNegotiationNeeded
        {
            get => onNegotiationNeeded;
            set
            {
                onNegotiationNeeded = value;
                NativeMethods.PeerConnectionRegisterOnRenegotiationNeeded(self, PCOnNegotiationNeeded);
            }
        }

        public DelegateOnTrack OnTrack
        {
            get => onTrack;
            set
            {
                onTrack = value;
                NativeMethods.PeerConnectionRegisterOnTrack(self, PCOnTrack);
            }
        }

        internal DelegateSetSessionDescSuccess OnSetSessionDescriptionSuccess
        {
            get => onSetSessionDescSuccess;
            set
            {
                onSetSessionDescSuccess = value;
                WebRTC.Context.PeerConnectionRegisterOnSetSessionDescSuccess(self,
                    OnSetSessionDescSuccess);
            }
        }

        internal DelegateSetSessionDescFailure OnSetSessionDescriptionFailure
        {
            get => onSetSetSessionDescFailure;
            set
            {
                onSetSetSessionDescFailure = value;
                WebRTC.Context.PeerConnectionRegisterOnSetSessionDescFailure(self,
                    OnSetSessionDescFailure);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnIceCandidate))]
        static void PCOnIceCandidate(IntPtr ptr, string sdp, string sdpMid, int sdpMlineIndex)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    var candidate =
                        new RTCIceCandidate​ {candidate = sdp, sdpMid = sdpMid, sdpMLineIndex = sdpMlineIndex};
                    connection.OnIceCandidate(candidate);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnIceConnectionChange))]
        static void PCOnIceConnectionChange(IntPtr ptr, RTCIceConnectionState state)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnIceConnectionChange(state);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnNegotiationNeeded))]
        static void PCOnNegotiationNeeded(IntPtr ptr)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnNegotiationNeeded();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnDataChannel))]
        static void PCOnDataChannel(IntPtr ptr, IntPtr ptrChannel)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnDataChannel(new RTCDataChannel(ptrChannel, connection));
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnTrack))]
        static void PCOnTrack(IntPtr ptr, IntPtr transceiver)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnTrack(new RTCTrackEvent(transceiver));
                }
            });
        }

        public RTCConfiguration GetConfiguration()
        {
            var str = NativeMethods.PeerConnectionGetConfiguration(self).AsAnsiStringWithFreeMem();
            return JsonUtility.FromJson<RTCConfiguration>(str);
        }

        public RTCErrorType SetConfiguration(ref RTCConfiguration config)
        {
            return NativeMethods.PeerConnectionSetConfiguration(self, JsonUtility.ToJson(config));
        }

        public RTCPeerConnection()
        {
            self = WebRTC.Context.CreatePeerConnection();
            if (self == IntPtr.Zero)
            {
                throw new ArgumentException("Could not instantiate RTCPeerConnection");
            }

            WebRTC.Table.Add(self, this);
            InitCallback();
        }

        public RTCPeerConnection(ref RTCConfiguration config)
        {
            string configStr = JsonUtility.ToJson(config);
            self = WebRTC.Context.CreatePeerConnection(configStr);
            if (self == IntPtr.Zero)
            {
                throw new ArgumentException("Could not instantiate RTCPeerConnection");
            }

            WebRTC.Table.Add(self, this);
            InitCallback();
        }

        void InitCallback()
        {
            onCreateSDSuccess = OnSuccessCreateSessionDesc;
            onCreateSDFailure = OnFailureCreateSessionDesc;
            m_onStatsDeliveredCallback = OnStatsDeliveredCallback;
            NativeMethods.PeerConnectionRegisterCallbackCreateSD(self, onCreateSDSuccess, onCreateSDFailure);
            NativeMethods.PeerConnectionRegisterCallbackCollectStats(self, m_onStatsDeliveredCallback);
        }

        public void Close()
        {
            if (self != IntPtr.Zero)
            {
                NativeMethods.PeerConnectionClose(self);
            }
        }

        public RTCRtpSender AddTrack(MediaStreamTrack track, MediaStream stream = null)
        {
            if(track == null)
            {
                throw new ArgumentNullException("");
            }

            var streamId = stream == null ? Guid.NewGuid().ToString() : stream.Id;
            return new RTCRtpSender(NativeMethods.PeerConnectionAddTrack(self, track.self, streamId));
        }

        public void RemoveTrack(RTCRtpSender sender)
        {
            NativeMethods.PeerConnectionRemoveTrack(self, sender.self);
        }

        public RTCRtpTransceiver AddTransceiver(MediaStreamTrack track)
        {
            return new RTCRtpTransceiver(NativeMethods.PeerConnectionAddTransceiver(self, track.self));
        }

        public void AddIceCandidate(ref RTCIceCandidate​ candidate)
        {
            NativeMethods.PeerConnectionAddIceCandidate(self, ref candidate);
        }

        public RTCSessionDescriptionAsyncOperation CreateOffer(ref RTCOfferOptions options)
        {
            m_opSessionDesc = new RTCSessionDescriptionAsyncOperation();
            NativeMethods.PeerConnectionCreateOffer(self, ref options);
            return m_opSessionDesc;
        }

        public RTCSessionDescriptionAsyncOperation CreateAnswer(ref RTCAnswerOptions options)
        {
            m_opSessionDesc = new RTCSessionDescriptionAsyncOperation();
            NativeMethods.PeerConnectionCreateAnswer(self, ref options);
            return m_opSessionDesc;
        }

        public RTCDataChannel CreateDataChannel(string label, ref RTCDataChannelInit options)
        {
            IntPtr ptr = WebRTC.Context.CreateDataChannel(self, label, ref options);
            if (ptr == IntPtr.Zero)
                throw new ArgumentException("RTCDataChannelInit object is incorrect.");
            return new RTCDataChannel(ptr, this);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateCreateSDSuccess))]
        static void OnSuccessCreateSessionDesc(IntPtr ptr, RTCSdpType type, string sdp)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.m_opSessionDesc.Desc = new RTCSessionDescription { sdp = sdp, type = type };
                    connection.m_opSessionDesc.Done();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateCreateSDFailure))]
        static void OnFailureCreateSessionDesc(IntPtr ptr)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.m_opSessionDesc.IsError = true;
                    connection.m_opSessionDesc.Done();
                }
            });
        }

        public RTCSetSessionDescriptionAsyncOperation SetLocalDescription(ref RTCSessionDescription desc)
        {
            var op = new RTCSetSessionDescriptionAsyncOperation(this);
            WebRTC.Context.PeerConnectionSetLocalDescription(self, ref desc);
            return op;
        }

        public void CollectStats()
        {
            /// TODO:: define async operation class
            //m_opSetDesc = new RTCSessionDescriptionAsyncOperation();
            //NativeMethods.PeerConnectionCollectStats(self);
        }

        public RTCSessionDescription LocalDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if(NativeMethods.PeerConnectionGetLocalDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("LocalDescription is not exist");
            }
        }

        public RTCSessionDescription RemoteDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if(NativeMethods.PeerConnectionGetRemoteDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("RemoteDescription is not exist");
            }
        }

        public RTCSessionDescription CurrentLocalDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if(NativeMethods.PeerConnectionGetCurrentLocalDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("CurrentLocalDescription is not exist");
            }
        }

        public RTCSessionDescription CurrentRemoteDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if (NativeMethods.PeerConnectionGetCurrentRemoteDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("CurrentRemoteDescription is not exist");
            }
        }
        public RTCSessionDescription PendingLocalDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if(NativeMethods.PeerConnectionGetPendingLocalDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("PendingLocalDescription is not exist");
            }
        }

        public RTCSessionDescription PendingRemoteDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if(NativeMethods.PeerConnectionGetPendingRemoteDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("PendingRemoteDescription is not exist");
            }
        }

        public RTCSetSessionDescriptionAsyncOperation SetRemoteDescription(ref RTCSessionDescription desc)
        {
            var op = new RTCSetSessionDescriptionAsyncOperation(this);
            WebRTC.Context.PeerConnectionSetRemoteDescription(self, ref desc);
            return op;
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativePeerConnectionSetSessionDescSuccess))]
        static void OnSetSessionDescSuccess(IntPtr ptr)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnSetSessionDescriptionSuccess();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativePeerConnectionSetSessionDescFailure))]
        static void OnSetSessionDescFailure(IntPtr ptr, RTCError error)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnSetSessionDescriptionFailure(error);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateCollectStats))]
        static void OnStatsDeliveredCallback(IntPtr ptr, string stats)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnStatsDelivered(stats);
                }
            });
        }
    }
}
