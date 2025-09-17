using System;
using System.Collections;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class NativeAPITest
    {
        IntPtr context;

        private static RenderTexture CreateRenderTexture(int width, int height)
        {
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var renderTexture = new RenderTexture(width, height, 0, format);
            renderTexture.Create();
            return renderTexture;
        }

        private static void GetTrackDataAndPtr(RenderTexture texture, IntPtr source, VideoStreamTrack.VideoStreamTrackAction action, out VideoStreamTrack.VideoStreamTrackData data, out IntPtr ptr)
        {
            data = new VideoStreamTrack.VideoStreamTrackData();
            data.action = action;
            data.ptrTexture = texture.GetNativeTexturePtr();
            data.ptrSource = source;
            data.width = texture.width;
            data.height = texture.height;
            data.format = texture.graphicsFormat;

            ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VideoStreamTrack.VideoStreamTrackData)));
            Marshal.StructureToPtr(data, ptr, true);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
        protected static void DebugLog(string str)
        {
            Debug.Log(str);
        }

        [SetUp]
        public void Init()
        {
#if UNITY_IOS && !UNITY_EDITOR
            NativeMethods.RegisterRenderingWebRTCPlugin();
#endif
            context = WebRTC.Context.self;
        }

        [Test]
        public void NothingToDo()
        {
        }

        [Test]
        public void CreateAndDeletePeerConnection()
        {
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            NativeMethods.ContextDeletePeerConnection(context, peer);
        }

        [Test]
        public void RestartIcePeerConnection()
        {
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            NativeMethods.PeerConnectionRestartIce(peer);
            NativeMethods.ContextDeletePeerConnection(context, peer);
        }

        // todo(kazuki):: crash on iOS device
        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public void PeerConnectionGetReceivers()
        {
            var connection = NativeMethods.ContextCreatePeerConnection(context);
            IntPtr buf = NativeMethods.PeerConnectionGetReceivers(context, connection, out ulong length);
            Assert.AreEqual(0, length);
            NativeMethods.ContextDeletePeerConnection(context, connection);
        }

        [Test]
        public void CreateAndDeleteDataChannel()
        {
            var peer = NativeMethods.ContextCreatePeerConnection(context);

            var init = (RTCDataChannelInitInternal)new RTCDataChannelInit();
            var channel = NativeMethods.ContextCreateDataChannel(context, peer, "test", ref init);
            NativeMethods.ContextDeleteDataChannel(context, channel);
            NativeMethods.ContextDeletePeerConnection(context, peer);
        }

        [Test]
        public void CreateAndDeleteMediaStream()
        {
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            NativeMethods.ContextDeleteRefPtr(context, stream);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnAddTrack))]
        static void MediaStreamOnAddTrack(IntPtr ptr, IntPtr track) { }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnRemoveTrack))]
        static void MediaStreamOnRemoveTrack(IntPtr ptr, IntPtr track) { }

        [Test]
        public void RegisterDelegateToMediaStream()
        {
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            NativeMethods.ContextRegisterMediaStreamObserver(context, stream);

            NativeMethods.MediaStreamRegisterOnAddTrack(context, stream, MediaStreamOnAddTrack);
            NativeMethods.MediaStreamRegisterOnRemoveTrack(context, stream, MediaStreamOnRemoveTrack);

            NativeMethods.ContextUnRegisterMediaStreamObserver(context, stream);
            NativeMethods.ContextDeleteRefPtr(context, stream);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL,
            "Not support VideoStreamTrack for OpenGL")]
        public void AddAndRemoveVideoTrack()
        {
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var source = NativeMethods.ContextCreateVideoTrackSource(context);

            var track = NativeMethods.ContextCreateVideoTrack(context, "video", source);
            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, source);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL,
            "Not support VideoStreamTrack for OpenGL")]
        public void AddAndRemoveVideoTrackToPeerConnection()
        {
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            string streamId = NativeMethods.MediaStreamGetID(stream).AsAnsiStringWithFreeMem();
            Assert.IsNotEmpty(streamId);
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var source = NativeMethods.ContextCreateVideoTrackSource(context);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", source);
            var error = NativeMethods.PeerConnectionAddTrack(peer, track, streamId, out var sender);
            Assert.That(error, Is.EqualTo(RTCErrorType.None));

            var track2 = NativeMethods.SenderGetTrack(sender);
            Assert.AreEqual(track, track2);
            Assert.That(NativeMethods.PeerConnectionRemoveTrack(peer, sender), Is.EqualTo(RTCErrorType.None));
            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, stream);
            NativeMethods.ContextDeleteRefPtr(context, source);
            NativeMethods.ContextDeletePeerConnection(context, peer);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL,
            "Not support VideoStreamTrack for OpenGL")]
        public void SenderGetParameter()
        {
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            string streamId = NativeMethods.MediaStreamGetID(stream).AsAnsiStringWithFreeMem();
            Assert.IsNotEmpty(streamId);
            var source = NativeMethods.ContextCreateVideoTrackSource(context);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", source);
            var error = NativeMethods.PeerConnectionAddTrack(peer, track, streamId, out var sender);
            Assert.That(error, Is.EqualTo(RTCErrorType.None));

            NativeMethods.SenderGetParameters(sender, out var ptr);
            var parameters = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            Marshal.FreeHGlobal(ptr);

            Assert.AreNotEqual(IntPtr.Zero, parameters.encodings);
            Assert.AreNotEqual(IntPtr.Zero, parameters.transactionId);

            Assert.That(NativeMethods.PeerConnectionRemoveTrack(peer, sender), Is.EqualTo(RTCErrorType.None));
            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, stream);
            NativeMethods.ContextDeleteRefPtr(context, source);
            NativeMethods.ContextDeletePeerConnection(context, peer);
        }

        // todo(kazuki): Crash occurs when calling NativeMethods.MediaStreamRemoveTrack method on iOS device
        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL,
            "Not support VideoStreamTrack for OpenGL")]
        public void AddAndRemoveVideoTrackToMediaStream()
        {
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var source = NativeMethods.ContextCreateVideoTrackSource(context);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", source);

            NativeMethods.MediaStreamAddTrack(stream, track);

            IntPtr buf = NativeMethods.MediaStreamGetVideoTracks(stream, out ulong length);
            Assert.AreNotEqual(buf, IntPtr.Zero);
            Assert.Greater(length, 0);

            // todo(kazuki):: Copying native buffer to managed array occurs crash
            // on linux with il2cpp
#if !(UNITY_STANDALONE_LINUX && ENABLE_IL2CPP)
            IntPtr[] array = new IntPtr[length];
            Marshal.Copy(buf, array, 0, (int)length);
            Marshal.FreeCoTaskMem(buf);
#endif

            NativeMethods.MediaStreamRemoveTrack(stream, track);
            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, stream);
            NativeMethods.ContextDeleteRefPtr(context, source);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        // todo(kazuki): Crash occurs when calling NativeMethods.MediaStreamRemoveTrack method on iOS device
        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public void AddAndRemoveAudioTrackToMediaStream()
        {
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            var source = NativeMethods.ContextCreateAudioTrackSource(context);
            var track = NativeMethods.ContextCreateAudioTrack(context, "audio", source);
            NativeMethods.MediaStreamAddTrack(stream, track);

            var trackNativePtr = NativeMethods.MediaStreamGetAudioTracks(stream, out ulong trackSize);
            Assert.AreNotEqual(trackNativePtr, IntPtr.Zero);
            Assert.Greater(trackSize, 0);

            IntPtr buf = NativeMethods.MediaStreamGetAudioTracks(stream, out ulong length);
            Assert.AreNotEqual(buf, IntPtr.Zero);
            Assert.Greater(length, 0);

            // todo(kazuki):: Copying native buffer to managed array occurs crash
            // on linux with il2cpp
#if !(UNITY_STANDALONE_LINUX && ENABLE_IL2CPP)
            IntPtr[] array = new IntPtr[length];
            Marshal.Copy(buf, array, 0, (int)length);
            Marshal.FreeCoTaskMem(buf);
#endif

            NativeMethods.MediaStreamRemoveTrack(stream, track);
            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, stream);
            NativeMethods.ContextDeleteRefPtr(context, source);
        }

        [Test]
        public void AddAndRemoveAudioTrack()
        {
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            var streamId = NativeMethods.MediaStreamGetID(stream).AsAnsiStringWithFreeMem();
            Assert.IsNotEmpty(streamId);
            var source = NativeMethods.ContextCreateAudioTrackSource(context);
            var track = NativeMethods.ContextCreateAudioTrack(context, "audio", source);
            var error = NativeMethods.PeerConnectionAddTrack(peer, track, streamId, out var sender);
            Assert.That(error, Is.EqualTo(RTCErrorType.None));

            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, source);
            Assert.That(NativeMethods.PeerConnectionRemoveTrack(peer, sender), Is.EqualTo(RTCErrorType.None));
            NativeMethods.ContextDeleteRefPtr(context, stream);
            NativeMethods.ContextDeletePeerConnection(context, peer);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateVideoFrameResize))]
        static void OnVideoFrameResize(IntPtr ptrRenderer, int width, int height) { }

        [Test]
        public void CreateAndDeleteVideoRenderer()
        {
            var renderer = NativeMethods.CreateVideoRenderer(context, OnVideoFrameResize, true);
            NativeMethods.DeleteVideoRenderer(context, renderer);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL,
            "Not support VideoStreamTrack for OpenGL")]
        public void AddAndRemoveVideoRendererToVideoTrack()
        {
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var source = NativeMethods.ContextCreateVideoTrackSource(context);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", source);
            var renderer = NativeMethods.CreateVideoRenderer(context, OnVideoFrameResize, true);
            NativeMethods.VideoTrackAddOrUpdateSink(track, renderer);
            NativeMethods.VideoTrackRemoveSink(track, renderer);
            NativeMethods.DeleteVideoRenderer(context, renderer);
            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, source);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        [Test]
        public void CallGetBatchUpdateEventFunc()
        {
            var callback = NativeMethods.GetBatchUpdateEventFunc(context);
            Assert.AreNotEqual(callback, IntPtr.Zero);
            NativeMethods.GetBatchUpdateEventFunc(IntPtr.Zero);
        }

        [Test]
        public void RTCRtpSendParametersCreateAndDeletePtr()
        {
            RTCRtpSendParametersInternal parametersInternal = default;

            int encodingsLength = 2;
            RTCRtpEncodingParametersInternal[] encodings = new RTCRtpEncodingParametersInternal[encodingsLength];
            for (int i = 0; i < encodingsLength; i++)
            {
                encodings[i].active = true;
                encodings[i].maxBitrate = 10000000;
                encodings[i].minBitrate = 10000000;
                encodings[i].maxFramerate = 30;
                encodings[i].scaleResolutionDownBy = 1.0;
                encodings[i].rid = Marshal.StringToCoTaskMemAnsi(string.Empty);
            }
            parametersInternal.transactionId = Marshal.StringToCoTaskMemAnsi(string.Empty);
            parametersInternal.encodings = encodings;

            RTCRtpSendParameters parameter = new RTCRtpSendParameters(ref parametersInternal);
            parameter.CreateInstance(out var instance);
            instance.Dispose();
        }

        [UnityTest, LongRunning]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL,
            "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator CallVideoUpdateMethodsEncode()
        {
            var peer = NativeMethods.ContextCreatePeerConnection(context);
            var stream = NativeMethods.ContextCreateMediaStream(context, "MediaStream");
            var streamId = NativeMethods.MediaStreamGetID(stream).AsAnsiStringWithFreeMem();
            Assert.IsNotEmpty(streamId);
            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var source = NativeMethods.ContextCreateVideoTrackSource(context);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", source);
            var error = NativeMethods.PeerConnectionAddTrack(peer, track, streamId, out var sender);
            Assert.That(error, Is.EqualTo(RTCErrorType.None));

            GetTrackDataAndPtr(renderTexture, source, VideoStreamTrack.VideoStreamTrackAction.Encode, out var data, out var ptr);

            var batchUpdateEvent = NativeMethods.GetBatchUpdateEventFunc(context);
            int batchUpdateEventID = NativeMethods.GetBatchUpdateEventID();
            Batch batch = new Batch();

            int trackIndex = 0;
            batch.data.tracks[trackIndex] = ptr;
            batch.data.tracksCount = ++trackIndex;

            yield return new WaitForSeconds(1.0f);

            Marshal.StructureToPtr(batch.data, batch.ptr, false);
            VideoUpdateMethods.BatchUpdate(batchUpdateEvent, batchUpdateEventID, batch.ptr);
            VideoUpdateMethods.Flush();

            yield return new WaitForSeconds(1.0f);

            batch.Dispose();
            Marshal.FreeHGlobal(ptr);
            Assert.That(NativeMethods.PeerConnectionRemoveTrack(peer, sender), Is.EqualTo(RTCErrorType.None));
            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, stream);
            NativeMethods.ContextDeleteRefPtr(context, source);

            NativeMethods.ContextDeletePeerConnection(context, peer);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        [Test]
        public void CallGetUpdateTextureFunc()
        {
            var callback = NativeMethods.GetUpdateTextureFunc(context);
            Assert.AreNotEqual(callback, IntPtr.Zero);
            NativeMethods.GetUpdateTextureFunc(IntPtr.Zero);
        }

        [UnityTest, LongRunning]
        [ConditionalIgnoreMultiple(ConditionalIgnore.UnsupportedPlatformVideoDecoder,
            "VideoUpdateMethods.UpdateRendererTexture is not supported on Direct3D12.")]
        [ConditionalIgnoreMultiple(ConditionalIgnore.UnsupportedPlatformOpenGL,
            "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator CallVideoUpdateMethodsUpdateRenderer()
        {
            if (ConditionalIgnore.IsUnstableOnGraphicsDevice(Application.platform, SystemInfo.graphicsDeviceName))
            {
                Assert.Ignore($"This test is not stable on {SystemInfo.graphicsDeviceName}");
            }

            const int width = 1280;
            const int height = 720;
            var renderTexture = CreateRenderTexture(width, height);
            var receiveTexture = CreateRenderTexture(width, height);
            var source = NativeMethods.ContextCreateVideoTrackSource(context);
            var track = NativeMethods.ContextCreateVideoTrack(context, "video", source);
            var renderer = NativeMethods.CreateVideoRenderer(context, OnVideoFrameResize, true);
            var rendererId = NativeMethods.GetVideoRendererId(renderer);
            NativeMethods.VideoTrackAddOrUpdateSink(track, renderer);

            GetTrackDataAndPtr(renderTexture, source, VideoStreamTrack.VideoStreamTrackAction.Encode, out var data, out var ptr);

            var batchUpdateEvent = NativeMethods.GetBatchUpdateEventFunc(context);
            int batchUpdateEventID = NativeMethods.GetBatchUpdateEventID();
            var updateTextureEvent = NativeMethods.GetUpdateTextureFunc(context);
            Batch batch = new Batch();

            int trackIndex = 0;
            batch.data.tracks[trackIndex] = ptr;
            batch.data.tracksCount = ++trackIndex;

            yield return new WaitForSeconds(1.0f);

            Marshal.StructureToPtr(batch.data, batch.ptr, false);
            VideoUpdateMethods.BatchUpdate(batchUpdateEvent, batchUpdateEventID, batch.ptr);

            // this method is not supported on Direct3D12
            VideoUpdateMethods.UpdateRendererTexture(updateTextureEvent, receiveTexture, rendererId);
            VideoUpdateMethods.Flush();

            yield return new WaitForSeconds(1.0f);

            batch.Dispose();
            Marshal.FreeHGlobal(ptr);
            NativeMethods.VideoTrackRemoveSink(track, renderer);
            NativeMethods.DeleteVideoRenderer(context, renderer);
            NativeMethods.ContextDeleteRefPtr(context, track);
            NativeMethods.ContextDeleteRefPtr(context, source);
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(receiveTexture);
        }
    }

    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
    class NativeAPITestAndEnterPlayModeOptionsEnabled : NativeAPITest, IPrebuildSetup
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
