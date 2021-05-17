using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.WebRTC
{
    public delegate void OnAudioReceived(float[] data, int bitsPerSample, int sampleRate, int numOfChannels, int numOfFrames);


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


        public event OnAudioReceived OnAudioReceived;

        public AudioSource Source { get; private set; }

        float m_prevTime;
        int m_sampleRate = 0;
        int m_channels = 0;
        int m_bitPerSample = 16; // 44kHz (16bit)
        float[] m_microphoneBuffer = new float[1440]; // 480 * 3 = 1440

        public AudioStreamTrack(AudioSource source) : this()
        {
            Source = source;
            m_sampleRate = Source.clip.frequency;
            m_channels = Source.clip.channels;
            m_prevTime = Time.time;
        }

        internal AudioStreamTrack(IntPtr ptr, bool receiver = false) : base(ptr)
        {
            tracks.Add(this);
            if (receiver)
            {
                WebRTC.Context.AudioTrackRegisterAudioReceiveCallback(self, OnAudioReceive);
            }
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

        internal void OnData()
        {
            float deltaTime = Time.time - m_prevTime;
            m_prevTime = Time.time;

            if (Source == null || !Source.isPlaying)
                return;

            Source.GetOutputData(m_microphoneBuffer, m_channels);

            //int frameLength = (int)(m_sampleRate * deltaTime);
            //NativeMethods.ProcessAudio(self, m_microphoneBuffer, m_bitPerSample, m_sampleRate, m_channels, frameLength);

            NativeMethods.ProcessAudioADM(m_microphoneBuffer, m_microphoneBuffer.Length);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateAudioReceive))]
        static void OnAudioReceive(
            IntPtr ptrTrack, float[] audioData, int size, int bitsPerSample, int sampleRate, int numOfChannels, int numOfFrames)
        {
            WebRTC.Sync(ptrTrack, () =>
            {
                if (WebRTC.Table[ptrTrack] is AudioStreamTrack track)
                {
                    track.OnAudioReceived?.Invoke(audioData, bitsPerSample, sampleRate, numOfChannels, numOfFrames);
                }
            });
        }
    }
}
