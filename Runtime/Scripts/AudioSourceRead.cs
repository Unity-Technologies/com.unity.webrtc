using UnityEngine;
using Unity.Collections;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="data"></param>
    /// <param name="channels"></param>
    delegate void AudioReadEventHandler(ref NativeSlice<float> data, int channels, int sampleRate);

    /// <summary>
    ///
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    internal class AudioSourceRead : MonoBehaviour
    {
        public event AudioReadEventHandler onAudioRead;

        private int sampleRate;
        private int channels;
        private int prevTimeSamples;
        private NativeArray<float> nativeArray;

        private AudioClip clip;
        private AudioSource source;

        void OnEnable()
        {
            source = GetComponent<AudioSource>();
            clip = source.clip;
            channels = clip.channels;
            sampleRate = clip.frequency;
            nativeArray = new NativeArray<float>(
                clip.channels * clip.samples, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        private void OnDestroy()
        {
            nativeArray.Dispose();
        }

        void OnAudioRead(int current, int prev)
        {
            var length = current - prev;
            var data = new float[length * channels];
            clip.GetData(data, prev);
            NativeArray<float>.Copy(data, 0, nativeArray, prev, data.Length);
            var slice = new NativeSlice<float>(nativeArray, prev, data.Length);
            onAudioRead?.Invoke(ref slice, channels, sampleRate);
        }

        void Update()
        {
            var timeSamples= source.timeSamples;
            if (timeSamples == prevTimeSamples)
                return;

            if (timeSamples < prevTimeSamples)
            {
                OnAudioRead(clip.samples, prevTimeSamples);
                prevTimeSamples = 0;
            }
            if (timeSamples == prevTimeSamples)
                return;
            
            OnAudioRead(timeSamples, prevTimeSamples);
            prevTimeSamples = timeSamples;
        }
    }
}
