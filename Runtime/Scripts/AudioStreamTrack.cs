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

            private float[] m_buffer;
            private int m_bufferLength;
            private int m_bufferPosition;
            private int m_bufferResetCycle;
            private int m_bufferingFrame;

            public AudioStreamRenderer(string name, int sampleRate, int channels)
            {
                m_sampleRate = sampleRate;
                m_channel = channels;
                int lengthSamples = m_sampleRate;  // sample length for a second

                m_bufferLength = sampleRate * channels;
                m_buffer = new float[m_bufferLength];
                m_bufferResetCycle = m_bufferLength * 5;  // reset buffer every 5 seconds
                m_bufferingFrame = m_sampleRate / 1000;  // set initial buffering frames

                // note:: OnSendAudio and OnAudioSetPosition callback is called before complete the constructor.
                m_clip = AudioClip.Create(name, lengthSamples, channels, m_sampleRate, true, OnReadBuffer);
            }

            internal void OnReadBuffer(float[] data)
            {
                int dataLength = data.Length;

                if (m_bufferingFrame > 0)
                {
                    m_bufferingFrame -= 1;

                    // Sounds silent while buffering
                    Array.Clear(data, 0, data.Length);
                }
                else if (m_bufferPosition > m_position + dataLength)
                {
                    int srcOffset = m_position % m_bufferLength;
                    int destOffset = 0;
                    int count = data.Length;

                    if (srcOffset + dataLength > m_bufferLength)
                    {
                        count = m_bufferLength - srcOffset;

                        Array.Copy(m_buffer, srcOffset, data, destOffset, count);

                        srcOffset = 0;
                        destOffset += count;
                        count = dataLength - count;
                    }

                    Array.Copy(m_buffer, srcOffset, data, destOffset, count);

                    m_position += dataLength;

                    if (m_position == m_bufferResetCycle)
                    {
                        int endPosition = m_bufferPosition;
                        int startPosition = endPosition - m_position;
                        int remainLength = endPosition - startPosition;

                        float[] temp = new float[remainLength];
                        Array.Copy(m_buffer, startPosition, temp, 0, remainLength);
                        Array.Copy(temp, m_buffer, remainLength);

                        m_position = 0;
                        m_bufferPosition = remainLength;
                    }
                }
                else
                {
                    // Set waiting frames for buffering
                    m_bufferingFrame += m_sampleRate / 1000; // if 48Khz, then 48 frames

                    // Sounds silent while buffering
                    Array.Clear(data, 0, data.Length);
                }
            }

            public void Dispose()
            {
                if(m_clip != null)
                    Object.Destroy(m_clip);
                m_clip = null;
            }

            internal void SetData(float[] data)
            {
                int dataLength = data.Length;
                int srcOffset = 0;
                int destOffset = m_bufferPosition % m_bufferLength;
                int count = data.Length;

                if (destOffset + dataLength > m_bufferLength)
                {
                    count = m_bufferLength - destOffset;

                    Array.Copy(data, srcOffset, m_buffer, destOffset, count);

                    srcOffset += count;
                    destOffset = 0;

                    count = dataLength - count;
                }

                Array.Copy(data, srcOffset, m_buffer, destOffset, count);

                m_bufferPosition += dataLength;
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
