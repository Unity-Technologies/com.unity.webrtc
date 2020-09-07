using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.WebRTC
{
    public class VideoStreamTrack : MediaStreamTrack
    {
        internal static List<VideoStreamTrack> tracks = new List<VideoStreamTrack>();

        readonly bool m_needFlip = false;
        readonly UnityEngine.Texture m_sourceTexture;
        readonly UnityEngine.RenderTexture m_destTexture;

        UnityVideoSink m_sink;

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

        public void InitializeReceiver()
        {
            //ToDo: on update write sink butter to tex
            m_sink = new UnityVideoSink(WebRTC.Context.CreateVideoRenderer());
            NativeMethods.VideoTrackAddOrUpdateSink(self, m_sink.self);
        }

        public Texture2D UpdateReceiveTexture()
        {
            if (self == IntPtr.Zero || m_sink.self == IntPtr.Zero)
            {
                throw new Exception("already receiver is disposed");
            }

            return m_sink?.UpdateTexture();
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
        public VideoStreamTrack(IntPtr sourceTrack) : base(sourceTrack)
        {
            tracks.Add(this);
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

                if (m_sink != null)
                {
                    NativeMethods.VideoTrackRemoveSink(self, m_sink.self);
                    m_sink.Dispose();
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

    public class UnityVideoSink : IDisposable
    {
        internal IntPtr self;
        private bool disposed;
        private Texture2D m_videoBuffer;

        public UnityVideoSink(IntPtr ptr)
        {
            self = ptr;
            m_videoBuffer = Texture2D.blackTexture;
        }

        ~UnityVideoSink()
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
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public Texture2D UpdateTexture()
        {
            NativeMethods.GetVideoRendererImageData(self, out var data);

            if (data.RawData == IntPtr.Zero || data.Height <= 0 || data.Width <= 0)
            {
                return m_videoBuffer;
            }

            if (data.Height != m_videoBuffer.height || data.Width != m_videoBuffer.width)
            {
                m_videoBuffer.Resize(data.Width, data.Height);
            }

            data.CopyBufferToTexture(m_videoBuffer);
            return m_videoBuffer;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 8)]
    internal struct ImageData
    {
        public IntPtr RawData;
        [MarshalAs(UnmanagedType.I4)] public readonly int Width;
        [MarshalAs(UnmanagedType.I4)] public readonly int Height;
    }

    internal static class ImageDataExtensions
    {
        public static void CopyBufferToTexture(this ImageData imageData, Texture2D tex)
        {
            int length = imageData.Width * imageData.Height * 4;
            unsafe
            {
                void* src = imageData.RawData.ToPointer();
                NativeArray<float> rawTextureData = tex.GetRawTextureData<float>();
                void* dest = rawTextureData.GetUnsafePtr();
                Buffer.MemoryCopy(src, dest, length, length);
            }

            tex.Apply();
        }
    }
}
