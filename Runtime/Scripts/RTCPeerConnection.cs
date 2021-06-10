using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="candidate"></param>
    public delegate void DelegateOnIceCandidate(RTCIceCandidate candidate);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public delegate void DelegateOnIceConnectionChange(RTCIceConnectionState state);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public delegate void DelegateOnConnectionStateChange(RTCPeerConnectionState state);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    public delegate void DelegateOnIceGatheringStateChange(RTCIceGatheringState state);
    /// <summary>
    /// 
    /// </summary>
    public delegate void DelegateOnNegotiationNeeded();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    public delegate void DelegateOnTrack(RTCTrackEvent e);
    /// <summary>
    /// 
    /// </summary>
    public delegate void DelegateSetSessionDescSuccess();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
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
        private IntPtr self;

        internal Action<IntPtr> OnStatsDelivered = null;

        private RTCSessionDescriptionAsyncOperation m_opSessionDesc;
        private RTCSessionDescriptionAsyncOperation m_opSetRemoteDesc;

        private bool disposed;

        ~RTCPeerConnection()
        {
            this.Dispose();
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
                DisposeAllTransceivers();
                WebRTC.Context.DeletePeerConnection(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        private void DisposeAllTransceivers()
        {
            var transceivers = GetTransceivers();
            foreach (var transceiver in transceivers)
            {
                // Dispose of MediaStreamTrack when disposing of RTCRtpReceiver.
                // On the other hand, do not dispose a track when disposing of RTCRtpSender.
                transceiver.Receiver?.Track?.Dispose();

                transceiver.Receiver?.Dispose();
                transceiver.Sender?.Dispose();
                transceiver.Dispose();
            }
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
        public RTCIceConnectionState IceConnectionState => NativeMethods.PeerConnectionIceConditionState(GetSelfOrThrow());

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
        public RTCPeerConnectionState ConnectionState => NativeMethods.PeerConnectionState(GetSelfOrThrow());

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
        public RTCSignalingState SignalingState => NativeMethods.PeerConnectionSignalingState(GetSelfOrThrow());

        /// <summary>
        /// 
        /// </summary>
        public RTCIceGatheringState GatheringState => NativeMethods.PeerConnectionIceGatheringState(GetSelfOrThrow());

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
#if !UNITY_WEBGL
            IntPtr buf = NativeMethods.PeerConnectionGetReceivers(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, CreateReceiver);
#else
            IntPtr pointers = NativeMethods.PeerConnectionGetReceivers(GetSelfOrThrow());
            var arr = NativeMethods.ptrToIntPtrArray(pointers);
            RTCRtpReceiver[] receivers = new RTCRtpReceiver[arr.Length];
            for (var i = 0; i < arr.Length; i++)
                receivers[i] = new RTCRtpReceiver(arr[i], this);
            return receivers;
#endif
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
#if !UNITY_WEBGL
            var buf = NativeMethods.PeerConnectionGetSenders(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, CreateSender);
#else
            IntPtr pointers = NativeMethods.PeerConnectionGetSenders(GetSelfOrThrow());
            var arr = NativeMethods.ptrToIntPtrArray(pointers);
            RTCRtpSender[] senders = new RTCRtpSender[arr.Length];
            for (var i = 0; i < arr.Length; i++)
                senders[i] = new RTCRtpSender(arr[i], this);
            return senders;
#endif
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
#if !UNITY_WEBGL
            var buf = NativeMethods.PeerConnectionGetTransceivers(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, CreateTransceiver);
#else
            IntPtr pointers = NativeMethods.PeerConnectionGetTransceivers(GetSelfOrThrow());
            var arr = NativeMethods.ptrToIntPtrArray(pointers);
            RTCRtpTransceiver[] transceivers = new RTCRtpTransceiver[arr.Length];
            for (var i = 0; i < arr.Length; i++)
                transceivers[i] = new RTCRtpTransceiver(arr[i], this);
            return transceivers;
#endif
        }

        RTCRtpReceiver CreateReceiver(IntPtr ptr)
        {
            return WebRTC.FindOrCreate(ptr, _ptr => new RTCRtpReceiver(_ptr, this));
        }

        RTCRtpSender CreateSender(IntPtr ptr)
        {
            return WebRTC.FindOrCreate(ptr, _ptr => new RTCRtpSender(_ptr, this));
        }

        RTCRtpTransceiver CreateTransceiver(IntPtr ptr)
        {
            return WebRTC.FindOrCreate(ptr, _ptr => new RTCRtpTransceiver(_ptr, this));
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
        public DelegateOnIceConnectionChange OnIceConnectionChange { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DelegateOnConnectionStateChange OnConnectionStateChange { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="GatheringState"/>
        public DelegateOnIceGatheringStateChange OnIceGatheringStateChange { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="RTCIceCandidate"/>
        public DelegateOnIceCandidate OnIceCandidate { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="RTCDataChannel"/>
        public DelegateOnDataChannel OnDataChannel { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DelegateOnNegotiationNeeded OnNegotiationNeeded { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="RTCTrackEvent"/>
        public DelegateOnTrack OnTrack { get; set; }

        internal IntPtr GetSelfOrThrow()
        {
            if (self == IntPtr.Zero)
            {
                throw new InvalidOperationException("This instance has been disposed.");
            }
            return self;
        }

        internal DelegateSetSessionDescSuccess OnSetSessionDescriptionSuccess { get; set; }

        internal DelegateSetSessionDescFailure OnSetSessionDescriptionFailure { get; set; }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnIceCandidate))]
#if !UNITY_WEBGL
        static void PCOnIceCandidate(IntPtr ptr, string sdp, string sdpMid, int sdpMlineIndex)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    var options = new RTCIceCandidateInit
                    {
                        candidate = sdp,
                        sdpMid = sdpMid,
                        sdpMLineIndex = sdpMlineIndex
                    };
                    var candidate = new RTCIceCandidate(options);
                    connection.OnIceCandidate?.Invoke(candidate);
                }
            });
        }
#else
        static void PCOnIceCandidate(IntPtr ptr, IntPtr iceCandidatePtr, string sdp, string sdpMid, int sdpMlineIndex)
        {
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                var options = new RTCIceCandidateInit
                {
                    candidate = sdp,
                    sdpMid = sdpMid,
                    sdpMLineIndex = sdpMlineIndex
                };
                var candidate = new RTCIceCandidate(options, iceCandidatePtr);
                connection.OnIceCandidate?.Invoke(candidate);
            }
        }
#endif

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnIceConnectionChange))]
        static void PCOnIceConnectionChange(IntPtr ptr, RTCIceConnectionState state)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnIceConnectionChange?.Invoke(state);
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.OnIceConnectionChange?.Invoke(state);
            }
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnConnectionStateChange))]
        static void PCOnConnectionStateChange(IntPtr ptr, RTCPeerConnectionState state)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnConnectionStateChange?.Invoke(state);
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.OnConnectionStateChange?.Invoke(state);
            }
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnIceGatheringChange))]
        static void PCOnIceGatheringChange(IntPtr ptr, RTCIceGatheringState state)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnIceGatheringStateChange?.Invoke(state);
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.OnIceGatheringStateChange?.Invoke(state);
            }
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnNegotiationNeeded))]
        static void PCOnNegotiationNeeded(IntPtr ptr)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnNegotiationNeeded?.Invoke();
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.OnNegotiationNeeded?.Invoke();
            }
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnDataChannel))]
        static void PCOnDataChannel(IntPtr ptr, IntPtr ptrChannel)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnDataChannel?.Invoke(new RTCDataChannel(ptrChannel, connection));
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.OnDataChannel?.Invoke(new RTCDataChannel(ptrChannel, connection));
            }
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnTrack))]
        static void PCOnTrack(IntPtr ptr, IntPtr transceiver)
        {
            Debug.Log("PCOnTrack");
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnTrack?.Invoke(new RTCTrackEvent(transceiver, connection));
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                Debug.Log($"connection.OnTrack:{connection.OnTrack}:{transceiver}");
                connection.OnTrack?.Invoke(new RTCTrackEvent(transceiver, connection));
            }
