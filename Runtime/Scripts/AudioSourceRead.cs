using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="data"></param>
    /// <param name="channels"></param>
    delegate void AudioReadEventHandler(float[] data, int channels);

    /// <summary>
    ///
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    internal class AudioSourceRead : MonoBehaviour
    {
        public event AudioReadEventHandler onAudioRead;

        /// <summary>
        /// Whether you want to be able to hear the audio source in game.
        /// </summary>
        /// <value></value>
        public bool Mute { get; set; } = false;

        void OnAudioFilterRead(float[] data, int channels)
        {
            onAudioRead?.Invoke(data, channels);

            if (Mute)
            {
                // Clear the audio before it reaches the playback layer
                System.Array.Clear(data, 0, data.Length);
            }
        }
    }
}
