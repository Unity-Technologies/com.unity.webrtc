using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.WebRTC
{
    public static class AudioSourceExtension
    {
        public static void SetTrack(this AudioSource source, AudioStreamTrack track)
        {
            if(track.Renderer != null)
            {
                throw new InvalidOperationException(
                    $"AudioStreamTrack already has AudioSource {track.Renderer.name}.");
            }
            track.SetAudioSource(source);
        }
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="renderer"></param>
    public delegate void OnAudioReceived(AudioSource renderer);

    /// <summary>
    ///
    /// </summary>
    public class AudioStreamTrack : MediaStreamTrack
    {
        /// <summary>
        ///
        /// </summary>
        public event OnAudioReceived OnAudioReceived;

        /// <summary>
        ///
        /// </summary>
        public AudioSource Source { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public AudioSource Renderer { get; private set; }


        internal class AudioBufferTracker
        {
            public const int NumOfFramesForBuffering = 5;
            public long BufferPosition { get; set; }
            public int SamplesPer10ms { get { return m_samplesPer10ms; } }

            private readonly int m_sampleLength;
            private readonly int m_samplesPer10ms;
            private readonly int m_samplesForBuffering;
            private long m_renderPos;
            private int m_prevTimeSamples;

            public AudioBufferTracker(int sampleLength)
            {
                m_sampleLength = sampleLength;
                m_samplesPer10ms = m_sampleLength / 100;
                m_samplesForBuffering = m_samplesPer10ms * NumOfFramesForBuffering;
            }

            public void Initialize(AudioSource source)
            {
                var timeSamples = source.timeSamples;
                m_prevTimeSamples = timeSamples;
                m_renderPos = timeSamples;
                BufferPosition = timeSamples;
            }

            public int CheckNeedCorrection(AudioSource source)
            {
                if (source != null && m_prevTimeSamples != source.timeSamples)
                {
                    var timeSamples = source.timeSamples;
                    m_renderPos += (timeSamples < m_prevTimeSamples ? m_sampleLength : 0) + timeSamples - m_prevTimeSamples;
                    m_prevTimeSamples = timeSamples;

                    if (m_renderPos >= BufferPosition)
                    {
                        return (int)(m_renderPos - BufferPosition) + m_samplesForBuffering;
                    }
                    else if (BufferPosition - m_renderPos <= m_samplesPer10ms)
                    {
                        return (int)(m_renderPos + m_samplesForBuffering - BufferPosition);
                    }
                }

                return 0;
            }
        }


        internal class AudioStreamRenderer : IDisposable
        {
            private bool m_bufferReady = false;
            private readonly Queue<float[]> m_recvBufs = new Queue<float[]>();
            private readonly AudioBufferTracker m_bufInfo;
            private AudioSource m_audioSource;

            public AudioSource source
            {
                get
                {
                    return m_audioSource;
                }
            }

            public AudioStreamRenderer(AudioSource source, int sampleRate, int channels)
            {
                if(source == null)
                    throw new ArgumentNullException("AudioSource argument is null");

                m_audioSource = source;
                int lengthSamples = sampleRate;  // sample length for 1 second
                string clipName = $"{source.name}-{GetHashCode():x}";
                m_audioSource.clip =
                    AudioClip.Create(clipName, lengthSamples, channels, sampleRate, false);
                m_bufInfo = new AudioBufferTracker(m_audioSource.clip.frequency);
            }

            public void Dispose()
            {
                if (m_audioSource != null)
                {
                    WebRTC.DestroyOnMainThread(m_audioSource.clip);
                }
                m_recvBufs.Clear();
            }

            internal void WriteToAudioClip(int numOfFrames = 1)
            {
                var clip = m_audioSource.clip;
                int baseOffset = (int)(m_bufInfo.BufferPosition % clip.samples);
                int writtenSamples = 0;

                while (numOfFrames-- > 0)
                {
                    writtenSamples += WriteBuffer(
                        m_recvBufs.Count > 0 ? m_recvBufs.Dequeue() : new float[m_bufInfo.SamplesPer10ms * clip.channels],
                        baseOffset + writtenSamples);
                }

                m_bufInfo.BufferPosition += writtenSamples;

                int WriteBuffer(float[] data, int offset)
                {
                    clip.SetData(data, offset % clip.samples);
                    return data.Length / clip.channels;
                }
            }

            internal void SetData(float[] data)
            {
                m_recvBufs.Enqueue(data);

                if (m_recvBufs.Count >= AudioBufferTracker.NumOfFramesForBuffering && !m_bufferReady)
                {
                    m_bufInfo.Initialize(m_audioSource);
                    WriteToAudioClip(AudioBufferTracker.NumOfFramesForBuffering - 1);
                    m_bufferReady = true;
                }

                if (m_bufferReady)
                {
                    int correctSize = m_bufInfo.CheckNeedCorrection(m_audioSource);
                    if (correctSize > 0)
                    {
                        WriteToAudioClip(correctSize / m_bufInfo.SamplesPer10ms +
                            ((correctSize % m_bufInfo.SamplesPer10ms) > 0 ? 1 : 0));
                    }
                    else
                    {
                        WriteToAudioClip();
                    }
                }
            }
        }

        
        /// <summary>
        /// The channel count of streaming receiving audio is changing at the first few frames.
        /// So This count is for ignoring the unstable audio frames
        /// </summary>
        const int MaxFrameCountReceiveDataForIgnoring = 5;

        readonly AudioSourceRead _audioSourceRead;
        AudioStreamRenderer _streamRenderer;
        AudioTrackSource _source;

        int frameCountReceiveDataForIgnoring = 0; 

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        public AudioStreamTrack(AudioSource source)
            : this(Guid.NewGuid().ToString(), new AudioTrackSource())
        {
            if (source == null)
                throw new ArgumentNullException("AudioSource argument is null.");
            if (source.clip == null)
                throw new ArgumentException("AudioClip must to be attached on AudioSource.");
            Source = source;

            // initialize audio streaming for sender
            WebRTC.Context.InitLocalAudio(self, source.clip.frequency, source.clip.channels);
            _audioSourceRead = source.gameObject.AddComponent<AudioSourceRead>();
            _audioSourceRead.hideFlags = HideFlags.HideInHierarchy;
            _audioSourceRead.onAudioRead += SetData;
        }

        internal AudioStreamTrack(string label, AudioTrackSource source)
            : this(WebRTC.Context.CreateAudioTrack(label, source.self))
        {
            _source = source;
        }

        internal AudioStreamTrack(IntPtr ptr) : base(ptr)
        {
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
                if (_audioSourceRead != null)
                {
                    // Unity API must be called from main thread.
                    _audioSourceRead.onAudioRead -= SetData;
                    WebRTC.DestroyOnMainThread(_audioSourceRead);
                    WebRTC.Context.UninitLocalAudio(self);
                }
                _streamRenderer?.Dispose();
                _source?.Dispose();
                WebRTC.Context.AudioTrackUnregisterAudioReceiveCallback(self);
            }
            base.Dispose();
        }

#if UNITY_2020_1_OR_NEWER
        /// <summary>
        ///
        /// </summary>
        /// <param name="nativeArray"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        public void SetData(ref NativeArray<float>.ReadOnly nativeArray, int channels, int sampleRate)
        {
            unsafe
            {
                void* ptr = nativeArray.GetUnsafeReadOnlyPtr();
                ProcessAudio(GetSelfOrThrow(), (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
            }
        }
#endif

        /// <summary>
        ///
        /// </summary>
        /// <param name="nativeArray"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        public void SetData(ref NativeArray<float> nativeArray, int channels, int sampleRate)
        {
            unsafe
            {
                void* ptr = nativeArray.GetUnsafeReadOnlyPtr();
                ProcessAudio(GetSelfOrThrow(), (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="nativeSlice"></param>
        /// <param name="channels"></param>
        public void SetData(ref NativeSlice<float> nativeSlice, int channels, int sampleRate)
        {
            unsafe
            {
                void* ptr = nativeSlice.GetUnsafeReadOnlyPtr();
                ProcessAudio(GetSelfOrThrow(), (IntPtr)ptr, sampleRate, channels, nativeSlice.Length);
            }
        }

        static void ProcessAudio(IntPtr track, IntPtr array, int sampleRate, int channels, int frames)
        {
            if (sampleRate == 0 || channels == 0 || frames == 0)
                throw new ArgumentException($"arguments are invalid values " +
                    $"sampleRate={sampleRate}, " +
                    $"channels={channels}, " +
                    $"frames={frames}");
            WebRTC.Context.ProcessLocalAudio(track, array, sampleRate, channels, frames);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="array"></param>
        /// <param name="channels"></param>
        public void SetData(float[] array, int channels, int sampleRate)
        {
            if (array == null)
                throw new ArgumentNullException("array is null");
            NativeArray<float> nativeArray = new NativeArray<float>(array, Allocator.Temp);
            SetData(ref nativeArray, channels, sampleRate);
            nativeArray.Dispose();
        }

        internal void SetAudioSource(AudioSource renderer)
        {
            Renderer = renderer;
        }

        private void OnAudioReceivedInternal(float[] audioData, int sampleRate, int channels, int numOfFrames)
        {
            if (Renderer == null)
                return;

            if (_streamRenderer == null)
            {
                if(frameCountReceiveDataForIgnoring < MaxFrameCountReceiveDataForIgnoring)
                {
                    frameCountReceiveDataForIgnoring++;
                    return;
                }
                _streamRenderer = new AudioStreamRenderer(Renderer, sampleRate, channels);
                OnAudioReceived?.Invoke(Renderer);
            }
            _streamRenderer?.SetData(audioData);
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
    internal class AudioTrackSource : RefCountedObject
    {
        public AudioTrackSource() : base(WebRTC.Context.CreateAudioTrackSource())
        {
            WebRTC.Table.Add(self, this);
        }

        ~AudioTrackSource()
        {
            this.Dispose();
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }
    }
}
