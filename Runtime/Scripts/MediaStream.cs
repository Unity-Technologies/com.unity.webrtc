using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    public delegate void DelegateOnAddTrack(MediaStreamTrackEvent e);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    public delegate void DelegateOnRemoveTrack(MediaStreamTrackEvent e);

    /// <summary>
    /// 
    /// </summary>
    public class MediaStream : RefCountedObject
    {
        private DelegateOnAddTrack onAddTrack;
        private DelegateOnRemoveTrack onRemoveTrack;

        private HashSet<MediaStreamTrack> cacheTracks = new HashSet<MediaStreamTrack>();

        /// <summary>
        ///
        /// </summary>
        public string Id =>
            NativeMethods.MediaStreamGetID(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        /// <summary>
        /// 
        /// </summary>
        ~MediaStream()
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
                WebRTC.Context.UnRegisterMediaStreamObserver(this);
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// todo:(kazuki) Rename to "onAddTrack"
        /// todo:(kazuki) Should we change the API to use UnityEvent or Action class?
        public DelegateOnAddTrack OnAddTrack
        {
            get => onAddTrack;
            set
            {
                onAddTrack = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// todo:(kazuki) Rename to "onAddTrack"
        /// todo:(kazuki) Should we change the API to use UnityEvent or Action class?
        public DelegateOnRemoveTrack OnRemoveTrack
        {
            get => onRemoveTrack;
            set
            {
                onRemoveTrack = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VideoStreamTrack> GetVideoTracks()
        {
            var buf = NativeMethods.MediaStreamGetVideoTracks(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new VideoStreamTrack(ptr));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AudioStreamTrack> GetAudioTracks()
        {
            var buf = NativeMethods.MediaStreamGetAudioTracks(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new AudioStreamTrack(ptr));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MediaStreamTrack> GetTracks()
        {
            return GetAudioTracks().Cast<MediaStreamTrack>().Concat(GetVideoTracks());
        }

        /// <summary>
        /// Add a new track to the stream.
        /// </summary>
        /// <remarks>
        /// This class keeps references of <see cref="MediaStreamTrack"/> to avoid GC.
        /// Please call the <see cref="RemoveTrack(MediaStreamTrack)"/> method when it's no longer needed.
        /// </remarks>
        /// <param name="track"></param>
        /// <returns></returns>
        /// <seealso cref="RemoveTrack(MediaStreamTrack)"/>
        public bool AddTrack(MediaStreamTrack track)
        {
            cacheTracks.Add(track);
            return NativeMethods.MediaStreamAddTrack(GetSelfOrThrow(), track.GetSelfOrThrow());
        }

        /// <summary>
        /// Remove a new track to the stream.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        /// <seealso cref="AddTrack(MediaStreamTrack)"/>
        public bool RemoveTrack(MediaStreamTrack track)
        {
            cacheTracks.Remove(track);
            return NativeMethods.MediaStreamRemoveTrack(GetSelfOrThrow(), track.GetSelfOrThrow());
        }

        /// <summary>
        /// 
        /// </summary>
        public MediaStream() : this(WebRTC.Context.CreateMediaStream(Guid.NewGuid().ToString()))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ptr"></param>
        internal MediaStream(IntPtr ptr) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            WebRTC.Context.RegisterMediaStreamObserver(this);
            WebRTC.Context.MediaStreamRegisterOnAddTrack(this, MediaStreamOnAddTrack);
            WebRTC.Context.MediaStreamRegisterOnRemoveTrack(this, MediaStreamOnRemoveTrack);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnAddTrack))]
        static void MediaStreamOnAddTrack(IntPtr ptr, IntPtr ptrTrack)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is MediaStream stream)
                {
                    var track = WebRTC.FindOrCreate(ptrTrack, MediaStreamTrack.Create);
                    var e = new MediaStreamTrackEvent(track);
                    stream.onAddTrack?.Invoke(e);
                    stream.cacheTracks.Add(e.Track);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnRemoveTrack))]
        static void MediaStreamOnRemoveTrack(IntPtr ptr, IntPtr ptrTrack)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is MediaStream stream)
                {
                    if (WebRTC.Table.ContainsKey(ptrTrack))
                    {
                        var track = WebRTC.Table[ptrTrack] as MediaStreamTrack;
                        var e = new MediaStreamTrackEvent(track);
                        stream.onRemoveTrack?.Invoke(e);
                        stream.cacheTracks.Remove(e.Track);
                    }
                }
            });
        }
    }
}
