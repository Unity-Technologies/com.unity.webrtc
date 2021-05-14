using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.WebRTC
{
    public class AudioStreamTrack : MediaStreamTrack
    {
        public AudioStreamTrack() : this(WebRTC.Context.CreateAudioTrack(Guid.NewGuid().ToString()))
        {
        }

        internal AudioStreamTrack(IntPtr sourceTrack) : base(sourceTrack)
        {
            tracks.Add(this);
        }

        internal static List<AudioStreamTrack> tracks = new List<AudioStreamTrack>();

        internal AudioSource m_source;

        float m_prevTime;
        int m_sampleRate = 0;
        int m_channels = 0;
        int m_bitPerSample = 16; // 44kHz (16bit)
        float[] m_microphoneBuffer = new float[1024];

        public AudioStreamTrack(AudioSource source) : this()
        {
            m_source = source;
            m_sampleRate = m_source.clip.frequency;
            m_channels = m_source.clip.channels;
            m_prevTime = Time.time;
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
                WebRTC.Context.DeleteMediaStreamTrack(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }
        }

        internal void OnData()
        {
            float deltaTime = Time.time - m_prevTime;
            m_prevTime = Time.time;

            if (m_source == null || !m_source.isPlaying)
                return;

            m_source.GetOutputData(m_microphoneBuffer, m_channels);

            int frameLength = (int)(m_sampleRate * deltaTime);
            NativeMethods.ProcessAudio(self, m_microphoneBuffer, m_bitPerSample, m_sampleRate, m_channels, frameLength);
        }
    }
}
