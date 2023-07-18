using System;
using NUnit.Framework;
using UnityEngine;

namespace Unity.WebRTC.RuntimeTest
{
    class ContextTest
    {
        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        static void DebugLog(string str, NativeLoggingSeverity severity)
        {
            LogType logType = LogType.Log;
            switch (severity)
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

            UnityEngine.Debug.unityLogger.Log(logType, str);
        }

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
            // ignore error message
            // Unhandled log message: '[Exception] [000:016](rtp_transceiver.cc:598): PeerConnection is closed. (INVALID_STATE)
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;

            // ContextManager.Init is already called when the process reaches here.
            ContextManager.Quit();
            ContextManager.Init();

#if UNITY_EDITOR
            // Reinitialize
            WebRTC.InitializeInternal();
#endif
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
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
            var init = (RTCDataChannelInitInternal)new RTCDataChannelInit();
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
            var track = context.CreateVideoTrack("video", source);
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
