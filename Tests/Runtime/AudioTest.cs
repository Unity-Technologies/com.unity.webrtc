using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    class AudioTest
    {
        [SetUp]
        public void SetUp()
        {
            WebRTC.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }

        [Test]
        public void Update()
        {
            var stream = Audio.CaptureStream();
            float[] audioData = new float[128];
            Audio.Update(audioData, 1);
            Audio.Stop();
            stream.Dispose();
        }
    }
}
