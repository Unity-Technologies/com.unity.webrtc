using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading;
using System.Collections;

namespace Unity.WebRTC
{
    public struct RTCIceCandidate​
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string candidate;
        [MarshalAs(UnmanagedType.LPStr)]
        public string sdpMid;
        public int sdpMLineIndex;
    }

    public struct RTCDataChannelInit
    {
        public bool reliable;
        public bool ordered;
        public int maxRetransmitTime;
        public int maxRetransmits;
        [MarshalAs(UnmanagedType.LPStr)]
        public string protocol;
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
    public struct RTCRtpReceiver
    {

    }

    public struct RTCRtpTransceiver
    {

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
        public RTCErrorDetailType errorDetail;
        public long sdpLineNumber;
        public long httpRequestStatusCode;
        public long sctpCauseCode;
        public ulong receivedAlert;
        public ulong sentAlert;
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
        InternalError
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
        Answer
    }

    public struct RTCSessionDescription
    {
        public RTCSdpType type;
        [MarshalAs(UnmanagedType.LPStr)]
        public string sdp;


    }

    public struct RTCOfferOptions
    {
        public bool iceRestart;
        public bool offerToReceiveAudio;
        public bool offerToReceiveVideo;
    }

    public struct RTCAnswerOptions
    {
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

    public enum RTCBundlePolicy
    {
        kBundlePolicyBalanced,
        kBundlePolicyMaxBundle,
        kBundlePolicyMaxCompat
    };

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

    public class CodecInitializationException : Exception
    {
        public CodecInitializationResult result { get; private set; }

        internal CodecInitializationException(CodecInitializationResult result)
        {
            this.result = result;
        }
    }

    public static class WebRTC
    {
#if UNITY_EDITOR_OSX
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/x86_64/abci.bundle/Contents/MacOS/webrtc";
#elif UNITY_EDITOR_LINUX
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/x86_64/webrtc.so";
#elif UNITY_EDITOR_WIN
        internal const string Lib = "Packages/com.unity.webrtc/Runtime/Plugins/x86_64/webrtc.dll";
#elif UNITY_STANDALONE
        internal const string Lib = "webrtc";
#endif

        private static Context s_context;
        private static IntPtr s_renderCallback;
        private static Material flipMat;

        public static void Initialize()
        {
            NativeMethods.RegisterDebugLog(DebugLog);
            s_context = Context.Create();
            var result = Context.GetCodecInitializationResult();
            if (result != CodecInitializationResult.Success)
            {
                switch (result)
                {
                    case CodecInitializationResult.DriverNotInstalled:
                        Debug.LogError("[WebRTC] The hardware codec driver not installed");
                        break;
                    case CodecInitializationResult.DriverVersionDoesNotSupportAPI:
                        Debug.LogError("[WebRTC] The version of the hardware codec driver does not support API");
                        break;
                    case CodecInitializationResult.EncoderInitializationFailed:
                        Debug.LogError("[WebRTC] Hardware encoder initialization failed");
                        break;
                    case CodecInitializationResult.APINotFound:
                        Debug.LogError("[WebRTC] Hardware encoder API not found");
                        break;
                }
            }

            s_renderCallback = s_context.GetRenderEventFunc();
            NativeMethods.SetCurrentContext(s_context.ptrNativeObj);
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
                //Blit is for DirectX Rendering API Only

                foreach (var k in CameraExtension.camCapturerTexturesDict.Keys)
                {
                    foreach (var rt in CameraExtension.camCapturerTexturesDict[k].webRTCTextures)
                    {
                        Graphics.Blit(CameraExtension.camCapturerTexturesDict[k].camRenderTexture, rt, flipMat);
                    }
                }
                GL.IssuePluginEvent(NativeMethods.GetRenderEventFunc(Context.ptrNativeObj), 0);
                Audio.Update();
            }
        }

        public static void Finalize(int id = 0)
        {
            s_context.Dispose();
            s_context = null;
            NativeMethods.RegisterDebugLog(null);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        static void DebugLog(string str)
        {
            Debug.Log(str);
        }

        internal static Context Context { get { return s_context; } }
        internal static SynchronizationContext SyncContext { get { return s_context.syncContext; } }
        internal static Hashtable Table { get { return s_context.table; } }

        public static bool HWEncoderSupport
        {
            get
            {
                if(s_context.IsNull)
                {
                    throw new CodecInitializationException(CodecInitializationResult.NotInitialized);
                }
                var result = Context.GetCodecInitializationResult();
                return result == CodecInitializationResult.Success;
            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateDebugLog([MarshalAs(UnmanagedType.LPStr)] string str);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateCreateSDSuccess(IntPtr ptr, RTCSdpType type, [MarshalAs(UnmanagedType.LPStr)] string sdp);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateCreateSDFailure(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateSetSDSuccess(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateSetSDFailure(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnIceConnectionChange(IntPtr ptr, RTCIceConnectionState state);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnIceCandidate(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string candidate, [MarshalAs(UnmanagedType.LPStr)] string sdpMid, int sdpMlineIndex);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //according to JS API naming, use OnNegotiationNeeded instead of OnRenegotiationNeeded
    internal delegate void DelegateNativeOnNegotiationNeeded(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnTrack(IntPtr ptr, IntPtr rtpTransceiverInterface);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnDataChannel(IntPtr ptr, IntPtr ptrChannel);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnMessage(IntPtr ptr, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] bytes, int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnOpen(IntPtr ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateNativeOnClose(IntPtr ptr);

    internal static class NativeMethods
    {
        [DllImport(WebRTC.Lib)]
        public static extern void ContextStopMediaStreamTrack(IntPtr context, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern CodecInitializationResult GetCodecInitializationResult();
        [DllImport(WebRTC.Lib)]
        public static extern void RegisterDebugLog(DelegateDebugLog func);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreate(int uid);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDestroy(int uid);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreatePeerConnection(IntPtr ctx);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreatePeerConnectionWithConfig(IntPtr ctx, string conf);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeletePeerConnection(IntPtr ctx, IntPtr pc);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionClose(IntPtr pc);
        [DllImport(WebRTC.Lib)]
        public static extern RTCErrorType PeerConnectionSetConfiguration(IntPtr pc, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string conf);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateDataChannel(IntPtr ctx, IntPtr ptrPeer, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label, ref RTCDataChannelInit options);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteDataChannel(IntPtr ctx, IntPtr ptrChannel);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionGetConfiguration(IntPtr pc, ref IntPtr conf, ref int len);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionCreateOffer(IntPtr pc, ref RTCOfferOptions options);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionCreateAnswer(IntPtr pc, ref RTCAnswerOptions options);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterCallbackCreateSD(IntPtr pc, DelegateCreateSDSuccess onSuccess, DelegateCreateSDFailure onFailure);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterCallbackSetSD(IntPtr pc, DelegateSetSDSuccess onSuccess, DelegateSetSDFailure onFailure);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterIceConnectionChange(IntPtr pc, DelegateNativeOnIceConnectionChange callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnIceCandidate(IntPtr pc, DelegateNativeOnIceCandidate callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionSetLocalDescription(IntPtr pc, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionGetLocalDescription(IntPtr pc, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionSetRemoteDescription(IntPtr pc, ref RTCSessionDescription desc);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr PeerConnectionAddTrack(IntPtr pc, IntPtr track, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string mediaStreamId);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRemoveTrack(IntPtr pc, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern bool PeerConnectionAddIceCandidate(IntPtr pc, ref RTCIceCandidate​ candidate);
        [DllImport(WebRTC.Lib)]
        public static extern RTCPeerConnectionState PeerConnectionState(IntPtr pc);
        [DllImport(WebRTC.Lib)]
        public static extern RTCIceConnectionState PeerConnectionIceConditionState(IntPtr pc);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnDataChannel(IntPtr pc, DelegateNativeOnDataChannel callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnRenegotiationNeeded(IntPtr pc, DelegateNativeOnNegotiationNeeded callback);
        [DllImport(WebRTC.Lib)]
        public static extern void PeerConnectionRegisterOnTrack(IntPtr pc, DelegateNativeOnTrack rtpTransceiverInterface);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr RtpTransceiverInterfaceGetTrack(IntPtr rtpTransceiverInterface);
        [DllImport(WebRTC.Lib)]
        public static extern int DataChannelGetID(IntPtr dc);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr DataChannelGetLabel(IntPtr dc);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelSend(IntPtr dc, [MarshalAs(UnmanagedType.LPStr)]string msg);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelSendBinary(IntPtr dc, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] bytes, int size);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelClose(IntPtr dc);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnMessage(IntPtr dc, DelegateNativeOnMessage callback);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnOpen(IntPtr dc, DelegateNativeOnOpen callback);
        [DllImport(WebRTC.Lib)]
        public static extern void DataChannelRegisterOnClose(IntPtr dc, DelegateNativeOnClose callback);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateMediaStream(IntPtr ctx, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label);
        [DllImport(WebRTC.Lib)]
        public static extern void ContextDeleteMediaStream(IntPtr ctx, IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateVideoTrack(IntPtr ctx, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label, IntPtr rt, int width, int height, int bitRate);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextCreateAudioTrack(IntPtr ctx, [MarshalAs(UnmanagedType.LPStr, SizeConst = 256)] string label);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr ContextDeleteMediaStreamTrack(IntPtr ctx, IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern void MediaStreamAddTrack(IntPtr stream, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern void MediaStreamRemoveTrack(IntPtr stream, IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamGetVideoTracks(IntPtr stream, ref int length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamGetAudioTracks(IntPtr stream, ref int length);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamGetID(IntPtr stream);
        [DllImport(WebRTC.Lib)]
        public static extern TrackKind MediaStreamTrackGetKind(IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern TrackState MediaStreamTrackGetReadyState(IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr MediaStreamTrackGetID(IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern bool MediaStreamTrackGetEnabled(IntPtr track);
        [DllImport(WebRTC.Lib)]
        public static extern void MediaStreamTrackSetEnabled(IntPtr track, bool enabled);
        [DllImport(WebRTC.Lib)]
        public static extern void SetCurrentContext(IntPtr ctx);
        [DllImport(WebRTC.Lib)]
        public static extern IntPtr GetRenderEventFunc(IntPtr ctx);
        [DllImport(WebRTC.Lib)]
        public static extern void ProcessAudio(float[] data, int size);
    }
}
