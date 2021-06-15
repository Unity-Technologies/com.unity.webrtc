using System;

namespace Unity.WebRTC
{
    public class AudioStreamTrack : MediaStreamTrack
    {
        public AudioStreamTrack() : this(WebRTC.Context.CreateAudioTrack(Guid.NewGuid().ToString()))
        {
        }

        internal AudioStreamTrack(IntPtr sourceTrack) : base(sourceTrack)
        {
        }
    }

    public static class Audio
    {
        private static bool started;

        public static MediaStream CaptureStream()
        {
            started = true;

            var stream = new MediaStream();
            var track = new AudioStreamTrack();

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
