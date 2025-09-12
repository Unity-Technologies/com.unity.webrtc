using System;
using System.Collections.Generic;

namespace Unity.WebRTC
{
    /// <summary>
    ///     Represents a single media track within a stream.
    /// </summary>
    /// <remarks>
    ///     `MediaStreamTrack` represents a single media track within a stream.
    ///     Typically, these are audio or video tracks, but other track types may exist as well.
    ///     Each track is associated with a `MediaStream` object.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         IEnumerable<MediaStreamTrack> mediaStreamTracks = mediaStream.GetTracks();
    ///     ]]></code>
    /// </example>
    /// <seealso cref="MediaStream"/>
    public class MediaStreamTrack : RefCountedObject
    {
        /// <summary>
        ///     Boolean value that indicates whether the track is allowed to render the source stream.
        /// </summary>
        /// <remarks>
        ///     When the value is `true`, a track's data is output from the source to the destination; otherwise, empty frames are output.
        /// </remarks>
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
        ///     TrackState value that describes the status of the track.
        /// </summary>
        public TrackState ReadyState =>
            NativeMethods.MediaStreamTrackGetReadyState(GetSelfOrThrow());

        /// <summary>
        ///     TrackKind value that describes the type of media for the track.
        /// </summary>
        public TrackKind Kind =>
            NativeMethods.MediaStreamTrackGetKind(GetSelfOrThrow());

        /// <summary>
        ///     String containing a unique identifier (GUID) for the track.
        /// </summary>
        public string Id =>
            NativeMethods.MediaStreamTrackGetID(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        internal MediaStreamTrack(IntPtr ptr) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
        }

        /// <summary>
        ///     Finalizer for MediaStreamTrack.
        /// </summary>
        /// <remarks>
        ///     Ensures that resources are released by calling the `Dispose` method
        /// </remarks>
        ~MediaStreamTrack()
        {
            this.Dispose();
        }

        /// <summary>
        ///     Disposes of MediaStreamTrack.
        /// </summary>
        /// <remarks>
        ///     `Dispose` method disposes of the `MediaStreamTrack` and releases the associated resources. 
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         mediaStreamTrack.Dispose();
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
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        ///     Stops the track.
        /// </summary>
        /// <remarks>
        ///     `Stop` method disassociates the track from its source (video or audio) without destroying the track.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         mediaStreamTrack.Stop();
        ///     ]]></code>
        /// </example>
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
    /// Specifies the type of media for a track.
    /// </summary>
    public enum TrackKind
    {
        /// <summary>
        /// Represents an audio track.
        /// </summary>
        Audio,

        /// <summary>
        /// Represents a video track.
        /// </summary>
        Video
    }

    /// <summary>
    /// Indicates the current state of a media track.
    /// </summary>
    public enum TrackState
    {
        /// <summary>
        /// The track is active and live.
        /// </summary>
        Live,

        /// <summary>
        /// The track has ended.
        /// </summary>
        Ended
    }

    /// <summary>
    /// Represents an event triggered when a track is added to a peer connection.
    /// </summary>
    public class RTCTrackEvent
    {
        /// <summary>
        /// The transceiver associated with the track event.
        /// </summary>
        public RTCRtpTransceiver Transceiver { get; }

        /// <summary>
        /// The receiver associated with the track event.
        /// </summary>
        public RTCRtpReceiver Receiver
        {
            get
            {
                return Transceiver.Receiver;
            }
        }

        /// <summary>
        /// The media track associated with the event.
        /// </summary>
        public MediaStreamTrack Track
        {
            get
            {
                return Receiver.Track;
            }
        }

        /// <summary>
        /// The media streams associated with the track event.
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
    /// Represents an event triggered when a media stream track is added or removed.
    /// </summary>
    public class MediaStreamTrackEvent
    {
        /// <summary>
        /// The media stream track associated with the event.
        /// </summary>
        public MediaStreamTrack Track { get; }

        internal MediaStreamTrackEvent(MediaStreamTrack track)
        {
            Track = track;
        }
    }
}
