using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.WebRTC
{
    public class VideoStreamTrack : MediaStreamTrack
    {
        internal static ConcurrentDictionary<IntPtr, WeakReference<VideoStreamTrack>> s_tracks =
            new ConcurrentDictionary<IntPtr, WeakReference<VideoStreamTrack>>();

        bool m_needFlip = false;
        Texture m_sourceTexture;
        RenderTexture m_destTexture;

        UnityVideoRenderer m_renderer;

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

        public Texture InitializeReceiver(int width, int height)
        {
            if (IsDecoderInitialized)
                throw new InvalidOperationException("Already initialized receiver, use Texture property");

            m_needFlip = true;
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            m_sourceTexture = new Texture2D(width, height, format, TextureCreationFlags.None);
            m_destTexture = CreateRenderTexture(m_sourceTexture.width, m_sourceTexture.height);

            m_renderer = new UnityVideoRenderer(WebRTC.Context.CreateVideoRenderer(), this);

            return m_destTexture;
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
            : base(WebRTC.Context.CreateVideoTrack(Guid.NewGuid().ToString()))
        {
            WebRTC.ValidateTextureSize(width, height, Application.platform, WebRTC.GetEncoderType());
            WebRTC.ValidateGraphicsFormat(format);
            WebRTC.Context.SetVideoEncoderParameter(GetSelfOrThrow(), width, height, format, texturePtr);
            WebRTC.Context.InitializeEncoder(GetSelfOrThrow());

            if(!s_tracks.TryAdd(self, new WeakReference<VideoStreamTrack>(this)))
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Creates from MediaStreamTrack object
        /// </summary>
        /// <param name="sourceTrack"></param>
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
                    UnityEngine.Object.DestroyImmediate(m_destTexture);
                }

                if (IsDecoderInitialized)
                {
                    m_renderer.Dispose();
                    UnityEngine.Object.DestroyImmediate(m_sourceTexture);
                }

                if(!s_tracks.TryRemove(self, out var value))
                    Debug.LogError("Invalid Operation");
                WebRTC.Context.DeleteMediaStreamTrack(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
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

    internal class UnityVideoRenderer : IDisposable
    {
        internal IntPtr self;
        private VideoStreamTrack track;
        internal uint id => NativeMethods.GetVideoRendererId(self);
        private bool disposed;

        public UnityVideoRenderer(IntPtr ptr, VideoStreamTrack track)
        {
            self = ptr;
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
    }
}
