using System;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace Unity.WebRTC.RuntimeTest
{
    class PeerConnectionTest
    {
        static RTCConfiguration GetDefaultConfiguration()
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
            config.iceTransportPolicy = RTCIceTransportPolicy.All;
            config.iceCandidatePoolSize = 0;
            config.bundlePolicy = RTCBundlePolicy.BundlePolicyBalanced;
            return config;
        }

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
        public void AccessAfterDisposed()
        {
            var peer = new RTCPeerConnection();
            peer.Dispose();
            Assert.That(() => {  var state = peer.ConnectionState; }, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        [Category("PeerConnection")]
        public void GetConfiguration()
        {
            var config = GetDefaultConfiguration();
            var peer = new RTCPeerConnection(ref config);

            var config2 = peer.GetConfiguration();
            Assert.That(config.iceServers, Is.Not.Null);
            Assert.That(config2.iceServers, Is.Not.Null);
            Assert.That(config.iceServers.Length, Is.EqualTo(config2.iceServers.Length));
            Assert.That(config.iceServers[0].username, Is.EqualTo(config2.iceServers[0].username));
            Assert.That(config.iceServers[0].credential, Is.EqualTo(config2.iceServers[0].credential));
            Assert.That(config.iceServers[0].urls, Is.EqualTo(config2.iceServers[0].urls));
            Assert.That(config.iceTransportPolicy, Is.EqualTo(RTCIceTransportPolicy.All));
            Assert.That(config.iceTransportPolicy, Is.EqualTo(config2.iceTransportPolicy));
            Assert.That(config.enableDtlsSrtp, Is.Null);
            Assert.That(config.enableDtlsSrtp, Is.EqualTo(config2.enableDtlsSrtp));
            Assert.That(config.iceCandidatePoolSize, Is.EqualTo(config2.iceCandidatePoolSize));
            Assert.That(config.bundlePolicy, Is.EqualTo(config2.bundlePolicy));

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
            var config = GetDefaultConfiguration();
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

            var track = new AudioStreamTrack();
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
            peer.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void AddTransceiverTrackKindAudio()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Audio);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.That(receiver, Is.Not.Null);
            MediaStreamTrack track = receiver.Track;
            Assert.That(track, Is.Not.Null);
            Assert.That(track.Kind, Is.EqualTo(TrackKind.Audio));
            Assert.That(track, Is.TypeOf<AudioStreamTrack>());
            Assert.That(receiver.Streams, Has.Count.EqualTo(0));

            Assert.That(peer.GetTransceivers(), Has.Count.EqualTo(1));
            Assert.That(peer.GetTransceivers(), Has.All.Not.Null);

            peer.Dispose();
        }

        [Test]
        [Category("PeerConnection")]
        public void AddTransceiverTrackKindVideo()
        {
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);
            RTCRtpReceiver receiver = transceiver.Receiver;
            Assert.That(receiver, Is.Not.Null);
            MediaStreamTrack track = receiver.Track;
            Assert.That(track, Is.Not.Null);
            Assert.That(track.Kind, Is.EqualTo(TrackKind.Video));
            Assert.That(track, Is.TypeOf<VideoStreamTrack>());
            Assert.That(receiver.Streams, Has.Count.EqualTo(0));

            Assert.That(peer.GetTransceivers(), Has.Count.EqualTo(1));
            Assert.That(peer.GetTransceivers(), Has.All.Not.Null);

            peer.Dispose();
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

        [Test]
        [Category("PeerConnection")]
        public void GetTransceivers()
        {
            var peer = new RTCPeerConnection();
            var track = new AudioStreamTrack();

            var sender = peer.AddTrack(track);
            Assert.That(peer.GetTransceivers().ToList(), Has.Count.EqualTo(1));
            Assert.That(peer.GetTransceivers().Select(t => t.Sender).ToList(), Has.Member(sender));

            track.Dispose();
            peer.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXPlayer })]
        public IEnumerator CurrentDirection()
        {
            var config = GetDefaultConfiguration();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            var audioTrack = new AudioStreamTrack();

            var transceiver1 = peer1.AddTransceiver(TrackKind.Audio);
            transceiver1.Direction = RTCRtpTransceiverDirection.RecvOnly;
            Assert.IsNull(transceiver1.CurrentDirection);

            var op1 = peer1.CreateOffer();
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;

            var transceiver2 = peer2.GetTransceivers().First(x => x.Receiver.Track.Kind == TrackKind.Audio);
            Assert.True(transceiver2.Sender.ReplaceTrack(audioTrack));
            transceiver2.Direction = RTCRtpTransceiverDirection.SendOnly;

            var op4 = peer2.CreateAnswer();
            yield return op4;
            desc = op4.Desc;
            var op5 = peer2.SetLocalDescription(ref desc);
            yield return op5;
            var op6 = peer1.SetRemoteDescription(ref desc);
            yield return op6;

            Assert.AreEqual(transceiver1.CurrentDirection, RTCRtpTransceiverDirection.RecvOnly);
            Assert.AreEqual(transceiver2.CurrentDirection, RTCRtpTransceiverDirection.SendOnly);

            //Assert.That(transceiver2.Stop(), Is.EqualTo(RTCErrorType.None));
            //Assert.That(transceiver2.Direction, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));

            // todo(kazuki):: Transceiver.CurrentDirection of Sender is not changed to "Stopped" even if waiting
            // yield return new WaitUntil(() => transceiver2.CurrentDirection == RTCRtpTransceiverDirection.Stopped);
            // Assert.That(transceiver2.CurrentDirection, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));

            // todo(kazuki):: Transceiver.CurrentDirection of Receiver is not changed to "Stopped" even if waiting
            // yield return new WaitUntil(() => transceiver1.Direction == RTCRtpTransceiverDirection.Stopped);
            // Assert.That(transceiver1.Direction, Is.EqualTo(RTCRtpTransceiverDirection.Stopped));

            audioTrack.Dispose();
            peer1.Close();
            peer2.Close();
            peer1.Dispose();
            peer2.Dispose();
        }


        [UnityTest]
        [Timeout(5000)]
        public IEnumerator TransceiverReturnsSender()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            AudioStreamTrack track1 = new AudioStreamTrack();
            peer1.AddTrack(track1);

            yield return SignalingOffer(peer1, peer2);

            Assert.That(peer2.GetTransceivers().Count(), Is.EqualTo(1));
            RTCRtpSender sender1 = peer2.GetTransceivers().First().Sender;
            Assert.That(sender1, Is.Not.Null);

            AudioStreamTrack track2 = new AudioStreamTrack();
            RTCRtpSender sender2 = peer2.AddTrack(track2);
            Assert.That(sender2, Is.Not.Null);
            Assert.That(sender1, Is.EqualTo(sender2));

            track1.Dispose();
            track2.Dispose();
            peer1.Dispose();
            peer2.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
        [Category("PeerConnection")]
        public IEnumerator CreateOffer()
        {
            var config = GetDefaultConfiguration();
            var peer = new RTCPeerConnection(ref config);
            var op = peer.CreateOffer();

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
            var config = GetDefaultConfiguration();
            var peer = new RTCPeerConnection(ref config);
            var op = peer.CreateAnswer();

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
            var config = GetDefaultConfiguration();

            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            peer1.CreateDataChannel("data");

            var op1 = peer1.CreateOffer();
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            var op4 = peer2.CreateAnswer();
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
            var op = peer.CreateOffer();
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
            var config = GetDefaultConfiguration();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            var channel1 = peer1.CreateDataChannel("data");

            var op1 = peer1.CreateOffer();
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            var op4 = peer2.CreateAnswer();
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
            var track = new AudioStreamTrack();
            var sender = peer.AddTrack(track, stream);

            var op = peer.CreateOffer();
            yield return op;
            Assert.True(op.IsDone);
            Assert.False(op.IsError);
            var desc = op.Desc;
            // change sdp to cannot parse
            desc.sdp = desc.sdp.Replace("a=mid:0", "a=mid:10");
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
            var config = GetDefaultConfiguration();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            var stream = new MediaStream();
            var track = new AudioStreamTrack();
            var sender = peer1.AddTrack(track, stream);

            var op1 = peer1.CreateOffer();
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            // change sdp to cannot parse
            desc.sdp = desc.sdp.Replace("a=mid:0", "a=mid:10");
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

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            var track = new AudioStreamTrack();
            peer1.AddTrack(track);

            var op1 = peer1.CreateOffer();
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            var op4 = peer2.CreateAnswer();
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

            track.Dispose();
            peer1.Close();
            peer2.Close();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator AddIceCandidate()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            var peer1ReceiveCandidateQueue = new Queue<RTCIceCandidate>();
            var peer2ReceiveCandidateQueue = new Queue<RTCIceCandidate>();

            peer1.OnIceCandidate = candidate => { peer2ReceiveCandidateQueue.Enqueue(candidate); };
            peer2.OnIceCandidate = candidate => { peer1ReceiveCandidateQueue.Enqueue(candidate); };

            var track = new AudioStreamTrack();
            peer1.AddTrack(track);

            var op1 = peer1.CreateOffer();
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;

            yield return new WaitUntil(() => peer2ReceiveCandidateQueue.Any());

            Assert.That(peer2.AddIceCandidate(peer2ReceiveCandidateQueue.Peek()), Is.False);

            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;

            Assert.That(peer2.AddIceCandidate(peer2ReceiveCandidateQueue.Dequeue()), Is.True);

            var op4 = peer2.CreateAnswer();
            yield return op4;
            desc = op4.Desc;
            var op5 = peer2.SetLocalDescription(ref desc);
            yield return op5;

            yield return new WaitUntil(() => peer1ReceiveCandidateQueue.Any());

            Assert.That(peer1.AddIceCandidate(peer1ReceiveCandidateQueue.Peek()), Is.False);

            var op6 = peer1.SetRemoteDescription(ref desc);
            yield return op6;

            Assert.That(peer1.AddIceCandidate(peer1ReceiveCandidateQueue.Dequeue()), Is.True);

            var op7 = new WaitUntilWithTimeout(
                () => peer1.IceConnectionState == RTCIceConnectionState.Connected ||
                      peer1.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op7;
            Assert.That(op7.IsCompleted, Is.True);
            var op8 = new WaitUntilWithTimeout(
                () => peer2.IceConnectionState == RTCIceConnectionState.Connected ||
                      peer2.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op8;
            Assert.That(op8.IsCompleted, Is.True);

            foreach (var candidate in peer1ReceiveCandidateQueue)
            {
                Assert.That(peer1.AddIceCandidate(candidate), Is.True);
            }

            peer1ReceiveCandidateQueue.Clear();

            foreach (var candidate in peer2ReceiveCandidateQueue)
            {
                Assert.That(peer2.AddIceCandidate(candidate), Is.True);
            }

            peer2ReceiveCandidateQueue.Clear();

            track.Dispose();
            peer1.Close();
            peer2.Close();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator MediaStreamTrackThrowExceptionAfterPeerDisposed()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            AudioStreamTrack track = new AudioStreamTrack();
            peer1.AddTrack(track);

            MediaStreamTrack track1 = null;
            peer2.OnTrack = e => { track1 = e.Track; };

            yield return SignalingOffer(peer1, peer2);

            Assert.That(track1, Is.Not.Null);
            peer2.Dispose();
            Assert.That(() => track1.Id, Throws.TypeOf<InvalidOperationException>());
            track.Dispose();
            track1.Dispose();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator PeerConnectionStateChange()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            RTCPeerConnectionState state1 = default;
            RTCPeerConnectionState state2 = default;
            RTCIceConnectionState iceState1 = default;
            RTCIceConnectionState iceState2 = default;

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };
            peer1.OnConnectionStateChange = state => { state1 = state; };
            peer2.OnConnectionStateChange = state => { state2 = state; };
            peer1.OnIceConnectionChange = state => { iceState1 = state; };
            peer2.OnIceConnectionChange = state => { iceState2 = state; };

            Assert.That(state1, Is.EqualTo(RTCPeerConnectionState.New));
            Assert.That(state2, Is.EqualTo(RTCPeerConnectionState.New));

            AudioStreamTrack track1 = new AudioStreamTrack();
            peer1.AddTrack(track1);

            var op1 = peer1.CreateOffer();
            yield return op1;
            var desc = op1.Desc;
            var op2 = peer1.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref desc);
            yield return op3;
            var op4 = peer2.CreateAnswer();
            yield return op4;
            desc = op4.Desc;
            var op5 = peer2.SetLocalDescription(ref desc);
            yield return op5;
            var op6 = peer1.SetRemoteDescription(ref desc);
            yield return op6;

            var op7 = new WaitUntilWithTimeout( () =>
                    state1 == RTCPeerConnectionState.Connected &&
                    state2 == RTCPeerConnectionState.Connected, 5000);
            yield return op7;
            Assert.That(op7.IsCompleted, Is.True);

            var op8 = new WaitUntilWithTimeout(() =>
                (iceState1 == RTCIceConnectionState.Connected || iceState1 == RTCIceConnectionState.Completed) &&
                (iceState2 == RTCIceConnectionState.Connected || iceState2 == RTCIceConnectionState.Completed)
                , 5000);
            yield return op8;
            Assert.That(op8.IsCompleted, Is.True);

            peer1.Close();

            var op9 = new WaitUntilWithTimeout(() =>
                state1 == RTCPeerConnectionState.Closed &&
                iceState2 == RTCIceConnectionState.Disconnected, 5000);
            yield return op9;
            Assert.That(op9.IsCompleted, Is.True);

            track1.Dispose();
            peer2.Close();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator GetStatsReturnsReport()
        {
            if (SystemInfo.processorType == "Apple M1")
                Assert.Ignore("todo:: This test will hang up on Apple M1");

            var stream = new MediaStream();

            var go = new GameObject("Test");
            var cam = go.AddComponent<Camera>();
            stream.AddTrack(cam.CaptureStreamTrack(1280, 720, 0));

            var source = go.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            stream.AddTrack(new AudioStreamTrack(source));

            yield return new WaitForSeconds(0.1f);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddStream(0, stream);
            yield return test;
            test.component.CoroutineUpdate();
            yield return new WaitForSeconds(0.1f);
            var op = test.component.GetPeerStats(0);
            yield return op;
            Assert.That(op.IsDone, Is.True);
            Assert.That(op.Value.Stats, Is.Not.Empty);
            Assert.That(op.Value.Stats.Keys, Is.Not.Empty);
            Assert.That(op.Value.Stats.Values, Is.Not.Empty);
            Assert.That(op.Value.Stats.Count, Is.GreaterThan(0));

            foreach (RTCStats stats in op.Value.Stats.Values)
            {
                Assert.That(stats, Is.Not.Null);
                Assert.That(stats.Timestamp, Is.GreaterThan(0));
                Assert.That(stats.Id, Is.Not.Empty);
                foreach (var pair in stats.Dict)
                {
                    Assert.That(pair.Key, Is.Not.Empty);
                }
                StatsCheck.Test(stats);
            }
            op.Value.Dispose();

            test.component.Dispose();
            foreach (var track in stream.GetTracks())
            {
                track.Dispose();
            }
            stream.Dispose();
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator RestartIceInvokeOnNegotiationNeeded()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            AudioStreamTrack track = new AudioStreamTrack();
            peer1.AddTrack(track);

            yield return SignalingOffer(peer1, peer2);

            bool isInvokeOnNegotiationNeeded1 = false;
            bool isInvokeOnNegotiationNeeded2 = false;

            peer1.OnNegotiationNeeded = () => isInvokeOnNegotiationNeeded1 = true;
            peer2.OnNegotiationNeeded = () => isInvokeOnNegotiationNeeded2 = true;

            peer1.RestartIce();
            var op9 = new WaitUntilWithTimeout(() => isInvokeOnNegotiationNeeded1, 5000);
            yield return op9;
            Assert.That(op9.IsCompleted, Is.True);

            peer2.RestartIce();
            var op10 = new WaitUntilWithTimeout(() => isInvokeOnNegotiationNeeded2, 5000);
            yield return op10;
            Assert.That(op10.IsCompleted, Is.True);

            track.Dispose();
            peer1.Close();
            peer2.Close();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator RemoteOnRemoveTrack()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            var stream = new MediaStream();
            MediaStream receiveStream = null;
            var track = new AudioStreamTrack();
            stream.AddTrack(track);
            RTCRtpSender sender = peer1.AddTrack(track, stream);

            bool isInvokeNegotiationNeeded1 = false;
            peer1.OnNegotiationNeeded = () => isInvokeNegotiationNeeded1 = true;

            bool isInvokeOnRemoveTrack = false;
            peer2.OnTrack = e =>
            {
                Assert.That(e.Streams, Has.Count.EqualTo(1));
                receiveStream = e.Streams.First();
                receiveStream.OnRemoveTrack = ev => isInvokeOnRemoveTrack = true;
            };

            yield return SignalingOffer(peer1, peer2);

            peer1.RemoveTrack(sender);

            var op9 = new WaitUntilWithTimeout(() => isInvokeNegotiationNeeded1, 5000);
            yield return op9;
            Assert.That(op9.IsCompleted, Is.True);

            yield return SignalingOffer(peer1, peer2);

            var op10 = new WaitUntilWithTimeout(() => isInvokeOnRemoveTrack, 5000);
            yield return op10;
            Assert.That(op10.IsCompleted, Is.True);

            stream.Dispose();
            receiveStream.Dispose();
            track.Dispose();
            peer1.Dispose();
            peer2.Dispose();
        }

        private IEnumerator SignalingOffer(RTCPeerConnection @from, RTCPeerConnection to)
        {
            var op1 = @from.CreateOffer();
            yield return op1;
            var desc = op1.Desc;
            var op2 = @from.SetLocalDescription(ref desc);
            yield return op2;
            var op3 = to.SetRemoteDescription(ref desc);
            yield return op3;
            var op4 = to.CreateAnswer();
            yield return op4;
            desc = op4.Desc;
            var op5 = to.SetLocalDescription(ref desc);
            yield return op5;
            var op6 = @from.SetRemoteDescription(ref desc);
            yield return op6;

            var op7 = new WaitUntilWithTimeout(
                () => @from.IceConnectionState == RTCIceConnectionState.Connected ||
                      @from.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op7;
            Assert.That(op7.IsCompleted, Is.True);
            var op8 = new WaitUntilWithTimeout(
                () => to.IceConnectionState == RTCIceConnectionState.Connected ||
                      to.IceConnectionState == RTCIceConnectionState.Completed, 5000);
            yield return op8;
            Assert.That(op8.IsCompleted, Is.True);
        }
    }
}
