using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
        public AudioClip Renderer
        {
            get { return _streamRenderer.clip; }
        }


        internal class AudioStreamRenderer : IDisposable
        {
            private AudioClip m_clip;
            private int m_sampleRate;
            private int m_position = 0;
            private int m_channel = 0;

            public AudioClip clip
            {
                get
                {
                    return m_clip;
                }
            }

            public AudioStreamRenderer(string name, int sampleRate, int channels)
            {
                m_sampleRate = sampleRate;
                m_channel = channels;
                int lengthSamples = m_sampleRate;  // sample length for a second

                // note:: OnSendAudio and OnAudioSetPosition callback is called before complete the constructor.
                m_clip = AudioClip.Create(name, lengthSamples, channels, m_sampleRate, false);
            }

            public void Dispose()
            {
                if(m_clip != null)
                    Object.Destroy(m_clip);
                m_clip = null;
            }

            internal void SetData(float[] data)
            {
                int length = data.Length / m_channel;

                if (m_position + length > m_clip.samples)
                {
                    int remain = m_position + length - m_clip.samples;
                    length = m_clip.samples - m_position;

                    // Split two arrays from original data
                    float[] _data = new float[length * m_channel];
                    Buffer.BlockCopy(data, 0, _data, 0, length * m_channel);
                    float[] _data2 = new float[remain * m_channel];
                    Buffer.BlockCopy(data, length * m_channel, _data2, 0, remain * m_channel);

                    // push the split array to the audio buffer
                    SetData(_data);

                    data = _data2;
                    length = remain;
                }
                m_clip.SetData(data, m_position);
                m_position += length;

                if (m_position == m_clip.samples)
                {
                    m_position = 0;
                }
            }
        }

        readonly AudioSourceRead _audioSourceRead;

        private AudioStreamRenderer _streamRenderer;

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
            _audioSourceRead.onAudioRead += SetData;
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
                if(_audioSourceRead != null)
                    Object.Destroy(_audioSourceRead);
                _streamRenderer?.Dispose();
                WebRTC.Context.AudioTrackUnregisterAudioReceiveCallback(self);
                WebRTC.Context.DeleteMediaStreamTrack(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

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
                NativeMethods.ProcessAudio(self, (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
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
                NativeMethods.ProcessAudio(self, (IntPtr)ptr, sampleRate, channels, nativeSlice.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="channels"></param>
        public void SetData(float[] array, int channels, int sampleRate)
        {
            NativeArray<float> nativeArray = new NativeArray<float>(array, Allocator.Temp);
            var readonlyNativeArray = nativeArray.AsReadOnly();
            SetData(ref readonlyNativeArray, channels, sampleRate);
            nativeArray.Dispose();
        }

        private void OnAudioReceivedInternal(float[] audioData, int sampleRate, int channels, int numOfFrames)
        {
            if (_streamRenderer == null)
            {
                _streamRenderer = new AudioStreamRenderer(this.Id, sampleRate, channels);

                OnAudioReceived?.Invoke(_streamRenderer.clip);
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
