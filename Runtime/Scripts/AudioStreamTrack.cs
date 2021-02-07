using System;

namespace Unity.WebRTC
{
    public class AudioStreamTrack : MediaStreamTrack
    {
        public AudioStreamTrack(string label) : base(WebRTC.Context.CreateAudioTrack(label))
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

            var stream = new MediaStream(WebRTC.Context.CreateMediaStream(streamlabel));
            var track = new AudioStreamTrack(WebRTC.Context.CreateAudioTrack(label));
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
