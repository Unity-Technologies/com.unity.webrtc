using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.WebRTC
{
    public class MediaStreamTrack : IDisposable
    {
        internal IntPtr self;
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
            Id = NativeMethods.MediaStreamTrackGetID(self).AsAnsiStringWithFreeMem();
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
            WebRTC.Context.StopMediaStreamTrack(self);
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
        public MediaStreamTrack Track { get; }

        public RTCRtpTransceiver Transceiver { get; }

        public RTCRtpReceiver Receiver
        {
            get
            {
                return Transceiver.Receiver;
            }
        }

        internal RTCTrackEvent(IntPtr ptrTransceiver, RTCPeerConnection peer)
        {
            IntPtr ptrTrack = NativeMethods.TransceiverGetTrack(ptrTransceiver);
            Track = WebRTC.FindOrCreate(ptrTrack, MediaStreamTrack.Create);
            Transceiver = new RTCRtpTransceiver(ptrTransceiver, peer);
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
