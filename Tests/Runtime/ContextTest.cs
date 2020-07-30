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
            NativeMethods.RegisterDebugLog(DebugLog);
        }

        [TearDown]
        public void TearDown()
        {
            NativeMethods.RegisterDebugLog(null);
        }

        [Test]
        [Category("Context")]
        public void CreateAndDelete()
        {
            var context = Context.Create();
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void GetSetEncoderType()
        {
            var context = Context.Create();
            Assert.AreEqual(EncoderType.Hardware, context.GetEncoderType());
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void CreateAndDeletePeerConnection()
        {
            var context = Context.Create();
            var peerPtr = context.CreatePeerConnection();
            context.DeletePeerConnection(peerPtr);
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void CreateAndDeleteDataChannel()
        {
            var context = Context.Create();
            var peerPtr = context.CreatePeerConnection();
            var init = new RTCDataChannelInit(true);
            var channelPtr = context.CreateDataChannel(peerPtr, "test", ref init);
            context.DeleteDataChannel(channelPtr);
            context.DeletePeerConnection(peerPtr);
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void CreateAndDeleteVideoTrack()
        {
            var context = Context.Create();
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var track = context.CreateVideoTrack("video", rt.GetNativeTexturePtr());
            context.DeleteMediaStreamTrack(track);
            context.Dispose();
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
