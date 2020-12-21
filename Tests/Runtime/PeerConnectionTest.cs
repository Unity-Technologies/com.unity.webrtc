using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using Object = UnityEngine.Object;

namespace Unity.WebRTC.RuntimeTest
{
    class PeerConnectionTest
    {
        static RTCConfiguration GetConfiguration()
        {
            RTCConfiguration config = default;
            config.iceServers = new[]
            {
                new RTCIceServer
                {
                    urls = new[] {"stun:stun.l.google.com:19302"},
                    username = "",
                    credential = "",
                    credentialType = RTCIceCredentialType.Password
                }
            };
            return config;
        }

        [SetUp]
        public void SetUp()
        {
            var value = NativeMethods.GetHardwareEncoderSupport();
            WebRTC.Initialize(value ? EncoderType.Hardware : EncoderType.Software);
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void Construct()
        {
            var peer = new RTCPeerConnection();
            Assert.AreEqual(0, peer.GetReceivers().Count());
            Assert.AreEqual(0, peer.GetSenders().Count());
            Assert.AreEqual(0, peer.GetTransceivers().Count());
            Assert.AreEqual(RTCPeerConnectionState.New, peer.ConnectionState);
            Assert.That(() => peer.LocalDescription, Throws.InvalidOperationException);
            Assert.That(() => peer.RemoteDescription, Throws.InvalidOperationException);
            Assert.That(() => peer.PendingLocalDescription, Throws.InvalidOperationException);
            Assert.That(() => peer.PendingRemoteDescription, Throws.InvalidOperationException);
            Assert.That(() => peer.CurrentLocalDescription, Throws.InvalidOperationException);
            Assert.That(() => peer.CurrentRemoteDescription, Throws.InvalidOperationException);
            peer.Close();

            Assert.AreEqual(RTCPeerConnectionState.Closed, peer.ConnectionState);
            peer.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void ConstructWithConfig()
        {
            var config = GetConfiguration();
            var peer = new RTCPeerConnection(ref config);

            var config2 = peer.GetConfiguration();
            Assert.NotNull(config.iceServers);
            Assert.NotNull(config2.iceServers);
            Assert.AreEqual(config.iceServers.Length, config2.iceServers.Length);
            Assert.AreEqual(config.iceServers[0].username, config2.iceServers[0].username);
            Assert.AreEqual(config.iceServers[0].credential, config2.iceServers[0].credential);
            Assert.AreEqual(config.iceServers[0].urls, config2.iceServers[0].urls);

            peer.Close();
            peer.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void ConstructWithConfigThrowException()
        {

            RTCConfiguration config = default;

            // To specify TURN server, also needs `username` and `credential`.
            config.iceServers = new[]
            {
                new RTCIceServer {  urls = new[] {"turn:127.0.0.1?transport=udp"} }
            };
            Assert.That(() => { new RTCPeerConnection(ref config); }, Throws.ArgumentException);
        }

        [Test]
        [Category("PeerConnection")]
        public void SetConfiguration()
        {
            var peer = new RTCPeerConnection();
            var config = GetConfiguration();
            var result = peer.SetConfiguration(ref config);
            Assert.AreEqual(RTCErrorType.None, result);
            peer.Close();
            peer.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void AddTransceiver()
        {
            var peer = new RTCPeerConnection();
            var stream = Audio.CaptureStream();
            var track = stream.GetAudioTracks().First();
            Assert.AreEqual(0, peer.GetTransceivers().Count());
            var transceiver = peer.AddTransceiver(track);
            Assert.NotNull(transceiver);
            Assert.IsNull(transceiver.CurrentDirection);
            RTCRtpSender sender = transceiver.Sender;
            Assert.NotNull(sender);
            Assert.AreEqual(track, sender.Track);

            RTCRtpSendParameters parameters = sender.GetParameters();
            Assert.NotNull(parameters);
            Assert.NotNull(parameters.encodings);
            foreach (var encoding in parameters.encodings)
            {
                Assert.True(encoding.active);
                Assert.Null(encoding.maxBitrate);
                Assert.Null(encoding.minBitrate);
                Assert.Null(encoding.maxFramerate);
                Assert.Null(encoding.scaleResolutionDownBy);
                Assert.IsNotEmpty(encoding.rid);
            }
            Assert.IsNotEmpty(parameters.transactionId);
            Assert.AreEqual(1, peer.GetTransceivers().Count());
            Assert.NotNull(peer.GetTransceivers().First());
            Assert.NotNull(parameters.codecs);
            foreach (var codec in parameters.codecs)
            {
                Assert.NotNull(codec);
                Assert.NotZero(codec.payloadType);
                Assert.IsNotEmpty(codec.mimeType);
                Assert.IsNotEmpty(codec.sdpFmtpLine);
                Assert.Null(codec.clockRate);
                Assert.Null(codec.channels);
            }
            Assert.NotNull(parameters.rtcp);
            Assert.NotNull(parameters.headerExtensions);
            foreach (var extension in parameters.headerExtensions)
            {
                Assert.NotNull(extension);
                Assert.IsNotEmpty(extension.uri);
                Assert.NotZero(extension.id);
            }

            track.Dispose();
            stream.Dispose();
            peer.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void AddTransceiverTrackKindAudio()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Audio);
            Assert.NotNull(transceiver);
            Assert.IsNull(transceiver.CurrentDirection);
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.NotNull(receiver);
            MediaStreamTrack track = receiver.Track;
            Assert.NotNull(track);
            Assert.AreEqual(TrackKind.Audio, track.Kind);
            Assert.True(track is AudioStreamTrack);

            Assert.AreEqual(1, peer.GetTransceivers().Count());
            Assert.NotNull(peer.GetTransceivers().First());
        }

        [Test]
        [Category("PeerConnection")]
        public void AddTransceiverTrackKindVideo()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.NotNull(transceiver);
            Assert.IsNull(transceiver.CurrentDirection);
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.NotNull(receiver);
            MediaStreamTrack track = receiver.Track;
            Assert.NotNull(track);
            Assert.AreEqual(TrackKind.Video, track.Kind);
            Assert.True(track is VideoStreamTrack);

            Assert.AreEqual(1, peer.GetTransceivers().Count());
            Assert.NotNull(peer.GetTransceivers().First());
        }

        [Test]
        [Category("PeerConnection")]
        public void GetAndSetDirectionTransceiver()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.NotNull(transceiver);
            transceiver.Direction = RTCRtpTransceiverDirection.SendOnly;
            Assert.AreEqual(RTCRtpTransceiverDirection.SendOnly, transceiver.Direction);
            transceiver.Direction = RTCRtpTransceiverDirection.RecvOnly;
            Assert.AreEqual(RTCRtpTransceiverDirection.RecvOnly, transceiver.Direction);

            peer.Close();
            peer.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXPlayer })]
        public IEnumerator CurrentDirection()
        {
            var config = GetConfiguration();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            var audioTrack = new AudioStreamTrack("audio");

            var transceiver1 = peer1.AddTransceiver(TrackKind.Audio);
            transceiver1.Direction = RTCRtpTransceiverDirection.RecvOnly;
            Assert.IsNull(transceiver1.CurrentDirection);

            RTCOfferOptions options1 = new RTCOfferOptions {offerToReceiveAudio = true};
            RTCAnswerOptions options2 = default;
            var op1 = peer1.CreateOffer(ref options1);
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;

            var transceiver2 = peer2.GetTransceivers().First(x => x.Receiver.Track.Kind == TrackKind.Audio);
            Assert.True(transceiver2.Sender.ReplaceTrack(audioTrack));
            transceiver2.Direction = RTCRtpTransceiverDirection.SendOnly;

            var op4 = peer2.CreateAnswer(ref options2);
            yield return op4;
            desc = op4.Desc;
            var op5 = peer2.SetLocalDescription(ref desc);
            yield return op5;
            var op6 = peer1.SetRemoteDescription(ref desc);
            yield return op6;

            Assert.AreEqual(transceiver1.CurrentDirection, RTCRtpTransceiverDirection.RecvOnly);
            Assert.AreEqual(transceiver2.CurrentDirection, RTCRtpTransceiverDirection.SendOnly);

            audioTrack.Dispose();
            peer1.Close();
            peer2.Close();
            peer1.Dispose();
            peer2.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        public IEnumerator CreateOffer()
        {
            var config = GetConfiguration();
            var peer = new RTCPeerConnection(ref config);
            RTCOfferOptions options = default;
            var op = peer.CreateOffer(ref options);

            yield return op;
            Assert.True(op.IsDone);
            Assert.False(op.IsError);

            peer.Close();
            peer.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        public IEnumerator CreateAnswerFailed()
        {
            var config = GetConfiguration();
            var peer = new RTCPeerConnection(ref config);
            RTCAnswerOptions options = default;
            var op = peer.CreateAnswer(ref options);

            yield return op;
            Assert.True(op.IsDone);

            // This is failed
            Assert.True(op.IsError);
            Assert.IsNotEmpty(op.Error.message);

            peer.Close();
            peer.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        public IEnumerator CreateAnswer()
        {
            var config = GetConfiguration();

            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            var conf = new RTCDataChannelInit(true);
            peer1.CreateDataChannel("data", ref conf);

            RTCOfferOptions options1 = default;
            RTCAnswerOptions options2 = default;
            var op1 = peer1.CreateOffer(ref options1);
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            var op4 = peer2.CreateAnswer(ref options2);
            yield return op4;

            Assert.True(op4.IsDone);
            Assert.False(op4.IsError);

            peer1.Close();
            peer2.Close();
            peer1.Dispose();
            peer2.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        public IEnumerator SetLocalDescription()
        {
            var peer = new RTCPeerConnection();
            RTCOfferOptions options = default;
            var op = peer.CreateOffer(ref options);
            yield return op;
            Assert.True(op.IsDone);
            Assert.False(op.IsError);
            var desc = op.Desc;
            var op2 = peer.SetLocalDescription(ref desc);
            yield return op2;
            Assert.True(op2.IsDone);
            Assert.False(op2.IsError);

            var desc2 = peer.LocalDescription;

            Assert.AreEqual(desc.sdp, desc2.sdp);
            Assert.AreEqual(desc.type, desc2.type);

            peer.Close();
            peer.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void SetLocalDescriptionThrowException()
        {
            var peer = new RTCPeerConnection();
            RTCSessionDescription empty = new RTCSessionDescription();
            Assert.Throws<ArgumentException>(() => peer.SetLocalDescription(ref empty));

            RTCSessionDescription invalid = new RTCSessionDescription { sdp = "this is invalid parameter" };
            Assert.Throws<RTCErrorException>(() => peer.SetLocalDescription(ref invalid));

            peer.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXPlayer })]
        public IEnumerator SetRemoteDescription()
        {
            var config = GetConfiguration();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            var conf = new RTCDataChannelInit(true);
            var channel1 = peer1.CreateDataChannel("data", ref conf);

            RTCOfferOptions options1 = default;
            RTCAnswerOptions options2 = default;
            var op1 = peer1.CreateOffer(ref options1);
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            var op4 = peer2.CreateAnswer(ref options2);
            yield return op4;
            desc = op4.Desc;
            var op5 = peer2.SetLocalDescription(ref desc);
            yield return op5;
            var op6 = peer1.SetRemoteDescription(ref desc);
            yield return op6;

            var desc2 = peer1.RemoteDescription;

            Assert.AreEqual(desc.sdp, desc2.sdp);
            Assert.AreEqual(desc.type, desc2.type);

            channel1.Dispose();
            peer1.Close();
            peer2.Close();
            peer1.Dispose();
            peer2.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void SetRemoteDescriptionThrowException()
        {
            var peer = new RTCPeerConnection();
            RTCSessionDescription empty = new RTCSessionDescription();
            Assert.Throws<ArgumentException>(() => peer.SetRemoteDescription(ref empty));

            RTCSessionDescription invalid = new RTCSessionDescription { sdp = "this is invalid parameter" };
            Assert.Throws<RTCErrorException>(() => peer.SetRemoteDescription(ref invalid));

            peer.Dispose();
        }


        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        public IEnumerator SetLocalDescriptionFailed()
        {
            var peer = new RTCPeerConnection();
            var stream = new MediaStream();
            var track = new AudioStreamTrack("audio");
            var sender = peer.AddTrack(track, stream);

            RTCOfferOptions options = default;
            var op = peer.CreateOffer(ref options);
            yield return op;
            Assert.True(op.IsDone);
            Assert.False(op.IsError);
            var desc = op.Desc;
            // change sdp to cannot parse
            desc.sdp = desc.sdp.Replace("m=audio", "m=audiable");
            var op2 = peer.SetLocalDescription(ref desc);
            yield return op2;
            Assert.True(op2.IsDone);
            Assert.True(op2.IsError);
            Assert.IsNotEmpty(op2.Error.message);

            peer.RemoveTrack(sender);
            track.Dispose();
            stream.Dispose();
            peer.Close();
            peer.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        public IEnumerator SetRemoteDescriptionFailed()
        {
            var config = GetConfiguration();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            var stream = new MediaStream();
            var track = new AudioStreamTrack("audio");
            var sender = peer1.AddTrack(track, stream);

            RTCOfferOptions options1 = default;
            var op1 = peer1.CreateOffer(ref options1);
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            // change sdp to cannot parse
            desc.sdp = desc.sdp.Replace("m=audio", "m=audiable");
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            Assert.True(op3.IsDone);
            Assert.True(op3.IsError);
            Assert.IsNotEmpty(op3.Error.message);

            peer1.RemoveTrack(sender);
            track.Dispose();
            stream.Dispose();
            peer1.Close();
            peer2.Close();
            peer1.Dispose();
            peer2.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator IceConnectionStateChange()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(ref candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(ref candidate); };

            MediaStream stream = Audio.CaptureStream();
            peer1.AddTrack(stream.GetTracks().First());

            RTCOfferOptions options1 = default;
            RTCAnswerOptions options2 = default;
            var op1 = peer1.CreateOffer(ref options1);
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            var op4 = peer2.CreateAnswer(ref options2);
            yield return op4;
            desc = op4.Desc;
            var op5 = peer2.SetLocalDescription(ref desc);
            yield return op5;
            var op6 = peer1.SetRemoteDescription(ref desc);
            yield return op6;

            var op7 = new WaitUntilWithTimeout(
                () => peer1.IceConnectionState == RTCIceConnectionState.Connected ||
                      peer1.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op7;
            Assert.True(op7.IsCompleted);
            var op8 = new WaitUntilWithTimeout(
                () => peer2.IceConnectionState == RTCIceConnectionState.Connected ||
                      peer2.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op8;
            Assert.True(op8.IsCompleted);

            stream.Dispose();
            peer1.Close();
            peer2.Close();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator GetStatsReturnsReport()
        {
            var camObj = new GameObject("Camera");
            var cam = camObj.AddComponent<Camera>();
            var videoStream = cam.CaptureStream(1280, 720, 1000000);
            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.SetStream(videoStream);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);
            var op = test.component.GetPeerStats(0);
            yield return op;
            Assert.True(op.IsDone);
            Assert.IsNotEmpty(op.Value.Stats);
            Assert.IsNotEmpty(op.Value.Stats.Keys);
            Assert.IsNotEmpty(op.Value.Stats.Values);
            Assert.Greater(op.Value.Stats.Count, 0);

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.NotNull(stats);
                Assert.Greater(stats.Timestamp, 0);
                Assert.IsNotEmpty(stats.Id);
                foreach (var pair in stats.Dict)
                {
                    Assert.IsNotEmpty(pair.Key);
                }
                StatsCheck.Test(stats);
            }
            op.Value.Dispose();

            test.component.Dispose();
            foreach (var track in videoStream.GetTracks())
            {
                track.Dispose();
            }
            // wait for disposing video track.
            yield return 0;

            videoStream.Dispose();
            Object.DestroyImmediate(camObj);
        }
    }
}
