using UnityEngine;
using System;
using System.Collections.Generic;

namespace Unity.WebRTC
{
    public delegate void DelegateOnIceCandidate(RTCIceCandidate candidate);
    public delegate void DelegateOnIceConnectionChange(RTCIceConnectionState state);
    public delegate void DelegateOnNegotiationNeeded();
    public delegate void DelegateOnTrack(RTCTrackEvent e);
    public delegate void DelegateSetSessionDescSuccess();
    public delegate void DelegateSetSessionDescFailure(RTCError error);

    /// <summary>
    /// Represents a WebRTC connection between the local peer and remote peer.
    /// </summary>
    ///
    /// <remarks>
    ///
    /// </remarks>
    ///
    public class RTCPeerConnection : IDisposable
    {
        internal IntPtr self;

        internal Action<IntPtr> OnStatsDelivered = null;

        private int m_id;
        private DelegateOnIceConnectionChange onIceConnectionChange;
        private DelegateOnIceCandidate onIceCandidate;
        private DelegateOnDataChannel onDataChannel;
        private DelegateOnTrack onTrack;
        private DelegateOnNegotiationNeeded onNegotiationNeeded;
        private DelegateSetSessionDescSuccess onSetSessionDescSuccess;
        private DelegateSetSessionDescFailure onSetSetSessionDescFailure;

        private RTCSessionDescriptionAsyncOperation m_opSessionDesc;
        private RTCSessionDescriptionAsyncOperation m_opSetRemoteDesc;

        private bool disposed;

        ~RTCPeerConnection()
        {
            this.Dispose();
            WebRTC.Table.Remove(self);
        }

        /// <summary>
        ///
        /// </summary>
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

        /// <summary>
        /// The readonly property of the <see cref="RTCPeerConnection"/> indicates
        /// the current state of the peer connection by returning one of the
        /// <see cref="RTCIceConnectionState"/> enum.
        /// </summary>
        /// <example>
        /// <code>
        /// var peerConnection = new RTCPeerConnection(configuration);
        /// var iceConnectionState = peerConnection.IceConnectionState;
        /// </code>
        /// </example>
        /// <seealso cref="ConnectionState"/>
        public RTCIceConnectionState IceConnectionState
        {
            get
            {
                return NativeMethods.PeerConnectionIceConditionState(self);
            }
        }

        /// <summary>
        /// The readonly property of the <see cref="RTCPeerConnection"/> indicates
        /// the current state of the peer connection by returning one of the
        /// <see cref="RTCPeerConnectionState"/> enum.
        /// </summary>
        /// <example>
        /// <code>
        /// var peerConnection = new RTCPeerConnection(configuration);
        /// var connectionState = peerConnection.ConnectionState;
        /// </code>
        /// </example>
        /// <seealso cref="IceConnectionState"/>
        public RTCPeerConnectionState ConnectionState
        {
            get
            {
                return NativeMethods.PeerConnectionState(self);
            }
        }

        /// <summary>
        /// The readonly property of the <see cref="RTCPeerConnection"/> indicates
        /// the current state of the peer connection by returning one of the
        /// <see cref="RTCSignalingState"/> enum.
        /// </summary>
        /// <example>
        /// <code>
        /// var peerConnection = new RTCPeerConnection(configuration);
        /// var signalingState = peerConnection.SignalingState;
        /// </code>
        /// </example>
        /// <seealso cref="ConnectionState"/>
        public RTCSignalingState SignalingState
        {
            get
            {
                return NativeMethods.PeerConnectionSignalingState(self);
            }
        }

        /// <summary>
        /// Returns array of objects each of which represents one RTP receiver.
        /// </summary>
        /// <example>
        /// <code>
        /// var senders = peerConnection.GetReceivers();
        /// </code>
        /// </example>
        /// <returns> Array of the senders </returns>
        /// <seealso cref="GetSenders()"/>
        /// <seealso cref="GetTransceivers()"/>
        public IEnumerable<RTCRtpReceiver> GetReceivers()
        {
            uint length = 0;
            var buf = NativeMethods.PeerConnectionGetReceivers(self, ref length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new RTCRtpReceiver(ptr, this));
        }

