using System;

namespace Unity.WebRTC
{
    public class MediaStreamTrack : IDisposable
    {
        protected IntPtr self;
        protected bool disposed;
        private bool enabled;
        private TrackState readyState;

        /// <summary>
        ///
        /// </summary>
        public bool Enabled
        {
            get
            {
                return NativeMethods.MediaStreamTrackGetEnabled(GetSelfOrThrow());
            }
            set
            {
                NativeMethods.MediaStreamTrackSetEnabled(GetSelfOrThrow(), value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public TrackState ReadyState =>
            NativeMethods.MediaStreamTrackGetReadyState(GetSelfOrThrow());

        /// <summary>
        ///
        /// </summary>
        public TrackKind Kind =>
            NativeMethods.MediaStreamTrackGetKind(GetSelfOrThrow());

        /// <summary>
        ///
        /// </summary>
        public string Id =>
            NativeMethods.MediaStreamTrackGetID(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        internal MediaStreamTrack(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
        }

        ~MediaStreamTrack()
        {
            this.Dispose();
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
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        //Disassociate track from its source(video or audio), not for destroying the track
        public void Stop()
        {
            WebRTC.Context.StopMediaStreamTrack(GetSelfOrThrow());
        }

        internal static MediaStreamTrack Create(IntPtr ptr)
        {
            if (NativeMethods.MediaStreamTrackGetKind(ptr) == TrackKind.Video)
            {
                return new VideoStreamTrack(ptr);
            }

            return new AudioStreamTrack(ptr);
        }

        internal IntPtr GetSelfOrThrow()
        {
            if (self == IntPtr.Zero)
            {
                throw new InvalidOperationException("This instance has been disposed.");
            }
            return self;
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
        public RTCRtpTransceiver Transceiver { get; }

        public RTCRtpReceiver Receiver
        {
            get
            {
                return Transceiver.Receiver;
            }
        }

        public MediaStreamTrack Track
        {
            get
            {
                return Receiver.Track;
            }
        }

        internal RTCTrackEvent(IntPtr ptrTransceiver, RTCPeerConnection peer)
        {
            Transceiver = WebRTC.FindOrCreate(
                ptrTransceiver, ptr => new RTCRtpTransceiver(ptr, peer));
        }
    }

    public class MediaStreamTrackEvent
    {
        public MediaStreamTrack Track { get; }

        internal MediaStreamTrackEvent(IntPtr ptrTrack)
        {
            Track = WebRTC.FindOrCreate(ptrTrack, MediaStreamTrack.Create);
        }
    }
}
