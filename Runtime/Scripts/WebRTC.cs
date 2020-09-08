using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace Unity.WebRTC
{
    public enum EncoderType
    {
        Software = 0,
        Hardware = 1
    }

    public struct RTCIceCandidate
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string candidate;
        [MarshalAs(UnmanagedType.LPStr)]
        public string sdpMid;
        public int sdpMLineIndex;
    }

    public struct RTCDataChannelInit
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool reliable;
        [MarshalAs(UnmanagedType.U1)]
        public bool ordered;
        public int maxRetransmitTime;
        public int maxRetransmits;
        [MarshalAs(UnmanagedType.LPStr)]
        public string protocol;
        [MarshalAs(UnmanagedType.U1)]
        public bool negotiated;
        public int id;

        public RTCDataChannelInit(bool reliable)
        {
            this.reliable = reliable;
            ordered = true;
            maxRetransmitTime = -1;
            maxRetransmits = -1;
            negotiated = false;
            id = -1;
            protocol = "";
        }
    }

    public enum RTCErrorDetailType
    {
        DataChannelFailure,
        DtlsFailure,
        FingerprintFailure,
        IdpBadScriptFailure,
        IdpExecutionFailure,
        IdpLoadFailure,
        IdpNeedLogin,
        IdpTimeout,
        IdpTlsFailure,
        IdpTokenExpired,
        IdpTokenInvalid,
        SctpFailure,
        SdpSyntaxError,
        HardwareEncoderNotAvailable,
        HardwareEncoderError
    }

    public struct RTCError
    {
        public RTCErrorType errorType;
        public string message;
    }

    public enum RTCPeerConnectionState
    {
        New,
        Connecting,
        Connected,
        Disconnected,
        Failed,
        Closed
    }

    public enum RTCIceConnectionState
    {
        New,
        Checking,
        Connected,
        Completed,
        Failed,
        Disconnected,
        Closed,
        Max
    }

    public enum RTCSignalingState
    {
        Stable,
        HaveLocalOffer,
        HaveRemoteOffer,
        HaveLocalPranswer,
        HaveRemotePranswer,
        Closed
    }

    public enum RTCErrorType
    {
        None,
        UnsupportedOperation,
        UnsupportedParameter,
        InvalidParameter,
        InvalidRange,
        SyntaxError,
        InvalidState,
        InvalidModification,
        NetworkError,
        ResourceExhausted,
        InternalError,
        OperationErrorWithData
    }

    public enum RTCPeerConnectionEventType
    {
        ConnectionStateChange,
        DataChannel,
        IceCandidate,
        IceConnectionStateChange,
        Track
    }

    public enum RTCSdpType
    {
        Offer,
        Pranswer,
        Answer,
        Rollback
    }

    public enum RTCBundlePolicy
    {
        BundlePolicyBalanced,
        BundlePolicyMaxBundle,
        BundlePolicyMaxCompat
    }

    public enum RTCDataChannelState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    public struct RTCSessionDescription
    {
        public RTCSdpType type;
        [MarshalAs(UnmanagedType.LPStr)]
        public string sdp;
    }

    public struct RTCOfferOptions
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool iceRestart;
        [MarshalAs(UnmanagedType.U1)]
        public bool offerToReceiveAudio;
        [MarshalAs(UnmanagedType.U1)]
        public bool offerToReceiveVideo;
    }

    public struct RTCAnswerOptions
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool iceRestart;
    }

    public enum RTCIceCredentialType
    {
        Password,
        OAuth
    }

    [Serializable]
    public struct RTCIceServer
    {
        [Tooltip("Optional: specifies the password to use when authenticating with the ICE server")]
        public string credential;
        [Tooltip("What type of credential the `password` value")]
        public RTCIceCredentialType credentialType;
        [Tooltip("Array to set URLs of your STUN/TURN servers")]
        public string[] urls;
        [Tooltip("Optional: specifies the username to use when authenticating with the ICE server")]
        public string username;
    }

    public enum RTCIceTransportPolicy
    {
        Relay,
        All
    }

    [Serializable]
    public struct RTCConfiguration
    {
        public RTCIceServer[] iceServers;
        public RTCIceTransportPolicy iceTransportPolicy;
        public RTCBundlePolicy bundlePolicy;
    }

    public enum CodecInitializationResult
    {
        NotInitialized,
        Success,
        DriverNotInstalled,
        DriverVersionDoesNotSupportAPI,
        APINotFound,
        EncoderInitializationFailed
    }

    public static class WebRTC
    {
#if UNITY_EDITOR_OSX
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/x86_64/webrtc.bundle/Contents/MacOS/webrtc";
#elif UNITY_EDITOR_LINUX
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/x86_64/libwebrtc.so";
#elif UNITY_EDITOR_WIN
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/x86_64/webrtc.dll";
#elif UNITY_STANDALONE
        internal const string Lib = "webrtc";
#endif
        private static Context s_context;
        private static SynchronizationContext s_syncContext;
        internal static Material flipMat;


#if UNITY_EDITOR
        static public void OnBeforeAssemblyReload()
        {
            Dispose();
        }
#endif

        public static void Initialize(EncoderType type = EncoderType.Hardware)
        {
            // todo(kazuki): Add this event to avoid crash caused by hot-reload.
            // Dispose of all before reloading assembly.
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
#endif
            if (Application.platform != RuntimePlatform.LinuxEditor &&
                Application.platform != RuntimePlatform.LinuxPlayer)
            {
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore ||
                    SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
                    SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                {
                    Debug.LogError($"Not Support OpenGL API on {Application.platform}.");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    return;
#else
                    throw new NotSupportedException($"Not Support OpenGL API on {Application.platform} in Unity WebRTC.");
#endif
                }
            }

            NativeMethods.RegisterDebugLog(DebugLog);
            s_context = Context.Create(encoderType:type);
            NativeMethods.SetCurrentContext(s_context.self);
            s_syncContext = SynchronizationContext.Current;
            var flipShader = Resources.Load<Shader>("Flip");
            if (flipShader != null)
            {
                flipMat = new Material(flipShader);
            }
        }
        public static IEnumerator Update()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();
                {
                    foreach(var track in VideoStreamTrack.tracks)
                    {
                        if (track.IsEncoderInitialized)
                        {
                            track.Update();
                        }
                        else if (track.IsDecoderInitialized)
                        {
                            track.UpdateReceiveTexture();
                        }
                    }
                }
            }
        }

        public static void Dispose()
        {
            if (s_context != null)
            {
                s_context.Dispose();
                s_context = null;
            }
            s_syncContext = null;
            NativeMethods.RegisterDebugLog(null);

#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
#endif
        }

        class CallbackObject
        {
            public readonly IntPtr ptr;
            public readonly Action callback;

            public CallbackObject(IntPtr ptr, Action callback)
            {
                this.ptr = ptr;
                this.callback = callback;
            }
        }

        public static void Sync(IntPtr ptr, Action callback)
        {
            s_syncContext.Post(SendOrPostCallback, new CallbackObject(ptr, callback));
        }

        static void SendOrPostCallback(object state)
        {
            var obj = state as CallbackObject;
            if (s_context == null || !Table.ContainsKey(obj.ptr)) {
                return;
            }
            obj.callback();
        }

        public static EncoderType GetEncoderType()
        {
            return s_context.GetEncoderType();
        }

        internal static string GetModuleName()
        {
            return System.IO.Path.GetFileName(Lib);
        }

        public static RenderTextureFormat GetSupportedRenderTextureFormat(UnityEngine.Rendering.GraphicsDeviceType type)
        {
            switch (type)
            {
                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D11:
                case UnityEngine.Rendering.GraphicsDeviceType.Direct3D12:
                case UnityEngine.Rendering.GraphicsDeviceType.Vulkan:
                    return RenderTextureFormat.BGRA32;
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                    return RenderTextureFormat.ARGB32;
                case UnityEngine.Rendering.GraphicsDeviceType.Metal:
                    return RenderTextureFormat.BGRA32;
            }
            return RenderTextureFormat.Default;
        }

        internal static IEnumerable<T> Deserialize<T>(IntPtr buf, int length, Func<IntPtr, T> constructor) where T : class
        {
            var array = new IntPtr[length];
            Marshal.Copy(buf, array, 0, length);
            Marshal.FreeCoTaskMem(buf);

            var list = new List<T>();
            foreach (var ptr in array)
            {
                list.Add(FindOrCreate(ptr, constructor));
            }
            return list;
        }

        internal static T FindOrCreate<T>(IntPtr ptr, Func<IntPtr, T> constructor) where T : class
        {
            if (Context.table.ContainsKey(ptr))
            {
                return Context.table[ptr] as T;
            }
            else
            {
                return constructor(ptr);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        static void DebugLog(string str)
        {
            Debug.Log(str);
        }

        internal static Context Context { get { return s_context; } }
        internal static Hashtable Table { get { return s_context?.table; } }

        public static bool SupportHardwareEncoder
        {
            get
            {
                return NativeMethods.GetHardwareEncoderSupport();
            }
        }

        /// <summary>
        /// Not implement this property.
        /// Please check each track about initialization. (VideoStreamTrack.IsInitialized)
        /// This property will be removed next major version up.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        [Obsolete("Use 'VideoStreamTrack.IsInitialized' instead.", true)]
        public static CodecInitializationResult CodecInitializationResult
        {
            get
            {
                throw new NotImplementedException("This property is obsoleted. Please use VideoStreamTrack.IsInitialized instead of this");
            }
        }

        public static IReadOnlyList<RTCPeerConnection> PeerList
        {
            get
            {
                var list = new List<RTCPeerConnection>();
                if (Table?.Values != null)
                {
                    foreach (var value in Table?.Values)
                    {
                        if (value is RTCPeerConnection peer)
                        {
                            list.Add(peer);
                        }
                    }

                    return list;
                }

                return null;
            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateDebugLog([MarshalAs(UnmanagedType.LPStr)] string str);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateCreateSDSuccess(IntPtr ptr, RTCSdpType type, [MarshalAs(UnmanagedType.LPStr)] string sdp);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateCollectStats(IntPtr ptr, IntPtr reportPtr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateCreateGetStats(IntPtr ptr, RTCSdpType type, [MarshalAs(UnmanagedType.LPStr)] string sdp);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateCreateSDFailure(IntPtr ptr, RTCErrorType type, [MarshalAs(UnmanagedType.LPStr)] string message);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativePeerConnectionSetSessionDescSuccess(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativePeerConnectionSetSessionDescFailure(IntPtr ptr, RTCErrorType type, [MarshalAs(UnmanagedType.LPStr)] string message);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnIceConnectionChange(IntPtr ptr, RTCIceConnectionState state);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnIceCandidate(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string candidate, [MarshalAs(UnmanagedType.LPStr)] string sdpMid, int sdpMlineIndex);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //according to JS API naming, use OnNegotiationNeeded instead of OnRenegotiationNeeded
    internal delegate void DelegateNativeOnNegotiationNeeded(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnTrack(IntPtr ptr, IntPtr transceiver);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnDataChannel(IntPtr ptr, IntPtr ptrChannel);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnMessage(IntPtr ptr, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] bytes, int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnOpen(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnClose(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeMediaStreamOnAddTrack(IntPtr stream, IntPtr track);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeMediaStreamOnRemoveTrack(IntPtr stream, IntPtr track);

    internal static class NativeMethods
    {
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GetHardwareEncoderSupport();
        [DllImport(WebRTC.Lib)]
        public static extern void RegisterDebugLog(DelegateDebugLog func);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreate(int uid, EncoderType encoderType);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDestroy(int uid);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreatePeerConnection(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreatePeerConnectionWithConfig(IntPtr ptr, string conf);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeletePeerConnection(IntPtr ptr, IntPtr ptrPeerConnection);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionClose(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType PeerConnectionSetConfiguration(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string conf);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateDataChannel(IntPtr ptr, IntPtr ptrPeer, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label, ref RTCDataChannelInit options);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteDataChannel(IntPtr ptr, IntPtr ptrChannel);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateVideoTrack(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label, IntPtr texturePtr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateAudioTrack(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextStopMediaStreamTrack(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteMediaStreamTrack(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteStatsReport(IntPtr context, IntPtr report);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextSetVideoEncoderParameter(IntPtr context, IntPtr track, int width, int height);
        [DllImport(WebRTC.Lib)]
        public static extern CodecInitializationResult GetInitializationResult(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetConfiguration(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionCreateOffer(IntPtr ptr, ref RTCOfferOptions options);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionCreateAnswer(IntPtr ptr, ref RTCAnswerOptions options);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterCallbackCreateSD(IntPtr ptr, DelegateCreateSDSuccess onSuccess, DelegateCreateSDFailure onFailure);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterCallbackCollectStats(IntPtr ptr, DelegateCollectStats onCollectStats);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnSetSessionDescSuccess(IntPtr context, IntPtr connection, DelegateNativePeerConnectionSetSessionDescSuccess onSuccess);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnSetSessionDescFailure(IntPtr context, IntPtr connection, DelegateNativePeerConnectionSetSessionDescFailure onFailure);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterIceConnectionChange(IntPtr ptr, DelegateNativeOnIceConnectionChange callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnIceCandidate(IntPtr ptr, DelegateNativeOnIceCandidate callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionSetLocalDescription(IntPtr context, IntPtr ptr, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionGetStats(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionSenderGetStats(IntPtr ptr, IntPtr sender);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionReceiverGetStats(IntPtr sender, IntPtr receiver);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionGetLocalDescription(IntPtr ptr, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionGetRemoteDescription(IntPtr ptr, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionGetPendingLocalDescription(IntPtr ptr, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionGetPendingRemoteDescription(IntPtr ptr, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionGetCurrentLocalDescription(IntPtr ptr, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionGetCurrentRemoteDescription(IntPtr ptr, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionSetRemoteDescription(IntPtr context, IntPtr ptr, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTrack(IntPtr pc, IntPtr track, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string streamId);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTransceiver(IntPtr pc, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRemoveTrack(IntPtr pc, IntPtr sender);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionAddIceCandidate(IntPtr ptr, ref RTCIceCandidate candidate);
        [DllImport(WebRTC.Lib)]
        public static extern RTCPeerConnectionState PeerConnectionState(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetReceivers(IntPtr ptr, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetSenders(IntPtr ptr, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetTransceivers(IntPtr ptr, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern RTCIceConnectionState PeerConnectionIceConditionState(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern RTCSignalingState PeerConnectionSignalingState(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnDataChannel(IntPtr ptr, DelegateNativeOnDataChannel callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnRenegotiationNeeded(IntPtr ptr, DelegateNativeOnNegotiationNeeded callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnTrack(IntPtr ptr, DelegateNativeOnTrack callback);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr TransceiverGetTrack(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool TransceiverGetCurrentDirection(IntPtr transceiver, ref RTCRtpTransceiverDirection direction);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool TransceiverStop(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr TransceiverGetReceiver(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr TransceiverGetSender(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr SenderGetTrack(IntPtr sender);
        [DllImport(WebRTC.Lib)]
        public static extern void SenderGetParameters(IntPtr sender, out IntPtr parameters);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType SenderSetParameters(IntPtr sender, IntPtr parameters);
        [DllImport(WebRTC.Lib)]
        public static extern int DataChannelGetID(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr DataChannelGetLabel(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern RTCDataChannelState DataChannelGetReadyState(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelSend(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)]string msg);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelSendBinary(IntPtr ptr, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] bytes, int size);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelClose(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnMessage(IntPtr ptr, DelegateNativeOnMessage callback);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnOpen(IntPtr ptr, DelegateNativeOnOpen callback);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnClose(IntPtr ptr, DelegateNativeOnClose callback);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateMediaStream(IntPtr ctx, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteMediaStream(IntPtr ctx, IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern EncoderType ContextGetEncoderType(IntPtr context);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool MediaStreamAddTrack(IntPtr stream, IntPtr track);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool MediaStreamRemoveTrack(IntPtr stream, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamGetVideoTracks(IntPtr stream, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamGetAudioTracks(IntPtr stream, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamGetID(IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern void MediaStreamRegisterOnAddTrack(IntPtr context, IntPtr stream, DelegateNativeMediaStreamOnAddTrack callback);
        [DllImport(WebRTC.Lib)]
        public static extern void MediaStreamRegisterOnRemoveTrack(IntPtr context, IntPtr stream, DelegateNativeMediaStreamOnRemoveTrack callback);
        [DllImport(WebRTC.Lib)]
        public static extern TrackKind MediaStreamTrackGetKind(IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern TrackState MediaStreamTrackGetReadyState(IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamTrackGetID(IntPtr track);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool MediaStreamTrackGetEnabled(IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern void MediaStreamTrackSetEnabled(IntPtr track, [MarshalAs(UnmanagedType.U1)] bool enabled);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr CreateVideoRenderer(IntPtr context);
        [DllImport(WebRTC.Lib)]
        public static extern uint GetVideoRendererId(IntPtr sink);
        [DllImport(WebRTC.Lib)]
        public static extern void DeleteVideoRenderer(IntPtr context, IntPtr sink);
        [DllImport(WebRTC.Lib)]
        public static extern void VideoTrackAddOrUpdateSink(IntPtr track, IntPtr sink);
        [DllImport(WebRTC.Lib)]
        public static extern void VideoTrackRemoveSink(IntPtr track, IntPtr sink);
        [DllImport(WebRTC.Lib)]
        public static extern void SetCurrentContext(IntPtr context);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr GetRenderEventFunc(IntPtr context);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr GetUpdateTextureFunc(IntPtr context);
        [DllImport(WebRTC.Lib)]
        public static extern void ProcessAudio(float[] data, int size);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsReportGetStatsList(IntPtr report, ref uint length, ref IntPtr types);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsGetJson(IntPtr stats);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsGetId(IntPtr stats);
        [DllImport(WebRTC.Lib)]
        public static extern RTCStatsType StatsGetType(IntPtr stats);
        [DllImport(WebRTC.Lib)]
        public static extern long StatsGetTimestamp(IntPtr stats);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsGetMembers(IntPtr stats, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetName(IntPtr member);
        [DllImport(WebRTC.Lib)]
        public static extern StatsMemberType StatsMemberGetType(IntPtr member);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool StatsMemberIsDefined(IntPtr member);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool StatsMemberGetBool(IntPtr member);
        [DllImport(WebRTC.Lib)]
        public static extern int StatsMemberGetInt(IntPtr member);
        [DllImport(WebRTC.Lib)]
        public static extern uint StatsMemberGetUnsignedInt(IntPtr member);
        [DllImport(WebRTC.Lib)]
        public static extern long StatsMemberGetLong(IntPtr member);
        [DllImport(WebRTC.Lib)]
        public static extern ulong StatsMemberGetUnsignedLong(IntPtr member);
        [DllImport(WebRTC.Lib)]
        public static extern double StatsMemberGetDouble(IntPtr member);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetString(IntPtr member);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetBoolArray(IntPtr member, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetIntArray(IntPtr member, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetUnsignedIntArray(IntPtr member, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetLongArray(IntPtr member, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetUnsignedLongArray(IntPtr member, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetDoubleArray(IntPtr member, ref uint length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetStringArray(IntPtr member, ref uint length);
    }

    internal static class VideoEncoderMethods
    {
        static UnityEngine.Rendering.CommandBuffer _command = new UnityEngine.Rendering.CommandBuffer();
        enum VideoStreamRenderEventId
        {
            Initialize = 0,
            Encode = 1,
            Finalize = 2,
        }

        public static void InitializeEncoder(IntPtr callback, IntPtr track)
        {
            _command.IssuePluginEventAndData(callback, (int)VideoStreamRenderEventId.Initialize, track);
            Graphics.ExecuteCommandBuffer(_command);
            _command.Clear();
        }

        public static void Encode(IntPtr callback, IntPtr track)
        {
            _command.IssuePluginEventAndData(callback, (int)VideoStreamRenderEventId.Encode, track);
            Graphics.ExecuteCommandBuffer(_command);
            _command.Clear();
        }
        public static void FinalizeEncoder(IntPtr callback, IntPtr track)
        {
            _command.IssuePluginEventAndData(callback, (int)VideoStreamRenderEventId.Finalize, track);
            Graphics.ExecuteCommandBuffer(_command);
            _command.Clear();
        }
    }

    internal static class VideoDecoderMethods
    {
        static UnityEngine.Rendering.CommandBuffer _command = new UnityEngine.Rendering.CommandBuffer();

        public static void UpdateRendererTexture(IntPtr callback, Texture texture, uint rendererId)
        {
            _command.IssuePluginCustomTextureUpdateV2(callback, texture, rendererId);
            Graphics.ExecuteCommandBuffer(_command);
            _command.Clear();
        }
    }
}


