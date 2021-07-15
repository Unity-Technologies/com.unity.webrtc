using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="WebRTC.Initialize(EncoderType)"/>
    public enum EncoderType
    {
        Software = 0,
        Hardware = 1
    }

    /// <summary>
    ///
    /// </summary>
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

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.ConnectionState"/>
    public enum RTCPeerConnectionState : int
    {
        New = 0,
        Connecting = 1,
        Connected = 2,
        Disconnected = 3,
        Failed = 4,
        Closed = 5
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.IceConnectionState"/>
    public enum RTCIceConnectionState : int
    {
        New = 0,
        Checking = 1,
        Connected = 2,
        Completed = 3,
        Failed = 4,
        Disconnected = 5,
        Closed = 6,
        Max =7
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.GatheringState"/>
    public enum RTCIceGatheringState : int
    {
        New = 0,
        Gathering = 1,
        Complete = 2
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.SignalingState"/>
    public enum RTCSignalingState : int
    {
        Stable = 0,
        HaveLocalOffer = 1,
        HaveLocalPrAnswer = 2,
        HaveRemoteOffer = 3,
        HaveRemotePrAnswer = 4,
        Closed = 5,
    }

    /// <summary>
    ///
    /// </summary>
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

    /// <summary>
    /// Please check the <see cref="RTCConfiguration.bundlePolicy"/> in the <see cref="RTCConfiguration"/> class.
    /// </summary>
    /// <seealso cref="RTCConfiguration.bundlePolicy"/>
    public enum RTCBundlePolicy : int
    {
        BundlePolicyBalanced = 0,
        BundlePolicyMaxBundle = 1,
        BundlePolicyMaxCompat = 2
    }

    /// <summary>
    /// Please check the <see cref="RTCDataChannel.ReadyState"> in the <see cref="RTCDataChannel"/> class.
    /// </summary>
    /// <seealso cref="RTCDataChannel.ReadyState"/>
    public enum RTCDataChannelState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    /// <summary>
    ///
    /// </summary>
    public struct RTCSessionDescription
    {
        public RTCSdpType type;
        [MarshalAs(UnmanagedType.LPStr)]
        public string sdp;
    }

    /// <summary>
    ///
    /// </summary>
    public struct RTCOfferAnswerOptions
    {
        public static RTCOfferAnswerOptions Default =
            new RTCOfferAnswerOptions {iceRestart = false, voiceActivityDetection = true};

        /// <summary>
        ///
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool iceRestart;
        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// this property is not supported yet.
        /// </remarks>
        [MarshalAs(UnmanagedType.U1)]
        public bool voiceActivityDetection;
    }

    /// <summary>
    /// Please check the <see cref="RTCIceServer.credentialType"> in the <see cref="RTCIceServer"/> struct.
    /// </summary>
    /// <seealso cref="RTCIceServer.credentialType"/>
    public enum RTCIceCredentialType
    {
        Password,
        OAuth
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCConfiguration"/>
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

    /// <summary>
    /// Please check the <see cref="RTCConfiguration.iceTransportPolicy"> in the <see cref="RTCConfiguration"/> class.
    /// </summary>
    /// <seealso cref="RTCConfiguration.iceTransportPolicy"/>
    public enum RTCIceTransportPolicy : int
    {
        /// <summary>
        ///
        /// </summary>
        Relay = 1,
        /// <summary>
        ///
        /// </summary>
        All = 3
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.GetConfiguration()"/>
    /// <seealso cref="RTCPeerConnection.SetConfiguration(ref RTCConfiguration)"/>
    [Serializable]
    public struct RTCConfiguration
    {
        /// <summary>
        ///
        /// </summary>
        public RTCIceServer[] iceServers;
        /// <summary>
        ///
        /// </summary>
        public RTCIceTransportPolicy? iceTransportPolicy;
        /// <summary>
        ///
        /// </summary>
        public RTCBundlePolicy? bundlePolicy;
        /// <summary>
        ///
        /// </summary>
        public int? iceCandidatePoolSize;
        /// <summary>
        ///
        /// </summary>
        public bool? enableDtlsSrtp;

        internal RTCConfiguration(ref RTCConfigurationInternal v)
        {
            iceServers = v.iceServers;
            iceTransportPolicy = v.iceTransportPolicy.AsEnum<RTCIceTransportPolicy>();
            bundlePolicy = v.bundlePolicy.AsEnum<RTCBundlePolicy>();
            iceCandidatePoolSize = v.iceCandidatePoolSize;
            enableDtlsSrtp = v.enableDtlsSrtp;
        }

        internal RTCConfigurationInternal Cast()
        {
            RTCConfigurationInternal instance = new RTCConfigurationInternal
            {
                iceServers = this.iceServers,
                iceTransportPolicy = OptionalInt.FromEnum(this.iceTransportPolicy),
                bundlePolicy = OptionalInt.FromEnum(this.bundlePolicy),
                iceCandidatePoolSize = this.iceCandidatePoolSize,
                enableDtlsSrtp = this.enableDtlsSrtp
            };
            return instance;
        }
    }

    [Serializable]
    struct RTCConfigurationInternal
    {
        public RTCIceServer[] iceServers;
        public OptionalInt iceTransportPolicy;
        public OptionalInt bundlePolicy;
        public OptionalInt iceCandidatePoolSize;
        public OptionalBool enableDtlsSrtp;
    }

    /// <summary>
    ///
    /// </summary>
    public enum CodecInitializationResult
    {
        NotInitialized,
        Success,
        DriverNotInstalled,
        DriverVersionDoesNotSupportAPI,
        APINotFound,
        EncoderInitializationFailed
    }

    /// <summary>
    ///
    /// </summary>
    public static class WebRTC
    {
#if UNITY_EDITOR_OSX
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/macOS/webrtc.bundle/Contents/MacOS/webrtc";
#elif UNITY_EDITOR_LINUX
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/x86_64/libwebrtc.so";
#elif UNITY_EDITOR_WIN
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/x86_64/webrtc.dll";
#elif UNITY_STANDALONE
        internal const string Lib = "webrtc";
#elif UNITY_IOS
        internal const string Lib = "__Internal";
#elif UNITY_ANDROID
        internal const string Lib = "webrtc";
#endif
        private static Context s_context = null;
        private static SynchronizationContext s_syncContext;
        internal static Material flipMat;
        private static bool s_limitTextureSize;

#if UNITY_EDITOR
        static public void OnBeforeAssemblyReload()
        {
            Dispose();
        }
#endif

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <param name="limitTextureSize"></param>
        public static void Initialize(EncoderType type = EncoderType.Hardware, bool limitTextureSize = true)
        {
            Initialize(type, limitTextureSize, false);
        }


        internal static void Initialize(EncoderType type, bool limitTextureSize, bool forTest)
        {
            // todo(kazuki): Add this event to avoid crash caused by hot-reload.
            // Dispose of all before reloading assembly.
#if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
#endif
            // OpenGL APIs on windows/osx are not supported
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.OSXPlayer)
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
#if UNITY_IOS && !UNITY_EDITOR
            NativeMethods.RegisterRenderingWebRTCPlugin();
#endif
            s_context = Context.Create(encoderType:type, forTest:forTest);
            NativeMethods.SetCurrentContext(s_context.self);
            s_syncContext = SynchronizationContext.Current;
            var flipShader = Resources.Load<Shader>("Flip");
            if (flipShader != null)
            {
                flipMat = new Material(flipShader);
            }

            s_limitTextureSize = limitTextureSize;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static IEnumerator Update()
        {
            while (true)
            {
                // Wait until all frame rendering is done
                yield return new WaitForEndOfFrame();
                {
                    foreach (var reference in VideoStreamTrack.s_tracks.Values)
                    {
                        if (!reference.TryGetTarget(out var track))
                            continue;
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

        /// <summary>
        ///
        /// </summary>
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

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static EncoderType GetEncoderType()
        {
            return s_context.GetEncoderType();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="platform"></param>
        /// <param name="encoderType"></param>
        public static void ValidateTextureSize(int width, int height, RuntimePlatform platform, EncoderType encoderType)
        {
            if (!s_limitTextureSize)
            {
                return;
            }

            // Check NVCodec capabilities
            // todo(kazuki):: The constant values should be replaced by values that are got from NvCodec API.
            // Use "nvEncGetEncodeCaps" function which is provided by the NvCodec API.
            if (encoderType == EncoderType.Hardware && NvEncSupportedPlatdorm(platform))
            {
                const int minWidth = 145;
                const int maxWidth = 4096;
                const int minHeight = 49;
                const int maxHeight = 4096;

                if (width < minWidth || maxWidth < width ||
                    height < minHeight || maxHeight < height)
                {
                    throw new ArgumentException(
                        $"Texture size is invalid. " +
                        $"minWidth:{minWidth}, maxWidth:{maxWidth} " +
                        $"minHeight:{minHeight}, maxHeight:{maxHeight} " +
                        $"current size width:{width} height:{height}");
                }
            }

            if (platform == RuntimePlatform.Android)
            {
                // Some android crash when smaller than this size
                const int minimumTextureSize = 114;
                if (width < minimumTextureSize || height < minimumTextureSize)
                {
                    throw new ArgumentException(
                        $"Texture size need {minimumTextureSize}, current size width:{width} height:{height}");
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="format"></param>
        public static void ValidateGraphicsFormat(GraphicsFormat format)
        {
            // can't recognize legacy format
            const int LegacyARGB32_sRGB = 87;
            const int LegacyARGB32_UNorm = 88;
            if ((int) format == LegacyARGB32_sRGB || (int) format == LegacyARGB32_UNorm)
            {
                return;
            }

            // ToDo: Increase the supported formats.
            GraphicsFormat supportedFormat = GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            if (format != supportedFormat)
            {
                throw new ArgumentException(
                    $"This graphics format {format} is not supported for streaming, please use supportedFormat: {supportedFormat}");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static RenderTextureFormat GetSupportedRenderTextureFormat(GraphicsDeviceType type)
        {
            var graphicsFormat = GetSupportedGraphicsFormat(type);
            return GraphicsFormatUtility.GetRenderTextureFormat(graphicsFormat);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static GraphicsFormat GetSupportedGraphicsFormat(GraphicsDeviceType type)
        {
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                switch (type)
                {
                    case GraphicsDeviceType.Direct3D11:
                    case GraphicsDeviceType.Direct3D12:
                    case GraphicsDeviceType.Vulkan:
                        return GraphicsFormat.B8G8R8A8_SRGB;
                    case GraphicsDeviceType.OpenGLCore:
                    case GraphicsDeviceType.OpenGLES2:
                    case GraphicsDeviceType.OpenGLES3:
                        return GraphicsFormat.R8G8B8A8_SRGB;
                    case GraphicsDeviceType.Metal:
                        return GraphicsFormat.B8G8R8A8_SRGB;
                }
            }
            else
            {
                switch (type)
                {
                    case GraphicsDeviceType.Direct3D11:
                    case GraphicsDeviceType.Direct3D12:
                    case GraphicsDeviceType.Vulkan:
                        return GraphicsFormat.B8G8R8A8_UNorm;
                    case GraphicsDeviceType.OpenGLCore:
                    case GraphicsDeviceType.OpenGLES2:
                    case GraphicsDeviceType.OpenGLES3:
                        return GraphicsFormat.R8G8B8A8_UNorm;
                    case GraphicsDeviceType.Metal:
                        return GraphicsFormat.B8G8R8A8_UNorm;
                }
            }

            throw new ArgumentException($"Graphics device type {type} not supported");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TextureFormat GetSupportedTextureFormat(GraphicsDeviceType type)
        {
            var graphicsFormat = GetSupportedGraphicsFormat(type);
            return GraphicsFormatUtility.GetTextureFormat(graphicsFormat);
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

        static bool NvEncSupportedPlatdorm(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                    return true;
            }
            return false;
        }

        internal static void Sync(IntPtr ptr, Action callback)
        {
            s_syncContext.Post(SendOrPostCallback, new CallbackObject(ptr, callback));
        }
        internal static string GetModuleName()
        {
            return System.IO.Path.GetFileName(Lib);
        }

        static void SendOrPostCallback(object state)
        {
            var obj = state as CallbackObject;
            if (s_context == null || !Table.ContainsKey(obj.ptr))
            {
                return;
            }
            obj.callback();
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
        internal static WeakReferenceTable Table { get { return s_context?.table; } }

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

        public static IReadOnlyList<WeakReference<RTCPeerConnection>> PeerList
        {
            get
            {
                var list = new List<WeakReference<RTCPeerConnection>>();
                var values = Table?.CopiedValues;
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        if (value is RTCPeerConnection peer)
                        {
                            list.Add(new WeakReference<RTCPeerConnection>(peer));
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
    internal delegate void DelegateNativeOnConnectionStateChange(IntPtr ptr, RTCPeerConnectionState state);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnIceGatheringChange(IntPtr ptr, RTCIceGatheringState state);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnIceCandidate(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string candidate, [MarshalAs(UnmanagedType.LPStr)] string sdpMid, int sdpMlineIndex);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //according to JS API naming, use OnNegotiationNeeded instead of OnRenegotiationNeeded
    internal delegate void DelegateNativeOnNegotiationNeeded(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnTrack(IntPtr ptr, IntPtr transceiver);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnRemoveTrack(IntPtr ptr, IntPtr receiver);
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
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateAudioReceive(
        IntPtr track, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] float[] audioData, int size,
        int sampleRate, int numOfChannels, int numOfFrames);

    internal static class NativeMethods
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport(WebRTC.Lib)]
        public static extern void RegisterRenderingWebRTCPlugin();
#endif
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool GetHardwareEncoderSupport();
        [DllImport(WebRTC.Lib)]
        public static extern void RegisterDebugLog(DelegateDebugLog func);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreate(int uid, EncoderType encoderType, [MarshalAs(UnmanagedType.U1)] bool forTest);
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
        public static extern void PeerConnectionRestartIce(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType PeerConnectionSetConfiguration(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string conf);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateDataChannel(IntPtr ptr, IntPtr ptrPeer, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label, ref RTCDataChannelInitInternal options);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteDataChannel(IntPtr ptr, IntPtr ptrChannel);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateVideoTrack(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateAudioTrack(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextStopMediaStreamTrack(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteMediaStreamTrack(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteStatsReport(IntPtr context, IntPtr report);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextSetVideoEncoderParameter(IntPtr context, IntPtr track, int width, int height, GraphicsFormat format, IntPtr texturePtr);
        [DllImport(WebRTC.Lib)]
        public static extern CodecInitializationResult GetInitializationResult(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetConfiguration(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionCreateOffer(IntPtr ptr, ref RTCOfferAnswerOptions options);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionCreateAnswer(IntPtr ptr, ref RTCOfferAnswerOptions options);
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
        public static extern void PeerConnectionRegisterConnectionStateChange(IntPtr ptr, DelegateNativeOnConnectionStateChange callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterIceGatheringChange(IntPtr ptr, DelegateNativeOnIceGatheringChange callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnIceCandidate(IntPtr ptr, DelegateNativeOnIceCandidate callback);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType PeerConnectionSetLocalDescription(IntPtr context, IntPtr ptr, ref RTCSessionDescription desc, ref IntPtr error);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType PeerConnectionSetLocalDescriptionWithoutDescription(IntPtr context, IntPtr ptr, ref IntPtr error);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType PeerConnectionSetRemoteDescription(IntPtr context, IntPtr ptr, ref RTCSessionDescription desc, ref IntPtr error);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionGetStats(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionSenderGetStats(IntPtr ptr, IntPtr sender);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextGetSenderCapabilities(IntPtr context, TrackKind kind, out IntPtr capabilities);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextGetReceiverCapabilities(IntPtr context, TrackKind kind, out IntPtr capabilities);
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
        public static extern IntPtr PeerConnectionAddTrack(IntPtr pc, IntPtr track, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string streamId);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTransceiver(IntPtr pc, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTransceiverWithType(IntPtr pc, TrackKind kind);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRemoveTrack(IntPtr pc, IntPtr sender);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionAddIceCandidate(IntPtr ptr, IntPtr candidate);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType CreateIceCandidate(ref RTCIceCandidateInitInternal options, out IntPtr candidate);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType DeleteIceCandidate(IntPtr candidate);
        [DllImport(WebRTC.Lib)]
        public static extern void IceCandidateGetCandidate(IntPtr candidate, out CandidateInternal dst);
        [DllImport(WebRTC.Lib)]
        public static extern int IceCandidateGetSdpLineIndex(IntPtr candidate);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string IceCandidateGetSdp(IntPtr candidate);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string IceCandidateGetSdpMid(IntPtr candidate);
        [DllImport(WebRTC.Lib)]
        public static extern RTCPeerConnectionState PeerConnectionState(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetReceivers(IntPtr ptr, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetSenders(IntPtr ptr, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetTransceivers(IntPtr ptr, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern RTCIceConnectionState PeerConnectionIceConditionState(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern RTCSignalingState PeerConnectionSignalingState(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern RTCIceGatheringState PeerConnectionIceGatheringState(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnDataChannel(IntPtr ptr, DelegateNativeOnDataChannel callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnRenegotiationNeeded(IntPtr ptr, DelegateNativeOnNegotiationNeeded callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnTrack(IntPtr ptr, DelegateNativeOnTrack callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnRemoveTrack(IntPtr ptr, DelegateNativeOnRemoveTrack callback);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool TransceiverGetCurrentDirection(IntPtr transceiver, ref RTCRtpTransceiverDirection direction);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool TransceiverStop(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern RTCRtpTransceiverDirection TransceiverGetDirection(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern void TransceiverSetDirection(IntPtr transceiver, RTCRtpTransceiverDirection direction);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType TransceiverSetCodecPreferences(IntPtr transceiver, IntPtr capabilities, long length);
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
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool SenderReplaceTrack(IntPtr sender, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ReceiverGetTrack(IntPtr receiver);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ReceiverGetStreams(IntPtr receiver, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern int DataChannelGetID(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr DataChannelGetLabel(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr DataChannelGetProtocol(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern ushort DataChannelGetMaxRetransmits(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern ushort DataChannelGetMaxRetransmitTime(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool DataChannelGetOrdered(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern ulong DataChannelGetBufferedAmount(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool DataChannelGetNegotiated(IntPtr ptr);
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
        public static extern void ContextRegisterMediaStreamObserver(IntPtr ctx, IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextUnRegisterMediaStreamObserver(IntPtr ctx, IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextRegisterAudioReceiveCallback(IntPtr context, IntPtr track, DelegateAudioReceive callback);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextUnregisterAudioReceiveCallback(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern EncoderType ContextGetEncoderType(IntPtr context);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool MediaStreamAddTrack(IntPtr stream, IntPtr track);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool MediaStreamRemoveTrack(IntPtr stream, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamGetVideoTracks(IntPtr stream, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamGetAudioTracks(IntPtr stream, out ulong length);
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
        public static extern void ProcessAudio(IntPtr track, IntPtr array, int sampleRate, int channels, int frames);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsReportGetStatsList(IntPtr report, out ulong length, ref IntPtr types);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsGetJson(IntPtr stats);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsGetId(IntPtr stats);
        [DllImport(WebRTC.Lib)]
        public static extern RTCStatsType StatsGetType(IntPtr stats);
        [DllImport(WebRTC.Lib)]
        public static extern long StatsGetTimestamp(IntPtr stats);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsGetMembers(IntPtr stats, out ulong length);
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
        public static extern IntPtr StatsMemberGetBoolArray(IntPtr member, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetIntArray(IntPtr member, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetUnsignedIntArray(IntPtr member, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetLongArray(IntPtr member, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetUnsignedLongArray(IntPtr member, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetDoubleArray(IntPtr member, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetStringArray(IntPtr member, out ulong length);
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
#if !UNITY_2020_1_OR_NEWER
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12)
            {
                throw new NotSupportedException(
                    "CommandBuffer.IssuePluginCustomTextureUpdateV2 method is not supported " +
                    "when using Direct3D12 on Unity2019 or older.");
            }
#endif
            _command.IssuePluginCustomTextureUpdateV2(callback, texture, rendererId);
            Graphics.ExecuteCommandBuffer(_command);
            _command.Clear();
        }
    }
}
