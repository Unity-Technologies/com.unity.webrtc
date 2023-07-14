using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class TransceiverTest
    {
        [Test]
        public void SenderGetVideoCapabilities()
        {
            RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
            Assert.NotNull(capabilities);
            Assert.NotNull(capabilities.codecs);
            Assert.IsNotEmpty(capabilities.codecs);
            Assert.NotNull(capabilities.headerExtensions);
            Assert.IsNotEmpty(capabilities.headerExtensions);

            foreach (var codec in capabilities.codecs)
            {
                Assert.NotNull(codec);
                Assert.IsNotEmpty(codec.mimeType);
            }
            foreach (var extensions in capabilities.headerExtensions)
            {
                Assert.NotNull(extensions);
                Assert.IsNotEmpty(extensions.uri);
            }
        }

        [Test]
        public void SenderGetAudioCapabilities()
        {
            RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Audio);
            Assert.NotNull(capabilities);
            Assert.NotNull(capabilities.codecs);
            Assert.IsNotEmpty(capabilities.codecs);
            Assert.NotNull(capabilities.headerExtensions);
            Assert.IsNotEmpty(capabilities.headerExtensions);

            foreach (var codec in capabilities.codecs)
            {
                Assert.NotNull(codec);
                Assert.IsNotEmpty(codec.mimeType);
            }
            foreach (var extensions in capabilities.headerExtensions)
            {
                Assert.NotNull(extensions);
                Assert.IsNotEmpty(extensions.uri);
            }
        }

        [Test]
        public void ReceiverGetVideoCapabilities()
        {
            RTCRtpCapabilities capabilities = RTCRtpReceiver.GetCapabilities(TrackKind.Video);
            Assert.NotNull(capabilities);
            Assert.NotNull(capabilities.codecs);
            Assert.IsNotEmpty(capabilities.codecs);
            Assert.NotNull(capabilities.headerExtensions);
            Assert.IsNotEmpty(capabilities.headerExtensions);

            foreach (var codec in capabilities.codecs)
            {
                Assert.NotNull(codec);
                Assert.IsNotEmpty(codec.mimeType);
            }
            foreach (var extensions in capabilities.headerExtensions)
            {
                Assert.NotNull(extensions);
                Assert.IsNotEmpty(extensions.uri);
            }
        }

        [Test]
        public void ReceiverGetAudioCapabilities()
        {
            RTCRtpCapabilities capabilities = RTCRtpReceiver.GetCapabilities(TrackKind.Audio);
            Assert.NotNull(capabilities);
            Assert.NotNull(capabilities.codecs);
            Assert.IsNotEmpty(capabilities.codecs);
            Assert.NotNull(capabilities.headerExtensions);
            Assert.IsNotEmpty(capabilities.headerExtensions);

            foreach (var codec in capabilities.codecs)
            {
                Assert.NotNull(codec);
                Assert.IsNotEmpty(codec.mimeType);
            }
            foreach (var extensions in capabilities.headerExtensions)
            {
                Assert.NotNull(extensions);
                Assert.IsNotEmpty(extensions.uri);
            }
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void TransceiverSetVideoCodecPreferences()
        {
            var peer = new RTCPeerConnection();
            var capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            var error = transceiver.SetCodecPreferences(capabilities.codecs);
            Assert.AreEqual(RTCErrorType.None, error);
        }

        [Test]
        public void TransceiverSetAudioCodecPreferences()
        {
            var peer = new RTCPeerConnection();
            var capabilities = RTCRtpSender.GetCapabilities(TrackKind.Audio);
            var transceiver = peer.AddTransceiver(TrackKind.Audio);
            var error = transceiver.SetCodecPreferences(capabilities.codecs);
            Assert.AreEqual(RTCErrorType.None, error);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void ReceiverGetTrackReturnsVideoTrack()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);

            // The receiver has a video track
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.That(receiver, Is.Not.Null);
            Assert.That(receiver.Track, Is.Not.Null);
            Assert.That(receiver.Track, Is.TypeOf<VideoStreamTrack>());

            // The receiver has no track
            RTCRtpSender sender = transceiver.Sender;
            Assert.That(sender, Is.Not.Null);
            Assert.That(sender.Track, Is.Null);

            peer.Dispose();
        }

        [Test]
        public void ReceiverGetTrackReturnsAudioTrack()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Audio);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);

            // The receiver has a audio track
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.That(receiver, Is.Not.Null);
            Assert.That(receiver.Track, Is.Not.Null);
            Assert.That(receiver.Track, Is.TypeOf<AudioStreamTrack>());

            // The receiver has no track
            RTCRtpSender sender = transceiver.Sender;
            Assert.That(sender, Is.Not.Null);
            Assert.That(sender.Track, Is.Null);

            peer.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ReceiverGetContributingSource()
        {
            var track = new AudioStreamTrack();
            var test = new MonoBehaviourTest<SignalingPeers>();
            var transceiver1 = test.component.AddTransceiver(0, track);
            RTCRtpReceiver receiver1 = transceiver1.Receiver;
            var sources1 = receiver1.GetContributingSources();
            Assert.That(sources1, Is.Empty);

            yield return test;
            test.component.CoroutineUpdate();

            // wait for OnNegotiationNeeded callback in SignalingPeers class
            yield return new WaitUntil(() => test.component.NegotiationCompleted());

            var transceiver2 = test.component.GetPeerTransceivers(1).First();
            RTCRtpReceiver receiver2 = transceiver2.Receiver;

            // Send audio data manually.
            var nativeArray = new NativeArray<float>(480, Allocator.Persistent);

            yield return new WaitUntil(() =>
            {
                track.SetData(nativeArray.AsReadOnly(), 1, 48000);
                return receiver2.GetContributingSources().Length > 0;
            });
            var sources2 = receiver2.GetContributingSources();
            Assert.That(sources2, Is.Not.Empty);
            Assert.That(sources2.Length, Is.EqualTo(1));
            Assert.That(sources2[0], Is.Not.Null);
            Assert.That(sources2[0].audioLevel, Is.Not.Null);
            Assert.That(sources2[0].source, Is.Null);

            // todo(kazuki): Returns zero on Linux platform.
            // Assert.That(sources2[0].rtpTimestamp, Is.Not.Zero);
            // Assert.That(sources2[0].timestamp, Is.Not.Zero);

            nativeArray.Dispose();
            test.component.Dispose();
            track.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }


        [UnityTest]
        [Timeout(5000)]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator TransceiverStop()
        {
            if (SystemInfo.processorType == "Apple M1")
                Assert.Ignore("todo:: This test will hang up on Apple M1");

            var go = new GameObject("Test");
            var cam = go.AddComponent<Camera>();
            var track = cam.CaptureStreamTrack(1280, 720);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddTransceiver(0, track);
            yield return test;
            test.component.CoroutineUpdate();

            var senderTransceivers = test.component.GetPeerTransceivers(0);
            Assert.That(senderTransceivers.Count(), Is.EqualTo(1));
            var transceiver1 = senderTransceivers.First();

            var receiverTransceivers = test.component.GetPeerTransceivers(1);
            Assert.That(receiverTransceivers.Count(), Is.EqualTo(1));
            var transceiver2 = receiverTransceivers.First();

            Assert.That(transceiver1.Stop(), Is.EqualTo(RTCErrorType.None));

            yield return new WaitUntil(() =>
            transceiver1.CurrentDirection == RTCRtpTransceiverDirection.Stopped &&
            transceiver2.CurrentDirection == RTCRtpTransceiverDirection.Stopped);

            Assert.That(transceiver1.Direction, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));
            Assert.That(transceiver1.CurrentDirection, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));
            Assert.That(transceiver2.Direction, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));
            Assert.That(transceiver2.CurrentDirection, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));

            //TODO:: Disposing process of MediaStreamTrack is unstable when using GC.
            //At the moment, Dispose methods needs to be called on the main thread for workaround.
            test.component.Dispose();
            track.Dispose();

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(test.gameObject);
        }
    }
}