#endif
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
            IntPtr ptr = NativeMethods.PeerConnectionGetConfiguration(GetSelfOrThrow());
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
            return NativeMethods.PeerConnectionSetConfiguration(GetSelfOrThrow(), JsonUtility.ToJson(configuration));
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
            NativeMethods.PeerConnectionRegisterConnectionStateChange(self, PCOnConnectionStateChange);
            NativeMethods.PeerConnectionRegisterIceGatheringChange(self, PCOnIceGatheringChange);
            NativeMethods.PeerConnectionRegisterOnIceCandidate(self, PCOnIceCandidate);
            NativeMethods.PeerConnectionRegisterOnDataChannel(self, PCOnDataChannel);
            NativeMethods.PeerConnectionRegisterOnRenegotiationNeeded(self, PCOnNegotiationNeeded);
            NativeMethods.PeerConnectionRegisterOnTrack(self, PCOnTrack);
            WebRTC.Context.PeerConnectionRegisterOnSetSessionDescSuccess(
                self, OnSetSessionDescSuccess);
            WebRTC.Context.PeerConnectionRegisterOnSetSessionDescFailure(
                self, OnSetSessionDescFailure);
        }

        public void RestartIce()
        {
            NativeMethods.PeerConnectionRestartIce(GetSelfOrThrow());
        }

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="Dispose"/>
        public void Close()
        {
            NativeMethods.PeerConnectionClose(GetSelfOrThrow());
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


#if !UNITY_WEBGL
            var streamId = stream == null ? Guid.NewGuid().ToString() : stream.Id;
            IntPtr ptr = NativeMethods.PeerConnectionAddTrack(GetSelfOrThrow(), track.GetSelfOrThrow(), streamId);
#else
            var streamPtr = stream == null ? IntPtr.Zero : stream.GetSelfOrThrow();
            IntPtr ptr = NativeMethods.PeerConnectionAddTrack(GetSelfOrThrow(), track.GetSelfOrThrow(), streamPtr);
#endif
            return CreateSender(ptr);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <seealso cref="AddTrack"/>
        public void RemoveTrack(RTCRtpSender sender)
        {
            NativeMethods.PeerConnectionRemoveTrack(
                GetSelfOrThrow(), sender.self);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public RTCRtpTransceiver AddTransceiver(MediaStreamTrack track)
        {
            IntPtr ptr = NativeMethods.PeerConnectionAddTransceiver(
                GetSelfOrThrow(), track.GetSelfOrThrow());
            return CreateTransceiver(ptr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public RTCRtpTransceiver AddTransceiver(TrackKind kind)
        {
            IntPtr ptr = NativeMethods.PeerConnectionAddTransceiverWithType(
                GetSelfOrThrow(), kind);
            return CreateTransceiver(ptr);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="candidate"></param>
        public bool AddIceCandidate(RTCIceCandidate candidate)
        {
            return NativeMethods.PeerConnectionAddIceCandidate(
                GetSelfOrThrow(), candidate.self);
        }

        /// <summary>
        /// Create an SDP (Session Description Protocol) offer to start a new connection
        /// to a remote peer.
        /// </summary>
        /// <param name="options"> A parameter to request for the offer. </param>
        /// <returns></returns>
        /// <seealso cref="CreateAnswer"/>
        public RTCSessionDescriptionAsyncOperation CreateOffer(ref RTCOfferAnswerOptions options)
        {
            m_opSessionDesc = new RTCSessionDescriptionAsyncOperation();
#if !UNITY_WEBGL
            NativeMethods.PeerConnectionCreateOffer(GetSelfOrThrow(), ref options);
#else
            // TODO : Handle RTCOfferAnswerOptions rather than booleans
            NativeMethods.PeerConnectionCreateOffer(GetSelfOrThrow(), options.iceRestart, options.voiceActivityDetection);
#endif
            return m_opSessionDesc;
        }

        public RTCSessionDescriptionAsyncOperation CreateOffer()
        {
            m_opSessionDesc = new RTCSessionDescriptionAsyncOperation();
#if !UNITY_WEBGL
            NativeMethods.PeerConnectionCreateOffer(GetSelfOrThrow(), ref RTCOfferAnswerOptions.Default);
#else
            NativeMethods.PeerConnectionCreateOffer(GetSelfOrThrow(), RTCOfferAnswerOptions.Default.iceRestart, RTCOfferAnswerOptions.Default.voiceActivityDetection);
#endif
            return m_opSessionDesc;
        }

        /// <summary>
        /// Create an SDP (Session Description Protocol) answer to start a new connection
        /// to a remote peer.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public RTCSessionDescriptionAsyncOperation CreateAnswer(ref RTCOfferAnswerOptions options)
        {
            m_opSessionDesc = new RTCSessionDescriptionAsyncOperation();
#if !UNITY_WEBGL
            NativeMethods.PeerConnectionCreateAnswer(GetSelfOrThrow(), ref options);
#else
            NativeMethods.PeerConnectionCreateAnswer(GetSelfOrThrow(), options.iceRestart);
#endif
            return m_opSessionDesc;
        }

        public RTCSessionDescriptionAsyncOperation CreateAnswer()
        {
            m_opSessionDesc = new RTCSessionDescriptionAsyncOperation();
#if !UNITY_WEBGL
            NativeMethods.PeerConnectionCreateAnswer(GetSelfOrThrow(), ref RTCOfferAnswerOptions.Default);
#else
            NativeMethods.PeerConnectionCreateAnswer(GetSelfOrThrow(), RTCOfferAnswerOptions.Default.iceRestart);
#endif
            return m_opSessionDesc;
        }

        /// <summary>
        /// Creates a new data channel related the remote peer.
        /// </summary>
        /// <param name="label"> A string for the data channel.
        /// This string may be checked by <see cref="RTCDataChannel.Label"/>. </param>
        /// <param name="options"> A struct provides configuration options for the data channel. </param>
        /// <returns> A new data channel. </returns>
        public RTCDataChannel CreateDataChannel(string label, RTCDataChannelInit options = null)
        {
            RTCDataChannelInitInternal _options =
                options == null ? new RTCDataChannelInitInternal() : (RTCDataChannelInitInternal)options;

            IntPtr ptr = WebRTC.Context.CreateDataChannel(GetSelfOrThrow(), label, ref _options);
            if (ptr == IntPtr.Zero)
                throw new ArgumentException("RTCDataChannelInit object is incorrect.");
            return new RTCDataChannel(ptr, this);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateCreateSDSuccess))]
        static void OnSuccessCreateSessionDesc(IntPtr ptr, RTCSdpType type, string sdp)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.m_opSessionDesc.Desc = new RTCSessionDescription { sdp = sdp, type = type };
                    connection.m_opSessionDesc.Done();
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.m_opSessionDesc.Desc = new RTCSessionDescription { sdp = sdp, type = type };
                connection.m_opSessionDesc.Done();
            }
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateCreateSDFailure))]
        static void OnFailureCreateSessionDesc(IntPtr ptr, RTCErrorType type, string message)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.m_opSessionDesc.IsError = true;
                    connection.m_opSessionDesc.Error = new RTCError{errorType = type, message = message};
                    connection.m_opSessionDesc.Done();
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.m_opSessionDesc.IsError = true;
                connection.m_opSessionDesc.Error = new RTCError { errorType = type, message = message };
                connection.m_opSessionDesc.Done();
            }
