using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class AudioStreamTrackTest
    {
        [UnityTest]
        [Timeout(5000)]
        public IEnumerator Constructor()
        {
            var test = new MonoBehaviourTest<SignalingPeers>();
            var audioTrack = new AudioStreamTrack();
            Assert.That(audioTrack.Source, Is.Null);
            Assert.That(audioTrack.Loopback, Is.False);
            var sender = test.component.AddTrack(0, audioTrack);
            yield return test;
            Assert.That(test.component.RemoveTrack(0, sender), Is.EqualTo(RTCErrorType.None));
            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ConstructorWithAudioSource()
        {
            var test = new MonoBehaviourTest<SignalingPeers>();
            var source = test.gameObject.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 48000, 2, 48000, false);
            var audioTrack = new AudioStreamTrack(source);
            Assert.That(audioTrack.Source, Is.EqualTo(source));
            Assert.That(audioTrack.Loopback, Is.False);
            var sender = test.component.AddTrack(0, audioTrack);
            yield return test;
            Assert.That(test.component.RemoveTrack(0, sender), Is.EqualTo(RTCErrorType.None));
            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(source.clip);
            UnityEngine.Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ConstructorWithAudioListener()
        {
            var test = new MonoBehaviourTest<SignalingPeers>();
            var listener = test.gameObject.AddComponent<AudioListener>();
            var audioTrack = new AudioStreamTrack(listener);
            Assert.That(audioTrack.Source, Is.Null);
            Assert.That(audioTrack.Loopback, Is.False);
            var sender = test.component.AddTrack(0, audioTrack);
            yield return test;
            Assert.That(test.component.RemoveTrack(0, sender), Is.EqualTo(RTCErrorType.None));
            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
        }

        [Test]
        public void SetLoopback()
        {
            var obj = new GameObject("test");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 48000, 2, 48000, false);
            var audioTrack = new AudioStreamTrack(source);
            Assert.That(audioTrack.Loopback, Is.False);
            audioTrack.Loopback = true;
            Assert.That(audioTrack.Loopback, Is.True);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        [Test]
        public void ConstructorThrowException()
        {
            AudioListener listener = null;
            Assert.That(() => new AudioStreamTrack(listener), Throws.TypeOf<ArgumentNullException>());

            AudioSource source = null;
            Assert.That(() => new AudioStreamTrack(source), Throws.TypeOf<ArgumentNullException>());
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator GCCollect()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();
            var test = new MonoBehaviourTest<SignalingPeers>();

            var track = new AudioStreamTrack(source);
            test.component.AddTrack(0, track);
            yield return test;
            GC.Collect();
            var receivers = test.component.GetPeerReceivers(1);
            Assert.That(receivers.Count(), Is.EqualTo(1));
            var receiver = receivers.First();
            var audioTrack = receiver.Track as AudioStreamTrack;
            Assert.That(audioTrack, Is.Not.Null);

            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
            UnityEngine.Object.DestroyImmediate(obj);
        }

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
            NativeArray<float> nativeArray = new NativeArray<float>(data, Allocator.Temp);
            Assert.That(() => track.SetData(data, 0, 0), Throws.ArgumentException);

            Assert.That(() => track.SetData(data, 1, 0), Throws.ArgumentException);
            Assert.That(() => track.SetData(data, 0, 48000), Throws.ArgumentException);
            Assert.That(() => track.SetData(data, 1, 48000), Throws.Nothing);

            var array = nativeArray.AsReadOnly();
            Assert.That(() => track.SetData(array, 1, 48000), Throws.Nothing);
            var slice = nativeArray.Slice();
            Assert.That(() => track.SetData(slice, 1, 48000), Throws.Nothing);

            // ReadOnlySpan<T> is supported since .NET Standard 2.1.
#if UNITY_2021_2_OR_NEWER
            unsafe
            {
                Assert.That(() => track.SetData(new ReadOnlySpan<float>(nativeArray.GetUnsafePtr(), nativeArray.Length), 1, 48000), Throws.Nothing);
            }
#endif
            nativeArray.Dispose();
            track.Dispose();
            UnityEngine.Object.DestroyImmediate(source.clip);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        //todo(kazuki): workaround ObjectDisposedException for Linux playmode test
        [Test]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxEditor })]
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
