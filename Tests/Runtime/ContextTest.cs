using System;
using NUnit.Framework;

using Debug = UnityEngine.Debug;

namespace Unity.WebRTC.RuntimeTest
{
    class ContextTest
    {
#if !UNITY_WEBGL
        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        static void DebugLog(string str)
        {
            Debug.Log(str);
        }
#else
        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        static void DebugLog(string level, string msg)
        {
            Debug.Log($"{level}:{msg}");
            if (level == "error") Debug.LogError(msg);
            if (level == "warning") Debug.LogWarning(msg);
            if (level == "log") Debug.Log(msg);
        }
#endif

        [SetUp]
        public void SetUp()
        {
            NativeMethods.RegisterDebugLog(DebugLog, true, NativeLoggingSeverity.Verbose);
        }

        [TearDown]
        public void TearDown()
        {
            NativeMethods.RegisterDebugLog(null, true, NativeLoggingSeverity.Verbose);
        }

        [Test]
        public void QuitAndInitContextManager()
        {
            // ContextManager.Init is already called when the process reaches here.
            ContextManager.Quit();
            ContextManager.Init();

#if UNITY_EDITOR
            // Reinitialize
            WebRTC.InitializeInternal();
#endif
        }

        [Test]
        public void CreateAndDeletePeerConnection()
        {
            var context = WebRTC.Context;
            var peerPtr = context.CreatePeerConnection();
            context.DeletePeerConnection(peerPtr);
        }

        [Test]
        public void CreateAndDeleteDataChannel()
        {
            var context = WebRTC.Context;
            var peerPtr = context.CreatePeerConnection();
            var init = (RTCDataChannelInitInternal) new RTCDataChannelInit();
            var channelPtr = context.CreateDataChannel(peerPtr, "test", ref init);
            context.DeleteDataChannel(channelPtr);
            context.DeletePeerConnection(peerPtr);
        }

        [Test]
        public void CreateAndDeleteAudioTrack()
        {
            var context = WebRTC.Context;
            var source = context.CreateAudioTrackSource();
            var track = context.CreateAudioTrack("audio", source);
            context.DeleteRefPtr(track);
            context.DeleteRefPtr(source);
        }

        [Test]
        public void CreateAndDeleteVideoTrack()
        {
            var context = WebRTC.Context;
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var source = context.CreateVideoTrackSource();
#if UNITY_WEBGL
            var track = context.CreateVideoTrack(source, System.IntPtr.Zero, width, height);
#else
            var track = context.CreateVideoTrack("video", source);
#endif
            context.DeleteRefPtr(track);
            context.DeleteRefPtr(source);
            UnityEngine.Object.DestroyImmediate(rt);
        }

        [Test]
        public void CreateAndDeleteAudioTrackSink()
        {
            var context = WebRTC.Context;
            var sink = context.CreateAudioTrackSink();
            context.DeleteAudioTrackSink(sink);
        }

        [Test]
        public void DeleteStatsReportIgnoreInvalidValue()
        {
            var context = WebRTC.Context;
            context.DeleteStatsReport(IntPtr.Zero);
        }
    }
}
