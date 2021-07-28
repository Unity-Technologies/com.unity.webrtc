using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="data"></param>
    /// <param name="channels"></param>
    delegate void AudioReadEventHandler(float[] data, int channels, int sampleRate);

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

        private AudioClip clip;
        private AudioSource source;

        void OnEnable()
        {
            source = GetComponent<AudioSource>();
            clip = source.clip;
            channels = clip.channels;
            sampleRate = clip.frequency;

            Debug.Log("channels:" + channels);
            Debug.Log("sampleRate:" + sampleRate);

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
                onAudioRead?.Invoke(data, channels, sampleRate);
                prevTimeSamples = 0;
            }

            if (timeSamples == prevTimeSamples)
                return;

            {
                var length = timeSamples - prevTimeSamples;
                var data = new float[length * channels];
                clip.GetData(data, prevTimeSamples);
                onAudioRead?.Invoke(data, channels, sampleRate);
                prevTimeSamples = timeSamples;
            }
    }
        // void OnAudioFilterRead(float[] data, int channels)
        // {
        //     onAudioRead?.Invoke(data, channels, sampleRate);
        // }
    }
}