#endif
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
                GetSelfOrThrow(), ref desc);
            if (error.errorType == RTCErrorType.None)
            {
                return op;
            }
            throw new RTCErrorException(ref error);
        }

        public RTCSetSessionDescriptionAsyncOperation SetLocalDescription()
        {
            var op = new RTCSetSessionDescriptionAsyncOperation(this);
            RTCError error = WebRTC.Context.PeerConnectionSetLocalDescription(GetSelfOrThrow());
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
                GetSelfOrThrow(), ref desc);
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
#if UNITY_WEBGL
        public RTCStatsReportAsyncOperation GetStats(MediaStreamTrack track)
        {
            return new RTCStatsReportAsyncOperation(this, track);
        }
#endif

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
#if !UNITY_WEBGL
                if (NativeMethods.PeerConnectionGetLocalDescription(GetSelfOrThrow(), ref desc))
                {
                    return desc;
                }
#else
                var ptr = NativeMethods.PeerConnectionGetLocalDescription(GetSelfOrThrow());
                var ret = ptr.AsAnsiStringWithFreeMem();
                if (ret != "false")
                {
                    desc = JsonConvert.DeserializeObject<RTCSessionDescription>(ret);
                    return desc;
                }
#endif
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
#if !UNITY_WEBGL
                if (NativeMethods.PeerConnectionGetRemoteDescription(GetSelfOrThrow(), ref desc))
                {
                    return desc;
                }
