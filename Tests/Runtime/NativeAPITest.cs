using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    [TestFixture, ConditionalIgnore("IgnoreHardwareEncoderTest", "Ignored hardware encoder test.")]
    class NativeAPITestWithHardwareEncoder : NativeAPITestWithSoftwareEncoder
    {
        [OneTimeSetUp]
        public new void OneTimeInit()
        {
            encoderType = EncoderType.Hardware;
        }

    }

    class NativeAPITestWithSoftwareEncoder
    {
        protected EncoderType encoderType;

        private static RenderTexture CreateRenderTexture(int width, int height)
        {
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var renderTexture = new RenderTexture(width, height, 0, format);
            renderTexture.Create();
            return renderTexture;
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        protected static void DebugLog(string str)
        {
            Debug.Log(str);
        }

        [SetUp]
        public void Init()
        {
            NativeMethods.RegisterDebugLog(DebugLog);
        }

        [TearDown]
        public void CleanUp()
        {
            NativeMethods.RegisterDebugLog(null);
        }

        [OneTimeSetUp]
        public void OneTimeInit()
        {
            encoderType = EncoderType.Software;
        }

        [Test]
        public void CreateAndDestroyContext()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void GetEncoderType()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            Assert.AreEqual(encoderType, NativeMethods.ContextGetEncoderType(context));
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void CreateAndDeletePeerConnection()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            NativeMethods.ContextDeletePeerConnection(context, peer);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void CreateAndDeleteDataChannel()
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
        public void CreateAndDeleteVideoStream()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var stream =
                NativeMethods.ContextCreateVideoStream(context, renderTexture.GetNativeTexturePtr(), width, height);
            NativeMethods.ContextDeleteVideoStream(context, stream);
            NativeMethods.ContextDestroy(0);

            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        [Test]
        public void MediaStreamGetAudioTracks()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var stream = NativeMethods.ContextCreateAudioStream(context);
            int trackSize = 0;
            IntPtr trackNativePtr = NativeMethods.MediaStreamGetAudioTracks(stream, ref trackSize);
            Assert.AreNotEqual(trackNativePtr, IntPtr.Zero);
            Assert.Greater(trackSize, 0);

            IntPtr[] tracksPtr = new IntPtr[trackSize];
            System.Runtime.InteropServices.Marshal.Copy(trackNativePtr, tracksPtr, 0, trackSize);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(trackNativePtr);

            for (int i = 0; i < trackSize; i++)
            {
                MediaStreamTrack track = new MediaStreamTrack(tracksPtr[i]);
                Assert.True(track.Enabled);
            }
            NativeMethods.ContextDeleteAudioStream(context, stream);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void CreateAndDeleteAudioStream()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var stream = NativeMethods.ContextCreateAudioStream(context);
            NativeMethods.ContextDeleteAudioStream(context, stream);
            NativeMethods.ContextDestroy(0);
        }


        [Test]
        public void CallGetRenderEventFunc()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var callback = NativeMethods.GetRenderEventFunc(context);
            Assert.AreNotEqual(callback, IntPtr.Zero);
            NativeMethods.ContextDestroy(0);
            NativeMethods.GetRenderEventFunc(IntPtr.Zero);
        }

        [UnityTest]
        public IEnumerator CallVideoEncoderMethods()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
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

            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    [TestFixture, ConditionalIgnore("IgnoreHardwareEncoderTest", "Ignored hardware encoder test.")]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
    class NativeAPITestWithHardwareEncoderAndEnterPlayModeOptionsEnabled : NativeAPITestWithHardwareEncoder, IPrebuildSetup
    {
        public void Setup()
        {
#if UNITY_EDITOR
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
#endif
        }
    }

    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
    class NativeAPITestWithSoftwareEncoderAndEnterPlayModeOptionsEnabled : NativeAPITestWithSoftwareEncoder, IPrebuildSetup
    {
        public void Setup()
        {
#if UNITY_EDITOR
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions =
                EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
#endif
        }
    }
}
