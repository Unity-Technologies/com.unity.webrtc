using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public class AudioStreamTrack : MediaStreamTrack
    {

        internal static List<AudioStreamTrack> tracks = new List<AudioStreamTrack>();

        /// <summary>
        /// 
        /// </summary>
        public AudioSource Source { get; private set; }

        readonly int _sampleRate;
        readonly AudioSourceRead _audioSourceRead;

        /// <summary>
        /// 
        /// </summary>
        public AudioStreamTrack() : this(WebRTC.Context.CreateAudioTrack(Guid.NewGuid().ToString()))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public AudioStreamTrack(AudioSource source) : this()
        {
            if (source == null)
                throw new ArgumentNullException("AudioSource argument is null");
            if (source.clip == null)
                throw new ArgumentException("AudioClip must to be attached on AudioSource");
            Source = source;

            _audioSourceRead = source.gameObject.AddComponent<AudioSourceRead>();
            _audioSourceRead.hideFlags = HideFlags.HideInHierarchy;
            _audioSourceRead.onAudioRead += OnAudioRead;
            _sampleRate = Source.clip.frequency;
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
                tracks.Remove(this);
                if(_audioSourceRead != null)
                    Object.Destroy(_audioSourceRead);
                WebRTC.Context.DeleteMediaStreamTrack(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        internal AudioStreamTrack(IntPtr ptr) : base(ptr)
        {
            tracks.Add(this);
        }

        internal void OnAudioRead(float[] data, int channels)
        {
            NativeMethods.ProcessAudio(self, data, _sampleRate, channels, data.Length);
        }
    }
}
