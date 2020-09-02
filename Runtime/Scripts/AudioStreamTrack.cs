namespace Unity.WebRTC
{
    public class AudioStreamTrack : MediaStreamTrack
    {
        public AudioStreamTrack(string label) : base(WebRTC.Context.CreateAudioTrack(label))
        {
        }
    }

    public static class Audio
    {
        private static bool started;

        public static MediaStream CaptureStream()
        {
            started = true;

            var stream = new MediaStream(WebRTC.Context.CreateMediaStream("audiostream"));
            var track = new MediaStreamTrack(WebRTC.Context.CreateAudioTrack("audio"));
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
