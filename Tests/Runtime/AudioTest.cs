using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    class AudioTest
    {
        [SetUp]
        public void SetUp()
        {
            var value = NativeMethods.GetHardwareEncoderSupport();
            WebRTC.Initialize(value ? EncoderType.Hardware : EncoderType.Software);
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
