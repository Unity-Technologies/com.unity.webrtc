using System;
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

        void Update()
        {
            var timeSamples= source.timeSamples;
            if (timeSamples == prevTimeSamples)
                return;

            if (timeSamples < prevTimeSamples)
            {
                var length = clip.samples - prevTimeSamples;
                var data = new float[length * channels];
                clip.GetData(data, prevTimeSamples);
                NativeArray<float>.Copy(data, 0, nativeArray, prevTimeSamples, length);
                var slice = new NativeSlice<float>(nativeArray, prevTimeSamples, length);
                onAudioRead?.Invoke(ref slice, channels, sampleRate);
                prevTimeSamples = 0;
            }

            if (timeSamples == prevTimeSamples)
                return;

            {
                var length = timeSamples - prevTimeSamples;
                var data = new float[length * channels];
                clip.GetData(data, prevTimeSamples);
                NativeArray<float>.Copy(data, 0, nativeArray, prevTimeSamples, length);
                var slice = new NativeSlice<float>(nativeArray, prevTimeSamples, length);
                onAudioRead?.Invoke(ref slice, channels, sampleRate);
                prevTimeSamples = timeSamples;
            }
        }
    }
}
