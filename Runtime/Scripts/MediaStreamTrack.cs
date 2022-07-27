using System;
using System.Collections.Generic;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaStreamTrack : RefCountedObject
    {
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

        internal MediaStreamTrack(IntPtr ptr) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
        }

        /// <summary>
        /// 
        /// </summary>
        ~MediaStreamTrack()
        {
            this.Dispose();
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
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        /// Disassociate track from its source(video or audio), not for destroying the track
        /// </summary>
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
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TrackKind
    {
        /// <summary>
        /// 
        /// </summary>
        Audio,

        /// <summary>
        /// 
        /// </summary>
        Video
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TrackState
    {
        /// <summary>
        /// 
        /// </summary>
        Live,

        /// <summary>
        /// 
        /// </summary>
        Ended
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCTrackEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public RTCRtpTransceiver Transceiver { get; }

        /// <summary>
        /// 
        /// </summary>
        public RTCRtpReceiver Receiver
        {
            get
            {
                return Transceiver.Receiver;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MediaStreamTrack Track
        {
            get
            {
                return Receiver.Track;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<MediaStream> Streams
        {
            get
            {
                return Receiver.Streams;
            }
        }

        internal RTCTrackEvent(IntPtr ptrTransceiver, RTCPeerConnection peer)
        {
            Transceiver = WebRTC.FindOrCreate(
                ptrTransceiver, ptr => new RTCRtpTransceiver(ptr, peer));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MediaStreamTrackEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public MediaStreamTrack Track { get; }

        internal MediaStreamTrackEvent(MediaStreamTrack track)
        {
            Track = track;
        }
    }
}
