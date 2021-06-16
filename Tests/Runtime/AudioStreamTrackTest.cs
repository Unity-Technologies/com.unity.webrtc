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
}
