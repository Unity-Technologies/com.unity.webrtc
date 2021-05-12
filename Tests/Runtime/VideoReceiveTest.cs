using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    //ToDo: decoder is not supported H.264 codec on Windows/Linux.
    [TestFixture]
    [ConditionalIgnore(ConditionalIgnore.UnsupportedReceiveVideoOnHardware, "Not supported hardware decoder")]
    class VideoReceiveTestWithHardwareEncoder : VideoReceiveTestWithSoftwareEncoder
    {
        [OneTimeSetUp]
        public new void OneTimeInit()
        {
            encoderType = EncoderType.Hardware;
        }
    }

    class VideoReceiveTestWithSoftwareEncoder
    {
        protected EncoderType encoderType;

        [OneTimeSetUp]
        public void OneTimeInit()
        {
            encoderType = EncoderType.Software;
        }

        [SetUp]
        public void SetUp()
        {
            WebRTC.Initialize(encoderType);
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator InitializeReceiver()
        {
            const int width = 256;
            const int height = 256;

            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.That(transceiver, Is.Not.Null);
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.That(receiver, Is.Not.Null);
            MediaStreamTrack track = receiver.Track;
            Assert.That(track, Is.Not.Null);
            Assert.AreEqual(TrackKind.Video, track.Kind);
            Assert.That(track.Kind, Is.EqualTo(TrackKind.Video));
            var videoTrack = track as VideoStreamTrack;
            Assert.That(videoTrack, Is.Not.Null);
            var rt = videoTrack.InitializeReceiver(width, height);
            Assert.That(videoTrack.IsDecoderInitialized, Is.True);
            videoTrack.Dispose();
            // wait for disposing video track.
            yield return 0;

            peer.Dispose();
            Object.DestroyImmediate(rt);
        }

        [UnityTest]
        [Timeout(5000)]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformVideoDecoder,
            "VideoStreamTrack.UpdateReceiveTexture is not supported on Direct3D12")]
        public IEnumerator VideoReceive()
        {
            const int width = 256;
            const int height = 256;

            var config = new RTCConfiguration
            {
                iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}}
            };
            var pc1 = new RTCPeerConnection(ref config);
            var pc2 = new RTCPeerConnection(ref config);

            VideoStreamTrack receiveVideoTrack = null;
            Texture receiveImage = null;

            pc2.OnTrack = e =>
            {
                if (e.Track is VideoStreamTrack track && !track.IsDecoderInitialized)
                {
                    receiveVideoTrack = track;
                    receiveImage = track.InitializeReceiver(width, height);
                }
            };

            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.backgroundColor = Color.red;
            var sendVideoTrack = cam.CaptureStreamTrack(width, height, 1000000);

            yield return new WaitForSeconds(0.1f);

            pc1.AddTrack(sendVideoTrack);

            yield return SignalingPeers(pc1, pc2);

            yield return new WaitUntil(() => receiveVideoTrack != null && receiveVideoTrack.IsDecoderInitialized);
            Assert.That(receiveImage, Is.Not.Null);

            sendVideoTrack.Update();
            yield return new WaitForSeconds(0.1f);

            receiveVideoTrack.UpdateReceiveTexture();
            yield return new WaitForSeconds(0.1f);

            receiveVideoTrack.Dispose();
            sendVideoTrack.Dispose();
            yield return 0;

            pc2.Dispose();
            pc1.Dispose();
            Object.DestroyImmediate(camObj);
        }

        private static IEnumerator SignalingPeers(RTCPeerConnection offerPc, RTCPeerConnection answerPc)
        {
            offerPc.OnIceCandidate = candidate => answerPc.AddIceCandidate(candidate);
            answerPc.OnIceCandidate = candidate => offerPc.AddIceCandidate(candidate);

            var pc1CreateOffer = offerPc.CreateOffer();
            yield return pc1CreateOffer;
            Assert.That(pc1CreateOffer.IsError, Is.False, () => $"Failed {nameof(pc1CreateOffer)}, error:{pc1CreateOffer.Error.message}");
            var offerDesc = pc1CreateOffer.Desc;

            var pc1SetLocalDescription = offerPc.SetLocalDescription(ref offerDesc);
            yield return pc1SetLocalDescription;
            Assert.That(pc1SetLocalDescription.IsError, Is.False, () => $"Failed {nameof(pc1SetLocalDescription)}, error:{pc1SetLocalDescription.Error.message}");

            var pc2SetRemoteDescription = answerPc.SetRemoteDescription(ref offerDesc);
            yield return pc2SetRemoteDescription;
            Assert.That(pc2SetRemoteDescription.IsError, Is.False, () => $"Failed {nameof(pc2SetRemoteDescription)}, error:{pc2SetRemoteDescription.Error.message}");

            var pc2CreateAnswer = answerPc.CreateAnswer();
            yield return pc2CreateAnswer;
            Assert.That(pc2CreateAnswer.IsError, Is.False, () => $"Failed {nameof(pc2CreateAnswer)}, error:{pc2CreateAnswer.Error.message}");
            var answerDesc = pc2CreateAnswer.Desc;

            var pc2SetLocalDescription = answerPc.SetLocalDescription(ref answerDesc);
            yield return pc2SetLocalDescription;
            Assert.That(pc2SetLocalDescription.IsError, Is.False, () => $"Failed {nameof(pc2SetLocalDescription)}, error:{pc2SetLocalDescription.Error.message}");

            var pc1SetRemoteDescription = offerPc.SetRemoteDescription(ref answerDesc);
            yield return pc1SetRemoteDescription;
            Assert.That(pc1SetRemoteDescription.IsError, Is.False, () => $"Failed {nameof(pc1SetRemoteDescription)}, error:{pc1SetRemoteDescription.Error.message}");

            var waitConnectOfferPc = new WaitUntilWithTimeout(() =>
                offerPc.IceConnectionState == RTCIceConnectionState.Connected ||
                offerPc.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return waitConnectOfferPc;
            Assert.That(waitConnectOfferPc.IsCompleted, Is.True);

            var waitConnectAnswerPc = new WaitUntilWithTimeout(() =>
                answerPc.IceConnectionState == RTCIceConnectionState.Connected ||
                answerPc.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return waitConnectAnswerPc;
            Assert.That(waitConnectAnswerPc.IsCompleted, Is.True);

            var checkSenders = new WaitUntilWithTimeout(() => offerPc.GetSenders().Any(), 5000);
            yield return checkSenders;
            Assert.That(checkSenders.IsCompleted, Is.True);

            var checkReceivers = new WaitUntilWithTimeout(() => answerPc.GetReceivers().Any(), 5000);
            yield return checkReceivers;
            Assert.That(checkReceivers.IsCompleted, Is.True);
        }
    }
}
