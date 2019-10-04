using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.WebRTC
{
    public class MediaStreamTrack : IDisposable
    {
        internal IntPtr ptrNativeObj;
        private TrackKind kind;
        private string id;

        private bool disposed;

        internal MediaStreamTrack(IntPtr ptr)
        {
            ptrNativeObj = ptr;
            kind = NativeMethods.MediaStreamTrackGetKind(ptrNativeObj);
            id = Marshal.PtrToStringAnsi(NativeMethods.MediaStreamTrackGetID(ptrNativeObj));
            WebRTC.Table.Add(ptrNativeObj, this);
        }

        ~MediaStreamTrack()
        {
            this.Dispose();
            WebRTC.Table.Remove(ptrNativeObj);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (ptrNativeObj != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteMediaStreamTrack(ptrNativeObj);
                ptrNativeObj = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public bool Enabled
        {
            get
            {
                return NativeMethods.MediaStreamTrackGetEnabled(ptrNativeObj);
            }
            set
            {
                NativeMethods.MediaStreamTrackSetEnabled(ptrNativeObj, value);
            }
        }
        public TrackState ReadyState
        {
            get
            {
                return NativeMethods.MediaStreamTrackGetReadyState(ptrNativeObj);
            }
            private set { }
        }

        // TODO::
        void Stop()
        {
            throw new NotImplementedException("not impletemented native code");
            WebRTC.Context.StopMediaStreamTrack(ptrNativeObj);
        }
            

        public TrackKind Kind { get => kind; private set { } }
        public string Id { get => id; private set { } }
    }

    public class VideoStreamTrack : MediaStreamTrack
    {
        public VideoStreamTrack(string label, RenderTexture rt, int bitRateMbps = 10000000) : base(WebRTC.Context.CreateVideoTrack(label, rt.GetNativeTexturePtr(), rt.width, rt.height, bitRateMbps))
        {
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
    public class RTCRtpSender
    {
        internal IntPtr self;
        private MediaStreamTrack track;

        internal RTCRtpSender(IntPtr ptr)
        {
            self = ptr;
        }
    }
    public class RTCTrackEvent
    {
        private IntPtr self;
        private MediaStreamTrack track;

        public MediaStreamTrack Track
        {
            get => new MediaStreamTrack(NativeMethods.RtpTransceiverInterfaceGetTrack(self));
            private set { }
        }
        internal RTCTrackEvent(IntPtr ptr)
        {
            self = ptr;
        }
    }

}

