using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    // [TestFixture(EncoderType.Software)]
    // [TestFixture(EncoderType.Hardware)]
    class NativeAPITest
    {
        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        static void DebugLog(string str)
        {
            Debug.Log(str);
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
        public void CreateAndDestroyContext([Values] EncoderType encoderType)
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void GetEncoderType([Values] EncoderType encoderType)
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            Assert.AreEqual(encoderType, NativeMethods.ContextGetEncoderType(context));
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void CreateAndDeletePeerConnection([Values] EncoderType encoderType)
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            NativeMethods.ContextDeletePeerConnection(context, peer);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void CreateAndDeleteDataChannel([Values] EncoderType encoderType)
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            var init = new RTCDataChannelInit(true);
            var channel = NativeMethods.ContextCreateDataChannel(context, peer, "test", ref init);
            NativeMethods.ContextDeleteDataChannel(context, channel);
            NativeMethods.ContextDeletePeerConnection(context, peer);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void CreateAndDeleteVideoStream([Values] EncoderType encoderType)
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            const int width = 1280;
            const int height = 720;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var renderTexture = new RenderTexture(width, height, 0, format);
            renderTexture.Create();
            var stream =
                NativeMethods.ContextCreateVideoStream(context, renderTexture.GetNativeTexturePtr(), width, height);
            NativeMethods.ContextDeleteVideoStream(context, stream);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void CreateAndDeleteAudioStream([Values] EncoderType encoderType)
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var stream = NativeMethods.ContextCreateAudioStream(context);
            NativeMethods.ContextDeleteAudioStream(context, stream);
            NativeMethods.ContextDestroy(0);
        }


        [Test]
        public void CallGetRenderEventFunc([Values] EncoderType encoderType)
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var callback = NativeMethods.GetRenderEventFunc(context);
            Assert.AreNotEqual(callback, IntPtr.Zero);
            NativeMethods.ContextDestroy(0);
            NativeMethods.GetRenderEventFunc(IntPtr.Zero);
        }

        [UnityTest]
        public IEnumerator CallVideoEncoderMethods([Values] EncoderType encoderType)
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            const int width = 1280;
            const int height = 720;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var renderTexture = new RenderTexture(width, height, 0, format);
            renderTexture.Create();
            var stream =
                NativeMethods.ContextCreateVideoStream(context, renderTexture.GetNativeTexturePtr(), width, height);
            var callback = NativeMethods.GetRenderEventFunc(context);

            // TODO::
            // note:: You must call `InitializeEncoder` method after `NativeMethods.ContextCaptureVideoStream`
            VideoEncoderMethods.InitializeEncoder(callback);
            yield return new WaitForSeconds(1.0f);
            VideoEncoderMethods.Encode(callback);
            yield return new WaitForSeconds(1.0f);
            VideoEncoderMethods.FinalizeEncoder(callback);
            yield return new WaitForSeconds(1.0f);

            NativeMethods.ContextDeleteVideoStream(context, stream);
            NativeMethods.ContextDestroy(0);
        }
    }
}
