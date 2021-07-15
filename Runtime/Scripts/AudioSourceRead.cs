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

        void OnEnable()
        {
            sampleRate = GetComponent<AudioSource>().clip.frequency;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            onAudioRead?.Invoke(data, channels, sampleRate);
        }
    }
}
