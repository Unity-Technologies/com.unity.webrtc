using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="renderer"></param>
    public delegate void OnVideoReceived(Texture renderer);

    public class VideoStreamTrack : MediaStreamTrack
    {
        internal static ConcurrentDictionary<IntPtr, WeakReference<VideoStreamTrack>> s_tracks =
            new ConcurrentDictionary<IntPtr, WeakReference<VideoStreamTrack>>();

        bool m_needFlip = false;
        Texture m_sourceTexture;
        RenderTexture m_destTexture;

        UnityVideoRenderer m_renderer;
        VideoTrackSource _source;

        private static RenderTexture CreateRenderTexture(int width, int height)
        {
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var tex = new RenderTexture(width, height, 0, format);
            tex.Create();
            return tex;
        }

        internal VideoStreamTrack(Texture source, RenderTexture dest, int width, int height)
            : this(dest.GetNativeTexturePtr(), width, height, source.graphicsFormat)
        {
            m_needFlip = true;
            m_sourceTexture = source;
            m_destTexture = dest;
        }

        /// <summary>
        /// note:
        /// The videotrack cannot be used if the encoder has not been initialized.
        /// Do not use it until the initialization is complete.
        /// </summary>
        public bool IsEncoderInitialized
        {
            get
            {
                return WebRTC.Context.GetInitializationResult(GetSelfOrThrow()) == CodecInitializationResult.Success;
            }
        }

        public bool IsDecoderInitialized
        {
            get
            {
                return m_renderer != null && m_renderer.self != IntPtr.Zero;
            }
        }

        /// <summary>
        /// encoded / decoded texture
        /// </summary>
        public Texture Texture => m_destTexture;

        /// <summary>
        ///
        /// </summary>
        public event OnVideoReceived OnVideoReceived;

        public void InitializeReceiver()
        {
            if (IsDecoderInitialized)
                throw new InvalidOperationException("Already initialized receiver, use Texture property");

            m_needFlip = true;
            m_renderer = new UnityVideoRenderer(this);
        }

        internal void UpdateReceiveTexture()
        {
            // [Note-kazuki: 2020-03-09] Flip vertically RenderTexture
            // note: streamed video is flipped vertical if no action was taken:
            //  - duplicate RenderTexture from its source texture
            //  - call Graphics.Blit command with flip material every frame
            //  - it might be better to implement this if possible
            if (m_needFlip)
            {
                Graphics.Blit(m_sourceTexture, m_destTexture, WebRTC.flipMat);
            }

            WebRTC.Context.UpdateRendererTexture(m_renderer.id, m_sourceTexture);
        }

        internal void Update()
        {
            // [Note-kazuki: 2020-03-09] Flip vertically RenderTexture
            // note: streamed video is flipped vertical if no action was taken:
            //  - duplicate RenderTexture from its source texture
            //  - call Graphics.Blit command with flip material every frame
            //  - it might be better to implement this if possible
            if (m_needFlip)
            {
                Graphics.Blit(m_sourceTexture, m_destTexture, WebRTC.flipMat);
            }

            WebRTC.Context.Encode(GetSelfOrThrow());
        }

        /// <summary>
        /// Creates a new VideoStream object.
        /// The track is created with a `source`.
        /// </summary>
        /// <param name="source"></param>
        public VideoStreamTrack(Texture source)
            : this(source,
                CreateRenderTexture(source.width, source.height),
                source.width,
                source.height)
        {
        }


        /// <summary>
        /// Creates a new VideoStream object.
        /// The track is created with a source texture `ptr`.
        /// It is noted that streamed video might be flipped when not action was taken. Almost case it has no problem to use other constructor instead.
        ///
        /// See Also: Texture.GetNativeTexturePtr
        /// </summary>
        /// <param name="texturePtr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        public VideoStreamTrack(IntPtr texturePtr, int width, int height, GraphicsFormat format)
            : this(Guid.NewGuid().ToString(), new VideoTrackSource())
        {
            WebRTC.ValidateTextureSize(width, height, Application.platform, WebRTC.GetEncoderType());
            WebRTC.ValidateGraphicsFormat(format);
            WebRTC.Context.SetVideoEncoderParameter(GetSelfOrThrow(), width, height, format, texturePtr);
            WebRTC.Context.InitializeEncoder(GetSelfOrThrow());
        }

        internal VideoStreamTrack(string label, VideoTrackSource source)
            : this(WebRTC.Context.CreateVideoTrack(label, source.self))
        {
            _source = source;
        }

        internal VideoStreamTrack(IntPtr sourceTrack) : base(sourceTrack)
        {
            if (!s_tracks.TryAdd(self, new WeakReference<VideoStreamTrack>(this)))
                throw new InvalidOperationException();
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                if (IsEncoderInitialized)
                {
                    WebRTC.Context.FinalizeEncoder(self);
                    if (RenderTexture.active == m_destTexture)
                        RenderTexture.active = null;

                    // Unity API must be called from main thread.
                    WebRTC.DestroyOnMainThread(m_destTexture);
                }

                if (IsDecoderInitialized)
                {
                    m_renderer.Dispose();

                    // Unity API must be called from main thread.
                    WebRTC.DestroyOnMainThread(m_destTexture);
                }

                _source?.Dispose();

                s_tracks.TryRemove(self, out var value);
            }
            base.Dispose();
        }

        internal void OnVideoFrameResize(int width, int height)
        {
            if (m_sourceTexture == null)
            {
                var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
                m_sourceTexture = new Texture2D(width, height, format, TextureCreationFlags.None);
                m_destTexture = CreateRenderTexture(m_sourceTexture.width, m_sourceTexture.height);
                OnVideoReceived?.Invoke(m_destTexture);
                return;
            }

            if (m_sourceTexture.width != width || m_sourceTexture.height != height)
            {
                var oldSource = m_sourceTexture;
                var oldDest = m_destTexture;
                var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
                m_sourceTexture = new Texture2D(width, height, format, TextureCreationFlags.None);
                m_destTexture = CreateRenderTexture(width, height);
                OnVideoReceived?.Invoke(m_destTexture);

                WebRTC.DestroyOnMainThread(oldSource);
                WebRTC.DestroyOnMainThread(oldDest);
            }
        }
    }

    public static class CameraExtension
    {
        public static VideoStreamTrack CaptureStreamTrack(this Camera cam, int width, int height, int bitrate,
            RenderTextureDepth depth = RenderTextureDepth.DEPTH_24)
        {
            switch (depth)
            {
                case RenderTextureDepth.DEPTH_16:
                case RenderTextureDepth.DEPTH_24:
                case RenderTextureDepth.DEPTH_32:
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(depth), (int)depth, typeof(RenderTextureDepth));
            }

            if (width == 0 || height == 0)
            {
                throw new ArgumentException("width and height are should be greater than zero.");
            }

            int depthValue = (int)depth;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, depthValue, format);
            rt.Create();
            cam.targetTexture = rt;
            return new VideoStreamTrack(rt);
        }


        public static MediaStream CaptureStream(this Camera cam, int width, int height, int bitrate,
            RenderTextureDepth depth = RenderTextureDepth.DEPTH_24)
        {
            var stream = new MediaStream();
            var track = cam.CaptureStreamTrack(width, height, bitrate, depth);
            stream.AddTrack(track);
            return stream;
        }
    }

    internal class VideoTrackSource : RefCountedObject
    {
        public VideoTrackSource() : base(WebRTC.Context.CreateVideoTrackSource())
        {
            WebRTC.Table.Add(self, this);
        }

        ~VideoTrackSource()
        {
            this.Dispose();
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }
    }

    internal class UnityVideoRenderer : IDisposable
    {
        internal IntPtr self;
        private VideoStreamTrack track;
        internal uint id => NativeMethods.GetVideoRendererId(self);
        private bool disposed;

        public UnityVideoRenderer(VideoStreamTrack track)
        {
            self = WebRTC.Context.CreateVideoRenderer(OnVideoFrameResize);
            this.track = track;
            NativeMethods.VideoTrackAddOrUpdateSink(track.GetSelfOrThrow(), self);
            WebRTC.Table.Add(self, this);
        }

        ~UnityVideoRenderer()
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
                IntPtr trackPtr = track.GetSelfOrThrow();
                if (trackPtr != IntPtr.Zero)
                {
                    NativeMethods.VideoTrackRemoveSink(trackPtr, self);
                }

                WebRTC.Context.DeleteVideoRenderer(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        private void OnVideoFrameResizeInternal(int width, int height)
        {
            track.OnVideoFrameResize(width, height);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateVideoFrameResize))]
        static void OnVideoFrameResize(IntPtr ptrRenderer, int width, int height)
        {
            WebRTC.Sync(ptrRenderer, () =>
            {
                if (WebRTC.Table[ptrRenderer] is UnityVideoRenderer renderer)
                {
                    renderer.OnVideoFrameResizeInternal(width, height);
                }
            });
        }
    }
}
