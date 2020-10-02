using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public delegate void DelegateOnAddTrack(MediaStreamTrackEvent e);
    public delegate void DelegateOnRemoveTrack(MediaStreamTrackEvent e);

    public class MediaStream : IDisposable
    {
        private DelegateOnAddTrack onAddTrack;
        private DelegateOnRemoveTrack onRemoveTrack;

        internal IntPtr self;
        private readonly string id;
        private bool disposed;


        public string Id
        {
            get
            {
                return id;
            }
        }

        ~MediaStream()
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
            if(self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteMediaStream(this);
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public DelegateOnAddTrack OnAddTrack
        {
            get => onAddTrack;
            set
            {
                onAddTrack = value;
            }
        }

        public DelegateOnRemoveTrack OnRemoveTrack
        {
            get => onRemoveTrack;
            set
            {
                onRemoveTrack = value;
            }
        }

        private void StopTrack(MediaStreamTrack track)
        {

            if (track.Kind == TrackKind.Video)
            {
                WebRTC.Context.StopMediaStreamTrack(track.self);
            }
            else
            {
                Audio.Stop();
            }
        }

        public IEnumerable<MediaStreamTrack> GetVideoTracks()
        {
            uint length = 0;
            var buf = NativeMethods.MediaStreamGetVideoTracks(self, ref length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new MediaStreamTrack(ptr));
        }

        public IEnumerable<MediaStreamTrack> GetAudioTracks()
        {
            uint length = 0;
            var buf = NativeMethods.MediaStreamGetAudioTracks(self, ref length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new MediaStreamTrack(ptr));
        }

        public IEnumerable<MediaStreamTrack> GetTracks()
        {
            return GetAudioTracks().Concat(GetVideoTracks());
        }

        public bool AddTrack(MediaStreamTrack track)
        {
            return NativeMethods.MediaStreamAddTrack(self, track.self);
        }
        public bool RemoveTrack(MediaStreamTrack track)
        {
            return NativeMethods.MediaStreamRemoveTrack(self, track.self);
        }

        public MediaStream() : this(WebRTC.Context.CreateMediaStream(Guid.NewGuid().ToString()))
        {
        }

        internal MediaStream(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            id = NativeMethods.MediaStreamGetID(self).AsAnsiStringWithFreeMem();

            WebRTC.Context.MediaStreamRegisterOnAddTrack(self, MediaStreamOnAddTrack);
            WebRTC.Context.MediaStreamRegisterOnRemoveTrack(self, MediaStreamOnRemoveTrack);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnAddTrack))]
        static void MediaStreamOnAddTrack(IntPtr ptr, IntPtr track)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is MediaStream stream)
                {
                    stream.onAddTrack?.Invoke(new MediaStreamTrackEvent(track));
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnRemoveTrack))]
        static void MediaStreamOnRemoveTrack(IntPtr ptr, IntPtr track)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is MediaStream stream)
                {
                    stream.onRemoveTrack?.Invoke(new MediaStreamTrackEvent(track));
                }
            });
        }
    }
}
