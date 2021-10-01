using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class AudioStreamTrackTest
    {
        [SetUp]
        public void SetUp()
        {
            var type = TestHelper.HardwareCodecSupport() ? EncoderType.Hardware : EncoderType.Software;
            WebRTC.Initialize(type: type, limitTextureSize: true, forTest: true);
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

        [Ignore("AudioManager is disabled when batch mode on CI")]
        [UnityTest]
        [Timeout(5000)]
        public IEnumerator AddMultiAudioTrack()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();

            int channels = 2;
            source.clip = AudioClip.Create("test", 48000, channels, 48000, false);
            source.Play();

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.gameObject.AddComponent<AudioListener>();

            // first track
            var track1 = new AudioStreamTrack(source);
            var sender1 = test.component.AddTrack(0, track1);
            yield return test;
            var receivers = test.component.GetPeerReceivers(1);
            Assert.That(receivers.Count(), Is.EqualTo(1));

            var receiver = receivers.First();
            var audioTrack = receiver.Track as AudioStreamTrack;
            Assert.That(audioTrack, Is.Not.Null);

            yield return new WaitUntil(() => audioTrack.Renderer != null);
            Assert.That(audioTrack.Renderer, Is.Not.Null);
            Assert.That(audioTrack.Renderer.channels, Is.EqualTo(channels));


            // second track
            var track2 = new AudioStreamTrack(source);
            var sender2 = test.component.AddTrack(0, track2);
            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            yield return new WaitUntil(() => test.component.GetPeerReceivers(1).Count() == 2);
            receivers = test.component.GetPeerReceivers(1);
            Assert.That(receivers.Count(), Is.EqualTo(2));

            receiver = receivers.Last();
            audioTrack = receiver.Track as AudioStreamTrack;
            Assert.That(audioTrack, Is.Not.Null);

            yield return new WaitUntil(() => audioTrack.Renderer != null);
            Assert.That(audioTrack.Renderer, Is.Not.Null);
            Assert.That(audioTrack.Renderer.channels, Is.EqualTo(channels));

            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
            Object.DestroyImmediate(obj);
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
        public void AudioStreamTrackSetData()
        {
            var track = new AudioStreamTrack();
            Assert.That(() => track.SetData(null, 0, 0), Throws.ArgumentNullException);

            float[] data = new float[2048];
            Assert.That(() => track.SetData(data, 0, 0), Throws.ArgumentException);

            Assert.That(() => track.SetData(data, 1, 0), Throws.ArgumentException);
            Assert.That(() => track.SetData(data, 0, 48000), Throws.ArgumentException);
            Assert.That(() => track.SetData(data, 1, 48000), Throws.Nothing);
            track.Dispose();
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
