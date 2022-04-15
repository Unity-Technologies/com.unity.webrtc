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

        bool m_needFlip = false;
        Texture m_sourceTexture;
        RenderTexture m_destTexture;

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
                return m_destTexture;
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
            if (m_source == null)
                return;

            // [Note-kazuki: 2020-03-09] Flip vertically RenderTexture
            // note: streamed video is flipped vertical if no action was taken:
            //  - duplicate RenderTexture from its source texture
            //  - call Graphics.Blit command with flip material every frame
            //  - it might be better to implement this if possible
            if (m_needFlip)
            {
                Graphics.Blit(m_sourceTexture, m_destTexture, WebRTC.flipMat);
            }
            else
            {
                Graphics.Blit(m_sourceTexture, m_destTexture);
            }
            m_source.Update(m_destTexture);
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

            m_sourceTexture = texture;
            m_destTexture = dest;
            m_needFlip = needFlip;
            m_source = source;
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

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                if (m_source != null)
                {
                    if (RenderTexture.active == m_destTexture)
                        RenderTexture.active = null;
                }

                m_sourceTexture = null;

                // Unity API must be called from main thread.
                // This texture is referred from the rendering thread,
                // so set the delay 100ms to wait the task of another thread.
                WebRTC.DestroyOnMainThread(m_destTexture, 0.1f);

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

    public static class CameraExtension
    {
        public static VideoStreamTrack CaptureStreamTrack(this Camera cam, int width, int height, int bitrate,
            RenderTextureDepth depth = RenderTextureDepth.DEPTH_24, bool needFlip = true)
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
            return new VideoStreamTrack(rt, needFlip);
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

        public void Update(Texture texture)
        {
            if (texture == null)
                Debug.LogError("texture is null");

            // todo:: This comparison is not sufficiency but it is for workaround of freeze bug.
            // Texture.GetNativeTexturePtr method freezes Unity Editor on apple silicon.
            if (prevTexture_ != texture)
            {
                data_ = new EncodeData(texture, self);
                Marshal.StructureToPtr(data_, ptr_, true);
                prevTexture_ = texture;
            }
            WebRTC.Context.Encode(ptr_);
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

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
