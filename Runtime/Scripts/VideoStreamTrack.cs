using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.WebRTC
{
    /// <summary>
    ///     Delegate to be called when the first frame of the video is received.
    /// </summary>
    /// <remarks>
    ///     `OnVideoReceived` delegate is called when the first frame of the video is received.
    /// </remarks>
    /// <param name="renderer">`Texture` object where the video stream is rendered.</param>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         [SerializeField]
    ///         RawImage receivedImage;
    ///
    ///         videoStreamTrack.OnVideoReceived += (texture) =>
    ///         {
    ///             receivedImage.texture = texture;
    ///         }
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCPeerConnection" />
    public delegate void OnVideoReceived(Texture renderer);

    /// <summary>
    ///     Delegate to be called to copy texture.
    /// </summary>
    /// <remarks>
    ///     `CopyTexture` delegate is called to copy texture when the texture is updated.
    /// </remarks>
    /// <param name="source">Source `Texture` object.</param>
    /// <param name="dest">Destination `Texture` object.</param>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         VideoStreamTrack videoStreamTrack = new VideoStreamTrack(texture, CopyTextureHelper.VerticalFlipCopy);
    ///     ]]>
    ///     </code>
    /// </example>
    /// <seealso cref="VideoStreamTrack(Texture, CopyTexture)" />
    public delegate void CopyTexture(Texture source, RenderTexture dest);

    /// <summary>
    ///     Represents a single video track within a stream
    /// </summary>
    /// <remarks>
    ///     `VideoStreamTrack` is a `MediaStreamTrack` that represents a single video track within a stream.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         VideoStreamTrack videoStreamTrack = new VideoStreamTrack(texture);
    ///     ]]></code>
    /// </example>
    /// <seealso cref="MediaStreamTrack" />
    /// <seealso cref="WebRTC" />
    public class VideoStreamTrack : MediaStreamTrack
    {
        /// <summary>
        ///     If the value is set to true, the received video is flipped vertically.
        /// </summary>
        /// <remarks>
        ///     Change this property before starting to receive video.
        /// </remarks>
        public static bool NeedReceivedVideoFlipVertically { get; set; } = true;

        internal static ConcurrentDictionary<IntPtr, WeakReference<VideoStreamTrack>> s_tracks =
            new ConcurrentDictionary<IntPtr, WeakReference<VideoStreamTrack>>();

        internal enum VideoStreamTrackAction
        {
            Ignore = 0,
            Decode = 1,
            Encode = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct VideoStreamTrackData
        {
            public VideoStreamTrackAction action;
            public IntPtr ptrTexture;
            public IntPtr ptrSource;
            public int width;
            public int height;
            public GraphicsFormat format;
        }

        internal VideoTrackSource m_source;

        UnityVideoRenderer m_renderer;
        VideoStreamTrackData m_data;
        IntPtr m_dataptr = IntPtr.Zero;

        private static RenderTexture CreateRenderTexture(int width, int height)
        {
            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            var tex = new RenderTexture(width, height, 0, format);
            tex.Create();
            return tex;
        }

        /// <summary>
        ///     When the track is configured to receive a video stream, represents the `Texture` object where the video stream is rendered.
        ///     When the track is configured to send a video stream, represents the destination `Texture` object to send.
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
        ///     When the track is configured to receive a video stream, represents the pointer to the `Texture` object where the video stream is rendered.
        ///     When the track is configured to send a video stream, represents the pointer to the destination `Texture` object to send.
        /// </summary>
        public IntPtr TexturePtr
        {
            get
            {
                if (m_renderer != null)
                    return m_renderer.TexturePtr;
                return m_source.destTexturePtr_;
            }
        }

        /// <summary>
        ///     Indicates that the track is configured to decode an incoming video stream.
        /// </summary>
        public bool Decoding => m_renderer != null;

        /// <summary>
        ///     Indicates that the track is configured to encode and send a video stream to a remote peer.
        /// </summary>
        public bool Encoding => m_source != null;

        /// <summary>
        ///     Pointer to the video stream data in the native memory.
        /// </summary>
        public IntPtr DataPtr => m_dataptr;

        /// <summary>
        ///     Event to be fired when the first frame of the video is received.
        /// </summary>
        /// <remarks>
        ///     `OnVideoReceived` event is fired when the first frame of the video is received.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         [SerializeField]
        ///         RawImage receivedImage;
        ///
        ///         videoStreamTrack.OnVideoReceived += (texture) =>
        ///         {
        ///             receivedImage.texture = texture;
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="RTCPeerConnection"/>
        public event OnVideoReceived OnVideoReceived;

        internal void UpdateTexture()
        {
            var texture = Texture;
            if (texture == null)
                return;

            m_source?.Update();
            if (m_renderer?.customTextureUpload == false)
                m_renderer?.Update();

            var texturePtr = TexturePtr;
            if (m_data.ptrTexture != texturePtr)
            {
                m_data.ptrTexture = texturePtr;
                m_data.ptrSource = IntPtr.Zero;
                m_data.action = VideoStreamTrackAction.Ignore;
                if (Encoding == true)
                {
                    m_data.ptrSource = (IntPtr)m_source?.self;
                    m_data.action = VideoStreamTrackAction.Encode;
                }
                else if (Decoding == true && m_renderer?.customTextureUpload == true)
                {
                    m_data.ptrSource = (IntPtr)m_renderer?.self;
                    m_data.action = VideoStreamTrackAction.Decode;
                }
                m_data.width = texture.width;
                m_data.height = texture.height;
                m_data.format = texture.graphicsFormat;
                Marshal.StructureToPtr(m_data, m_dataptr, false);
            }
        }

        /// <summary>
        ///     Creates a new VideoStreamTrack object.
        /// </summary>
        /// <remarks>
        ///     `VideoStreamTrack` constructor creates an instance of `VideoStreamTrack` with a `source`.
        /// </remarks>
        /// <param name="texture">
        ///     `Texture` object that provides the input source for the video stream and is used in creating the video track.
        /// </param>
        /// <param name="copyTexture">
        ///     By default, textures are copied vertically flipped, using `CopyTextureHelper.VerticalFlipCopy`,
        ///     use `Graphics.Blit` for copy as is, `CopyTextureHelper` for flip,
        ///     or write your own `CopyTexture` function.
        /// </param>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         VideoStreamTrack videoStreamTrack = new VideoStreamTrack(texture, CopyTextureHelper.VerticalFlipCopy);
        ///     ]]></code>
        /// </example>
        /// <exception cref="InvalidOperationException"></exception>
        public VideoStreamTrack(Texture texture, CopyTexture copyTexture = null)
            : base(CreateVideoTrack(texture, out var source))
        {
            if (!s_tracks.TryAdd(self, new WeakReference<VideoStreamTrack>(this)))
                throw new InvalidOperationException();

            m_dataptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VideoStreamTrackData)));
            Marshal.StructureToPtr(m_data, m_dataptr, false);

            var dest = CreateRenderTexture(texture.width, texture.height);

            m_source = source;
            m_source.copyTexture_ = copyTexture ?? CopyTextureHelper.VerticalFlipCopy;
            m_source.sourceTexture_ = texture;
            m_source.destTexture_ = dest;
            m_source.destTexturePtr_ = dest.GetNativeTexturePtr();
        }

        /// <summary>
        /// Video Receiver
        /// </summary>
        /// <param name="ptr"></param>
        internal VideoStreamTrack(IntPtr ptr)
            : base(CreateVideoTrack(ptr))
        {
            if (!s_tracks.TryAdd(self, new WeakReference<VideoStreamTrack>(this)))
                throw new InvalidOperationException();

            m_dataptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VideoStreamTrackData)));
            Marshal.StructureToPtr(m_data, m_dataptr, false);

            m_renderer = new UnityVideoRenderer(this, NeedReceivedVideoFlipVertically);
        }

        /// <summary>
        ///     Disposes of VideoStremTrack
        /// </summary>
        /// <remarks>
        ///     `Dispose` method disposes of the `VideoStreamTrack` and releases the associated resources.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         videoStreamTrack.Dispose();
        ///     ]]></code>
        /// </example>
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

                if (m_dataptr != IntPtr.Zero)
                {
                    // This buffer is referred from the rendering thread,
                    // so set the delay 100ms to wait the task of another thread.
                    WebRTC.DelayActionOnMainThread(() =>
                    {
                        Marshal.FreeHGlobal(m_dataptr);
                        m_dataptr = IntPtr.Zero;
                    }, 0.1f);
                }

                s_tracks.TryRemove(self, out var value);
            }
            base.Dispose();
        }

        internal void OnVideoFrameResize(Texture texture)
        {
            OnVideoReceived?.Invoke(texture);
        }

        /// <summary>
        /// On Windows or macOS, VideoStreamTrack doesn't work with OpenGL.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="graphicsDeviceType"></param>
        /// <returns></returns>
        internal static bool IsSupported(RuntimePlatform platform, UnityEngine.Rendering.GraphicsDeviceType graphicsDeviceType)
        {
            if (platform != RuntimePlatform.WindowsEditor &&
                platform != RuntimePlatform.WindowsPlayer &&
                platform != RuntimePlatform.OSXEditor &&
                platform != RuntimePlatform.OSXPlayer)
                return true;
            if (graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore &&
                graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 &&
                graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
                return true;
            return false;
        }

        /// <summary>
        /// for sender.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        static IntPtr CreateVideoTrack(Texture texture, out VideoTrackSource source)
        {
            if (!IsSupported(Application.platform, SystemInfo.graphicsDeviceType))
                throw new NotSupportedException($"Not Support OpenGL API on {Application.platform} in Unity WebRTC.");

            WebRTC.ValidateGraphicsFormat(texture.graphicsFormat);

            var error = WebRTC.ValidateTextureSize(texture.width, texture.height, Application.platform);
            if (error.errorType != RTCErrorType.None)
                throw new ArgumentException(error.message);

            var label = Guid.NewGuid().ToString();
            source = new VideoTrackSource();
            return WebRTC.Context.CreateVideoTrack(label, source.GetSelfOrThrow());
        }

        /// <summary>
        /// for receiver.
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        static IntPtr CreateVideoTrack(IntPtr ptr)
        {
            if (!IsSupported(Application.platform, SystemInfo.graphicsDeviceType))
                throw new NotSupportedException($"Not Support OpenGL API on {Application.platform} in Unity WebRTC.");
            return ptr;
        }
    }

    internal class VideoTrackSource : RefCountedObject
    {
        internal Texture sourceTexture_;
        internal RenderTexture destTexture_;
        internal IntPtr destTexturePtr_;
        internal CopyTexture copyTexture_;

        internal bool SyncApplicationFramerate
        {
            get => NativeMethods.VideoSourceGetSyncApplicationFramerate(GetSelfOrThrow());
            set => NativeMethods.VideoSourceSetSyncApplicationFramerate(GetSelfOrThrow(), value);
        }

        public VideoTrackSource()
            : base(WebRTC.Context.CreateVideoTrackSource())
        {
            WebRTC.Table.Add(self, this);
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
            copyTexture_(sourceTexture_, destTexture_);
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
        public IntPtr TexturePtr { get; private set; }
        public bool customTextureUpload { get; private set; }

        public UnityVideoRenderer(VideoStreamTrack track, bool needFlip)
        {
            self = WebRTC.Context.CreateVideoRenderer(OnVideoFrameResize, needFlip);
            this.track = track;
            NativeMethods.VideoTrackAddOrUpdateSink(track.GetSelfOrThrow(), self);
            WebRTC.Table.Add(self, this);

            // If false, upload textures through built-in Unity plugin interface (CPU buffer upload in render thread)
            customTextureUpload = false;
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
                TexturePtr = IntPtr.Zero;
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
                TexturePtr = IntPtr.Zero;
            }

            var format = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            Texture = new Texture2D(width, height, format, TextureCreationFlags.None);
            TexturePtr = Texture.GetNativeTexturePtr();
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

    internal static class CopyTextureHelper
    {
        // Blit parameter to flip vertically
        private static readonly Vector2 s_verticalScale = new Vector2(1f, -1f);
        private static readonly Vector2 s_verticalOffset = new Vector2(0f, 1f);
        // Blit parameter to flip horizontally
        private static readonly Vector2 s_horizontalScale = new Vector2(-1f, 1f);
        private static readonly Vector2 s_horixontalOffset = new Vector2(1f, 0f);
        // Blit parameter to flip diagonally
        private static readonly Vector2 s_diagonalScale = new Vector2(-1f, -1f);
        private static readonly Vector2 s_diagonalOffset = new Vector2(1f, 1f);

        public static void HorizontalFlipCopy(Texture source, RenderTexture dest)
        {
            Graphics.Blit(source, dest, s_horizontalScale, s_horixontalOffset);
        }

        public static void VerticalFlipCopy(Texture source, RenderTexture dest)
        {
            Graphics.Blit(source, dest, s_verticalScale, s_verticalOffset);
        }

        public static void DiagonalFlipCopy(Texture source, RenderTexture dest)
        {
            Graphics.Blit(source, dest, s_diagonalScale, s_diagonalOffset);
        }
    }
}
