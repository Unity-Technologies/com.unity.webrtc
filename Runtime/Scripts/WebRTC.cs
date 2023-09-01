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
    public enum RTCErrorDetailType
    {
        /// <summary>
        ///
        /// </summary>
        DataChannelFailure,

        /// <summary>
        ///
        /// </summary>
        DtlsFailure,

        /// <summary>
        ///
        /// </summary>
        FingerprintFailure,

        /// <summary>
        ///
        /// </summary>
        IdpBadScriptFailure,

        /// <summary>
        ///
        /// </summary>
        IdpExecutionFailure,

        /// <summary>
        ///
        /// </summary>
        IdpLoadFailure,

        /// <summary>
        ///
        /// </summary>
        IdpNeedLogin,

        /// <summary>
        ///
        /// </summary>
        IdpTimeout,

        /// <summary>
        ///
        /// </summary>
        IdpTlsFailure,

        /// <summary>
        ///
        /// </summary>
        IdpTokenExpired,

        /// <summary>
        ///
        /// </summary>
        IdpTokenInvalid,

        /// <summary>
        ///
        /// </summary>
        SctpFailure,

        /// <summary>
        ///
        /// </summary>
        SdpSyntaxError,

        /// <summary>
        ///
        /// </summary>
        HardwareEncoderNotAvailable,

        /// <summary>
        ///
        /// </summary>
        HardwareEncoderError
    }

    /// <summary>
    ///
    /// </summary>
    public struct RTCError
    {
        /// <summary>
        ///
        /// </summary>
        public RTCErrorType errorType;

        /// <summary>
        ///
        /// </summary>
        public string message;
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.ConnectionState"/>
    public enum RTCPeerConnectionState : int
    {
        /// <summary>
        ///
        /// </summary>
        New = 0,

        /// <summary>
        ///
        /// </summary>
        Connecting = 1,

        /// <summary>
        ///
        /// </summary>
        Connected = 2,

        /// <summary>
        ///
        /// </summary>
        Disconnected = 3,

        /// <summary>
        ///
        /// </summary>
        Failed = 4,

        /// <summary>
        ///
        /// </summary>
        Closed = 5
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.IceConnectionState"/>
    public enum RTCIceConnectionState : int
    {
        /// <summary>
        ///
        /// </summary>
        New = 0,

        /// <summary>
        ///
        /// </summary>
        Checking = 1,

        /// <summary>
        ///
        /// </summary>
        Connected = 2,

        /// <summary>
        ///
        /// </summary>
        Completed = 3,

        /// <summary>
        ///
        /// </summary>
        Failed = 4,

        /// <summary>
        ///
        /// </summary>
        Disconnected = 5,

        /// <summary>
        ///
        /// </summary>
        Closed = 6,

        /// <summary>
        ///
        /// </summary>
        Max = 7
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.GatheringState"/>
    public enum RTCIceGatheringState : int
    {
        /// <summary>
        ///
        /// </summary>
        New = 0,

        /// <summary>
        ///
        /// </summary>
        Gathering = 1,

        /// <summary>
        ///
        /// </summary>
        Complete = 2
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.SignalingState"/>
    public enum RTCSignalingState : int
    {
        /// <summary>
        ///
        /// </summary>
        Stable = 0,

        /// <summary>
        ///
        /// </summary>
        HaveLocalOffer = 1,

        /// <summary>
        ///
        /// </summary>
        HaveLocalPrAnswer = 2,

        /// <summary>
        ///
        /// </summary>
        HaveRemoteOffer = 3,

        /// <summary>
        ///
        /// </summary>
        HaveRemotePrAnswer = 4,

        /// <summary>
        ///
        /// </summary>
        Closed = 5,
    }

    /// <summary>
    ///
    /// </summary>
    public enum RTCErrorType
    {
        /// <summary>
        ///
        /// </summary>
        None,

        /// <summary>
        ///
        /// </summary>
        UnsupportedOperation,

        /// <summary>
        ///
        /// </summary>
        UnsupportedParameter,

        /// <summary>
        ///
        /// </summary>
        InvalidParameter,

        /// <summary>
        ///
        /// </summary>
        InvalidRange,

        /// <summary>
        ///
        /// </summary>
        SyntaxError,

        /// <summary>
        ///
        /// </summary>
        InvalidState,

        /// <summary>
        ///
        /// </summary>
        InvalidModification,

        /// <summary>
        ///
        /// </summary>
        NetworkError,

        /// <summary>
        ///
        /// </summary>
        ResourceExhausted,

        /// <summary>
        ///
        /// </summary>
        InternalError,

        /// <summary>
        ///
        /// </summary>
        OperationErrorWithData
    }

    /// <summary>
    ///
    /// </summary>
    public enum RTCPeerConnectionEventType
    {
        /// <summary>
        ///
        /// </summary>
        ConnectionStateChange,

        /// <summary>
        ///
        /// </summary>
        DataChannel,

        /// <summary>
        ///
        /// </summary>
        IceCandidate,

        /// <summary>
        ///
        /// </summary>
        IceConnectionStateChange,

        /// <summary>
        ///
        /// </summary>
        Track
    }

    /// <summary>
    ///
    /// </summary>
    public enum RTCSdpType
    {
        /// <summary>
        ///
        /// </summary>
        Offer,

        /// <summary>
        ///
        /// </summary>
        Pranswer,

        /// <summary>
        ///
        /// </summary>
        Answer,

        /// <summary>
        ///
        /// </summary>
        Rollback
    }

    /// <summary>
    /// Please check the <see cref="RTCConfiguration.bundlePolicy"/> in the <see cref="RTCConfiguration"/> class.
    /// </summary>
    /// <seealso cref="RTCConfiguration.bundlePolicy"/>
    public enum RTCBundlePolicy : int
    {
        /// <summary>
        ///
        /// </summary>
        BundlePolicyBalanced = 0,

        /// <summary>
        ///
        /// </summary>
        BundlePolicyMaxBundle = 1,

        /// <summary>
        ///
        /// </summary>
        BundlePolicyMaxCompat = 2
    }

    /// <summary>
    /// Please check the <see cref="RTCDataChannel.ReadyState"/> in the <see cref="RTCDataChannel"/> class.
    /// </summary>
    /// <seealso cref="RTCDataChannel.ReadyState"/>
    public enum RTCDataChannelState
    {
        /// <summary>
        ///
        /// </summary>
        Connecting,

        /// <summary>
        ///
        /// </summary>
        Open,

        /// <summary>
        ///
        /// </summary>
        Closing,

        /// <summary>
        ///
        /// </summary>
        Closed
    }

    /// <summary>
    ///
    /// </summary>
    public struct RTCSessionDescription
    {
        /// <summary>
        ///
        /// </summary>
        public RTCSdpType type;

        /// <summary>
        ///
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public string sdp;
    }

    /// <summary>
    ///
    /// </summary>
    public struct RTCOfferAnswerOptions
    {
        /// <summary>
        ///
        /// </summary>
        public static RTCOfferAnswerOptions Default =
            new RTCOfferAnswerOptions { iceRestart = false, voiceActivityDetection = true };

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
    /// Please check the <see cref="RTCIceServer.credentialType"/> in the <see cref="RTCIceServer"/> struct.
    /// </summary>
    /// <seealso cref="RTCIceServer.credentialType"/>
    public enum RTCIceCredentialType
    {
        /// <summary>
        ///
        /// </summary>
        Password,

        /// <summary>
        ///
        /// </summary>
        OAuth
    }

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCConfiguration"/>
    [Serializable]
    public struct RTCIceServer
    {
        /// <summary>
        ///
        /// </summary>
        [Tooltip("Optional: specifies the password to use when authenticating with the ICE server")]
        public string credential;

        /// <summary>
        ///
        /// </summary>
        [Tooltip("What type of credential the `password` value")]
        public RTCIceCredentialType credentialType;

        /// <summary>
        ///
        /// </summary>
        [Tooltip("Array to set URLs of your STUN/TURN servers")]
        public string[] urls;

        /// <summary>
        ///
        /// </summary>
        [Tooltip("Optional: specifies the username to use when authenticating with the ICE server")]
        public string username;
    }

    /// <summary>
    /// Please check the <see cref="RTCConfiguration.iceTransportPolicy"/> in the <see cref="RTCConfiguration"/> class.
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

        internal RTCConfiguration(ref RTCConfigurationInternal v)
        {
            iceServers = v.iceServers;
            iceTransportPolicy = v.iceTransportPolicy.AsEnum<RTCIceTransportPolicy>();
            bundlePolicy = v.bundlePolicy.AsEnum<RTCBundlePolicy>();
            iceCandidatePoolSize = v.iceCandidatePoolSize;
        }

        internal RTCConfigurationInternal Cast()
        {
            RTCConfigurationInternal instance = new RTCConfigurationInternal
            {
                iceServers = this.iceServers,
                iceTransportPolicy = OptionalInt.FromEnum(this.iceTransportPolicy),
                bundlePolicy = OptionalInt.FromEnum(this.bundlePolicy),
                iceCandidatePoolSize = this.iceCandidatePoolSize,
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
    public enum NativeLoggingSeverity
    {
        /// <summary>
        ///
        /// </summary>
        Verbose,

        /// <summary>
        ///
        /// </summary>
        Info,

        /// <summary>
        ///
        /// </summary>
        Warning,

        /// <summary>
        ///
        /// </summary>
        Error,

        /// <summary>
        ///
        /// </summary>
        None,
    };

    /// <summary>
    ///
    /// </summary>
    public static class WebRTC
    {
#if UNITY_IOS
        internal const string Lib = "__Internal";
#else
        internal const string Lib = "webrtc";
#endif
        private static Context s_context = null;
        private static SynchronizationContext s_syncContext;
        private static ILogger s_logger;

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInitializeOnLoadMethod()
        {
            // Initialize a custom invokable synchronization context to wrap the main thread UnitySynchronizationContext
            s_syncContext = new ExecutableUnitySynchronizationContext(SynchronizationContext.Current);
        }

        internal static void InitializeInternal(bool limitTextureSize = true, bool enableNativeLog = false,
            NativeLoggingSeverity nativeLoggingSeverity = NativeLoggingSeverity.Info)
        {
            if (s_context != null)
                throw new InvalidOperationException("Already initialized WebRTC.");

            NativeMethods.RegisterDebugLog(DebugLog, enableNativeLog, nativeLoggingSeverity);
            NativeMethods.StatsCollectorRegisterCallback(OnCollectStatsCallback);
            NativeMethods.CreateSessionDescriptionObserverRegisterCallback(OnCreateSessionDescription);
            NativeMethods.SetLocalDescriptionObserverRegisterCallback(OnSetLocalDescription);
            NativeMethods.SetRemoteDescriptionObserverRegisterCallback(OnSetRemoteDescription);
            NativeMethods.SetTransformedFrameRegisterCallback(OnSetTransformedFrame);
#if UNITY_IOS && !UNITY_EDITOR
            NativeMethods.RegisterRenderingWebRTCPlugin();
#endif
            s_context = Context.Create();
            s_context.limitTextureSize = limitTextureSize;

            NativeMethods.SetCurrentContext(s_context.self);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static IEnumerator Update()
        {
            var instruction = new WaitForEndOfFrame();

            while (true)
            {
                // Wait until all frame rendering is done
                yield return instruction;
                {
                    var tempTextureActive = RenderTexture.active;
                    RenderTexture.active = null;

                    var batch = Context.batch;
                    batch.ResizeCapacity(VideoStreamTrack.s_tracks.Count);

                    int trackIndex = 0;
                    foreach (var reference in VideoStreamTrack.s_tracks.Values)
                    {
                        if (!reference.TryGetTarget(out var track))
                            continue;

                        track.UpdateTexture();
                        if (track.DataPtr != IntPtr.Zero)
                        {
                            batch.data.tracks[trackIndex] = track.DataPtr;
                            trackIndex++;
                        }
                    }

                    batch.data.tracksCount = trackIndex;
                    if (trackIndex > 0)
                        batch.Submit();

                    RenderTexture.active = tempTextureActive;
                }
            }
        }

        /// <summary>
        /// Executes any pending tasks generated asynchronously during the WebRTC runtime.
        /// </summary>
        /// <param name="millisecondTimeout">
        /// The amount of time in milliseconds that the task queue can take before task execution will cease.
        /// </param>
        /// <returns>
        /// <c>true</c> if all pending tasks were completed within <see cref="millisecondTimeout"/> milliseconds and <c>false</c>
        /// otherwise.
        /// </returns>
        public static bool ExecutePendingTasks(int millisecondTimeout)
        {
            if (s_syncContext is ExecutableUnitySynchronizationContext executableContext)
            {
                return executableContext.ExecutePendingTasks(millisecondTimeout);
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool enableLimitTextureSize
        {
            get { return s_context.limitTextureSize; }
            set { s_context.limitTextureSize = value; }
        }

        /// <summary>
        /// Get & set the logger to use when logging debug messages inside the WebRTC package.
        /// By default will use Debug.unityLogger.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws if setting a null logger.</exception>
        public static ILogger Logger
        {
            get
            {
                if (s_logger == null)
                {
                    return Debug.unityLogger;
                }

                return s_logger;
            }
            set
            {
                s_logger = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Configure native logging settings for WebRTC.
        /// </summary>
        /// <param name="enableNativeLogging">Enables or disable native logging.</param>
        /// <param name="nativeLoggingSeverity">Sets the native logging level.</param>
        public static void ConfigureNativeLogging(bool enableNativeLogging, NativeLoggingSeverity nativeLoggingSeverity)
        {
            if (enableNativeLogging)
            {
                NativeMethods.RegisterDebugLog(DebugLog, enableNativeLogging, nativeLoggingSeverity);
            }
            else
            {
                NativeMethods.RegisterDebugLog(null, false, nativeLoggingSeverity);
            }
        }

        internal static void DisposeInternal()
        {
            if (s_context != null)
            {
                s_context.Dispose();
                s_context = null;
            }
            NativeMethods.RegisterDebugLog(null, false, NativeLoggingSeverity.Info);
        }

        internal static RTCError ValidateTextureSize(int width, int height, RuntimePlatform platform)
        {
            if (!s_context.limitTextureSize)
            {
                return new RTCError { errorType = RTCErrorType.None };
            }

            const int maxPixelCount = 3840 * 2160;

            // Using codec is determined when the Encoder initialization process.
            // Therefore, it is not possible to limit the resolution before that. (supported resolutions depend on the codec and its profile.)
            // For workaround, all 4k resolutions and above are considered as errors.
            // (Because under 4k resolution is almost supported by the supported codecs.)
            // todo: Resize the texture size when encoder initialization process, or fall back to another encoder.
            if (width * height > maxPixelCount)
            {
                return new RTCError
                {
                    errorType = RTCErrorType.InvalidRange,
                    message = $"Texture pixel count is invalid. " +
                              $"width:{width} x height:{height} is over 4k pixel count ({maxPixelCount})."
                };
            }

            // Check NVCodec capabilities
            // todo(kazuki):: The constant values should be replaced by values that are got from NvCodec API.
            // Use "nvEncGetEncodeCaps" function which is provided by the NvCodec API.
            if (NvEncSupportedPlatdorm(platform))
            {
                const int minWidth = 145;
                const int maxWidth = 4096;
                const int minHeight = 49;
                const int maxHeight = 4096;

                if (width < minWidth || maxWidth < width ||
                    height < minHeight || maxHeight < height)
                {
                    return new RTCError
                    {
                        errorType = RTCErrorType.InvalidRange,
                        message = $"Texture size is invalid. " +
                                  $"minWidth:{minWidth}, maxWidth:{maxWidth} " +
                                  $"minHeight:{minHeight}, maxHeight:{maxHeight} " +
                                  $"current size width:{width} height:{height}"
                    };
                }
            }

            if (platform == RuntimePlatform.Android)
            {
                // Some android crash when smaller than this size
                const int minimumTextureSize = 114;
                if (width < minimumTextureSize || height < minimumTextureSize)
                {
                    return new RTCError
                    {
                        errorType = RTCErrorType.InvalidRange,
                        message =
                            $"Texture size need {minimumTextureSize}, current size width:{width} height:{height}"
                    };
                }
            }

            return new RTCError { errorType = RTCErrorType.None };
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
            if ((int)format == LegacyARGB32_sRGB || (int)format == LegacyARGB32_UNorm)
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
        internal static void DestroyOnMainThread(UnityEngine.Object obj, float delay = 0f)
        {
            if (delay < 0f)
                throw new ArgumentException($"The delay value is smaller than zero. delay:{delay}");
            if (Mathf.Approximately(delay, 0f))
                s_syncContext.Post(DestroyImmediate, obj);
            else
                s_syncContext.Post(Destroy, Tuple.Create(obj, delay));
        }

        internal static void DelayActionOnMainThread(Action callback, float delay)
        {
            s_syncContext.Post(DelayAction, Tuple.Create(callback, delay));
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

        static void DestroyImmediate(object state)
        {
            var obj = state as UnityEngine.Object;
            UnityEngine.Object.DestroyImmediate(obj);
        }

        static void Destroy(object state)
        {
            (UnityEngine.Object obj, float delay) = state as Tuple<UnityEngine.Object, float>;

            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(obj);
                return;
            }
            UnityEngine.Object.Destroy(obj, delay);
        }

        static void DelayAction(object state)
        {
            (Action callback, float delay) = state as Tuple<Action, float>;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                int milliseconds = (int)(delay * 1000f);
                Thread.Sleep(milliseconds);
                callback();
            });
        }


        internal static IEnumerable<T> Deserialize<T>(IntPtr buf, int length, Func<IntPtr, T> constructor) where T : class
        {
            var array = new IntPtr[length];
            Marshal.Copy(buf, array, 0, length);
            Marshal.FreeCoTaskMem(buf);

            var list = new List<T>();
            foreach (var ptr in array)
            {
                if (ptr == IntPtr.Zero)
                    WebRTC.Logger.Log(LogType.Error, "IntPtr is zero");
                list.Add(FindOrCreate(ptr, constructor));
            }
            return list;
        }

        internal static T FindOrCreate<T>(IntPtr ptr, Func<IntPtr, T> constructor) where T : class
        {
            if (Context.table.ContainsKey(ptr))
            {
                if (Context.table[ptr] == null)
                {
                    // The object has been garbage collected.
                    // But the finalizer has not been called.
                    Context.table.Remove(ptr);
                    return constructor(ptr);
                }
                if (Context.table[ptr] is T value)
                {
                    return value;
                }
                throw new InvalidCastException($"{ptr} is not {typeof(T).Name}");
            }
            else
            {
                return constructor(ptr);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        static void DebugLog(string str, NativeLoggingSeverity loggingSeverity)
        {
            LogType logType = LogType.Log;
            switch (loggingSeverity)
            {
                case NativeLoggingSeverity.Warning:
                    {
                        logType = LogType.Warning;
                        break;
                    }
                case NativeLoggingSeverity.Error:
                    {
                        logType = LogType.Exception;
                        break;
                    }
            }

            Logger.LogFormat(logType, "{0}", str);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateSetLocalDescription))]
        static void OnSetLocalDescription(IntPtr ptr, IntPtr ptrObserver, RTCErrorType type, string message)
        {
            Sync(ptr, () =>
            {
                if (Table[ptr] is RTCPeerConnection connection)
                {
                    var observer = connection.FindObserver<SetSessionDescriptionObserver>(ptrObserver);
                    if (observer == null)
                        return;
                    connection.RemoveObserver(observer);
                    observer.Invoke(type, message);
                    observer.Dispose();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateSetRemoteDescription))]
        static void OnSetRemoteDescription(IntPtr ptr, IntPtr ptrObserver, RTCErrorType type, string message)
        {
            Sync(ptr, () =>
            {
                if (Table[ptr] is RTCPeerConnection connection)
                {
                    var observer = connection.FindObserver<SetSessionDescriptionObserver>(ptrObserver);
                    if (observer == null)
                        return;
                    connection.RemoveObserver(observer);
                    observer.Invoke(type, message);
                    observer.Dispose();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeCreateSessionDesc))]
        static void OnCreateSessionDescription(IntPtr ptr, IntPtr ptrObserver, RTCSdpType type, string sdp, RTCErrorType errorType, string message)
        {
            Sync(ptr, () =>
            {
                if (Table[ptr] is RTCPeerConnection connection)
                {
                    var observer = connection.FindObserver<CreateSessionDescriptionObserver>(ptrObserver);
                    if (observer == null)
                        return;
                    connection.RemoveObserver(observer);
                    observer.Invoke(type, sdp, errorType, message);
                    observer.Dispose();
                }
            });
        }


        [AOT.MonoPInvokeCallback(typeof(DelegateCollectStats))]
        static void OnCollectStatsCallback(IntPtr ptr, IntPtr ptrCallback, IntPtr ptrReport)
        {
            Sync(ptr, () =>
            {
                RTCStatsReport report = WebRTC.FindOrCreate(ptrReport, ptr_ => new RTCStatsReport(ptr_));
                if (Table[ptr] is RTCPeerConnection connection)
                {
                    RTCStatsCollectorCallback callback = connection.FindCollectStatsCallback(ptrCallback);
                    if (callback == null)
                        return;
                    connection.RemoveCollectStatsCallback(callback);
                    callback.Invoke(report);
                    callback.Dispose();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateTransformedFrame))]
        static void OnSetTransformedFrame(IntPtr ptr, IntPtr frame)
        {
            // Run on worker thread, not on main thread.
            if (WebRTC.Table.TryGetValue(ptr, out RTCRtpTransform transform))
            {
                if (transform == null)
                    return;

                RTCEncodedFrame frame_;
                if (transform.Kind == TrackKind.Video)
                    frame_ = new RTCEncodedVideoFrame(frame);
                else
                    frame_ = new RTCEncodedAudioFrame(frame);
                transform.callback_(new RTCTransformEvent(frame_));
            }
        }


        internal static Context Context { get { return s_context; } }
        internal static WeakReferenceTable Table { get { return s_context?.table; } }

        internal static IReadOnlyList<WeakReference<RTCPeerConnection>> PeerList
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
    internal delegate void DelegateDebugLog([MarshalAs(UnmanagedType.LPStr)] string str, NativeLoggingSeverity severity);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateCollectStats(IntPtr ptr, IntPtr ptrCallback, IntPtr reportPtr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateCreateGetStats(IntPtr ptr, RTCSdpType type, [MarshalAs(UnmanagedType.LPStr)] string sdp);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeCreateSessionDesc(IntPtr ptr, IntPtr ptrObserver, RTCSdpType type, [MarshalAs(UnmanagedType.LPStr)] string sdp, RTCErrorType errorType, [MarshalAs(UnmanagedType.LPStr)] string message);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateSetLocalDescription(IntPtr ptr, IntPtr ptrObserver, RTCErrorType type, [MarshalAs(UnmanagedType.LPStr)] string message);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateSetRemoteDescription(IntPtr ptr, IntPtr ptrObserver, RTCErrorType type, [MarshalAs(UnmanagedType.LPStr)] string message);
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
    internal delegate void DelegateNativeOnError(IntPtr ptr, RTCErrorType errorType, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] message, int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeMediaStreamOnAddTrack(IntPtr stream, IntPtr track);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeMediaStreamOnRemoveTrack(IntPtr stream, IntPtr track);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateAudioReceive(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateVideoFrameResize(IntPtr renderer, int width, int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateTransformedFrame(IntPtr transform, IntPtr frame);

    internal static class NativeMethods
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport(WebRTC.Lib)]
        public static extern void RegisterRenderingWebRTCPlugin();
#endif
        [DllImport(WebRTC.Lib)]
        public static extern void RegisterDebugLog(DelegateDebugLog func, [MarshalAs(UnmanagedType.U1)] bool enableNativeLog,
            NativeLoggingSeverity nativeLoggingSeverity);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreate(int uid);
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
        public static extern IntPtr ContextCreateAudioTrackSource(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateVideoTrackSource(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateVideoTrack(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label, IntPtr trackSource);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateAudioTrack(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label, IntPtr trackSource);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextStopMediaStreamTrack(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextGetStatsList(IntPtr context, IntPtr report, out ulong length, ref IntPtr types);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteStatsReport(IntPtr context, IntPtr report);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextAddRefPtr(IntPtr context, IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteRefPtr(IntPtr context, IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateFrameTransformer(IntPtr context);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetConfiguration(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern CreateSessionDescriptionObserver PeerConnectionCreateOffer(IntPtr context, IntPtr ptr, ref RTCOfferAnswerOptions options);
        [DllImport(WebRTC.Lib)]
        public static extern CreateSessionDescriptionObserver PeerConnectionCreateAnswer(IntPtr context, IntPtr ptr, ref RTCOfferAnswerOptions options);
        [DllImport(WebRTC.Lib)]
        public static extern void StatsCollectorRegisterCallback(DelegateCollectStats onCollectStats);
        [DllImport(WebRTC.Lib)]
        public static extern void CreateSessionDescriptionObserverRegisterCallback(DelegateNativeCreateSessionDesc callback);
        [DllImport(WebRTC.Lib)]
        public static extern void SetLocalDescriptionObserverRegisterCallback(DelegateSetLocalDescription callback);
        [DllImport(WebRTC.Lib)]
        public static extern void SetRemoteDescriptionObserverRegisterCallback(DelegateSetRemoteDescription callback);
        [DllImport(WebRTC.Lib)]
        public static extern void SetTransformedFrameRegisterCallback(DelegateTransformedFrame callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterIceConnectionChange(IntPtr ptr, DelegateNativeOnIceConnectionChange callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterConnectionStateChange(IntPtr ptr, DelegateNativeOnConnectionStateChange callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterIceGatheringChange(IntPtr ptr, DelegateNativeOnIceGatheringChange callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnIceCandidate(IntPtr ptr, DelegateNativeOnIceCandidate callback);
        [DllImport(WebRTC.Lib)]
        public static extern SetSessionDescriptionObserver PeerConnectionSetLocalDescription(IntPtr ptr, ref RTCSessionDescription desc, out RTCErrorType errorType, ref IntPtr error);
        [DllImport(WebRTC.Lib)]
        public static extern SetSessionDescriptionObserver PeerConnectionSetLocalDescriptionWithoutDescription(IntPtr ptr, out RTCErrorType errorType, ref IntPtr error);
        [DllImport(WebRTC.Lib)]
        public static extern SetSessionDescriptionObserver PeerConnectionSetRemoteDescription(IntPtr ptr, ref RTCSessionDescription desc, out RTCErrorType errorType, ref IntPtr error);
        [DllImport(WebRTC.Lib)]
        public static extern RTCStatsCollectorCallback PeerConnectionGetStats(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern RTCStatsCollectorCallback PeerConnectionSenderGetStats(IntPtr ptr, IntPtr sender);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextGetSenderCapabilities(IntPtr context, TrackKind kind, out IntPtr capabilities);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextGetReceiverCapabilities(IntPtr context, TrackKind kind, out IntPtr capabilities);
        [DllImport(WebRTC.Lib)]
        public static extern RTCStatsCollectorCallback PeerConnectionReceiverGetStats(IntPtr sender, IntPtr receiver);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool PeerConnectionCanTrickleIceCandidates(IntPtr ptr, [MarshalAs(UnmanagedType.U1)] out bool value);
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
        public static extern RTCErrorType PeerConnectionAddTrack(IntPtr pc, IntPtr track, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string streamId, out IntPtr sender);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTransceiver(IntPtr pc, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTransceiverWithInit(IntPtr pc, IntPtr track, ref RTCRtpTransceiverInitInternal init);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTransceiverWithType(IntPtr pc, TrackKind kind);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTransceiverWithTypeAndInit(IntPtr pc, TrackKind kind, ref RTCRtpTransceiverInitInternal init);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType PeerConnectionRemoveTrack(IntPtr pc, IntPtr sender);
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
        public static extern IntPtr PeerConnectionGetReceivers(IntPtr context, IntPtr ptr, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetSenders(IntPtr context, IntPtr ptr, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionGetTransceivers(IntPtr context, IntPtr ptr, out ulong length);
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
        public static extern bool TransceiverGetCurrentDirection(IntPtr transceiver, out RTCRtpTransceiverDirection direction);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType TransceiverStop(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr TransceiverGetMid(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern RTCRtpTransceiverDirection TransceiverGetDirection(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType TransceiverSetDirection(IntPtr transceiver, RTCRtpTransceiverDirection direction);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType TransceiverSetCodecPreferences(IntPtr transceiver, IntPtr capabilities, long length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr TransceiverGetReceiver(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr TransceiverGetSender(IntPtr transceiver);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr SenderGetTrack(IntPtr sender);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr SenderSetTransform(IntPtr sender, IntPtr transform);
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
        public static extern IntPtr ReceiverGetSources(IntPtr receiver, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern void ReceiverSetTransform(IntPtr receiver, IntPtr transform);
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
        public static extern void DataChannelSend(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string msg);
        [DllImport(WebRTC.Lib, EntryPoint = "DataChannelSendBinary")]
        public static extern void DataChannelSendPtr(IntPtr ptr, IntPtr dataPtr, int size);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelSendBinary(IntPtr ptr, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] bytes, int size);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelClose(IntPtr ptr);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnMessage(IntPtr ctx, IntPtr ptr, DelegateNativeOnMessage callback);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnOpen(IntPtr ctx, IntPtr ptr, DelegateNativeOnOpen callback);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnClose(IntPtr ctx, IntPtr ptr, DelegateNativeOnClose callback);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnError(IntPtr ctx, IntPtr ptr, DelegateNativeOnError callback);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateMediaStream(IntPtr ctx, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextRegisterMediaStreamObserver(IntPtr ctx, IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextUnRegisterMediaStreamObserver(IntPtr ctx, IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateAudioTrackSink(IntPtr context);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteAudioTrackSink(IntPtr context, IntPtr sink);
        [DllImport(WebRTC.Lib)]
        public static extern void AudioTrackAddSink(IntPtr track, IntPtr sink);
        [DllImport(WebRTC.Lib)]
        public static extern void AudioTrackRemoveSink(IntPtr track, IntPtr sink);
        [DllImport(WebRTC.Lib)]
        public static extern void AudioTrackSinkProcessAudio(
            IntPtr sink, float[] data, int length, int channels, int sampleRate);
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
        public static extern IntPtr CreateVideoRenderer(
            IntPtr context, DelegateVideoFrameResize callback, [MarshalAs(UnmanagedType.U1)] bool needFlip);
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
        public static extern IntPtr GetBatchUpdateEventFunc(IntPtr context);
        [DllImport(WebRTC.Lib)]
        public static extern int GetBatchUpdateEventID();
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr GetUpdateTextureFunc(IntPtr context);
        [DllImport(WebRTC.Lib)]
        public static extern void AudioSourceProcessLocalAudio(IntPtr source, IntPtr array, int sampleRate, int channels, int frames);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool VideoSourceGetSyncApplicationFramerate(IntPtr source);
        [DllImport(WebRTC.Lib)]
        public static extern void VideoSourceSetSyncApplicationFramerate(IntPtr source, [MarshalAs(UnmanagedType.U1)] bool value);
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
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetMapStringUint64(IntPtr member, out IntPtr values, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr StatsMemberGetMapStringDouble(IntPtr member, out IntPtr values, out ulong length);
        [DllImport(WebRTC.Lib)]
        public static extern uint FrameGetTimestamp(IntPtr frame);
        [DllImport(WebRTC.Lib)]
        public static extern uint FrameGetSsrc(IntPtr frame);
        [DllImport(WebRTC.Lib)]
        public static extern void FrameGetData(IntPtr frame, out IntPtr data, out int size);
        [DllImport(WebRTC.Lib)]
        public static extern void FrameSetData(IntPtr frame, IntPtr data, int size);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr VideoFrameGetMetadata(IntPtr frame);
        [DllImport(WebRTC.Lib)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern bool VideoFrameIsKeyFrame(IntPtr frame);
        [DllImport(WebRTC.Lib)]
        public static extern void FrameTransformerSendFrameToSink(IntPtr transform, IntPtr frame);

    }

    internal static class VideoUpdateMethods
    {
        static CommandBuffer _command = new CommandBuffer();

        static VideoUpdateMethods()
        {
            _command.name = "WebRTC";
        }

        public static void Flush()
        {
            Graphics.ExecuteCommandBuffer(_command);
            _command.Clear();
        }

        public static void BatchUpdate(IntPtr callback, int eventID, IntPtr batchData)
        {
            _command.IssuePluginEventAndData(callback, eventID, batchData);
        }

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
        }
    }
}
