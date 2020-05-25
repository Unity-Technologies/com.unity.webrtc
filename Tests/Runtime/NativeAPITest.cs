using System;
using System.Collections;
using System.Runtime.InteropServices;
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

        [AOT.MonoPInvokeCallback(typeof(DelegateNativePeerConnectionSetSessionDescSuccess))]
        static void PeerConnectionSetSessionDescSuccess(IntPtr connection)
        {
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativePeerConnectionSetSessionDescFailure))]
        static void PeerConnectionSetSessionDescFailure(IntPtr connection, RTCError error)
        {
        }

        [Test]
        public void RegisterDelegateToPeerConnection()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var connection = NativeMethods.ContextCreatePeerConnection(context);

            NativeMethods.PeerConnectionRegisterOnSetSessionDescSuccess(context, connection, PeerConnectionSetSessionDescSuccess);
            NativeMethods.PeerConnectionRegisterOnSetSessionDescFailure(context, connection, PeerConnectionSetSessionDescFailure);
            NativeMethods.ContextDeletePeerConnection(context, connection);
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
        public void CreateAndDeleteMediaStream()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            NativeMethods.ContextDeleteMediaStream(context, stream);
            NativeMethods.ContextDestroy(0);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnAddTrack))]
        static void MediaStreamOnAddTrack(IntPtr ptr, IntPtr track) { }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnRemoveTrack))]
        static void MediaStreamOnRemoveTrack(IntPtr ptr, IntPtr track) { }

        [Test]
        public void RegisterDelegateToMediaStream()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");

            NativeMethods.MediaStreamRegisterOnAddTrack(context, stream, MediaStreamOnAddTrack);
            NativeMethods.MediaStreamRegisterOnRemoveTrack(context, stream, MediaStreamOnRemoveTrack);
            NativeMethods.ContextDeleteMediaStream(context, stream);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void AddAndRemoveVideoTrackToPeerConnection()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            string streamId = NativeMethods.MediaStreamGetID(stream).AsAnsiStringWithFreeMem();
            Assert.IsNotEmpty(streamId);
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", renderTexture.GetNativeTexturePtr());
            var sender = NativeMethods.PeerConnectionAddTrack(peer, track, streamId);
            NativeMethods.PeerConnectionRemoveTrack(peer, sender);
            NativeMethods.ContextDeleteMediaStreamTrack(context, track);
            NativeMethods.ContextDeleteMediaStream(context, stream);
            NativeMethods.ContextDeletePeerConnection(context, peer);
            NativeMethods.ContextDestroy(0);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        [Test]
        public void AddAndRemoveVideoTrackToMediaStream()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", renderTexture.GetNativeTexturePtr());
            NativeMethods.MediaStreamAddTrack(stream, track);

            int trackSize = 0;
            var trackNativePtr = NativeMethods.MediaStreamGetVideoTracks(stream, ref trackSize);
            Assert.AreNotEqual(trackNativePtr, IntPtr.Zero);
            Assert.Greater(trackSize, 0);

            IntPtr[] tracksPtr = new IntPtr[trackSize];
            Marshal.Copy(trackNativePtr, tracksPtr, 0, trackSize);
            Marshal.FreeCoTaskMem(trackNativePtr);

            NativeMethods.MediaStreamRemoveTrack(stream, track);
            NativeMethods.ContextDeleteMediaStreamTrack(context, track);
            NativeMethods.ContextDeleteMediaStream(context, stream);
            NativeMethods.ContextDestroy(0);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        [Test]
        public void AddAndRemoveAudioTrackToMediaStream()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            var track = NativeMethods.ContextCreateAudioTrack(context, "audio");
            NativeMethods.MediaStreamAddTrack(stream, track);
            int trackSize = 0;
            var trackNativePtr = NativeMethods.MediaStreamGetAudioTracks(stream, ref trackSize);
            Assert.AreNotEqual(trackNativePtr, IntPtr.Zero);
            Assert.Greater(trackSize, 0);

            IntPtr[] tracksPtr = new IntPtr[trackSize];
            Marshal.Copy(trackNativePtr, tracksPtr, 0, trackSize);
            Marshal.FreeCoTaskMem(trackNativePtr);

            NativeMethods.MediaStreamRemoveTrack(stream, track);
            NativeMethods.ContextDeleteMediaStreamTrack(context, track);
            NativeMethods.ContextDeleteMediaStream(context, stream);
            NativeMethods.ContextDestroy(0);
        }

        [Test]
        public void AddAndRemoveAudioTrack()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            var streamId = NativeMethods.MediaStreamGetID(stream).AsAnsiStringWithFreeMem();
            Assert.IsNotEmpty(streamId);
            var track = NativeMethods.ContextCreateAudioTrack(context, "audio");
            var sender = NativeMethods.PeerConnectionAddTrack(peer, track, streamId);
            NativeMethods.ContextDeleteMediaStreamTrack(context, track);
            NativeMethods.PeerConnectionRemoveTrack(peer, sender);
            NativeMethods.ContextDeleteMediaStream(context, stream);
            NativeMethods.ContextDeletePeerConnection(context, peer);
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

        /// <todo>
        /// This unittest failed standalone mono 2019.3 on linux
        /// </todo>
        [UnityTest]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxPlayer })]
        public IEnumerator CallVideoEncoderMethods()
        {
            var context = NativeMethods.ContextCreate(0, encoderType);
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            var streamId = NativeMethods.MediaStreamGetID(stream).AsAnsiStringWithFreeMem();
            Assert.IsNotEmpty(streamId);
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", renderTexture.GetNativeTexturePtr());
            var sender = NativeMethods.PeerConnectionAddTrack(peer, track, streamId);

            var callback = NativeMethods.GetRenderEventFunc(context);
            Assert.AreEqual(CodecInitializationResult.NotInitialized, NativeMethods.GetInitializationResult(context, track));

            // TODO::
            // note:: You must call `InitializeEncoder` method after `NativeMethods.ContextCaptureVideoStream`
            NativeMethods.ContextSetVideoEncoderParameter(context, track, width, height);
            VideoEncoderMethods.InitializeEncoder(callback, track);
            yield return new WaitForSeconds(1.0f);
            Assert.AreEqual(CodecInitializationResult.Success, NativeMethods.GetInitializationResult(context, track));

            VideoEncoderMethods.Encode(callback, track);
            yield return new WaitForSeconds(1.0f);
            VideoEncoderMethods.FinalizeEncoder(callback, track);
            yield return new WaitForSeconds(1.0f);

            NativeMethods.PeerConnectionRemoveTrack(peer, sender);
            NativeMethods.ContextDeleteMediaStreamTrack(context, track);
            NativeMethods.ContextDeleteMediaStream(context, stream);
            NativeMethods.ContextDeletePeerConnection(context, peer);
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
