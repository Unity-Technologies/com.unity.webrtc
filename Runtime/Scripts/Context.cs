using System;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.WebRTC
{
    // Ensure class initializer is called whenever scripts recompile
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    class ContextManager
    {
#if UNITY_EDITOR
        static ContextManager()
        {
            Init();
        }

        static void OnBeforeAssemblyReload()
        {
            WebRTC.DisposeInternal();
        }

        static void OnAfterAssemblyReload()
        {
            WebRTC.InitializeInternal();
        }

        internal static void Init()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.quitting += Quit;
        }
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Init()
        {
            Application.quitting += Quit;
            WebRTC.InitializeInternal();
        }
#endif
        internal static void Quit()
        {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
#endif
            WebRTC.DisposeInternal();
        }
    }

    internal class Context : IDisposable
    {
        internal IntPtr self;
        internal WeakReferenceTable table;
        internal bool limitTextureSize;

        private int id;
        private bool disposed;
        private IntPtr renderFunction;
        private IntPtr releaseBuffersFunction;
        private IntPtr textureUpdateFunction;

        public static Context Create(int id = 0)
        {
            var ptr = NativeMethods.ContextCreate(id);
            return new Context(ptr, id);
        }

        public bool IsNull
        {
            get { return self == IntPtr.Zero; }
        }

        private Context(IntPtr ptr, int id)
        {
            self = ptr;
            this.id = id;
            this.table = new WeakReferenceTable();
        }

        ~Context()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (self != IntPtr.Zero)
            {
                foreach (var value in table.CopiedValues)
                {
                    if (value == null)
                        continue;
                    var disposable = value as IDisposable;
                    disposable?.Dispose();
                }
                table.Clear();

                // Release buffers on the renedering thread.
                ReleaseBuffers();

                NativeMethods.ContextDestroy(id);
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public void AddRefPtr(IntPtr ptr)
        {
            NativeMethods.ContextAddRefPtr(self, ptr);
        }


        public void DeleteRefPtr(IntPtr ptr)
        {
            NativeMethods.ContextDeleteRefPtr(self, ptr);
        }

        public IntPtr CreatePeerConnection()
        {
            return NativeMethods.ContextCreatePeerConnection(self);
        }

        public IntPtr CreatePeerConnection(string conf)
        {
            return NativeMethods.ContextCreatePeerConnectionWithConfig(self, conf);
        }

        public void DeletePeerConnection(IntPtr ptr)
        {
            NativeMethods.ContextDeletePeerConnection(self, ptr);
        }

        public IntPtr PeerConnectionGetReceivers(IntPtr ptr, out ulong length)
        {
            return NativeMethods.PeerConnectionGetReceivers(self, ptr, out length);
        }

        public IntPtr PeerConnectionGetSenders(IntPtr ptr, out ulong length)
        {
            return NativeMethods.PeerConnectionGetSenders(self, ptr, out length);
        }

        public IntPtr PeerConnectionGetTransceivers(IntPtr ptr, out ulong length)
        {
            return NativeMethods.PeerConnectionGetTransceivers(self, ptr, out length);
        }

        public CreateSessionDescriptionObserver PeerConnectionCreateOffer(IntPtr ptr, ref RTCOfferAnswerOptions options)
        {
            return NativeMethods.PeerConnectionCreateOffer(self, ptr, ref options);
        }

        public CreateSessionDescriptionObserver PeerConnectionCreateAnswer(IntPtr ptr, ref RTCOfferAnswerOptions options)
        {
            return NativeMethods.PeerConnectionCreateAnswer(self, ptr, ref options);
        }

        public IntPtr CreateDataChannel(IntPtr ptr, string label, ref RTCDataChannelInitInternal options)
        {
            return NativeMethods.ContextCreateDataChannel(self, ptr, label, ref options);
        }

        public void DeleteDataChannel(IntPtr ptr)
        {
            NativeMethods.ContextDeleteDataChannel(self, ptr);
        }

        public void DataChannelRegisterOnMessage(IntPtr channel, DelegateNativeOnMessage callback)
        {
            NativeMethods.DataChannelRegisterOnMessage(self, channel, callback);
        }
        public void DataChannelRegisterOnOpen(IntPtr channel, DelegateNativeOnOpen callback)
        {
            NativeMethods.DataChannelRegisterOnOpen(self, channel, callback);
        }
        public void DataChannelRegisterOnClose(IntPtr channel, DelegateNativeOnClose callback)
        {
            NativeMethods.DataChannelRegisterOnClose(self, channel, callback);
        }

        public IntPtr CreateMediaStream(string label)
        {
            return NativeMethods.ContextCreateMediaStream(self, label);
        }

        public void RegisterMediaStreamObserver(MediaStream stream)
        {
            NativeMethods.ContextRegisterMediaStreamObserver(self, stream.GetSelfOrThrow());
        }

        public void UnRegisterMediaStreamObserver(MediaStream stream)
        {
            NativeMethods.ContextUnRegisterMediaStreamObserver(self, stream.GetSelfOrThrow());
        }

        public void MediaStreamRegisterOnAddTrack(MediaStream stream, DelegateNativeMediaStreamOnAddTrack callback)
        {
            NativeMethods.MediaStreamRegisterOnAddTrack(self, stream.GetSelfOrThrow(), callback);
        }

        public void MediaStreamRegisterOnRemoveTrack(MediaStream stream, DelegateNativeMediaStreamOnRemoveTrack callback)
        {
            NativeMethods.MediaStreamRegisterOnRemoveTrack(self, stream.GetSelfOrThrow(), callback);
        }

        public IntPtr CreateAudioTrackSink()
        {
            return NativeMethods.ContextCreateAudioTrackSink(self);
        }

        public void DeleteAudioTrackSink(IntPtr sink)
        {
            NativeMethods.ContextDeleteAudioTrackSink(self, sink);
        }

        public IntPtr GetRenderEventFunc()
        {
            return NativeMethods.GetRenderEventFunc(self);
        }

        public IntPtr GetReleaseBufferFunc()
        {
            return NativeMethods.GetReleaseBuffersFunc(self);
        }

        public IntPtr GetUpdateTextureFunc()
        {
            return NativeMethods.GetUpdateTextureFunc(self);
        }

        public IntPtr CreateVideoTrackSource()
        {
            return NativeMethods.ContextCreateVideoTrackSource(self);
        }

        public IntPtr CreateAudioTrackSource()
        {
            return NativeMethods.ContextCreateAudioTrackSource(self);
        }

        public IntPtr CreateAudioTrack(string label, IntPtr trackSource)
        {
            return NativeMethods.ContextCreateAudioTrack(self, label, trackSource);
        }

        public IntPtr CreateVideoTrack(string label, IntPtr source)
        {
            return NativeMethods.ContextCreateVideoTrack(self, label, source);
        }

        public void StopMediaStreamTrack(IntPtr track)
        {
            NativeMethods.ContextStopMediaStreamTrack(self, track);
        }

        public IntPtr CreateVideoRenderer(
            DelegateVideoFrameResize callback, bool needFlip)
        {
            return NativeMethods.CreateVideoRenderer(self, callback, needFlip);
        }

        public void DeleteVideoRenderer(IntPtr sink)
        {
            NativeMethods.DeleteVideoRenderer(self, sink);
        }

        public IntPtr GetStatsList(IntPtr report, out ulong length, ref IntPtr types)
        {
            return NativeMethods.ContextGetStatsList(self, report, out length, ref types);
        }

        public void DeleteStatsReport(IntPtr report)
        {
            NativeMethods.ContextDeleteStatsReport(self, report);
        }

        public void GetSenderCapabilities(TrackKind kind, out IntPtr capabilities)
        {
            NativeMethods.ContextGetSenderCapabilities(self, kind, out capabilities);
        }

        public void GetReceiverCapabilities(TrackKind kind, out IntPtr capabilities)
        {
            NativeMethods.ContextGetReceiverCapabilities(self, kind, out capabilities);
        }

        internal void Encode(IntPtr ptr)
        {
            renderFunction = renderFunction == IntPtr.Zero ? GetRenderEventFunc() : renderFunction;
            VideoEncoderMethods.Encode(renderFunction, ptr);
        }

        internal void ReleaseBuffers()
        {
            releaseBuffersFunction = releaseBuffersFunction == IntPtr.Zero ? GetReleaseBufferFunc() : releaseBuffersFunction;
            VideoEncoderMethods.ReleaseBuffers(releaseBuffersFunction);
        }

        internal void UpdateRendererTexture(uint rendererId, UnityEngine.Texture texture)
        {
            textureUpdateFunction = textureUpdateFunction == IntPtr.Zero ? GetUpdateTextureFunc() : textureUpdateFunction;
            VideoDecoderMethods.UpdateRendererTexture(textureUpdateFunction, texture, rendererId);
        }
    }
}
