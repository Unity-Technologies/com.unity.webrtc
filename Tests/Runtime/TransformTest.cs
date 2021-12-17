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
            var type = TestHelper.HardwareCodecSupport() ? EncoderType.Hardware : EncoderType.Software;
            WebRTC.Initialize(type: type, limitTextureSize: true, forTest: true);
            mainThreadContext = SynchronizationContext.Current;
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
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
            Assert.That(metadata.spatialIndex, Is.Zero);
            Assert.That(metadata.temporalIndex, Is.Zero);
            Assert.That(metadata.dependencies, Is.Empty);
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

        class CountdownEvent<T> : CountdownEvent
        {
            public T Result { get; private set; }

            public CountdownEvent(int initialCount) : base(initialCount) {}

            public bool Signal(T result)
            {
                Result = result;
                return Signal();
            }
        }

        Action<T> CatchException<T>(Action<T> callback, CountdownEvent<Exception> cde)
        {
            Action<T> callback_ = e =>
            {
                Exception exception = null;
                try
                {
                    callback(e);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                cde.Signal(exception);
            };
            return callback_;
        }


        [Test]
        [Category("RTCRtpTransform")]
        public void CreateVideoTransform()
        {
            void TransformedFrame(RTCTransformEvent e) {}
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Video, callback);
            transform.Dispose();
        }

        [Test]
        [Category("RTCRtpTransform")]
        public void CreateAudioTransform()
        {
            void TransformedFrame(RTCTransformEvent e) { }
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Audio, callback);
            transform.Dispose();
        }

        [Test]
        [Category("RTCRtpTransform")]
        public void SenderSetTransform()
        {
            void TransformedFrame(RTCTransformEvent e) { }
            TransformedFrameCallback callback = TransformedFrame;
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Video, callback);

            RTCPeerConnection pc = new RTCPeerConnection();
            RTCRtpTransceiver transceiver = pc.AddTransceiver(TrackKind.Video);
            Assert.That(transceiver.Sender, Is.Not.Null);
            transceiver.Sender.Transform = transform;

            transform.Dispose();
            pc.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("RTCRtpTransform")]
        public IEnumerator ReceiverSetTransform()
        {
            var rt = CreateRenderTexture();
            var track = new VideoStreamTrack(rt);
            var test = new MonoBehaviourTest<SignalingPeers>();
            var sender = test.component.AddTrack(0, track);
            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            yield return new WaitUntil(() => test.component.GetPeerReceivers(1).Any());
            var receiver = test.component.GetPeerReceivers(1).First();

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

        [UnityTest]
        [Timeout(1000)]
        [Category("RTCRtpTransform")]
        public IEnumerator TransformedAudioFrameCallback()
        {
            GameObject obj = new GameObject("audio");
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);

            var track = new AudioStreamTrack(source);
            source.Play();
            var test = new MonoBehaviourTest<SignalingPeers>();
            var sender = test.component.AddTrack(0, track);
            var cde = new CountdownEvent<Exception>(1);

            var callback = new TransformedFrameCallback(
                CatchException<RTCTransformEvent>(OnTransformedAudioFrame, cde));
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Audio, callback);
            sender.Transform = transform;

            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            yield return new WaitUntil(() => test.component.GetPeerReceivers(1).Any());
            var receiver = test.component.GetPeerReceivers(1).First();

            yield return new WaitForSeconds(0.1f);

            // waiting another thread.
            cde.Wait(1000);
            Assert.That(() => cde.Result, Is.Null);

            transform.Dispose();
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
            UnityEngine.Object.DestroyImmediate(source.clip);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("RTCRtpTransform")]
        public IEnumerator TransformedVideoFrameCallback()
        {
            var rt = CreateRenderTexture();
            var track = new VideoStreamTrack(rt);
            var test = new MonoBehaviourTest<SignalingPeers>();
            var sender = test.component.AddTrack(0, track);
            var cde = new CountdownEvent<Exception>(1);

            var callback = new TransformedFrameCallback(
                CatchException<RTCTransformEvent>(OnTransformedVideoFrame, cde));
            RTCRtpScriptTransform transform =
                new RTCRtpScriptTransform(TrackKind.Video, callback);
            sender.Transform = transform;

            yield return new WaitUntil(() => test.component.NegotiationCompleted());
            yield return new WaitUntil(() => test.component.GetPeerReceivers(1).Any());
            var receiver = test.component.GetPeerReceivers(1).First();

            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);

            // waiting another thread.
            cde.Wait(1000);
            Assert.That(() => cde.Result, Is.Null);

            transform.Dispose();
            test.component.Dispose();
            UnityEngine.Object.DestroyImmediate(test.gameObject);
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
