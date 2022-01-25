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
            track._streamRenderer.Source = source;
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
                _streamRenderer.OnAudioReceived += value;
            }
            remove
            {
                _streamRenderer.OnAudioReceived -= value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public AudioSource Source { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public AudioSource Renderer => _streamRenderer.Source;

        internal class AudioStreamRenderer : IDisposable
        {
            public event OnAudioReceived OnAudioReceived;
            private bool disposed;

            internal IntPtr self;

            private AudioSource m_audioSource;
            private AudioRendererFilter m_filter;
            private bool invokedReceivedAtOnce = false;

            private int m_sampleRate;
            private int m_channels;


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
                return m_sampleRate == sampleRate &&
                    m_channels == channels;
            }

            private static T GetOrAddComponent<T>(GameObject go) where T : Component
            {
                T comp = go.GetComponent<T>();
                if (!comp)
                    comp = go.AddComponent<T>();
                return comp;
            }

            private void UpdateParams(int sampleRate, int channels)
            {
                m_sampleRate = sampleRate;
                m_channels = channels;

                if(m_filter == null)
                {
                    m_filter = GetOrAddComponent<AudioRendererFilter>(m_audioSource.gameObject);
                    m_filter.hideFlags = HideFlags.HideInHierarchy;
                }
                m_filter.AllocateBuffer(sampleRate, channels);
            }

            public AudioStreamRenderer(int sampleRate, int channel)
                : this(WebRTC.Context.CreateAudioTrackSink(OnAudioReceive, sampleRate, channel))
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
                this.disposed = true;
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// </summary>
            /// <note>
            /// This method is called on worker thread, not main thread.
            /// So almost Unity APIs are not able to use.
            /// </note>
            /// <param name="data"></param>
            internal void SetData(float[] data)
            {
                m_filter?.SetData(data);
            }

            private void OnAudioReceivedInternal(
                float[] audioData, int sampleRate, int channels, int numOfFrames)
            {
                if (Source == null)
                    return;

                if (!IsSameParams(sampleRate, channels))
                    UpdateParams(sampleRate, channels);

                // delegate
                if (!invokedReceivedAtOnce)
                {
                    OnAudioReceived?.Invoke(Source);
                    invokedReceivedAtOnce = true;
                }
            }

            [AOT.MonoPInvokeCallback(typeof(DelegateAudioReceive))]
            static void OnAudioReceive(
                IntPtr ptr, float[] audioData, int size,
                int sampleRate, int numOfChannels, int numOfFrames)
            {
                // Call SetData method from worker thread.
                if (WebRTC.Table[ptr] is AudioStreamRenderer receiver)
                    receiver.SetData(audioData);

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

        readonly AudioCapturerFilter _audioCapturer;
        internal AudioStreamRenderer _streamRenderer;
        internal AudioTrackSource _source;

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

            _audioCapturer = source.gameObject.AddComponent<AudioCapturerFilter>();
            _audioCapturer.hideFlags = HideFlags.HideInHierarchy;
            _audioCapturer.onAudioRead += SetData;
        }

        internal AudioStreamTrack(string label, AudioTrackSource source)
            : base(WebRTC.Context.CreateAudioTrack(label, source.self))
        {
            _source = source;
        }

        internal AudioStreamTrack(IntPtr ptr) : base(ptr)
        {
            var sampleRate = AudioSettings.outputSampleRate;
            var speakerMode = AudioSettings.GetConfiguration().speakerMode;
            var channels = AudioSettingsUtility.SpeakerModeToChannel(speakerMode);
            _streamRenderer = new AudioStreamRenderer(sampleRate, channels);
            AddSink(_streamRenderer);
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
                if (_audioCapturer != null)
                {
                    // Unity API must be called from main thread.
                    _audioCapturer.onAudioRead -= SetData;
                    WebRTC.DestroyOnMainThread(_audioCapturer);
                }
                if(_streamRenderer != null)
                {
                    RemoveSink(_streamRenderer);
                    _streamRenderer?.Dispose();
                    _streamRenderer = null;
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
