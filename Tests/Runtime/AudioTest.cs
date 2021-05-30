using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class AudioTest
    {
        [SetUp]
        public void SetUp()
        {
            var value = TestHelper.HardwareCodecSupport();
            WebRTC.Initialize(value ? EncoderType.Hardware : EncoderType.Software);
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }

        //[Test]
        //public void Update()
        //{
        //    var stream = Audio.CaptureStream();
        //    float[] audioData = new float[128];
        //    Audio.Update(audioData, 1);
        //    Audio.Stop();
        //    stream.Dispose();
        //}
    }

    class AudioStreamClipTest
    {
        [SetUp]
        public void SetUp()
        {
            var value = TestHelper.HardwareCodecSupport();
            WebRTC.Initialize(value ? EncoderType.Hardware : EncoderType.Software);
        }

        [UnityTest]
        public IEnumerator Constructor()
        {
            //const int position = 0;
            //const int samplerate = 44100;
            ////const float frequency = 440;
            //var clip = new AudioStreamTrack.AudioStreamRenderer("audio", samplerate, 1, samplerate);
            yield return new WaitForSeconds(0.3f);
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }
    }
}
