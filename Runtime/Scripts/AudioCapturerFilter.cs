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
    internal class AudioCapturerFilter : MonoBehaviour
    {
        public event AudioReadEventHandler onAudioRead;
        private int sampleRate;

        void OnEnable()
        {
            sampleRate = AudioSettings.outputSampleRate;
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
            onAudioRead?.Invoke(data, channels, sampleRate);
        }
    }
}
