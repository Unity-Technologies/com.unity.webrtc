using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    ///     Delegate to be called when a new RTCIceCandidate is identified and added to the local peer, when all candidates for a specific generation are identified and added, and when the ICE gathering on all transports is complete.
    /// </summary>
    /// <remarks>
    ///     This delegate is called when:
    ///     * An `RTCIceCandidate` is added to the local peer using `SetLocalDescription`.
    ///     * Every `RTCIceCandidate` correlated with a specific username/password combination are added.
    ///     * ICE gathering for all transports is finished.
    /// </remarks>
    /// <param name="candidate">`RTCIceCandidate` object containing the candidate associated with the event.</param>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         peerConnection.OnIceCandidate = candidate =>
    ///         {
    ///             otherPeerConnection.AddIceCandidate(candidate);
    ///         }
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCIceCandidate" />
    public delegate void DelegateOnIceCandidate(RTCIceCandidate candidate);

    /// <summary>
    ///     Delegate to be called when the ICE connection state is changed.
    /// </summary>
    /// <remarks>
    ///     This delegate is called each time the ICE connection state changes during the negotiation process.
    /// </remarks>
    /// <param name="state">`RTCIceConnectionState` value.</param>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         peerConnection.OnIceConnectionChange = state =>
    ///         {
    ///             if (state == RTCIceConnectionState.Connected || state == RTCIceConnectionState.Completed)
    ///             {
    ///                 foreach (RTCRtpSender sender in peerConnection.GetSenders())
    ///                 {
    ///                     sender.SyncApplicationFramerate = true;
    ///                 }
    ///             }
    ///         }
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCIceConnectionState" />
    public delegate void DelegateOnIceConnectionChange(RTCIceConnectionState state);

    /// <summary>
    /// Delegate to be called after a new track has been added to an RTCRtpReceiver which is part of the connection.
    /// </summary>
    /// <remarks>
    /// This delegate is called after a new track has been added to an `RTCRtpReceiver` which is part of the connection.
    /// </remarks>
    /// <param name="state">New connection state.</param>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         peerConnection.OnConnectionStateChange = state =>
    ///         {
    ///             Debug.Log($"Connection state changed to {state}");
    ///         }
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCPeerConnectionState" />
    public delegate void DelegateOnConnectionStateChange(RTCPeerConnectionState state);

    /// <summary>
    ///     Delegate to be called when the state of the ICE candidate gathering process changes.
    /// </summary>
    /// <remarks>
    ///    This delegate is called when the state of the ICE candidate gathering process changes.
    /// </remarks>
    /// <param name="state">New ICE gathering state.</param>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         peerConnection.OnIceGatheringStateChange = state =>
    ///         {
    ///             if (state == RTCIceGatheringState.Complete)
    ///             {
    ///                 GameObject newCandidate = Instantiate(candidateElement, candidateParent);
    ///             }
    ///         }
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCIceGatheringState" />
    public delegate void DelegateOnIceGatheringStateChange(RTCIceGatheringState state);

    /// <summary>
    ///     Delegate to be called when negotiation of the connection through the signaling channel is required.
    /// </summary>
    /// <remarks>
    ///     This delegate is called when negotiation of the connection through the signaling channel is required.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         peerConnection.OnNegotiationNeeded = () =>
    ///         {
    ///             StartCoroutine(NegotiationProcess());
    ///         }
    ///
    ///         IEnumerator NegotiationProcess()
    ///         {
    ///             RTCSessionDescriptionAsyncOperation asyncOperation = peerConnection.CreateOffer();
    ///             yield return asyncOperation;
    ///
    ///             if (!asyncOperation.IsError)
    ///             {
    ///                 RTCSessionDescription description = asyncOperation.Desc;
    ///                 RTCSetSessionDescriptionAsyncOperation asyncOperation = peerConnection.SetLocalDescription(ref description);
    ///                 yield return asyncOperation;
    ///             }
    ///         }
    ///     ]]></code>
    /// </example>
    /// <seealso cref="AddTrack" />
    /// <seealso cref="RestartIce" />
    public delegate void DelegateOnNegotiationNeeded();

    /// <summary>
    ///     Delegate to be called after a new track has been added to an RTCRtpReceiver which is part of the connection.
    /// </summary>
    /// <remarks>
    ///     This delegate is called after a new track has been added to an `RTCRtpReceiver` which is part of the connection.
    /// </remarks>
    /// <param name="e">`RTCTrackEvent` object.</param>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         MediaStream receiveStream = new MediaStream();
    ///         peerConnection.OnTrack = e =>
    ///         {
    ///             receiveStream.AddTrack(e.Track);
    ///         }
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCTrackEvent" />
    public delegate void DelegateOnTrack(RTCTrackEvent e);


    internal delegate void DelegateSetSessionDescSuccess();
    internal delegate void DelegateSetSessionDescFailure(RTCError error);

    /// <summary>
    ///     Represents a WebRTC connection between the local peer and remote peer.
    /// </summary>
    /// <remarks>
    ///     `RTCPeerConnection` class represents a WebRTC connection between the local computer and a remote peer.
    ///     It provides methods to connect to a remote peer, maintain and monitor the connection, and close the connection once it's no longer needed.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         RTCPeerConnection peerConnection = new RTCPeerConnection();
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCConfiguration" />
    /// <seealso cref="RTCIceCandidate" />
    /// <seealso cref="RTCSessionDescription" />
    /// <seealso cref="RTCTrackEvent" />
    /// <seealso cref="WebRTC" />
    public class RTCPeerConnection : IDisposable
    {
        private IntPtr self;
        private HashSet<MediaStreamTrack> cacheTracks = new HashSet<MediaStreamTrack>();
        private bool disposed;

        /// <summary>
        ///     Finalizer for RTCPeerConnection.
        /// </summary>
        /// <remarks>
        ///     Ensures that resources are released by calling the `Dispose` method.
        /// </remarks>
        ~RTCPeerConnection()
        {
            this.Dispose();
        }

        /// <summary>
        ///     Disposes of RTCPeerConnection.
        /// </summary>
        /// <remarks>
        ///     `Dispose` method releases resources used by the `RTCPeerConnection`.
        ///     This method closes the current peer connection and disposes of all transceivers.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         peerConnection.Dispose();
        ///     ]]></code>
        /// </example>
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
        ///     <code lang="cs"><![CDATA[
        ///         RTCPeerConnection peerConnection = new RTCPeerConnection(configuration);
        ///         RTCIceConnectionState iceConnectionState = peerConnection.IceConnectionState;
        ///     ]]></code>
        /// </example>
        /// <seealso cref="ConnectionState"/>
        public RTCIceConnectionState IceConnectionState => NativeMethods.PeerConnectionIceConditionState(GetSelfOrThrow());

        /// <summary>
        /// The readonly property of the <see cref="RTCPeerConnection"/> indicates
        /// the current state of the peer connection by returning one of the
        /// <see cref="RTCPeerConnectionState"/> enum.
        /// </summary>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCPeerConnection peerConnection = new RTCPeerConnection(configuration);
        ///         RTCPeerConnectionState connectionState = peerConnection.ConnectionState;
        ///     ]]></code>
        /// </example>
        /// <seealso cref="IceConnectionState"/>
        public RTCPeerConnectionState ConnectionState => NativeMethods.PeerConnectionState(GetSelfOrThrow());

        /// <summary>
        /// The readonly property of the <see cref="RTCPeerConnection"/> indicates
        /// the current state of the peer connection by returning one of the
        /// <see cref="RTCSignalingState"/> enum.
        /// </summary>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCPeerConnection peerConnection = new RTCPeerConnection(configuration);
        ///         RTCSignalingState signalingState = peerConnection.SignalingState;
        ///     ]]></code>
        /// </example>
        /// <seealso cref="ConnectionState"/>
        public RTCSignalingState SignalingState => NativeMethods.PeerConnectionSignalingState(GetSelfOrThrow());

        /// <summary>
        /// RTCIceGatheringState value that describes the overall ICE gathering state for the RTCPeerConnection.
        /// </summary>
        public RTCIceGatheringState GatheringState => NativeMethods.PeerConnectionIceGatheringState(GetSelfOrThrow());

        /// <summary>
        ///     Returns array of objects each of which represents one RTP receiver.
        /// </summary>
        /// <remarks>
        ///     `GetReceivers` method returns an array of `RTCRtpReceiver` objects, each of which represents one RTP receiver.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         IEnumerable<RTCRtpReceiver> receivers = peerConnection.GetReceivers();
        ///     ]]></code>
        /// </example>
        /// <returns>An array of `RTCRtpReceiver` objects, one for each track on the connection.</returns>
        /// <seealso cref="GetSenders()"/>
        /// <seealso cref="GetTransceivers()"/>
        public IEnumerable<RTCRtpReceiver> GetReceivers()
        {
            IntPtr buf = WebRTC.Context.PeerConnectionGetReceivers(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, CreateReceiver);
        }

        /// <summary>
        ///     Returns array of objects each of which represents one RTP sender.
        /// </summary>
        /// <remarks>
        ///     `GetSenders` method returns an array of `RTCRtpSender` objects, each of which represents the RTP sender responsible for transmitting one track's data.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpSender sender = peerConnection.GetSenders().First();
        ///         RTCRtpSendParameters parameters = sender.GetParameters();
        ///         parameters.encodings[0].maxBitrate = bandwidth * 1000;
        ///         RTCError error = sender.SetParameters(parameters);
        ///         if (error != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set parameters: {error}");
        ///         }
        ///     ]]></code>
        /// </example>
        /// <returns>An array of `RTCRtpSender` objects, one for each track on the connection.</returns>
        /// <seealso cref="GetReceivers()"/>
        /// <seealso cref="GetTransceivers()"/>
        public IEnumerable<RTCRtpSender> GetSenders()
        {
            var buf = WebRTC.Context.PeerConnectionGetSenders(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, CreateSender);
        }

        /// <summary>
        ///     Returns array of objects each of which represents one RTP transceiver.
        /// </summary>
        /// <remarks>
        ///     `GetTransceivers` method returns an array of the `RTCRtpTransceiver` objects being used to send and receive data on the connection.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
        ///         RTCRtpTransceiver transceiver = peerConnection.GetTransceivers().First();
        ///         RTCErrorType error = transceiver.SetCodecPreferences(capabilities.codecs);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set codec preferences: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        /// <returns>An array of the `RTCRtpTransceiver` objects representing the transceivers handling sending and receiving all media on the `RTCPeerConnection`.</returns>
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
        ///     Delegate to be called when the IceConnectionState is changed.
        /// </summary>
        /// <value>A delegate containing <see cref="IceConnectionState"/>.</value>
        /// <example>
        ///     <code><![CDATA[
        ///         peerConnection.OnIceConnectionChange = iceConnectionState =>
        ///         {
        ///             ...
        ///         };
        ///     ]]></code>
        /// </example>
        /// <seealso cref="IceConnectionState"/>
        public DelegateOnIceConnectionChange OnIceConnectionChange { get; set; }

        /// <summary>
        ///     Delegate to be called after a new track has been added to an RTCRtpReceiver which is part of the connection.
        /// </summary>
        public DelegateOnConnectionStateChange OnConnectionStateChange { get; set; }

        /// <summary>
        ///     Delegate to be called when the state of the ICE candidate gathering process changes.
        /// </summary>
        public DelegateOnIceGatheringStateChange OnIceGatheringStateChange { get; set; }

        /// <summary>
        ///　   Delegate to be called when a new RTCIceCandidate is identified and added to the local peer, when all candidates for a specific generation are identified and added, and when the ICE gathering on all transports is complete.
        /// </summary>
        public DelegateOnIceCandidate OnIceCandidate { get; set; }

        /// <summary>
        ///     Delegate to be called when an RTCDataChannel has been added to the connection, as a result of the remote peer calling RTCPeerConnection.CreateDataChannel.
        /// </summary>
        public DelegateOnDataChannel OnDataChannel { get; set; }

        /// <summary>
        ///     Delegate to be called when negotiation of the connection through the signaling channel is required.
        /// </summary>
        public DelegateOnNegotiationNeeded OnNegotiationNeeded { get; set; }

        /// <summary>
        ///     Delegate to be called after a new track has been added to an RTCRtpReceiver which is part of the connection.
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
        ///     Returns an object which indicates the current configuration of the RTCPeerConnection.
        /// </summary>
        /// <remarks>
        ///     `GetConfiguration` method returns an object which indicates the current configuration of the `RTCPeerConnection`.
        /// </remarks>
        /// <returns>An object describing the <see cref="RTCPeerConnection"/>'s current configuration.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCConfiguration configuration = myPeerConnection.GetConfiguration();
        ///         if(configuration.urls.length == 0)
        ///         {
        ///             configuration.urls = new[] {"stun:stun.l.google.com:19302"};
        ///         }
        ///         myPeerConnection.SetConfiguration(configuration);
        ///     ]]></code>
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
        ///     Sets the current configuration of the RTCPeerConnection.
        /// </summary>
        /// <remarks>
        ///     `SetConfiguration` method sets the current configuration of the connection based on the values included in the specified object.
        ///     This lets you change the ICE servers used by the connection and which transport policies to use.
        /// </remarks>
        /// <param name="configuration">
        ///     `RTCConfiguration` object which provides the options to be set.
        ///     The changes are not additive; instead, the new values completely replace the existing ones.
        /// </param>
        /// <returns> Error code. </returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCConfiguration configuration = new RTCConfiguration
        ///         {
        ///             iceServers = new[]
        ///             {
        ///                 new RTCIceServer
        ///                 {
        ///                     urls = new[] {"stun:stun.l.google.com:19302"},
        ///                     username = "",
        ///                     credential = "",
        ///                     credentialType = RTCIceCredentialType.Password
        ///                 }
        ///             }
        ///         };
        ///         RTCErrorType error = myPeerConnection.SetConfiguration(ref configuration);
        ///         if(error == RTCErrorType.None)
        ///         {
        ///             ...
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="GetConfiguration()"/>
        public RTCErrorType SetConfiguration(ref RTCConfiguration configuration)
        {
            var conf_ = configuration.Cast();
            string str = JsonUtility.ToJson(conf_);
            return NativeMethods.PeerConnectionSetConfiguration(GetSelfOrThrow(), str);
        }

        /// <summary>
        ///     Creates an instance of peer connection with a default configuration.
        /// </summary>
        /// <remarks>
        ///    `RTCPeerConnection` constructor creates an instance of peer connection with a default configuration.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCPeerConnection peerConnection = new RTCPeerConnection();
        ///     ]]></code>
        /// </example>
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
        ///     Creates an instance of peer connection with a configuration provided by user.
        /// </summary>
        /// <remarks>
        ///    `RTCPeerConnection` constructor creates an instance of peer connection with a default configuration.
        ///     An <see cref="RTCConfiguration "/> object providing options to configure the new connection.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCPeerConnection peerConnection = new RTCPeerConnection(ref configuration);
        ///     ]]></code>
        /// </example>
        /// <param name="configuration">`RTCConfiguration` object to configure the new connection.</param>
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
        ///     Requests that ICE candidate gathering be redone on both ends of the connection.
        /// </summary>
        /// <remarks>
        ///     `RestartIce` method requests that ICE candidate gathering be redone on both ends of the connection.
        ///     After `RestartIce` is called, the offer returned by the next call to `CreateOffer` automatically configured to trigger ICE restart on both the local and remote peers.
        ///     This method triggers an `OnNegotiationNeeded` event.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         peerConnection.RestartIce();
        ///     ]]></code>
        /// </example>
        public void RestartIce()
        {
            NativeMethods.PeerConnectionRestartIce(GetSelfOrThrow());
        }

        /// <summary>
        ///     Closes the current peer connection.
        /// </summary>
        /// <remarks>
        ///     `Close` method closes the current peer connection.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         peerConnection.Close();
        ///     ]]></code>
        /// </example>
        /// <seealso cref="Dispose"/>
        public void Close()
        {
            NativeMethods.PeerConnectionClose(GetSelfOrThrow());
        }

        /// <summary>
        ///     Adds a new media track to the set of tracks which is transmitted to the other peer.
        /// </summary>
        /// <remarks>
        ///     `AddTrack` method adds a new media track to the set of tracks which is transmitted to the other peer.
        ///     Adding a track to a connection triggers renegotiation by firing an `OnNegotiationNeeded` event.
        /// </remarks>
        /// <param name="track">`MediaStreamTrack` object representing the media track to add to the peer connection.</param>
        /// <param name="stream">
        ///     Local `MediaStream` object to which the track should be added.
        ///     If this is not specified, then the track is **streamless**.
        /// </param>
        /// <returns>`RTCRtpSender` object which is used to transmit the media data.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         MediaStream sendStream = new MediaStream();
        ///         AudioStreamTrack audioTrack = new AudioStreamTrack(inputAudioSource)
        ///         RTCRtpSender sender = peerConnection.AddTrack(audioTrack, sendStream);
        ///     ]]></code>
        /// </example>
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
        ///     Tells the local end of the connection to stop sending media from the specified track.
        /// </summary>
        /// <remarks>
        ///     `RemoveTrack` method tells the local end of the connection to stop sending media from the specified track, without actually removing the corresponding `RTCRtpSender` from the list of senders.
        ///     If the track is already stopped, or is not in the connection's senders list, this method has no effect.
        /// </remarks>
        /// <param name="sender">`RTCRtpSender` object specifying the sender to remove from the connection.</param>
        /// <returns>`RTCErrorType` value.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCErrorType error = peerConnection.RemoveTrack(sender);
        ///     ]]></code>
        /// </example>
        /// <seealso cref="AddTrack"/>
        public RTCErrorType RemoveTrack(RTCRtpSender sender)
        {
            cacheTracks.Remove(sender.Track);
            return NativeMethods.PeerConnectionRemoveTrack(GetSelfOrThrow(), sender.self);
        }

        /// <summary>
        ///     Creates a new RTCRtpTransceiver and adds it to the set of transceivers associated with the RTCPeerConnection.
        /// </summary>
        /// <remarks>
        ///     `AddTransceiver` method creates a new `RTCRtpTransceiver` instance and adds it to the set of transceivers associated with the `RTCPeerConnection`.
        ///     Each transceiver represents a bidirectional stream, with both an `RTCRtpSender` and an `RTCRtpReceiver` associated with it.
        /// </remarks>
        /// <param name="track">`MediaStreamTrack` object to associate with the transceiver.</param>
        /// <param name="init">`RTCRtpTransceiverInit` object for specifying options when creating the new transceiver.</param>
        /// <returns>`RTCRtpTransceiver` object which is used to exchange the media data.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpTransceiverInit init = new RTCRtpTransceiverInit();
        ///         init.direction = RTCRtpTransceiverDirection.SendOnly;
        ///         RTCRtpTransceiver transceiver = peerConnection.AddTransceiver(videoStreamTrack, init);
        ///     ]]></code>
        /// </example>
        public RTCRtpTransceiver AddTransceiver(MediaStreamTrack track, RTCRtpTransceiverInit init = null)
        {
            if (track == null)
                throw new ArgumentNullException("track is null.");
            IntPtr ptr = PeerConnectionAddTransceiver(
                GetSelfOrThrow(), track.GetSelfOrThrow(), init);
            return CreateTransceiver(ptr);
        }

        /// <summary>
        ///     Creates a new RTCRtpTransceiver and adds it to the set of transceivers associated with the RTCPeerConnection.
        /// </summary>
        /// <remarks>
        ///     `AddTransceiver` method creates a new `RTCRtpTransceiver` instance and adds it to the set of transceivers associated with the `RTCPeerConnection`.
        ///     Each transceiver represents a bidirectional stream, with both an `RTCRtpSender` and an `RTCRtpReceiver` associated with it.
        /// </remarks>
        /// <param name="kind">`TrackKind` value which is used as the kind of the receiver's track.</param>
        /// <param name="init">`RTCRtpTransceiverInit` object for specifying options when creating the new transceiver.</param>
        /// <returns>`RTCRtpTransceiver` object which is used to exchange the media data.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpTransceiverInit init = new RTCRtpTransceiverInit();
        ///         init.direction = RTCRtpTransceiverDirection.RecvOnly;
        ///         RTCRtpTransceiver transceiver = peerConnection.AddTransceiver(TrackKind.Audio, init);
        ///     ]]></code>
        /// </example>
        public RTCRtpTransceiver AddTransceiver(TrackKind kind, RTCRtpTransceiverInit init = null)
        {
            IntPtr ptr = PeerConnectionAddTransceiverWithType(
                GetSelfOrThrow(), kind, init);
            return CreateTransceiver(ptr);
        }

        /// <summary>
        ///     Adds a new remote candidate to the connection's remote description.
        /// </summary>
        /// <remarks>
        ///     `AddIceCandidate` method adds a new remote ICE candidate to the connection's remote description, which describes the current state of the remote end of the connection.
        /// </remarks>
        /// <param name="candidate">
        ///     `RTCIceCandidate` object that describes the properties of the new remote candidate.
        ///     If the value is null, the added ICE candidate is an "end-of-candidates" indicator.
        /// </param>
        /// <returns>`true` if the candidate has been successfully added to the remote peer's description by the ICE agent.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         peerConnection.OnIceCandidate = candidate =>
        ///         {
        ///             bool result = otherPeerConnection.AddIceCandidate(candidate);
        ///         }
        ///     ]]></code>
        /// </example>
        public bool AddIceCandidate(RTCIceCandidate candidate)
        {
            return NativeMethods.PeerConnectionAddIceCandidate(
                GetSelfOrThrow(), candidate.self);
        }

        /// <summary>
        ///     Create an SDP (Session Description Protocol) offer to start a new connection to a remote peer.
        /// </summary>
        /// <remarks>
        ///     `CreateOffer` initiates the creation of an SDP offer for the purpose of starting a new WebRTC connection to a remote peer.
        ///     The SDP offer contains details about `MediaStreamTrack` objects, supported codecs and options, and ICE candidates.
        /// </remarks>
        /// <param name="options">`RTCOfferAnswerOptions` object providing the options requested for the offer.</param>
        /// <returns>`RTCSessionDescriptionAsyncOperation` object containing `RTCSessionDescription` object.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCSessionDescriptionAsyncOperation asyncOperation = peerConnection.CreateOffer(ref options);
        ///         yield return asyncOperation;
        ///
        ///         if (!asyncOperation.IsError)
        ///         {
        ///             RTCSessionDescription description = asyncOperation.Desc;
        ///             RTCSetSessionDescriptionAsyncOperation asyncOperation = peerConnection.SetLocalDescription(ref description);
        ///             yield return asyncOperation;
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="CreateAnswer"/>
        public RTCSessionDescriptionAsyncOperation CreateOffer(ref RTCOfferAnswerOptions options)
        {
            CreateSessionDescriptionObserver observer =
                WebRTC.Context.PeerConnectionCreateOffer(GetSelfOrThrow(), ref options);
            return CreateDescription(observer);
        }

        /// <summary>
        ///     Create an SDP (Session Description Protocol) offer to start a new connection to a remote peer.
        /// </summary>
        /// <remarks>
        ///     `CreateOffer` initiates the creation of an SDP offer for the purpose of starting a new WebRTC connection to a remote peer.
        ///     The SDP offer contains details about `MediaStreamTrack` objects, supported codecs and options, and ICE candidates.
        /// </remarks>
        /// <returns>`RTCSessionDescriptionAsyncOperation` object containing `RTCSessionDescription` object.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCSessionDescriptionAsyncOperation asyncOperation = peerConnection.CreateOffer();
        ///         yield return asyncOperation;
        ///
        ///         if (!asyncOperation.IsError)
        ///         {
        ///             RTCSessionDescription description = asyncOperation.Desc;
        ///             RTCSetSessionDescriptionAsyncOperation asyncOperation = peerConnection.SetLocalDescription(ref description);
        ///             yield return asyncOperation;
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCSessionDescriptionAsyncOperation CreateOffer()
        {
            CreateSessionDescriptionObserver observer =
                WebRTC.Context.PeerConnectionCreateOffer(GetSelfOrThrow(), ref RTCOfferAnswerOptions.Default);
            return CreateDescription(observer);
        }

        /// <summary>
        ///     Create an SDP (Session Description Protocol) answer to start a new connection to a remote peer.
        /// </summary>
        /// <remarks>
        ///     `CreateAnswer` method creates an SDP answer to an offer received from a remote peer during the offer/answer negotiation of a WebRTC connection.
        ///     The SDP answer contains details about the session's media, supported codecs, and ICE candidates.
        /// </remarks>
        /// <param name="options">`RTCOfferAnswerOptions` object providing options requested for the answer.</param>
        /// <returns>`RTCSessionDescriptionAsyncOperation` object containing `RTCSessionDescription` object.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCSessionDescriptionAsyncOperation asyncOperation = peerConnection.CreateAnswer(ref options);
        ///         yield return asyncOperation;
        ///
        ///         if (!asyncOperation.IsError)
        ///         {
        ///             RTCSessionDescription description = asyncOperation.Desc;
        ///             RTCSetSessionDescriptionAsyncOperation asyncOperation = peerConnection.SetLocalDescription(ref description);
        ///             yield return asyncOperation;
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCSessionDescriptionAsyncOperation CreateAnswer(ref RTCOfferAnswerOptions options)
        {
            CreateSessionDescriptionObserver observer =
                WebRTC.Context.PeerConnectionCreateAnswer(GetSelfOrThrow(), ref options);
            return CreateDescription(observer);
        }

        /// <summary>
        ///     Create an SDP (Session Description Protocol) answer to start a new connection to a remote peer.
        /// </summary>
        /// <remarks>
        ///     `CreateAnswer` method creates an SDP answer to an offer received from a remote peer during the offer/answer negotiation of a WebRTC connection.
        ///     The SDP answer contains details about the session's media, supported codecs, and ICE candidates.
        /// </remarks>
        /// <returns>`RTCSessionDescriptionAsyncOperation` object containing `RTCSessionDescription` object.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCSessionDescriptionAsyncOperation asyncOperation = peerConnection.CreateAnswer();
        ///         yield return asyncOperation;
        ///
        ///         if (!asyncOperation.IsError)
        ///         {
        ///             RTCSessionDescription description = asyncOperation.Desc;
        ///             RTCSetSessionDescriptionAsyncOperation asyncOperation = peerConnection.SetLocalDescription(ref description);
        ///             yield return asyncOperation;
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCSessionDescriptionAsyncOperation CreateAnswer()
        {
            CreateSessionDescriptionObserver observer =
                WebRTC.Context.PeerConnectionCreateAnswer(GetSelfOrThrow(), ref RTCOfferAnswerOptions.Default);
            return CreateDescription(observer);
        }

        /// <summary>
        ///     Creates a new data channel related the remote peer.
        /// </summary>
        /// <remarks>
        ///     `CreateDataChannel` method creates a new data channel with the remote peer for transmitting any type of data.
        /// </remarks>
        /// <param name="label">
        ///     A string for the data channel.
        ///     This string may be checked by <see cref="RTCDataChannel.Label"/>.
        /// </param>
        /// <param name="options">A struct provides configuration options for the data channel.</param>
        /// <returns>A new data channel.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         var dataChannel = peerConnection.CreateDataChannel(label, options);
        ///     ]]></code>
        /// </example>
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
        ///     Changes the session description of the local connection to negotiate with other connections.
        /// </summary>
        /// <remarks>
        ///     `SetLocalDescription` method changes the local description associated with the connection, specifying the properties of the local end of the connection, including the media format.
        /// </remarks>
        /// <param name="desc">`RTCSessionDescription` object which specifies the configuration to be applied to the local end of the connection.</param>
        /// <returns>
        ///     An AsyncOperation which resolves with an <see cref="RTCSessionDescription"/> object providing a description of the session.
        /// </returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCSessionDescriptionAsyncOperation asyncOperation = peerConnection.CreateOffer();
        ///         yield return asyncOperation;
        ///
        ///         if (!asyncOperation.IsError)
        ///         {
        ///             RTCSessionDescription description = asyncOperation.Desc;
        ///             RTCSetSessionDescriptionAsyncOperation asyncOperation = peerConnection.SetLocalDescription(ref description);
        ///             yield return asyncOperation;
        ///         }
        ///     ]]></code>
        /// </example>
        /// <exception cref="ArgumentException">
        ///     Thrown when an argument has an invalid value.
        ///     For example, when passed the sdp which is null or empty.
        /// </exception>
        /// <exception cref="RTCErrorException">
        ///     Thrown when an argument has an invalid value.
        ///     For example, when passed the sdp which is not be able to parse.
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
        ///     Changes the session description of the local connection to negotiate with other connections.
        /// </summary>
        /// <remarks>
        ///     `SetLocalDescription` method automatically adjusts the local description associated with the connection, specifying the properties of the local end of the connection, including the media format.
        /// </remarks>
        /// <returns>
        ///     An AsyncOperation which resolves with an <see cref="RTCSessionDescription"/> object providing a description of the session.
        /// </returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         peerConnection.OnNegotiationNeeded = () =>
        ///         {
        ///             StartCoroutine(NegotiationProcess());
        ///         }
        ///
        ///         IEnumerator NegotiationProcess()
        ///         {
        ///             RTCSetSessionDescriptionAsyncOperation asyncOperation = peerConnection.SetLocalDescription(ref description);
        ///             yield return asyncOperation;
        ///             
        ///             if (asyncOperation.IsError)
        ///             {
        ///                 Debug.LogError("Failed to set local description: " + asyncOperation.Error.message);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCSetSessionDescriptionAsyncOperation SetLocalDescription()
        {
            SetSessionDescriptionObserver observer =
                PeerConnectionSetLocalDescription(GetSelfOrThrow(), out var error);
            return SetDescription(observer, error);
        }

        /// <summary>
        ///     This method changes the session description of the remote connection to negotiate with local connections.
        /// </summary>
        /// <remarks>
        ///     `SetRemoteDescription` method changes the specified session description as the remote peer's current offer or answer, specifying the properties of the remote end of the connection, including the media format.
        /// </remarks>
        /// <param name="desc">`RTCSessionDescription` object which specifies the remote peer's current offer or answer.</param>
        /// <returns>
        ///     An AsyncOperation which resolves with an <see cref="RTCSessionDescription"/> object providing a description of the session.
        /// </returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCSessionDescriptionAsyncOperation asyncOperation = peerConnection.CreateOffer();
        ///         yield return asyncOperation;
        ///
        ///         if (!asyncOperation.IsError)
        ///         {
        ///             RTCSessionDescription description = asyncOperation.Desc;
        ///             RTCSetSessionDescriptionAsyncOperation asyncOperation = otherPeerConnection.SetRemoteDescription(ref description);
        ///             yield return asyncOperation;
        ///         }
        ///     ]]></code>
        /// </example>
        /// <exception cref="ArgumentException">
        ///     Thrown when an argument has an invalid value.
        ///     For example, when passed the sdp which is null or empty.
        /// </exception>
        /// <exception cref="RTCErrorException">
        ///     Thrown when an argument has an invalid value.
        ///     For example, when passed the sdp which is not be able to parse.
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
        ///     Returns an AsyncOperation which resolves with data providing statistics.
        /// </summary>
        /// <remarks>
        ///     `GetStats` method returns a promise which resolves with data providing statistics about either the overall connection or about the specified `MediaStreamTrack`.
        /// </remarks>
        /// <returns>
        ///     An AsyncOperation which resolves with an <see cref="RTCStatsReport"/> object providing connection statistics.
        /// </returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         // Already instantiated peerConnection as RTCPeerConnection.
        ///         RTCStatsReportAsyncOperation operation = peerConnection.GetStats();
        ///         yield return operation;
        ///
        ///         if (!operation.IsError)
        ///         {
        ///             RTCStatsReport report = operation.Value;
        ///             foreach (RTCStats stat in report.Stats.Values)
        ///             {
        ///                 Debug.Log(stat.Type.ToString());
        ///             }
        ///         }
        ///     ]]></code>
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

        /// <summary>
        ///     Boolean value that indicates whether the remote peer can accept trickled ICE candidates.
        /// </summary>
        /// <remarks>
        ///     When the value is true, the remote peer can accept trickled ICE candidates.
        ///     When the value is false, the remote peer cannot accept trickled ICE candidates.
        ///     When the value is null, the remote peer has not been established.
        /// </remarks>
        public bool? CanTrickleIceCandidates
        {
            get
            {
                bool hasValue = NativeMethods.PeerConnectionCanTrickleIceCandidates(GetSelfOrThrow(), out var value);
                return hasValue ? value : (bool?)null;
            }
        }

        /// <summary>
        ///     RTCSessionDescription object that describes the session for the local end of the RTCPeerConnection.
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
        ///     RTCSessionDescription object that describes the session (which includes configuration and media information) for the remote end of the connection.
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
        ///　   RTCSessionDescription object describing the local end of the connection from the last successful negotiation with a remote peer.
        ///     It also includes ICE candidates generated by the ICE agent since the initial offer or answer was first created.
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
        ///     RTCSessionDescription object describing the remote end of the connection from the last successful negotiation with a remote peer.
        ///     It also includes ICE candidates generated by the ICE agent since the initial offer or answer was first created.
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
        ///     RTCSessionDescription object that describes a pending configuration change for the local end of the connection.
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
        ///     RTCSessionDescription object that describes a pending configuration change for the remote end of the connection.
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
