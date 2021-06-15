using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.WebRTC
{
    public delegate void OnAudioReceived(AudioClip renderer);
    //public delegate void OnAudioReceived(float[] data, int bitsPerSample, int sampleRate, int numOfChannels, int numOfFrames);

    public class AudioStreamTrack : MediaStreamTrack
    {
        internal class AudioStreamRenderer
        {
            private AudioClip m_clip;
            private int m_frequency;
            private int m_sampleRate;
            private int m_position = 0;
            private AudioStreamTrack m_track;

            public AudioClip clip
            {
                get
                {
                    return m_clip;
                }
            }

            public AudioStreamRenderer(AudioStreamTrack track, int sampleRate, int channels, int frequency)
            {
                m_frequency = frequency;
                m_sampleRate = sampleRate;
                m_track = track;

                // note:: OnAudioRead and OnAudioSetPosition callback is called before complete the constructor.
                m_clip = AudioClip.Create(track.Id, m_sampleRate, channels, m_frequency, true, OnAudioRead, OnAudioSetPosition);
            }

            void OnAudioRead(float[] data)
            {
                // todo(kazuki):: change position
                WebRTC.Context.AudioTrackReadAudioData(m_track.self, data);
                m_position = data.Length;
            }

            void OnAudioSetPosition(int newPosition)
            {
                m_position = newPosition;
            }
        }

        internal static List<AudioStreamTrack> tracks = new List<AudioStreamTrack>();


        public event OnAudioReceived OnAudioReceived;

        public AudioSource Source { get; private set; }

        public AudioClip Renderer { get; private set; }

        readonly int _sampleRate = 0;
        readonly int _channels = 0;
        readonly float[] _microphoneBuffer = new float[1440]; // 480 * 3 = 1440

        private AudioStreamRenderer _mStreamRenderer;

        public AudioStreamTrack() : this(WebRTC.Context.CreateAudioTrack(Guid.NewGuid().ToString()))
        {
        }

        public AudioStreamTrack(AudioSource source) : this()
        {
            if (source == null)
                throw new ArgumentNullException("AudioSource argument is null");
            if (source.clip == null)
                throw new ArgumentException("AudioClip must to be attached on AudioSource");
            Source = source;
            _sampleRate = Source.clip.frequency;
            _channels = Source.clip.channels;
        }

        internal AudioStreamTrack(IntPtr ptr) : base(ptr)
        {
            tracks.Add(this);
            WebRTC.Context.AudioTrackRegisterAudioReceiveCallback(self, OnAudioReceive);
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                tracks.Remove(this);
                WebRTC.Context.AudioTrackUnregisterAudioReceiveCallback(self);
                WebRTC.Context.DeleteMediaStreamTrack(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        // Called each every frame from WebRTC.Update
        internal void OnData()
        {
            if (Source == null || !Source.isPlaying)
                return;

            Source.GetOutputData(_microphoneBuffer, _channels);

            NativeMethods.ProcessAudio(self, _microphoneBuffer, _sampleRate, _channels, _microphoneBuffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audioData"></param>
        /// <param name="bitsPerSample">default=16</param>
        /// <param name="sampleRate">default=48000</param>
        /// <param name="channels">default=1</param>
        /// <param name="numOfFrames">default=480</param>
        internal void OnAudioReceivedInternal(float[] audioData, int bitsPerSample, int sampleRate, int channels, int numOfFrames)
        {
            if (Renderer == null)
            {
                _mStreamRenderer = new AudioStreamRenderer(this, sampleRate, channels,sampleRate);
                Renderer = _mStreamRenderer.clip;

                OnAudioReceived?.Invoke(Renderer);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateAudioReceive))]
        static void OnAudioReceive(
            IntPtr ptrTrack, float[] audioData, int size, int bitsPerSample, int sampleRate, int numOfChannels, int numOfFrames)
        {
            WebRTC.Sync(ptrTrack, () =>
            {
                if (WebRTC.Table[ptrTrack] is AudioStreamTrack track)
                {
                    track.OnAudioReceivedInternal(audioData, bitsPerSample, sampleRate, numOfChannels, numOfFrames);
                }
            });
        }
    }
}
