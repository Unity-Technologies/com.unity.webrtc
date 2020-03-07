using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.WebRTC
{
    public class MediaStreamTrack : IDisposable
    {
        internal IntPtr self;
        private TrackKind kind;
        private string id;
        protected bool disposed;
        private bool enabled;
        private TrackState readyState;
        internal Action<MediaStreamTrack> stopTrack;
        internal Func<MediaStreamTrack, RenderTexture[]> getRts;

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
        public TrackState ReadyState
        {
            get
            {
                return NativeMethods.MediaStreamTrackGetReadyState(self);
            }
            private set { }
        }


        public TrackKind Kind { get => kind; private set { } }
        public string Id { get => id; private set { } }

        internal MediaStreamTrack(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            kind = NativeMethods.MediaStreamTrackGetKind(self);
            id = Marshal.PtrToStringAnsi(NativeMethods.MediaStreamTrackGetID(self));
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
        public VideoStreamTrack(string label, IntPtr ptr, int width, int height, int bitrate) : base(WebRTC.Context.CreateVideoTrack(label, ptr, width, height, bitrate))
        {
            WebRTC.Context.InitializeEncoder(self);
            CameraExtension.tracks.Add(this);
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
                CameraExtension.tracks.Remove(this);
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
        private IntPtr self;
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

