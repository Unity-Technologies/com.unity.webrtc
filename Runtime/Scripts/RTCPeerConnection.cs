using System;
using System.Collections.Generic;
using UnityEngine;

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


    internal delegate void DelegateSetSessionDescSuccess();
    internal delegate void DelegateSetSessionDescFailure(RTCError error);

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
        private HashSet<MediaStreamTrack> cacheTracks = new HashSet<MediaStreamTrack>();
        private bool disposed;

        /// <summary>
        /// 
        /// </summary>
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
                transceiver.Stop();
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
            IntPtr buf = WebRTC.Context.PeerConnectionGetReceivers(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, CreateReceiver);
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
            var buf = WebRTC.Context.PeerConnectionGetSenders(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, CreateSender);
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
            var buf = WebRTC.Context.PeerConnectionGetTransceivers(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, CreateTransceiver);
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
                throw new ObjectDisposedException(
                    GetType().FullName, "This instance has been disposed.");
            }
            return self;
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnIceCandidate))]
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

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnConnectionStateChange))]
        static void PCOnConnectionStateChange(IntPtr ptr, RTCPeerConnectionState state)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnConnectionStateChange?.Invoke(state);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnIceGatheringChange))]
        static void PCOnIceGatheringChange(IntPtr ptr, RTCIceGatheringState state)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    connection.OnIceGatheringStateChange?.Invoke(state);
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
                    var e = new RTCTrackEvent(transceiver, connection);
                    connection.OnTrack?.Invoke(e);
                    connection.cacheTracks.Add(e.Track);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnRemoveTrack))]
        static void PCOnRemoveTrack(IntPtr ptr, IntPtr receiverPtr)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCPeerConnection connection)
                {
                    var receiver = WebRTC.FindOrCreate(
                        receiverPtr, _ptr => new RTCRtpReceiver(_ptr, connection));
                    if (receiver != null)
                        connection.cacheTracks.Remove(receiver.Track);
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
            IntPtr ptr = NativeMethods.PeerConnectionGetConfiguration(GetSelfOrThrow());
            string str = ptr.AsAnsiStringWithFreeMem();
            var conf = JsonUtility.FromJson<RTCConfigurationInternal>(str);
            return new RTCConfiguration(ref conf);
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
            var conf_ = configuration.Cast();
            string str = JsonUtility.ToJson(conf_);
            return NativeMethods.PeerConnectionSetConfiguration(GetSelfOrThrow(), str);
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
            var conf_ = configuration.Cast();
            string configStr = JsonUtility.ToJson(conf_);
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
            NativeMethods.PeerConnectionRegisterIceConnectionChange(self, PCOnIceConnectionChange);
            NativeMethods.PeerConnectionRegisterConnectionStateChange(self, PCOnConnectionStateChange);
            NativeMethods.PeerConnectionRegisterIceGatheringChange(self, PCOnIceGatheringChange);
            NativeMethods.PeerConnectionRegisterOnIceCandidate(self, PCOnIceCandidate);
            NativeMethods.PeerConnectionRegisterOnDataChannel(self, PCOnDataChannel);
            NativeMethods.PeerConnectionRegisterOnRenegotiationNeeded(self, PCOnNegotiationNeeded);
            NativeMethods.PeerConnectionRegisterOnTrack(self, PCOnTrack);
            NativeMethods.PeerConnectionRegisterOnRemoveTrack(self, PCOnRemoveTrack);
        }

        /// <summary>
        /// 
        /// </summary>
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
                throw new ArgumentNullException("track is null.");

            var streamId = stream?.Id;
            RTCErrorType error = NativeMethods.PeerConnectionAddTrack(
                GetSelfOrThrow(), track.GetSelfOrThrow(), streamId, out var ptr);
            if (error != RTCErrorType.None)
                throw new InvalidOperationException($"error occurred :{error}");
            cacheTracks.Add(track);
            return CreateSender(ptr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        /// <seealso cref="AddTrack"/>
        public RTCErrorType RemoveTrack(RTCRtpSender sender)
        {
            cacheTracks.Remove(sender.Track);
            return NativeMethods.PeerConnectionRemoveTrack(GetSelfOrThrow(), sender.self);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        public RTCRtpTransceiver AddTransceiver(MediaStreamTrack track, RTCRtpTransceiverInit init = null)
        {
            if (track == null)
                throw new ArgumentNullException("track is null.");
            IntPtr ptr = PeerConnectionAddTransceiver(
                GetSelfOrThrow(), track.GetSelfOrThrow(), init);
            return CreateTransceiver(ptr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        public RTCRtpTransceiver AddTransceiver(TrackKind kind, RTCRtpTransceiverInit init = null)
        {
            IntPtr ptr = PeerConnectionAddTransceiverWithType(
                GetSelfOrThrow(), kind, init);
            return CreateTransceiver(ptr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        public bool AddIceCandidate(RTCIceCandidate candidate)
        {
            return NativeMethods.PeerConnectionAddIceCandidate(
                GetSelfOrThrow(), candidate.self);
        }

        /// <summary>
        /// Create an SDP (Session Description Protocol) offer to start a new connection
        /// to a remote peer.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <seealso cref="CreateAnswer"/>
        public RTCSessionDescriptionAsyncOperation CreateOffer(ref RTCOfferAnswerOptions options)
        {
            CreateSessionDescriptionObserver observer =
                WebRTC.Context.PeerConnectionCreateOffer(GetSelfOrThrow(), ref options);
            return CreateDescription(observer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RTCSessionDescriptionAsyncOperation CreateOffer()
        {
            CreateSessionDescriptionObserver observer =
                WebRTC.Context.PeerConnectionCreateOffer(GetSelfOrThrow(), ref RTCOfferAnswerOptions.Default);
            return CreateDescription(observer);
        }

        /// <summary>
        /// Create an SDP (Session Description Protocol) answer to start a new connection
        /// to a remote peer.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public RTCSessionDescriptionAsyncOperation CreateAnswer(ref RTCOfferAnswerOptions options)
        {
            CreateSessionDescriptionObserver observer =
                WebRTC.Context.PeerConnectionCreateAnswer(GetSelfOrThrow(), ref options);
            return CreateDescription(observer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RTCSessionDescriptionAsyncOperation CreateAnswer()
        {
            CreateSessionDescriptionObserver observer =
                WebRTC.Context.PeerConnectionCreateAnswer(GetSelfOrThrow(), ref RTCOfferAnswerOptions.Default);
            return CreateDescription(observer);
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
            if (string.IsNullOrEmpty(desc.sdp))
                throw new ArgumentException("sdp is null or empty");

            SetSessionDescriptionObserver observer =
                PeerConnectionSetLocalDescription(GetSelfOrThrow(), ref desc, out var error);
            return SetDescription(observer, error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RTCSetSessionDescriptionAsyncOperation SetLocalDescription()
        {
            SetSessionDescriptionObserver observer =
                PeerConnectionSetLocalDescription(GetSelfOrThrow(), out var error);
            return SetDescription(observer, error);
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

            SetSessionDescriptionObserver observer =
                PeerConnectionSetRemoteDescription(GetSelfOrThrow(), ref desc, out var error);
            return SetDescription(observer, error);
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
            RTCStatsCollectorCallback callback = NativeMethods.PeerConnectionGetStats(GetSelfOrThrow());
            return GetStats(callback);
        }

        internal RTCStatsReportAsyncOperation GetStats(RTCRtpSender sender)
        {
            RTCStatsCollectorCallback callback = NativeMethods.PeerConnectionSenderGetStats(GetSelfOrThrow(), sender.self);
            return GetStats(callback);
        }
        internal RTCStatsReportAsyncOperation GetStats(RTCRtpReceiver receiver)
        {
            RTCStatsCollectorCallback callback = NativeMethods.PeerConnectionReceiverGetStats(GetSelfOrThrow(), receiver.self);
            return GetStats(callback);
        }

        RTCStatsReportAsyncOperation GetStats(RTCStatsCollectorCallback callback)
        {
            IntPtr ptr = callback.DangerousGetHandle();
            if (!dictCollectStatsCallback.ContainsKey(ptr))
                dictCollectStatsCallback.Add(ptr, callback);
            return new RTCStatsReportAsyncOperation(callback);
        }

        public bool? CanTrickleIceCandidates
        {
            get
            {
                bool hasValue = NativeMethods.PeerConnectionCanTrickleIceCandidates(GetSelfOrThrow(), out var value);
                return hasValue ? value : (bool?)null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public RTCSessionDescription LocalDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if (NativeMethods.PeerConnectionGetLocalDescription(GetSelfOrThrow(), ref desc))
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
                if (NativeMethods.PeerConnectionGetRemoteDescription(GetSelfOrThrow(), ref desc))
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
                if (NativeMethods.PeerConnectionGetCurrentLocalDescription(GetSelfOrThrow(), ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("CurrentLocalDescription is not exist");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RTCSessionDescription CurrentRemoteDescription
        {
            get
            {
                RTCSessionDescription desc = default;
                if (NativeMethods.PeerConnectionGetCurrentRemoteDescription(GetSelfOrThrow(), ref desc))
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
                if (NativeMethods.PeerConnectionGetPendingLocalDescription(GetSelfOrThrow(), ref desc))
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
                if (NativeMethods.PeerConnectionGetPendingRemoteDescription(GetSelfOrThrow(), ref desc))
                {
                    return desc;
                }
                throw new InvalidOperationException("PendingRemoteDescription is not exist");
            }
        }

        Dictionary<IntPtr, SetSessionDescriptionObserver> dictSetSessionDescriptionObserver =
            new Dictionary<IntPtr, SetSessionDescriptionObserver>();
        Dictionary<IntPtr, CreateSessionDescriptionObserver> dictCreateSessionDescriptionObserver =
            new Dictionary<IntPtr, CreateSessionDescriptionObserver>();

        internal T FindObserver<T>(IntPtr ptr) where T : class
        {
            if (typeof(T) == typeof(SetSessionDescriptionObserver))
            {
                if (dictSetSessionDescriptionObserver.TryGetValue(ptr, out var value))
                    return value as T;
            }
            else if (typeof(T) == typeof(CreateSessionDescriptionObserver))
            {
                if (dictCreateSessionDescriptionObserver.TryGetValue(ptr, out var value))
                    return value as T;
            }
            return null;
        }

        internal void RemoveObserver<T>(T observer) where T : System.Runtime.InteropServices.SafeHandle
        {
            if (typeof(T) == typeof(SetSessionDescriptionObserver))
                dictSetSessionDescriptionObserver.Remove(observer.DangerousGetHandle());
            if (typeof(T) == typeof(CreateSessionDescriptionObserver))
                dictCreateSessionDescriptionObserver.Remove(observer.DangerousGetHandle());

        }

        RTCSessionDescriptionAsyncOperation CreateDescription(CreateSessionDescriptionObserver observer)
        {
            IntPtr ptr = observer.DangerousGetHandle();
            if (!dictCreateSessionDescriptionObserver.ContainsKey(ptr))
                dictCreateSessionDescriptionObserver.Add(ptr, observer);
            return new RTCSessionDescriptionAsyncOperation(observer);
        }


        RTCSetSessionDescriptionAsyncOperation SetDescription(SetSessionDescriptionObserver observer, RTCError error)
        {
            if (error.errorType != RTCErrorType.None)
                throw new RTCErrorException(ref error);

            IntPtr ptr = observer.DangerousGetHandle();
            if (!dictSetSessionDescriptionObserver.ContainsKey(ptr))
                dictSetSessionDescriptionObserver.Add(ptr, observer);
            return new RTCSetSessionDescriptionAsyncOperation(observer);
        }

        Dictionary<IntPtr, RTCStatsCollectorCallback> dictCollectStatsCallback = new Dictionary<IntPtr, RTCStatsCollectorCallback>();

        internal RTCStatsCollectorCallback FindCollectStatsCallback(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentException("The argument is IntPtr.Zero.", "ptr");
            if (dictCollectStatsCallback.TryGetValue(ptr, out var callback))
                return callback;
            return null;
        }

        internal void RemoveCollectStatsCallback(RTCStatsCollectorCallback callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            dictCollectStatsCallback.Remove(callback.DangerousGetHandle());
        }

        internal void RemoveObserver(SetSessionDescriptionObserver observer)
        {
            dictSetSessionDescriptionObserver.Remove(observer.DangerousGetHandle());
        }

        static SetSessionDescriptionObserver PeerConnectionSetLocalDescription(
            IntPtr ptr, ref RTCSessionDescription desc, out RTCError error)
        {
            IntPtr ptrError = IntPtr.Zero;
            SetSessionDescriptionObserver observer =
                NativeMethods.PeerConnectionSetLocalDescription(ptr, ref desc, out var errorType, ref ptrError);
            string message = ptrError != IntPtr.Zero ? ptrError.AsAnsiStringWithFreeMem() : null;
            error = new RTCError { errorType = errorType, message = message };
            return observer;
        }

        static SetSessionDescriptionObserver PeerConnectionSetLocalDescription(IntPtr ptr, out RTCError error)
        {
            IntPtr ptrError = IntPtr.Zero;
            SetSessionDescriptionObserver observer =
                NativeMethods.PeerConnectionSetLocalDescriptionWithoutDescription(ptr, out var errorType, ref ptrError);
            string message = ptrError != IntPtr.Zero ? ptrError.AsAnsiStringWithFreeMem() : null;
            error = new RTCError { errorType = errorType, message = message };
            return observer;
        }

        static SetSessionDescriptionObserver PeerConnectionSetRemoteDescription(
            IntPtr ptr, ref RTCSessionDescription desc, out RTCError error)
        {
            IntPtr ptrError = IntPtr.Zero;
            SetSessionDescriptionObserver observer =
                NativeMethods.PeerConnectionSetRemoteDescription(ptr, ref desc, out var errorType, ref ptrError);
            string message = ptrError != IntPtr.Zero ? ptrError.AsAnsiStringWithFreeMem() : null;
            error = new RTCError { errorType = errorType, message = message };
            return observer;
        }

        static IntPtr PeerConnectionAddTransceiver(IntPtr pc, IntPtr track, RTCRtpTransceiverInit init)
        {
            if (init == null)
                return NativeMethods.PeerConnectionAddTransceiver(pc, track);
            RTCRtpTransceiverInitInternal _init = init.Cast();
            return NativeMethods.PeerConnectionAddTransceiverWithInit(pc, track, ref _init);
        }

        static IntPtr PeerConnectionAddTransceiverWithType(IntPtr pc, TrackKind kind, RTCRtpTransceiverInit init)
        {
            if (init == null)
                return NativeMethods.PeerConnectionAddTransceiverWithType(pc, kind);
            RTCRtpTransceiverInitInternal _init = init.Cast();
            return NativeMethods.PeerConnectionAddTransceiverWithTypeAndInit(pc, kind, ref _init);
        }
    }
}
