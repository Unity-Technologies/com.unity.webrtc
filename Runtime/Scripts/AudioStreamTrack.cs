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
    public class AudioStreamTrack : MediaStreamTrack
    {
        /// <summary>
        ///
        /// </summary>
        public AudioSource Source
        {
            get
            {
                if (_source != null)
                    return _source;
                return _streamRenderer.Source;

            }
        }

        private AudioSource _source;

        internal class AudioStreamRenderer : IDisposable
        {
            private bool disposed;

            internal IntPtr self;

            private AudioSource _audioSource;
            private AudioCustomFilter _filter;
            private AudioStreamTrack _track;


            public AudioSource Source 
            {
                get
                {
                    return _audioSource;
                }
                set
                {
                    _audioSource = value;
                    AddFilter(_audioSource);
                }
            }


            private static T GetOrAddComponent<T>(GameObject go) where T : Component
            {
                T comp = go.GetComponent<T>();
                if (!comp)
                    comp = go.AddComponent<T>();
                return comp;
            }

            private void AddFilter(AudioSource source)
            {
                if (_filter != null)
                    return;
                _filter = GetOrAddComponent<AudioCustomFilter>(source.gameObject);
                _filter.hideFlags = HideFlags.HideInInspector;
                _filter.onAudioRead += SetData;
                source.Play();
            }

            public AudioStreamRenderer(AudioStreamTrack track)
                : this(WebRTC.Context.CreateAudioTrackSink())
            {
                _track = track;
                _track?.AddSink(this);
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
                    _track?.RemoveSink(this);
                    WebRTC.Table.Remove(self);
                    WebRTC.Context.DeleteAudioTrackSink(self);
                }
                if (_filter != null)
                {
                    _filter.onAudioRead -= SetData;
                    WebRTC.DestroyOnMainThread(_filter);
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
            internal void SetData(float[] data, int channels, int sampleRate)
            {
                NativeMethods.AudioTrackSinkProcessAudio(self, data, data.Length, channels, sampleRate);
            }
        }

        readonly AudioCustomFilter _audioCapturer;
        internal AudioStreamRenderer _streamRenderer;
        internal AudioTrackSource _trackSource;

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
            _source = source;

            _audioCapturer = source.gameObject.AddComponent<AudioCustomFilter>();
            _audioCapturer.hideFlags = HideFlags.HideInInspector;
            _audioCapturer.onAudioRead += SetData;
        }

        internal AudioStreamTrack(string label, AudioTrackSource source)
            : base(WebRTC.Context.CreateAudioTrack(label, source.self))
        {
            _trackSource = source;
        }

        internal AudioStreamTrack(IntPtr ptr) : base(ptr)
        {
            _streamRenderer = new AudioStreamRenderer(this);
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
                    _streamRenderer?.Dispose();
                    _streamRenderer = null;
                }
                _trackSource?.Dispose();
                _trackSource = null;
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
                ProcessAudio(_trackSource, (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
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
                ProcessAudio(_trackSource, (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
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
                ProcessAudio(_trackSource, (IntPtr)ptr, sampleRate, channels, nativeSlice.Length);
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
