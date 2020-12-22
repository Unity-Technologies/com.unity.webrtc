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
            var value = NativeMethods.GetHardwareEncoderSupport();
            var context = Context.Create(
                encoderType:value ? EncoderType.Hardware : EncoderType.Software);
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void GetSetEncoderType()
        {
            var value = NativeMethods.GetHardwareEncoderSupport();
            var encoderType = value? EncoderType.Hardware: EncoderType.Software;
            var context = Context.Create(
                encoderType: encoderType);
            Assert.AreEqual(encoderType, context.GetEncoderType());
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void CreateAndDeletePeerConnection()
        {
            var value = NativeMethods.GetHardwareEncoderSupport();
            var context = Context.Create(
                encoderType: value ? EncoderType.Hardware : EncoderType.Software);
            var peerPtr = context.CreatePeerConnection();
            context.DeletePeerConnection(peerPtr);
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void CreateAndDeleteDataChannel()
        {
            var value = NativeMethods.GetHardwareEncoderSupport();
            var context = Context.Create(
                encoderType: value ? EncoderType.Hardware : EncoderType.Software);
            var peerPtr = context.CreatePeerConnection();
            var init = (RTCDataChannelInitInternal) new RTCDataChannelInit();
            var channelPtr = context.CreateDataChannel(peerPtr, "test", ref init);
            context.DeleteDataChannel(channelPtr);
            context.DeletePeerConnection(peerPtr);
            context.Dispose();
        }

        [Test]
        [Category("Context")]
        public void CreateAndDeleteVideoTrack()
        {
            var value = NativeMethods.GetHardwareEncoderSupport();
            var context = Context.Create(
                encoderType: value ? EncoderType.Hardware : EncoderType.Software);
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var track = context.CreateVideoTrack("video");
            context.DeleteMediaStreamTrack(track);
            context.Dispose();
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
