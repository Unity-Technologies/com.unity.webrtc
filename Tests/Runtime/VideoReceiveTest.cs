using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    public class VideoReceiveTest
    {
        [SetUp]
        public void SetUp()
        {
            WebRTC.Initialize(EncoderType.Software);
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }

        [Test]
        public void InitializeReceiver()
        {
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var videoTrack = new VideoStreamTrack("video", rt);
            var rt1 = videoTrack.InitializeReceiver();
            Assert.True(videoTrack.IsDecoderInitialized);
            videoTrack.Dispose();
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(rt1);
        }

        // todo::Software encoder does not support yet on linux
        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.LinuxEditor, RuntimePlatform.LinuxPlayer })]
        public IEnumerator VideoReceive()
        {
            var config = new RTCConfiguration
            {
                iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}}
            };
            var pc1 = new RTCPeerConnection(ref config);
            var pc2 = new RTCPeerConnection(ref config);
            var sendStream = new MediaStream();
            var receiveStream = new MediaStream();
            VideoStreamTrack receiveVideoTrack = null;
            RenderTexture receiveImage = null;
            receiveStream.OnAddTrack = e =>
            {
                if (e.Track.Kind == TrackKind.Video)
                {
                    receiveVideoTrack = (VideoStreamTrack)e.Track;
                    receiveImage = receiveVideoTrack.InitializeReceiver();
                }
            };
            pc2.OnTrack = e => receiveStream.AddTrack(e.Track);

            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            cam.backgroundColor = Color.red;
            var sendVideoTrack = cam.CaptureStreamTrack(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            pc1.AddTrack(sendVideoTrack, sendStream);
            pc2.AddTransceiver(TrackKind.Video);

            yield return SignalingPeers(pc1, pc2);

            yield return new WaitUntil(() => receiveVideoTrack != null && receiveVideoTrack.IsDecoderInitialized);

            sendVideoTrack.Update();
            yield return new WaitForEndOfFrame();

            receiveVideoTrack.UpdateReceiveTexture();
            yield return new WaitForEndOfFrame();

            receiveVideoTrack.Dispose();
            receiveStream.Dispose();
            sendVideoTrack.Dispose();
            sendStream.Dispose();
            pc2.Dispose();
            pc1.Dispose();
            Object.DestroyImmediate(receiveImage);
        }

        private static IEnumerator SignalingPeers(RTCPeerConnection offerPc, RTCPeerConnection answerPc)
        {
            offerPc.OnIceCandidate = candidate => answerPc.AddIceCandidate(ref candidate);
            answerPc.OnIceCandidate = candidate => offerPc.AddIceCandidate(ref candidate);

            var offerOption = new RTCOfferOptions {offerToReceiveVideo = true};
            var answerOption = new RTCAnswerOptions {iceRestart = false};

            var pc1CreateOffer = offerPc.CreateOffer(ref offerOption);
            yield return pc1CreateOffer;
            Assert.False(pc1CreateOffer.IsError);
            var offerDesc = pc1CreateOffer.Desc;

            var pc1SetLocalDescription = offerPc.SetLocalDescription(ref offerDesc);
            yield return pc1SetLocalDescription;
            Assert.False(pc1SetLocalDescription.IsError);

            var pc2SetRemoteDescription = answerPc.SetRemoteDescription(ref offerDesc);
            yield return pc2SetRemoteDescription;
            Assert.False(pc2SetRemoteDescription.IsError);

            var pc2CreateAnswer = answerPc.CreateAnswer(ref answerOption);
            yield return pc2CreateAnswer;
            Assert.False(pc2CreateAnswer.IsError);
            var answerDesc = pc2CreateAnswer.Desc;

            var pc2SetLocalDescription = answerPc.SetLocalDescription(ref answerDesc);
            yield return pc2SetLocalDescription;
            Assert.False(pc2SetLocalDescription.IsError);

            var pc1SetRemoteDescription = offerPc.SetRemoteDescription(ref answerDesc);
            yield return pc1SetRemoteDescription;
            Assert.False(pc1SetRemoteDescription.IsError);
        }
    }
}
