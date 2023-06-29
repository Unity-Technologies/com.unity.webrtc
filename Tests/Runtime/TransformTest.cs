using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Threading;
using Unity.Collections;

namespace Unity.WebRTC.RuntimeTest
{
    class TransformTest
    {
        SynchronizationContext mainThreadContext = null;

        [SetUp]
        public void SetUp()
        {
            mainThreadContext = SynchronizationContext.Current;
        }

        static RenderTexture CreateRenderTexture()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            return rt;
        }

        void OnTransformedVideoFrame(RTCTransformEvent e)
        {
            // This process is not on a main thread.
            Assert.That(SynchronizationContext.Current, Is.Not.EqualTo(mainThreadContext));

            Assert.That(e.Frame, Is.Not.Null);
            Assert.That(e.Frame, Is.TypeOf<RTCEncodedVideoFrame>());
            var videoFrame = e.Frame as RTCEncodedVideoFrame;
            Assert.That(videoFrame, Is.Not.Null);
            Assert.That(videoFrame.Timestamp, Is.GreaterThan(0));
            Assert.That(videoFrame.Ssrc, Is.GreaterThan(0));

            NativeArray<byte>.ReadOnly array = videoFrame.GetData();
            Assert.That(array, Is.Not.Null);
            Assert.That(array.Length, Is.GreaterThan(0));
            videoFrame.SetData(array);

            RTCEncodedVideoFrameMetadata metadata = videoFrame.GetMetadata();
            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.frameId.HasValue, Is.True);
            Assert.That(metadata.width, Is.GreaterThan(0));
            Assert.That(metadata.height, Is.GreaterThan(0));
            Assert.That(metadata.simulcastIndex, Is.Zero);
            Assert.That(metadata.temporalIndex, Is.Zero);

            // Sometimes this parameter is not empty.
            // Assert.That(metadata.dependencies, Is.Empty);
        }

        void OnTransformedAudioFrame(RTCTransformEvent e)
        {
            // This process is not on a main thread.
            Assert.That(SynchronizationContext.Current, Is.Not.EqualTo(mainThreadContext));

            Assert.That(e.Frame, Is.Not.Null);
            Assert.That(e.Frame, Is.TypeOf<RTCEncodedAudioFrame>());
            var audioFrame = e.Frame as RTCEncodedAudioFrame;
            Assert.That(audioFrame, Is.Not.Null);
            Assert.That(audioFrame.Timestamp, Is.Not.Zero);
            Assert.That(audioFrame.Ssrc, Is.GreaterThan(0));

            NativeArray<byte>.ReadOnly array = audioFrame.GetData();
            Assert.That(array, Is.Not.Null);
            Assert.That(array.Length, Is.GreaterThan(0));
            audioFrame.SetData(array);
        }

        [Test]
        public void CreateVideoTransform()
        {
            void TransformedFrame(RTCTransformEvent e) {}
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Video, callback);
            transform.Dispose();
        }

        [Test]
        public void CreateAudioTransform()
        {
            void TransformedFrame(RTCTransformEvent e) { }
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Audio, callback);
            transform.Dispose();
        }

        [Test]
        public void SenderSetTransform()
        {
            void TransformedFrame(RTCTransformEvent e) { }
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Video, callback);

            RTCPeerConnection pc = new RTCPeerConnection();
            RTCRtpTransceiver transceiver = pc.AddTransceiver(TrackKind.Video);
            Assert.That(transceiver.Sender, Is.Not.Null);

            RTCRtpSender sender = transceiver.Sender;
            Assert.That(sender.Transform, Is.Null);
            Assert.That(() => sender.Transform = null, Throws.ArgumentNullException);

            transceiver.Sender.Transform = transform;

            transform.Dispose();
            pc.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXEditor })]
        public IEnumerator ReceiverSetTransform()
        {
            var rt = CreateRenderTexture();
            var track = new VideoStreamTrack(rt);
            var test = new MonoBehaviourTest<SignalingPeers>();
            var sender = test.component.AddTrack(0, track);
            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            yield return new WaitUntil(() => test.component.GetPeerReceivers(1).Any());
            var receiver = test.component.GetPeerReceivers(1).First();

            Assert.That(receiver.Transform, Is.Null);
            Assert.That(() => receiver.Transform = null, Throws.ArgumentNullException);

            void TransformedFrame(RTCTransformEvent e) {}
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Video, callback);
            receiver.Transform = transform;

            transform.Dispose();
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
            UnityEngine.Object.DestroyImmediate(rt);
        }

        // todo:
        // Crash on Android player on CI testing
        [UnityTest]
        [Timeout(1000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.Android })]
        public IEnumerator TransformedAudioFrameCallback()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);

            var track = new AudioStreamTrack(source);
            source.Play();
            var test = new MonoBehaviourTest<SignalingPeers>();
            var sender = test.component.AddTrack(0, track);
            var raisedTransformedFrame = false;
            void TransformedFrame(RTCTransformEvent e)
            {
                OnTransformedAudioFrame(e);
                raisedTransformedFrame = true;
            }
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Audio, callback);
            sender.Transform = transform;

            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            yield return new WaitUntil(() => test.component.GetPeerReceivers(1).Any());
            var receiver = test.component.GetPeerReceivers(1).First();
            test.component.CoroutineUpdate();

            yield return new WaitUntil(() => raisedTransformedFrame);
            Assert.That(raisedTransformedFrame, Is.True);

            transform.Dispose();
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
            UnityEngine.Object.DestroyImmediate(source.clip);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        // todo:
        // This test is failed for OSXEditor and Android platform on CI.
        // OSX standalone works well.
        [UnityTest]
        [Timeout(1000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXEditor, RuntimePlatform.Android,
            RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor })]
        public IEnumerator TransformedVideoFrameCallback()
        {
            var rt = CreateRenderTexture();
            var track = new VideoStreamTrack(rt);
            var test = new MonoBehaviourTest<SignalingPeers>();
            var sender = test.component.AddTrack(0, track);
            var raisedTransformedFrame = false;
            void TransformedFrame(RTCTransformEvent e)
            {
                OnTransformedVideoFrame(e);
                raisedTransformedFrame = true;
            }
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Video, callback);
            sender.Transform = transform;

            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            yield return new WaitUntil(() => test.component.GetPeerReceivers(1).Any());
            var receiver = test.component.GetPeerReceivers(1).First();
            test.component.CoroutineUpdate();

            yield return new WaitUntil(() => raisedTransformedFrame);
            Assert.That(raisedTransformedFrame, Is.True);

            transform.Dispose();
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
