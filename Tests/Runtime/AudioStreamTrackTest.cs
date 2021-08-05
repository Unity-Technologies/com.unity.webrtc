using NUnit.Framework;
using UnityEngine;
using System.Collections;
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

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator AddAndRemoveAudioTrack()
        {
            var audioTrack = new AudioStreamTrack();
            var test = new MonoBehaviourTest<SignalingPeers>();
            var sender = test.component.AddTrack(0, audioTrack);
            yield return test;
            Assert.That(test.component.RemoveTrack(0, sender), Is.EqualTo(RTCErrorType.None));
            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
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
