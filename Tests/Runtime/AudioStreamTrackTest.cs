using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

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

        [Test]
        public void AudioStreamTrackInstantiateOnce()
        {
            var track = new AudioStreamTrack();
            track.Dispose();
        }

        [Test]
        public void AudioStreamTrackInstantiateMultiple()
        {
            var track1 = new AudioStreamTrack();
            var track2 = new AudioStreamTrack();
            track1.Dispose();
            track2.Dispose();
        }

        [Test]
        public void AudioStreamTrackPlayAudio()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);

            var track = new AudioStreamTrack(source);
            source.Play();
            track.Dispose();
            Object.DestroyImmediate(obj);
        }

        [Test]
        public void AudioStreamRenderer()
        {
            var renderer = new AudioStreamTrack.AudioStreamRenderer("test", 48000, 2);
            Assert.That(renderer.clip, Is.Not.Null);

            for (int i = 0; i < 300; i++)
            {
                float[] data = new float[2048];
                renderer.SetData(data);
            }
            renderer.Dispose();
        }
    }
}
