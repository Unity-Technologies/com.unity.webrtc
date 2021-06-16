using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class AudioStreamTrackTest
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

        [UnityTest]
        public IEnumerator AudioStreamTrackInstantiateOnce()
        {
            var track = new AudioStreamTrack();
            yield return 0;
            track.Dispose();
        }

        [UnityTest]
        public IEnumerator AudioStreamTrackInstantiateMultiple()
        {
            var track1 = new AudioStreamTrack();
            var track2 = new AudioStreamTrack();
            yield return 0;
            track1.Dispose();
            track2.Dispose();
        }

        [UnityTest]
        [Timeout(100)]
        public IEnumerator AudioStreamTrackPlayAudio()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);

            var track = new AudioStreamTrack(source);
            source.Play();

            yield return new WaitWhile(() => source.isPlaying);
            track.Dispose();
            Object.DestroyImmediate(obj);
        }

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
