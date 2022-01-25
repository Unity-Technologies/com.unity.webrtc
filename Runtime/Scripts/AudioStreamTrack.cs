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
            track.streamRenderer.Source = source;
        }
    }

    public static class AudioSettingsUtility
    {
        static Dictionary<AudioSpeakerMode, int> pairs =
            new Dictionary<AudioSpeakerMode, int>()
        {
            {AudioSpeakerMode.Mono, 1},
            {AudioSpeakerMode.Stereo, 2},
            {AudioSpeakerMode.Quad, 4},
            {AudioSpeakerMode.Surround, 5},
            {AudioSpeakerMode.Mode5point1, 6},
            {AudioSpeakerMode.Mode7point1, 8},
            {AudioSpeakerMode.Prologic, 2},
        };
        public static int SpeakerModeToChannel(AudioSpeakerMode mode)
        {
            return pairs[mode];
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
        public event OnAudioReceived OnAudioReceived
        {
            add
            {
                streamRenderer.OnAudioReceived += value;
            }
            remove
            {
                streamRenderer.OnAudioReceived -= value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public AudioSource Source { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public AudioSource Renderer => streamRenderer.Source;


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
            public event OnAudioReceived OnAudioReceived;
            private bool disposed;

            internal IntPtr self;

            private AudioSource m_audioSource;
            private bool invokedReceivedAtOnce = false;

            public AudioSource Source 
            {
                get
                {
                    return m_audioSource;
                }
                set
                {
                    m_audioSource = value;
                }
            }

            private bool IsSameParams(int sampleRate, int channels)
            {
                return Source.clip.samples == sampleRate &&
                    Source.clip.channels == channels;
            }

            private void UpdateParams(int sampleRate, int channels)
            {
                var isPlaying = m_audioSource.isPlaying;

                // Replace AudioClip for updating parameter
                AudioClip oldClip = m_audioSource.clip;
                UnityEngine.Object.DestroyImmediate(oldClip);
                m_audioSource.clip =
                    CreateClip($"{m_audioSource.name}-{GetHashCode():x}", sampleRate, channels);

                // Restart AudioSource
                if (isPlaying)
                    m_audioSource.Play();
            }

            static AudioClip CreateClip(string clipName, int sampleRate, int channels)
            {
                int lengthSamples = sampleRate;  // sample length for 1 second
                return AudioClip.Create(
                    clipName, lengthSamples, channels, sampleRate, false);
            }

            public AudioStreamRenderer()
                : this(WebRTC.Context.CreateAudioTrackSink(OnAudioReceive))
            {
            }

            public AudioStreamRenderer(IntPtr ptr)
            {
                self = ptr;
                WebRTC.Table.Add(self, this);
            }

            ~AudioStreamRenderer()
            {
                this.Dispose();
            }

            public void Dispose()
            {
                if (this.disposed)
                {
                    return;
                }

                if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
                {
                    WebRTC.Table.Remove(self);
                    WebRTC.Context.DeleteAudioTrackSink(self);
                }
                if (m_audioSource != null && m_audioSource.clip != null)
                {
                    WebRTC.DestroyOnMainThread(m_audioSource.clip);
                }
                m_recvBufs.Clear();
                this.disposed = true;
                GC.SuppressFinalize(this);
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

            private void OnAudioReceivedInternal(
                float[] audioData, int sampleRate, int channels, int numOfFrames)
            {
                if (Source == null)
                    return;

                // delegate
                if (!invokedReceivedAtOnce)
                {
                    OnAudioReceived?.Invoke(Source);
                    invokedReceivedAtOnce = true;
                }

                if (!IsSameParams(sampleRate, channels))
                    UpdateParams(sampleRate, channels);
                SetData(audioData);
            }

            [AOT.MonoPInvokeCallback(typeof(DelegateAudioReceive))]
            static void OnAudioReceive(
                IntPtr ptr, float[] audioData, int size,
                int sampleRate, int numOfChannels, int numOfFrames)
            {
                WebRTC.Sync(ptr, () =>
                {
                    if (WebRTC.Table[ptr] is AudioStreamRenderer receiver)
                    {
                        receiver.OnAudioReceivedInternal(
                            audioData, sampleRate, numOfChannels, numOfFrames);
                    }
                });
            }
        }

        readonly AudioSourceRead _audioSourceRead;
        internal AudioStreamRenderer streamRenderer;
        AudioTrackSource _source;

        /// <summary>
        ///
        /// </summary>
        public AudioStreamTrack()
            : this(Guid.NewGuid().ToString(), new AudioTrackSource())
        {
        }

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

            _audioSourceRead = source.gameObject.AddComponent<AudioSourceRead>();
            _audioSourceRead.hideFlags = HideFlags.HideInHierarchy;
            _audioSourceRead.onAudioRead += SetData;
        }

        internal AudioStreamTrack(string label, AudioTrackSource source)
            : base(WebRTC.Context.CreateAudioTrack(label, source.self))
        {
            _source = source;
        }

        internal AudioStreamTrack(IntPtr ptr) : base(ptr)
        {
            streamRenderer = new AudioStreamRenderer();
            AddSink(streamRenderer);
        }

        internal void AddSink(AudioStreamRenderer renderer)
        {
            NativeMethods.AudioTrackAddSink(
                GetSelfOrThrow(), renderer.self);
        }
        internal void RemoveSink(AudioStreamRenderer renderer)
        {
            NativeMethods.AudioTrackRemoveSink(
                GetSelfOrThrow(), renderer.self);
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
                }
                if(streamRenderer != null)
                {
                    RemoveSink(streamRenderer);
                    streamRenderer?.Dispose();
                    streamRenderer = null;
                }
                _source?.Dispose();
                _source = null;
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
                ProcessAudio(_source, (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
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
                ProcessAudio(_source, (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
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
                ProcessAudio(_source, (IntPtr)ptr, sampleRate, channels, nativeSlice.Length);
            }
        }

        static void ProcessAudio(AudioTrackSource source, IntPtr array, int sampleRate, int channels, int frames)
        {
            if (sampleRate == 0 || channels == 0 || frames == 0)
                throw new ArgumentException($"arguments are invalid values " +
                    $"sampleRate={sampleRate}, " +
                    $"channels={channels}, " +
                    $"frames={frames}");
            source.Update(array, sampleRate, channels, frames);
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
    }

    internal class AudioTrackSource : RefCountedObject
    {
        bool inited = false;

        public AudioTrackSource() : base(WebRTC.Context.CreateAudioTrackSource())
        {
            WebRTC.Table.Add(self, this);
        }

        ~AudioTrackSource()
        {
            this.Dispose();
        }

        public void Initialize(int sampleRate, int channels)
        {
            // initialize audio streaming for sender
            WebRTC.Context.AudioSourceInitLocalAudio(GetSelfOrThrow(), sampleRate, channels);
            inited = true;
        }

        public void Uninitialize()
        {
            WebRTC.Context.AudioSourceUninitLocalAudio(GetSelfOrThrow());
            inited = false;
        }

        public void Update(IntPtr array, int sampleRate, int channels, int frames)
        {
            if (!inited)
                Initialize(sampleRate, channels);
            NativeMethods.AudioSourceProcessLocalAudio(GetSelfOrThrow(), array, sampleRate, channels, frames);
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
            if (inited)
                Uninitialize();
            base.Dispose();
        }
    }
}
