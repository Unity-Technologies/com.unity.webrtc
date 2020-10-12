using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.WebRTC
{
    public delegate void DelegateOnVideoFrameEncoded(int encoderID, int width, int height, IntPtr buffer, int bufferSize);

    public class VideoStreamTrack : MediaStreamTrack
    {
        internal static List<VideoStreamTrack> tracks = new List<VideoStreamTrack>();

        bool m_needFlip = false;
        UnityEngine.Texture m_sourceTexture;
        UnityEngine.RenderTexture m_destTexture;
        bool m_isInitialised = false;

        UnityVideoRenderer m_renderer;
        private DelegateOnVideoFrameEncoded onFrameEncoded;

        private static UnityEngine.RenderTexture CreateRenderTexture(int width, int height,
            UnityEngine.RenderTextureFormat format)
        {
            var tex = new UnityEngine.RenderTexture(width, height, 0, format);
            tex.Create();
            return tex;
        }

        internal VideoStreamTrack(string label, UnityEngine.Texture source, UnityEngine.RenderTexture dest, int width,
            int height)
            : this(label, dest.GetNativeTexturePtr(), width, height)
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
                return WebRTC.Context.GetInitializationResult(self) == CodecInitializationResult.Success;
            }
        }

        public bool IsDecoderInitialized
        {
            get
            {
                return m_renderer != null && m_renderer.self != IntPtr.Zero;
            }
        }

        public RenderTexture InitializeReceiver()
        {
            if (IsDecoderInitialized)
                throw new InvalidOperationException("Already initialized receiver");

            m_needFlip = true;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            m_sourceTexture = CreateRenderTexture(1280, 720, format);
            m_destTexture = CreateRenderTexture(m_sourceTexture.width, m_sourceTexture.height, format);

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
                UnityEngine.Graphics.Blit(m_sourceTexture, m_destTexture, WebRTC.flipMat);
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
                UnityEngine.Graphics.Blit(m_sourceTexture, m_destTexture, WebRTC.flipMat);
            }

            WebRTC.Context.Encode(self);
        }

        /// <summary>
        /// Creates a new VideoStream object.
        /// The track is created with a `source`.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="source"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public VideoStreamTrack(string label, UnityEngine.RenderTexture source)
            : this(label, source, CreateRenderTexture(source.width, source.height, source.format), source.width,
                source.height)
        {
        }

        public VideoStreamTrack(string label, UnityEngine.Texture source)
            : this(label,
                source,
                CreateRenderTexture(source.width, source.height,
                    WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType)),
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
        /// <param name="label"></param>
        /// <param name="ptr"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public VideoStreamTrack(string label, IntPtr ptr, int width, int height)
            : base(WebRTC.Context.CreateVideoTrack(label, ptr))
        {
            WebRTC.Context.SetVideoEncoderParameter(self, width, height);
            WebRTC.Context.InitializeEncoder(self);
            tracks.Add(this);
        }

        /// <summary>
        /// Creates from MediaStreamTrack object
        /// </summary>
        /// <param name="sourceTrack"></param>
        internal VideoStreamTrack(IntPtr sourceTrack) : base(sourceTrack)
        {
            tracks.Add(this);
        }

        /// <summary>
        /// This property sets a callback on the underlying native video source. It will be
        /// called for each source video frame after the post-capture operations have been applied.
        /// Set to null to cancel the subscription.
        /// </summary>
        public DelegateOnVideoFrameEncoded OnFrameEncoded
        {
            private get => onFrameEncoded;
            set
            {
                if (!m_isInitialised)
                {
                    WebRTC.Context.SetOnVideoFrameEncodedCallback(self, OnVideoFrameEncoded);
                    m_isInitialised = true;
                }

                onFrameEncoded = value;
            }
        }

        /// <summary>
        /// This method only gets called if the application wishes to receive callbacks for the on encoded event
        /// of native video sources. The method will only be called once per managed video track. It is responsible for
        /// matching native callbacks to the managed video track.
        /// </summary>
        /// <param name="ptr">The pointer to the native media stream track that matches this managed instance.</param>
        /// <param name="encoderID">The ID of the native encoder that was used to process the video frame.</param>
        /// <param name="width">The width of the frame.</param>
        /// <param name="height">The height of the frame.</param>
        /// <param name="buffer">The video frame data. Will be either an I420 buffer or an encoded video sample depending on
        /// whether the WebRTC context is using hardware or software encoding.</param>
        /// <param name="bufferSize">The size of the video frame data.</param>
        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnVideoFrameEncoded))]
        static void OnVideoFrameEncoded(IntPtr ptr, int encoderID, int width, int height, IntPtr buffer, int bufferSize)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is VideoStreamTrack videoStreamTrack)
                {
                    videoStreamTrack.OnFrameEncoded?.Invoke(encoderID, width, height, buffer, bufferSize);
                }
            });
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
                    UnityEngine.Object.DestroyImmediate(m_destTexture);
                }

                if (IsDecoderInitialized)
                {
                    m_renderer.Dispose();
                    UnityEngine.Object.DestroyImmediate(m_sourceTexture);
                }

                tracks.Remove(this);
                WebRTC.Context.DeleteMediaStreamTrack(self);
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

            int depthValue = (int)depth;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, depthValue, format);
            rt.Create();
            cam.targetTexture = rt;
            return new VideoStreamTrack(cam.name, rt);
        }


        public static MediaStream CaptureStream(this Camera cam, int width, int height, int bitrate,
            RenderTextureDepth depth = RenderTextureDepth.DEPTH_24)
        {
            var stream = new MediaStream(WebRTC.Context.CreateMediaStream("videostream"));
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
            NativeMethods.VideoTrackAddOrUpdateSink(track.self, self);
            WebRTC.Table.Add(self, this);
        }

        ~UnityVideoRenderer()
        {
            this.Dispose();
            WebRTC.Table.Remove(self);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero)
            {
                if (track.self != IntPtr.Zero)
                {
                    NativeMethods.VideoTrackRemoveSink(track.self, self);
                }

                WebRTC.Context.DeleteVideoRenderer(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
