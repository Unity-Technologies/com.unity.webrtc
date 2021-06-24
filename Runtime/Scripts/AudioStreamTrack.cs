using System;
using UnityEngine;

namespace Unity.WebRTC
{
    public class AudioStreamTrack : MediaStreamTrack
    {

        UnityAudioFrameObserver m_observer;

        public AudioStreamTrack(string label) : base(WebRTC.Context.CreateAudioTrack(label))
        {
        }

        public AudioStreamTrack(IntPtr sourceTrack) : base(sourceTrack)
        {
        }

        public bool IsObserverInitialized
        {
            get
            {
                return m_observer != null && m_observer.self != IntPtr.Zero;
            }
        }

        public void InitializeFrameObserver()
        {
            if (IsObserverInitialized)
                throw new InvalidOperationException("Already initialized frame observer");

            m_observer = new UnityAudioFrameObserver(WebRTC.Context.CreateAudioFrameObserver(), this);
        }

        public void SetFrameObserverCallback(DelegateOnFrameReady onFrameReady)
        {
            m_observer.OnFrameReady = onFrameReady;
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                if (IsObserverInitialized)
                {
                    m_observer.Dispose();
                }

                WebRTC.Context.DeleteMediaStreamTrack(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    public static class Audio
    {
        private static bool started;

        public static MediaStream CaptureStream(string streamlabel = "audiostream", string label = "audio")
        {
            started = true;

            var stream = new MediaStream(WebRTC.Context.CreateMediaStream(streamlabel));
            var track = new AudioStreamTrack(WebRTC.Context.CreateAudioTrack(label));
            stream.AddTrack(track);
            return stream;
        }

        public static void Update(float[] audioData, int channels)
        {
            if (started)
            {
                NativeMethods.ProcessAudio(audioData, audioData.Length);
            }
        }

        public static void Stop()
        {
            if (started)
            {
                started = false;
            }
        }
    }

    public struct AudioFrame
    {
        /// <summary>
        /// Buffer of audio samples for all channels.
        /// </summary>
        public IntPtr audioData;

        /// <summary>
        /// Number of bits per sample, generally 8 or 16.
        /// </summary>
        public uint bitsPerSample;

        /// <summary>
        /// Sample rate, in Hz. Generally in the range 8-48 kHz.
        /// </summary>
        public uint sampleRate;

        /// <summary>
        /// Number of audio channels.
        /// </summary>
        public uint channelCount;

        /// <summary>
        /// Number of consecutive samples in the audio data buffer.
        /// WebRTC generally delivers frames in 10ms chunks, so for e.g. a 16 kHz
        /// sample rate the sample count would be 1000.
        /// </summary>
        public uint sampleCount;
    }

    public delegate void DelegateOnFrameReady(AudioFrame frame);
    internal class UnityAudioFrameObserver : IDisposable
    {
        internal IntPtr self;
        private AudioStreamTrack track;
        internal uint id => NativeMethods.GetAudioFrameObserverId(self);
        private bool disposed;

        private DelegateOnFrameReady _onFrameReady;

        public UnityAudioFrameObserver(IntPtr ptr, AudioStreamTrack track)
        {
            self = ptr;
            this.track = track;
            NativeMethods.AudioTrackAddSink(track.GetSelfOrThrow(), self);
            WebRTC.Table.Add(self, this);
            WebRTC.Context.AudioFrameObserverRegisterOnFrameReady(self, AudioFrameObserverOnFrameReady);
        }

        ~UnityAudioFrameObserver()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero)
            {
                IntPtr trackPtr = track.GetSelfOrThrow();
                if (trackPtr != IntPtr.Zero)
                {
                    NativeMethods.AudioTrackRemoveSink(trackPtr, self);
                }

                WebRTC.Context.DeleteAudioFrameObserver(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public DelegateOnFrameReady OnFrameReady
        {
            get => _onFrameReady;
            set
            {
                _onFrameReady = value;
            }
        }


        [AOT.MonoPInvokeCallback(typeof(DelegateNativeAudioFrameObserverOnFrameReady))]
        static void AudioFrameObserverOnFrameReady(IntPtr ptr, AudioFrame frame)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is UnityAudioFrameObserver observer)
                {
                    observer._onFrameReady?.Invoke(frame);
                }
            });
        }
    }
}