        /// <summary>
        /// Returns array of objects each of which represents one RTP sender.
        /// </summary>
        /// <example>
        /// <code>
        /// var senders = peerConnection.GetSenders();
        /// </code>
        /// </example>
        /// <returns> Array of the receivers </returns>
        /// <seealso cref="GetReceivers()"/>
        /// <seealso cref="GetTransceivers()"/>
        public IEnumerable<RTCRtpSender> GetSenders()
        {
            uint length = 0;
            var buf = NativeMethods.PeerConnectionGetSenders(self, ref length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new RTCRtpSender(ptr, this));
        }

        /// <summary>
        /// Returns array of objects each of which represents one RTP transceiver.
        /// </summary>
        /// <example>
        /// <code>
        /// var transceivers = peerConnection.GetTransceivers();
        /// </code>
        /// </example>
        /// <returns> Array of the transceivers </returns>
        /// <seealso cref="GetSenders()"/>
        /// <seealso cref="GetReceivers()"/>
        public IEnumerable<RTCRtpTransceiver> GetTransceivers()
        {
            uint length = 0;
            var buf = NativeMethods.PeerConnectionGetTransceivers(self, ref length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new RTCRtpTransceiver(ptr, this));
        }

        /// <summary>
        /// This property is delegate to be called when the <see cref ="IceConnectionState"/> is changed.
        /// </summary>
        /// <returns> A delegate containing <see cref="IceConnectionState"/>. </returns>
        /// <example>
        /// <code>
        /// peerConnection.OnIceConnectionChange = iceConnectionState =>
        /// {
        ///     ...
        /// };
        /// </code>
        /// </example>
        /// <seealso cref="IceConnectionState"/>
        public DelegateOnIceConnectionChange OnIceConnectionChange
        {
            private get => onIceConnectionChange;
            set
            {
                onIceConnectionChange = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="RTCIceCandidate"/>
        public DelegateOnIceCandidate OnIceCandidate
        {
            private get => onIceCandidate;
            set
            {
                onIceCandidate = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public DelegateOnDataChannel OnDataChannel
        {
            private get => onDataChannel;
            set
            {
                onDataChannel = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public DelegateOnNegotiationNeeded OnNegotiationNeeded
        {
            private get => onNegotiationNeeded;
            set
            {
                onNegotiationNeeded = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public DelegateOnTrack OnTrack
        {
            private get => onTrack;
            set
            {
                onTrack = value;
            }
        }

        internal DelegateSetSessionDescSuccess OnSetSessionDescriptionSuccess
        {
            private get => onSetSessionDescSuccess;
            set
            {
                onSetSessionDescSuccess = value;
            }
        }

        internal DelegateSetSessionDescFailure OnSetSessionDescriptionFailure
        {
            private get => onSetSetSessionDescFailure;
            set
            {
                onSetSetSessionDescFailure = value;
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
                        new RTCIceCandidate { candidate = sdp, sdpMid = sdpMid, sdpMLineIndex = sdpMlineIndex };
                    connection.OnIceCandidate?.Invoke(candidate);
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
                    connection.OnIceConnectionChange?.Invoke(state);
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
                    connection.OnNegotiationNeeded?.Invoke();
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
                    connection.OnDataChannel?.Invoke(new RTCDataChannel(ptrChannel, connection));
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
                    connection.OnTrack?.Invoke(new RTCTrackEvent(transceiver, connection));
                }
            });
        }

        /// <summary>
        /// Returns an object which indicates the current configuration
        /// of the <see cref="RTCPeerConnection"/>.
        /// </summary>
        /// <returns> An object describing the <see cref="RTCPeerConnection"/>'s
        /// current configuration. </returns>
        /// <example>
        /// <code>
        /// var configuration = myPeerConnection.GetConfiguration();
        /// if(configuration.urls.length == 0)
        /// {
        ///     configuration.urls = new[] {"stun:stun.l.google.com:19302"};
        /// }
        /// myPeerConnection.SetConfiguration(configuration);
        /// </code>
        /// </example>
        /// <seealso cref="SetConfiguration(ref RTCConfiguration)"/>
        public RTCConfiguration GetConfiguration()
        {
            IntPtr ptr = NativeMethods.PeerConnectionGetConfiguration(self);
            string str = ptr.AsAnsiStringWithFreeMem();
            return JsonUtility.FromJson<RTCConfiguration>(str);
        }

        /// <summary>
        /// This method sets the current configuration of the <see cref="RTCPeerConnection"/>
        /// This lets you change the ICE servers used by the connection
        /// and which transport policies to use.
        /// </summary>
        /// <param name="configuration">The changes are not additive; instead,
        /// the new values completely replace the existing ones.</param>
        /// <returns> Error code. </returns>
        /// <example>
        /// <code>
        /// var configuration = new RTCConfiguration
        /// {
        ///     iceServers = new[]
        ///     {
        ///         new RTCIceServer
        ///         {
        ///             urls = new[] {"stun:stun.l.google.com:19302"},
        ///             username = "",
        ///             credential = "",
        ///             credentialType = RTCIceCredentialType.Password
        ///         }
        ///     }
        /// };
        /// var error = myPeerConnection.SetConfiguration(ref configuration);
        /// if(error == RTCErrorType.None)
        /// {
        ///     ...
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="GetConfiguration()"/>
        public RTCErrorType SetConfiguration(ref RTCConfiguration configuration)
        {
            return NativeMethods.PeerConnectionSetConfiguration(self, JsonUtility.ToJson(configuration));
        }

        /// <summary>
        /// This constructor creates an instance of peer connection with a default configuration.
        /// </summary>
        /// <seealso cref="RTCPeerConnection(ref RTCConfiguration)"/>
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

        /// <summary>
        /// This constructor creates an instance of peer connection with a configuration provided by user.
        /// An <seealso cref="RTCConfiguration "/> object providing options to configure the new connection.
        /// </summary>
        /// <param name="configuration"></param>
        /// <seealso cref="RTCPeerConnection()"/>
        public RTCPeerConnection(ref RTCConfiguration configuration)
        {
            string configStr = JsonUtility.ToJson(configuration);
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
            NativeMethods.PeerConnectionRegisterCallbackCreateSD(self, OnSuccessCreateSessionDesc, OnFailureCreateSessionDesc);
            NativeMethods.PeerConnectionRegisterCallbackCollectStats(self, OnStatsDeliveredCallback);
            NativeMethods.PeerConnectionRegisterIceConnectionChange(self, PCOnIceConnectionChange);
            NativeMethods.PeerConnectionRegisterOnIceCandidate(self, PCOnIceCandidate);
            NativeMethods.PeerConnectionRegisterOnDataChannel(self, PCOnDataChannel);
            NativeMethods.PeerConnectionRegisterOnRenegotiationNeeded(self, PCOnNegotiationNeeded);
            NativeMethods.PeerConnectionRegisterOnTrack(self, PCOnTrack);
            WebRTC.Context.PeerConnectionRegisterOnSetSessionDescSuccess(
                self, OnSetSessionDescSuccess);
            WebRTC.Context.PeerConnectionRegisterOnSetSessionDescFailure(
                self, OnSetSessionDescFailure);
        }

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="Dispose"/>
        public void Close()
        {
            if (self != IntPtr.Zero)
            {
                NativeMethods.PeerConnectionClose(self);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="track"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <seealso cref="RemoveTrack"/>
        public RTCRtpSender AddTrack(MediaStreamTrack track, MediaStream stream = null)
        {
            if (track == null)
            {
                throw new ArgumentNullException("");
            }

            var streamId = stream == null ? Guid.NewGuid().ToString() : stream.Id;
            return new RTCRtpSender(NativeMethods.PeerConnectionAddTrack(self, track.self, streamId), this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <seealso cref="AddTrack"/>
        public void RemoveTrack(RTCRtpSender sender)
        {
            NativeMethods.PeerConnectionRemoveTrack(self, sender.self);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public RTCRtpTransceiver AddTransceiver(MediaStreamTrack track)
        {
            return new RTCRtpTransceiver(NativeMethods.PeerConnectionAddTransceiver(self, track.self), this);
        }

        public RTCRtpTransceiver AddTransceiver(TrackKind kind)
        {
            return new RTCRtpTransceiver(NativeMethods.PeerConnectionAddTransceiverWithType(self, kind), this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="candidate"></param>
        public void AddIceCandidate(ref RTCIceCandidate candidate)
        {
            NativeMethods.PeerConnectionAddIceCandidate(self, ref candidate);
        }

        /// <summary>
        /// Create an SDP (Session Description Protocol) offer to start a new connection
        /// to a remote peer.
        /// </summary>
        /// <param name="options"> A parameter to request for the offer. </param>
        /// <returns></returns>
        /// <seealso cref="CreateAnswer"/>
        public RTCSessionDescriptionAsyncOperation CreateOffer(ref RTCOfferOptions options)
        {
            m_opSessionDesc = new RTCSessionDescriptionAsyncOperation();
            NativeMethods.PeerConnectionCreateOffer(self, ref options);
            return m_opSessionDesc;
        }

        /// <summary>
        /// Create an SDP (Session Description Protocol) answer to start a new connection
        /// to a remote peer.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public RTCSessionDescriptionAsyncOperation CreateAnswer(ref RTCAnswerOptions options)
        {
            m_opSessionDesc = new RTCSessionDescriptionAsyncOperation();
            NativeMethods.PeerConnectionCreateAnswer(self, ref options);
            return m_opSessionDesc;
        }

        /// <summary>
        /// Creates a new data channel related the remote peer.
        /// </summary>
        /// <param name="label"> A string for the data channel.
        /// This string may be checked by <see cref="RTCDataChannel.Label"/>. </param>
        /// <param name="options"> A struct provides configuration options for the data channel. </param>
        /// <returns> A new data channel. </returns>
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
        static void OnFailureCreateSessionDesc(IntPtr ptr, RTCErrorType type, string message)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.m_opSessionDesc.IsError = true;
                    connection.m_opSessionDesc.Error = new RTCError{errorType = type, message = message};
                    connection.m_opSessionDesc.Done();
                }
            });
        }

        /// <summary>
        /// This method changes the session description
        /// of the local connection to negotiate with other connections.
        /// </summary>
        /// <param name="desc"></param>
        /// <returns>
        /// An AsyncOperation which resolves with an <see cref="RTCSessionDescription"/>
        /// object providing a description of the session.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when an argument has an invalid value.
        /// For example, when passed the sdp which is null or empty.
        /// </exception>
        /// <exception cref="RTCErrorException">
        /// Thrown when an argument has an invalid value.
        /// For example, when passed the sdp which is not be able to parse.
        /// </exception>
        /// <seealso cref="LocalDescription"/>
        public RTCSetSessionDescriptionAsyncOperation SetLocalDescription(
            ref RTCSessionDescription desc)
        {
            if(string.IsNullOrEmpty(desc.sdp))
                throw new ArgumentException("sdp is null or empty");

            var op = new RTCSetSessionDescriptionAsyncOperation(this);
            RTCError error = WebRTC.Context.PeerConnectionSetLocalDescription(
                self, ref desc);
            if (error.errorType == RTCErrorType.None)
            {
                return op;
            }
            throw new RTCErrorException(ref error);
        }


        /// <summary>
        /// This method changes the session description
        /// of the remote connection to negotiate with local connections.
        /// </summary>
        /// <param name="desc"></param>
        /// <returns>
        /// An AsyncOperation which resolves with an <see cref="RTCSessionDescription"/>
        /// object providing a description of the session.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when an argument has an invalid value.
        /// For example, when passed the sdp which is null or empty.
        /// </exception>
        /// <exception cref="RTCErrorException">
        /// Thrown when an argument has an invalid value.
        /// For example, when passed the sdp which is not be able to parse.
        /// </exception>
        /// <seealso cref="RemoteDescription"/>
        public RTCSetSessionDescriptionAsyncOperation SetRemoteDescription(
            ref RTCSessionDescription desc)
        {
            if (string.IsNullOrEmpty(desc.sdp))
                throw new ArgumentException("sdp is null or empty");

            var op = new RTCSetSessionDescriptionAsyncOperation(this);
            RTCError error = WebRTC.Context.PeerConnectionSetRemoteDescription(
                self, ref desc);
            if (error.errorType == RTCErrorType.None)
            {
                return op;
            }
            throw new RTCErrorException(ref error);
        }

        /// <summary>
        /// Returns an AsyncOperation which resolves with data providing statistics.
        /// </summary>
        /// <returns>
        /// An AsyncOperation which resolves with an <see cref="RTCStatsReport"/>
        /// object providing connection statistics.
        /// </returns>
        /// <example>
        /// <code>
        /// // Already instantiated peerConnection as RTCPeerConnection.
        /// var operation = peerConnection.GetStats();
        /// yield return operation;
        ///
        /// if (!operation.IsError)
        /// {
        ///     var report = operation.Value;
        ///     foreach (var stat in report.Stats.Values)
        ///     {
        ///         Debug.Log(stat.Type.ToString());
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="RTCStatsReport"/>
        public RTCStatsReportAsyncOperation GetStats()
        {
            return new RTCStatsReportAsyncOperation(this);
        }

        internal RTCStatsReportAsyncOperation GetStats(RTCRtpSender sender)
        {
            return new RTCStatsReportAsyncOperation(this, sender);
        }
        internal RTCStatsReportAsyncOperation GetStats(RTCRtpReceiver receiver)
        {
            return new RTCStatsReportAsyncOperation(this, receiver);
        }

        /// <summary>
        ///
        /// </summary>
        public RTCSessionDescription LocalDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if (NativeMethods.PeerConnectionGetLocalDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("LocalDescription is not exist");
            }
        }

        /// <summary>
        ///
        /// </summary>
        public RTCSessionDescription RemoteDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if (NativeMethods.PeerConnectionGetRemoteDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("RemoteDescription is not exist");
            }
        }

        /// <summary>
        ///
        /// </summary>
        public RTCSessionDescription CurrentLocalDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if (NativeMethods.PeerConnectionGetCurrentLocalDescription(self, ref desc))
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

        /// <summary>
        ///
        /// </summary>
        public RTCSessionDescription PendingLocalDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if (NativeMethods.PeerConnectionGetPendingLocalDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("PendingLocalDescription is not exist");
            }
        }

        /// <summary>
        ///
        /// </summary>
        public RTCSessionDescription PendingRemoteDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if (NativeMethods.PeerConnectionGetPendingRemoteDescription(self, ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("PendingRemoteDescription is not exist");
            }
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
        static void OnSetSessionDescFailure(IntPtr ptr, RTCErrorType type, string message)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    RTCError error = new RTCError { errorType = type, message = message };
                    connection.OnSetSessionDescriptionFailure(error);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateCollectStats))]
        static void OnStatsDeliveredCallback(IntPtr ptr, IntPtr report)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnStatsDelivered(report);
                }
            });
        }
    }
}
