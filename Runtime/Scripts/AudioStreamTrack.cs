using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="renderer"></param>
    public delegate void OnAudioReceived(AudioClip renderer);

    /// <summary>
    /// 
    /// </summary>
    public class AudioStreamTrack : MediaStreamTrack
    {
        internal class AudioStreamRenderer
        {
            private AudioClip m_clip;
            private int m_frequency;
            private int m_sampleRate;
            private int m_position = 0;
            private int m_channel = 0;
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
                m_channel = channels;

                // note:: OnSendAudio and OnAudioSetPosition callback is called before complete the constructor.
                // PCMRenderCallback is not worked 
                //m_clip = AudioClip.Create(track.Id, m_sampleRate, channels, m_frequency, true, OnAudioRead, OnAudioSetPosition);
                m_clip = AudioClip.Create(track.Id, m_sampleRate, channels, m_frequency, false);
            }

            internal void SetData(float[] data)
            {
                int length = data.Length / m_channel;

                if (m_position + length > m_clip.samples)
                {

                    int remain = m_position + length - m_clip.samples;
                    length = m_clip.samples - m_position;
                    float[] _data = new float[length * m_channel];
                    Buffer.BlockCopy(data, 0, _data, 0, length * m_channel);
                    float[] _data2 = new float[remain * m_channel];
                    Buffer.BlockCopy(data, length * m_channel, _data2, 0, remain * m_channel);
                    SetData(_data);

                    data = _data2;
                    length = remain;
                    m_position = 0;
                }
                m_clip.SetData(data, m_position);
                m_position += length;

                if (m_position == m_clip.samples)
                {
                    m_position = 0;
                }
            }


        }

        internal static List<AudioStreamTrack> tracks = new List<AudioStreamTrack>();

        public event OnAudioReceived OnAudioReceived;

        public AudioSource Source { get; private set; }

        public AudioClip Renderer { get; private set; }

        readonly int _sampleRate = 0;
        readonly AudioSourceRead _audioSourceRead;

        private AudioStreamRenderer _streamRenderer;

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
            _audioSourceRead.onAudioRead += OnSendAudio;
            _sampleRate = Source.clip.frequency;
        }

        internal AudioStreamTrack(IntPtr ptr) : base(ptr)
        {
            tracks.Add(this);
            WebRTC.Context.AudioTrackRegisterAudioReceiveCallback(self, OnAudioReceive);
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
                WebRTC.Context.AudioTrackUnregisterAudioReceiveCallback(self);
                WebRTC.Context.DeleteMediaStreamTrack(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        internal void OnSendAudio(float[] data, int channels)
        {
            NativeMethods.ProcessAudio(self, data, _sampleRate, channels, data.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audioData"></param>
        /// <param name="bitsPerSample">default=16</param>
        /// <param name="sampleRate">default=48000</param>
        /// <param name="channels">default=1</param>
        /// <param name="numOfFrames">default=480</param>
        internal void OnAudioReceivedInternal(float[] audioData, int sampleRate, int channels, int numOfFrames)
        {
            if (Renderer == null)
            {
                _streamRenderer = new AudioStreamRenderer(this, sampleRate, channels,sampleRate);
                Renderer = _streamRenderer.clip;

                OnAudioReceived?.Invoke(Renderer);
            }
            _streamRenderer.SetData(audioData);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateAudioReceive))]
        static void OnAudioReceive(
            IntPtr ptrTrack, float[] audioData, int size, int sampleRate, int numOfChannels, int numOfFrames)
        {
            WebRTC.Sync(ptrTrack, () =>
            {
                if (WebRTC.Table[ptrTrack] is AudioStreamTrack track)
                {
                    track.OnAudioReceivedInternal(audioData, sampleRate, numOfChannels, numOfFrames);
                }
            });
        }
    }
}