#else
                var ptr = NativeMethods.PeerConnectionGetRemoteDescription(GetSelfOrThrow());
                var ret = ptr.AsAnsiStringWithFreeMem();
                if (ret != "false")
                {
                    desc = JsonConvert.DeserializeObject<RTCSessionDescription>(ret);
                    return desc;
                }
#endif
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
#if !UNITY_WEBGL
                if (NativeMethods.PeerConnectionGetCurrentLocalDescription(GetSelfOrThrow(), ref desc))
                {
                    return desc;
                }
#else
                var ptr = NativeMethods.PeerConnectionGetCurrentLocalDescription(GetSelfOrThrow());
                var ret = ptr.AsAnsiStringWithFreeMem();
                if (ret != "false")
                {
                    desc = JsonConvert.DeserializeObject<RTCSessionDescription>(ret);
                    return desc;
                }
#endif
                throw new InvalidOperationException("CurrentLocalDescription is not exist");
            }
        }

        public RTCSessionDescription CurrentRemoteDescription
        {
            get
            {
                RTCSessionDescription desc = default;
#if !UNITY_WEBGL
                if (NativeMethods.PeerConnectionGetCurrentRemoteDescription(GetSelfOrThrow(), ref desc))
                {
                    return desc;
                }
#else
                var ptr = NativeMethods.PeerConnectionGetCurrentRemoteDescription(GetSelfOrThrow());
                var ret = ptr.AsAnsiStringWithFreeMem();
                if (ret != "false")
                {
                    desc = JsonConvert.DeserializeObject<RTCSessionDescription>(ret);
                    return desc;
                }
#endif
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
#if !UNITY_WEBGL
                if (NativeMethods.PeerConnectionGetPendingLocalDescription(GetSelfOrThrow(), ref desc))
                {
                    return desc;
                }
#else
                var ptr = NativeMethods.PeerConnectionGetPendingLocalDescription(GetSelfOrThrow());
                var ret = ptr.AsAnsiStringWithFreeMem();
                if (ret != "false")
                {
                    desc = JsonConvert.DeserializeObject<RTCSessionDescription>(ret);
                    return desc;
                }
#endif
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
#if !UNITY_WEBGL
                if (NativeMethods.PeerConnectionGetPendingRemoteDescription(GetSelfOrThrow(), ref desc))
                {
                    return desc;
                }
#else
                var ptr = NativeMethods.PeerConnectionGetPendingRemoteDescription(GetSelfOrThrow());
                var ret = ptr.AsAnsiStringWithFreeMem();
                if (ret != "false")
                {
                    desc = JsonConvert.DeserializeObject<RTCSessionDescription>(ret);
                    return desc;
                }
#endif
                throw new InvalidOperationException("PendingRemoteDescription is not exist");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativePeerConnectionSetSessionDescSuccess))]
        static void OnSetSessionDescSuccess(IntPtr ptr)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnSetSessionDescriptionSuccess();
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.OnSetSessionDescriptionSuccess();
            }
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativePeerConnectionSetSessionDescFailure))]
        static void OnSetSessionDescFailure(IntPtr ptr, RTCErrorType type, string message)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    RTCError error = new RTCError { errorType = type, message = message };
                    connection.OnSetSessionDescriptionFailure(error);
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                RTCError error = new RTCError { errorType = type, message = message };
                connection.OnSetSessionDescriptionFailure(error);
            }
#endif
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateCollectStats))]
        static void OnStatsDeliveredCallback(IntPtr ptr, IntPtr report)
        {
#if !UNITY_WEBGL
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnStatsDelivered(report);
                }
            });
#else
            if (WebRTC.Table[ptr] is RTCPeerConnection connection)
            {
                connection.OnStatsDelivered(report);
            }
#endif
        }
    }
}
