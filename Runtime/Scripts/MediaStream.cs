using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.WebRTC
{
    /// <summary>
    /// Delegate invoked when a <see cref="MediaStreamTrack"/> is added to the <see cref="MediaStream"/>.
    /// </summary>
    /// <param name="e">Event containing the added track information.</param>
    public delegate void DelegateOnAddTrack(MediaStreamTrackEvent e);

    /// <summary>
    /// Delegate invoked when a <see cref="MediaStreamTrack"/> is removed from the <see cref="MediaStream"/>.
    /// </summary>
    /// <param name="e">Event containing the removed track information.</param>
    public delegate void DelegateOnRemoveTrack(MediaStreamTrackEvent e);

    /// <summary>
    ///     Represents a stream of media content.
    /// </summary>
    /// <remarks>
    ///     <see cref="MediaStream"/> represents a stream of media content.
    ///     A stream consists of several tracks, such as video or audio tracks.
    ///     Each track is specified as an instance of <see cref="MediaStreamTrack"/>.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         MediaStream mediaStream = new MediaStream();
    ///     ]]></code>
    /// </example>
    /// <seealso cref="MediaStreamTrack"/>
    public class MediaStream : RefCountedObject
    {
        private DelegateOnAddTrack onAddTrack;
        private DelegateOnRemoveTrack onRemoveTrack;

        private HashSet<MediaStreamTrack> cacheTracks = new HashSet<MediaStreamTrack>();

        /// <summary>
        ///     String containing 36 characters denoting a unique identifier for the object.
        /// </summary>
        public string Id =>
            NativeMethods.MediaStreamGetID(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        /// <summary>
        ///     Finalizer for <see cref="MediaStream"/>.
        /// </summary>
        /// <remarks>
        ///     Ensures that resources are released by calling the `Dispose` method.
        /// </remarks>
        ~MediaStream()
        {
            this.Dispose();
        }

        /// <summary>
        ///     Disposes of MediaStream.
        /// </summary>
        /// <remarks>
        ///     `Dispose` method disposes of the MediaStream and releases the associated resources. 
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         mediaStream.Dispose();
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
                WebRTC.Context.UnRegisterMediaStreamObserver(this);
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        /// Delegate to be called when a new <see cref="MediaStreamTrack"/> object has been added.
        /// </summary>
        // todo:(kazuki) Rename to "onAddTrack"
        // todo:(kazuki) Should we change the API to use UnityEvent or Action class?
        public DelegateOnAddTrack OnAddTrack
        {
            get => onAddTrack;
            set
            {
                onAddTrack = value;
            }
        }

        /// <summary>
        /// Delegate to be called when a new <see cref="MediaStreamTrack"/> object has been removed.
        /// </summary>
        // todo:(kazuki) Rename to "onAddTrack"
        // todo:(kazuki) Should we change the API to use UnityEvent or Action class?
        public DelegateOnRemoveTrack OnRemoveTrack
        {
            get => onRemoveTrack;
            set
            {
                onRemoveTrack = value;
            }
        }

        /// <summary>
        ///     Returns a list of VideoStreamTrack objects in the stream.
        /// </summary>
        /// <remarks>
        ///     `GetVideoTracks` method returns a sequence that represents all the <see cref="VideoStreamTrack"/> objects in this stream's track set.
        /// </remarks>
        /// <returns>List of <see cref="MediaStreamTrack"/> objects, one for each video track contained in the media stream.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         IEnumerable<VideoStreamTrack> videoTracks = mediaStream.GetVideoTracks();
        ///     ]]></code>
        /// </example>
        public IEnumerable<VideoStreamTrack> GetVideoTracks()
        {
            var buf = NativeMethods.MediaStreamGetVideoTracks(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new VideoStreamTrack(ptr));
        }

        /// <summary>
        ///     Returns a list of AudioStreamTrack objects in the stream.
        /// </summary>
        /// <remarks>
        ///     `GetAudioTracks` method returns a sequence that represents all the <see cref="AudioStreamTrack"/> objects in this stream's track set.
        /// </remarks>
        /// <returns>List of <see cref="AudioStreamTrack"/> objects, one for each audio track contained in the stream.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         IEnumerable<AudioStreamTrack> audioTracks = mediaStream.GetAudioTracks();
        ///     ]]></code>
        /// </example>
        public IEnumerable<AudioStreamTrack> GetAudioTracks()
        {
            var buf = NativeMethods.MediaStreamGetAudioTracks(GetSelfOrThrow(), out ulong length);
            return WebRTC.Deserialize(buf, (int)length, ptr => new AudioStreamTrack(ptr));
        }

        /// <summary>
        ///     Returns a list of MediaStreamTrack objects in the stream.
        /// </summary>
        /// <remarks>
        ///     `GetTracks` method returns a sequence that represents all the <see cref="MediaStreamTrack"/> objects in this stream's track set.
        /// </remarks>
        /// <returns>List of <see cref="MediaStreamTrack"/> objects.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         IEnumerable<MediaStreamTrack> tracks = mediaStream.GetTracks();
        ///     ]]></code>
        /// </example>
        public IEnumerable<MediaStreamTrack> GetTracks()
        {
            return GetAudioTracks().Cast<MediaStreamTrack>().Concat(GetVideoTracks());
        }

        /// <summary>
        ///     Add a new track to the stream.
        /// </summary>
        /// <remarks>
        ///     `AddTrack` method adds a new track to the stream.
        ///     This class keeps references of <see cref="MediaStreamTrack"/> to avoid GC.
        ///     Please call the <see cref="RemoveTrack(MediaStreamTrack)"/> method when it's no longer needed.
        /// </remarks>
        /// <param name="track">`MediaStreamTrack` object to add to the stream.</param>
        /// <returns>`true` if the track successfully added to the stream.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         MediaStream receiveStream = new MediaStream();
        ///         peerConnection.OnTrack = e =>
        ///         {
        ///             bool result = receiveStream.AddTrack(e.Track);
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="RemoveTrack(MediaStreamTrack)"/>
        public bool AddTrack(MediaStreamTrack track)
        {
            cacheTracks.Add(track);
            return NativeMethods.MediaStreamAddTrack(GetSelfOrThrow(), track.GetSelfOrThrow());
        }

        /// <summary>
        ///     Remove a track from the stream.
        /// </summary>
        /// <remarks>
        ///     `RemoveTrack` method removes a track from the stream.
        /// </remarks>
        /// <param name="track">`MediaStreamTrack` object to remove from the stream.</param>
        /// <returns>`true` if the track successfully removed from the stream.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         bool result = mediaStream.RemoveTrack(track);
        ///     ]]></code>
        /// </example>
        /// <seealso cref="AddTrack(MediaStreamTrack)"/>
        public bool RemoveTrack(MediaStreamTrack track)
        {
            cacheTracks.Remove(track);
            return NativeMethods.MediaStreamRemoveTrack(GetSelfOrThrow(), track.GetSelfOrThrow());
        }

        /// <summary>
        ///     Creates a MediaStream instance.
        /// </summary>
        /// <remarks>
        ///     `MediaStream` constructor creates an instance of `MediaStream`,
        ///     which serves as a collection of media tracks,
        ///     each represented by a `MediaStreamTrack` object.
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         MediaStream mediaStream = new MediaStream();
        ///     ]]></code>
        /// </example>
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
