using System;
using System.Collections;
using System.Linq;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Collections;

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
            var test = new MonoBehaviourTest<SignalingPeers>();
            var source = test.gameObject.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 48000, 2, 48000, false);
            var audioTrack = new AudioStreamTrack(source);
            var sender = test.component.AddTrack(0, audioTrack);
            yield return test;
            Assert.That(test.component.RemoveTrack(0, sender), Is.EqualTo(RTCErrorType.None));
            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(source.clip);
            UnityEngine.Object.DestroyImmediate(test.gameObject);
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

            yield return new WaitUntil(() => audioTrack.Source != null);
            Assert.That(audioTrack.Source, Is.Not.Null);
            Assert.That(audioTrack.Source.clip.channels, Is.EqualTo(channels));


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

            yield return new WaitUntil(() => audioTrack.Source != null);
            Assert.That(audioTrack.Source, Is.Not.Null);
            Assert.That(audioTrack.Source.clip.channels, Is.EqualTo(channels));

            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
            UnityEngine.Object.DestroyImmediate(obj);
        }


        [Test]
        public void AudioStreamTrackInstantiateOnce()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 48000, 2, 48000, false);
            var track = new AudioStreamTrack(source);
            track.Dispose();
            UnityEngine.Object.DestroyImmediate(source.clip);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        [Test]
        public void AudioStreamTrackInstantiateMultiple()
        {
            GameObject obj1 = new GameObject("audio1");
            AudioSource source1 = obj1.AddComponent<AudioSource>();
            source1.clip = AudioClip.Create("test1", 48000, 2, 48000, false);
            GameObject obj2 = new GameObject("audio2");
            AudioSource source2 = obj2.AddComponent<AudioSource>();
            source2.clip = AudioClip.Create("test2", 48000, 2, 48000, false);
            var track1 = new AudioStreamTrack(source1);
            var track2 = new AudioStreamTrack(source2);
            track1.Dispose();
            track2.Dispose();
            UnityEngine.Object.DestroyImmediate(source1.clip);
            UnityEngine.Object.DestroyImmediate(source2.clip);
            UnityEngine.Object.DestroyImmediate(obj1);
            UnityEngine.Object.DestroyImmediate(obj2);
        }

        [Test]
        public void AudioStreamTrackSetData()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test1", 48000, 2, 48000, false);
            var track = new AudioStreamTrack(source);
            Assert.That(() => track.SetData(null, 0, 0), Throws.ArgumentNullException);

            float[] data = new float[2048];
            Assert.That(() => track.SetData(data, 0, 0), Throws.ArgumentException);

            Assert.That(() => track.SetData(data, 1, 0), Throws.ArgumentException);
            Assert.That(() => track.SetData(data, 0, 48000), Throws.ArgumentException);
            Assert.That(() => track.SetData(data, 1, 48000), Throws.Nothing);
            track.Dispose();
            UnityEngine.Object.DestroyImmediate(source.clip);
            UnityEngine.Object.DestroyImmediate(obj);
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
            UnityEngine.Object.DestroyImmediate(source.clip);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        [Test]
        public void AudioStreamRenderer()
        {
            var obj = new GameObject("audio");
            var renderer = new AudioStreamTrack.AudioStreamRenderer(null);
            renderer.Source = obj.AddComponent<AudioSource>();
            renderer.Dispose();
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
