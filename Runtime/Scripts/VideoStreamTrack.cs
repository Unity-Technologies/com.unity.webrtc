using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="renderer"></param>
    public delegate void OnVideoReceived(Texture renderer);

    /// <summary>
    ///
    /// </summary>
    public class VideoStreamTrack : MediaStreamTrack
    {
        internal static ConcurrentDictionary<IntPtr, WeakReference<VideoStreamTrack>> s_tracks =
            new ConcurrentDictionary<IntPtr, WeakReference<VideoStreamTrack>>();

        UnityVideoRenderer m_renderer;
        VideoTrackSource m_source;

        private static RenderTexture CreateRenderTexture(int width, int height)
        {
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var tex = new RenderTexture(width, height, 0, format);
            tex.Create();
            return tex;
        }

        /// <summary>
        /// encoded / decoded texture
        /// </summary>
        public Texture Texture
        {
            get
            {
                if (m_renderer != null)
                    return m_renderer.Texture;
                return m_source.destTexture_;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public event OnVideoReceived OnVideoReceived;

        internal void UpdateReceiveTexture()
        {
            m_renderer?.Update();
        }

        internal void UpdateSendTexture()
        {
            m_source?.Update();
        }

        /// <summary>
        /// Creates a new VideoStream object.
        /// The track is created with a `source`.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="needFlip"></param>
        public VideoStreamTrack(Texture source, bool needFlip = true)
            : this(source,
                  CreateRenderTexture(source.width, source.height),
                  Guid.NewGuid().ToString(),
                  new VideoTrackSource(),
                  needFlip)
        {
        }

        internal VideoStreamTrack(Texture texture, RenderTexture dest, string label, VideoTrackSource source, bool needFlip)
            : base(WebRTC.Context.CreateVideoTrack(label, source.self))
        {
            var error = WebRTC.ValidateTextureSize(texture.width, texture.height, Application.platform);
            if (error.errorType != RTCErrorType.None)
            {
                throw new ArgumentException(error.message);
            }
            WebRTC.ValidateGraphicsFormat(texture.graphicsFormat);

            if (!s_tracks.TryAdd(self, new WeakReference<VideoStreamTrack>(this)))
                throw new InvalidOperationException();

            m_source = source;
            m_source.sourceTexture_ = texture;
            m_source.destTexture_ = dest;
            m_source.needFlip_ = needFlip;
        }

        /// <summary>
        /// Video Receiver
        /// </summary>
        /// <param name="ptr"></param>
        internal VideoStreamTrack(IntPtr ptr) : base(ptr)
        {
            if (!s_tracks.TryAdd(self, new WeakReference<VideoStreamTrack>(this)))
                throw new InvalidOperationException();

            m_renderer = new UnityVideoRenderer(this, true);
        }

        /// <summary>
        ///
        /// </summary>
        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                m_renderer?.Dispose();
                m_source?.Dispose();

                s_tracks.TryRemove(self, out var value);
            }
            base.Dispose();
        }

        internal void OnVideoFrameResize(Texture texture)
        {
            OnVideoReceived?.Invoke(texture);
        }
    }

    /// <summary>
    ///
    /// </summary>
    public static class CameraExtension
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="needFlip"></param>
        /// <returns></returns>
        public static VideoStreamTrack CaptureStreamTrack(this Camera cam, int width, int height,
            RenderTextureDepth depth = RenderTextureDepth.Depth24, bool needFlip = true)
        {
            switch (depth)
            {
                case RenderTextureDepth.Depth16:
                case RenderTextureDepth.Depth24:
                case RenderTextureDepth.Depth32:
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(depth), (int)depth, typeof(RenderTextureDepth));
            }

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("width and height are should be greater than zero.");
            }

            int depthValue = (int)depth;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, depthValue, format);
            rt.Create();
            cam.targetTexture = rt;
            return new VideoStreamTrack(rt, needFlip);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static MediaStream CaptureStream(this Camera cam, int width, int height,
            RenderTextureDepth depth = RenderTextureDepth.Depth24)
        {
            var stream = new MediaStream();
            var track = cam.CaptureStreamTrack(width, height, depth);
            stream.AddTrack(track);
            return stream;
        }
    }

    internal class VideoTrackSource : RefCountedObject
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct EncodeData
        {
            public IntPtr ptrTexture;
            public IntPtr ptrTrackSource;
            public int width;
            public int height;
            public GraphicsFormat format;

            public EncodeData(Texture texture, IntPtr ptrSource)
            {
                ptrTexture = texture.GetNativeTexturePtr();
                ptrTrackSource = ptrSource;
                width = texture.width;
                height = texture.height;
                format = texture.graphicsFormat;
            }
        }

        // Blit parameter to flip vertically
        static Vector2 s_scale = new Vector2(1f, -1f);
        static Vector2 s_offset = new Vector2(0, 1f);

        internal bool needFlip_ = false;
        internal Texture sourceTexture_;
        internal RenderTexture destTexture_;

        IntPtr ptr_ = IntPtr.Zero;
        EncodeData data_;
        Texture prevTexture_;

        public VideoTrackSource()
            : base(WebRTC.Context.CreateVideoTrackSource())
        {
            WebRTC.Table.Add(self, this);
            ptr_ = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(EncodeData)));
        }

        ~VideoTrackSource()
        {
            this.Dispose();
        }

        public void Update()
        {
            // [Note-kazuki: 2020-03-09] Flip vertically RenderTexture
            // note: streamed video is flipped vertical if no action was taken:
            //  - duplicate RenderTexture from its source texture
            //  - call Graphics.Blit command with flip material every frame
            //  - it might be better to implement this if possible
            if (needFlip_)
            {
                Graphics.Blit(sourceTexture_, destTexture_, s_scale, s_offset);
            }
            else
            {
                Graphics.Blit(sourceTexture_, destTexture_);
            }

            // todo:: This comparison is not sufficiency but it is for workaround of freeze bug.
            // Texture.GetNativeTexturePtr method freezes Unity Editor on apple silicon.
            if (prevTexture_ != destTexture_)
            {
                data_ = new EncodeData(destTexture_, self);
                Marshal.StructureToPtr(data_, ptr_, true);
                prevTexture_ = destTexture_;
            }
            WebRTC.Context.Encode(ptr_);
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            sourceTexture_ = null;

            // Unity API must be called from main thread.
            // This texture is referred from the rendering thread,
            // so set the delay 100ms to wait the task of another thread.
            WebRTC.DestroyOnMainThread(destTexture_, 0.1f);

            if (ptr_ != IntPtr.Zero)
            {
                // This buffer is referred from the rendering thread,
                // so set the delay 100ms to wait the task of another thread.
                WebRTC.DelayActionOnMainThread(() =>
                {
                    Marshal.FreeHGlobal(ptr_);
                }, 0.1f);
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

        public Texture Texture { get; private set; }

        public UnityVideoRenderer(VideoStreamTrack track, bool needFlip)
        {
            self = WebRTC.Context.CreateVideoRenderer(OnVideoFrameResize, needFlip);
            this.track = track;
            NativeMethods.VideoTrackAddOrUpdateSink(track.GetSelfOrThrow(), self);
            WebRTC.Table.Add(self, this);
        }

        public void Update()
        {
            if (Texture == null)
                return;
            WebRTC.Context.UpdateRendererTexture(id, Texture);
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
                WebRTC.DestroyOnMainThread(Texture);
                WebRTC.Context.DeleteVideoRenderer(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        private void OnVideoFrameResizeInternal(int width, int height)
        {
            if (Texture != null &&
                Texture.width == width &&
                Texture.height == height)
            {
                return;
            }

            if (Texture != null)
            {
                WebRTC.DestroyOnMainThread(Texture);
                Texture = null;
            }

            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            Texture = new Texture2D(width, height, format, TextureCreationFlags.None);
            track.OnVideoFrameResize(Texture);
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
