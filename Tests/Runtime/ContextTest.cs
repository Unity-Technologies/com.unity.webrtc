using System;
using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    class ContextTest
    {
        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        static void DebugLog(string str)
        {
            UnityEngine.Debug.Log(str);
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
        public void CreateAndDelete()
        {
            var context = Context.Create();
            context.Dispose();
        }

        [Test]
        public void CreateAndDeletePeerConnection()
        {
            var context = Context.Create();
            var peerPtr = context.CreatePeerConnection();
            context.DeletePeerConnection(peerPtr);
            context.Dispose();
        }

        [Test]
        public void CreateAndDeleteDataChannel()
        {
            var context = Context.Create();
            var peerPtr = context.CreatePeerConnection();
            var init = (RTCDataChannelInitInternal) new RTCDataChannelInit();
            var channelPtr = context.CreateDataChannel(peerPtr, "test", ref init);
            context.DeleteDataChannel(channelPtr);
            context.DeletePeerConnection(peerPtr);
            context.Dispose();
        }

        [Test]
        public void CreateAndDeleteAudioTrack()
        {
            var context = Context.Create();
            var source = context.CreateAudioTrackSource();
            var track = context.CreateAudioTrack("audio", source);
            context.DeleteRefPtr(track);
            context.DeleteRefPtr(source);
            context.Dispose();
        }

        [Test]
        public void CreateAndDeleteVideoTrack()
        {
            var context = Context.Create();
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var source = context.CreateVideoTrackSource();
            var track = context.CreateVideoTrack("video", source);
            context.DeleteRefPtr(track);
            context.DeleteRefPtr(source);
            context.Dispose();
            UnityEngine.Object.DestroyImmediate(rt);
        }

        [Test]
        public void CreateAndDeleteAudioTrackSink()
        {
            var context = Context.Create();
            var sink = context.CreateAudioTrackSink();
            context.DeleteAudioTrackSink(sink);
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void DeleteStatsReportIgnoreInvalidValue()
        {
            var context = Context.Create();
            context.DeleteStatsReport(IntPtr.Zero);
            context.Dispose();
        }

    }
}
