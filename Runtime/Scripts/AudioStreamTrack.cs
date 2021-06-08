using System;
using UnityEngine;

namespace Unity.WebRTC
{
    public class AudioStreamTrack : MediaStreamTrack
    {
        public AudioStreamTrack(string label) : base(WebRTC.Context.CreateAudioTrack(Hash128.Compute(label).ToString()))
        {
        }

        public AudioStreamTrack(IntPtr sourceTrack) : base(sourceTrack)
        {
        }
    }

    public static class Audio
    {
        private static bool started;

        public static MediaStream CaptureStream(string streamlabel = "audiostream", string label="audio")
        {
            started = true;

            var stream = new MediaStream(WebRTC.Context.CreateMediaStream(Hash128.Compute(streamlabel).ToString()));
            var track = new AudioStreamTrack(WebRTC.Context.CreateAudioTrack(Hash128.Compute(label).ToString()));
            stream.AddTrack(track);
            return stream;
        }

        public static void Update(float[] audioData, int channels)
        {
            if (started)
            {
                NativeMethods.ProcessAudio(audioData, audioData.Length);
            }
        }

        public static void Stop()
        {
            if (started)
            {
                started = false;
            }
        }
    }
}
