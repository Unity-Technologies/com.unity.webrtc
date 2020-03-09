using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public class MediaStreamTrack : IDisposable
    {
        internal IntPtr self;
        protected bool disposed;
        private bool enabled;
        private TrackState readyState;
        internal Action<MediaStreamTrack> stopTrack;

        /// <summary>
        /// 
        /// </summary>
        public bool Enabled
        {
            get
            {
                return NativeMethods.MediaStreamTrackGetEnabled(self);
            }
            set
            {
                NativeMethods.MediaStreamTrackSetEnabled(self, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TrackState ReadyState
        {
            get
            {
                return NativeMethods.MediaStreamTrackGetReadyState(self);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TrackKind Kind { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; }

        internal MediaStreamTrack(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            Kind = NativeMethods.MediaStreamTrackGetKind(self);
            Id = Marshal.PtrToStringAnsi(NativeMethods.MediaStreamTrackGetID(self));
        }

        ~MediaStreamTrack()
        {
            this.Dispose();
            WebRTC.Table.Remove(self);
        }

        public virtual void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteMediaStreamTrack(self);
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        //Disassociate track from its source(video or audio), not for destroying the track
        public void Stop()
        {
            stopTrack(this);
        }
    }

    public class VideoStreamTrack : MediaStreamTrack
    {
        internal static List<VideoStreamTrack> tracks = new List<VideoStreamTrack>();

        bool needFlip = false;
        UnityEngine.Texture source;
        UnityEngine.RenderTexture dest;

        private static UnityEngine.RenderTexture CopyRenderTexture(UnityEngine.RenderTexture source)
        {
            var tex = new UnityEngine.RenderTexture(source.width, source.height, 0, source.format);
            tex.Create();
            return tex;
        }

        internal VideoStreamTrack(string label, UnityEngine.RenderTexture source, UnityEngine.RenderTexture dest, int width, int height, int bitrate)
            : this(label, dest.GetNativeTexturePtr(), width, height, bitrate)
        {
            this.needFlip = true;
            this.source = source;
            this.dest = dest;
        }

        internal void Update()
        {
            // [Note-kazuki: 2020-03-09] Flip vertically RenderTexture
            // note: streamed video is flipped vertical if no action was taken:
            //  - duplicate RenderTexture from its source texture
            //  - call Graphics.Blit command with flip material every frame
            //  - it might be better to implement this if possible
            if (needFlip)
            {
                UnityEngine.Graphics.Blit(source, dest, WebRTC.flipMat);
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
        /// <param name="bitrate"></param>
        public VideoStreamTrack(string label, UnityEngine.RenderTexture source, int bitrate)
            : this(label, source, CopyRenderTexture(source), source.width, source.height, bitrate)
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
        /// <param name="bitrate"></param>
        public VideoStreamTrack(string label, IntPtr ptr, int width, int height, int bitrate)
            : base(WebRTC.Context.CreateVideoTrack(label, ptr, width, height, bitrate))
        {
            WebRTC.Context.InitializeEncoder(self);
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
                WebRTC.Context.FinalizeEncoder(self);
                tracks.Remove(this);
                WebRTC.Context.DeleteMediaStreamTrack(self);
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    public class AudioStreamTrack : MediaStreamTrack
    {
        public AudioStreamTrack(string label) : base(WebRTC.Context.CreateAudioTrack(label))
        {
        }
    }

    public enum TrackKind
    {
        Audio,
        Video
    }
    public enum TrackState
    {
        Live,
        Ended
    }

    public class RTCTrackEvent
    {
        private readonly IntPtr self;
        private readonly MediaStreamTrack track;
        private readonly RTCRtpTransceiver transceiver;

        public MediaStreamTrack Track
        {
            get => track;
        }

        public RTCRtpTransceiver Transceiver
        {
            get => transceiver;
        }

        internal RTCTrackEvent(IntPtr ptr)
        {
            self = ptr;
            track = new MediaStreamTrack(NativeMethods.TransceiverGetTrack(self));
            transceiver = new RTCRtpTransceiver(self);
        }
    }

    public class MediaStreamTrackEvent
    {
        private readonly MediaStreamTrack track;

        public MediaStreamTrack Track
        {
            get => track;
        }

        internal MediaStreamTrackEvent(IntPtr ptr)
        {
            track = new MediaStreamTrack(ptr);
        }
    }
}

