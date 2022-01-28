using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    /// This event is called on audio thread.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="channels"></param>
    delegate void AudioReadEventHandler(float[] data, int channels, int sampleRate);

    /// <summary>
    ///
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    internal class AudioCustomFilter : MonoBehaviour
    {
        public event AudioReadEventHandler onAudioRead;
        private int m_sampleRate;

        void OnEnable()
        {
            OnAudioConfigurationChanged(false);
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
        }

        void OnDisable()
        {
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
        }

        void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            m_sampleRate = AudioSettings.outputSampleRate;
        }

        /// <summary>
        /// </summary>
        /// <note>
        /// Call on the audio thread, not main thread.
        /// </note>
        /// <param name="data"></param>
        /// <param name="channels"></param>
        void OnAudioFilterRead(float[] data, int channels)
        {
            onAudioRead?.Invoke(data, channels, m_sampleRate);
        }
    }
}
