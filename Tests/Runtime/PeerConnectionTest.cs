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

        [Test]
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
        public void AccessAfterDisposed()
        {
            var peer = new RTCPeerConnection();
            peer.Dispose();
            Assert.That(() => { var state = peer.ConnectionState; }, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
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
            Assert.That(config.iceCandidatePoolSize, Is.EqualTo(config2.iceCandidatePoolSize));
            Assert.That(config.bundlePolicy, Is.EqualTo(config2.bundlePolicy));

            peer.Close();
            peer.Dispose();
        }

        [Test]
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
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void AddTrack()
        {
            var peer = new RTCPeerConnection();
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();

            var track = new VideoStreamTrack(rt);
            var sender = peer.AddTrack(track);

            Assert.That(sender, Is.Not.Null);
            Assert.That(track, Is.EqualTo(sender.Track));

            RTCRtpSendParameters parameters = sender.GetParameters();
            Assert.That(parameters, Is.Not.Null);
            Assert.That(parameters.encodings[0].active, Is.True);
            Assert.That(parameters.transactionId, Is.Not.Empty);

            track.Dispose();
            peer.Dispose();
            Object.DestroyImmediate(rt);
        }

        [Test]
        public void AddTrackThrowException()
        {
            var peer = new RTCPeerConnection();
            Assert.Throws<ArgumentNullException>(() => peer.AddTrack(null));
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void AddTransceiver()
        {
            var peer = new RTCPeerConnection();
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();

            var track = new VideoStreamTrack(rt);
            Assert.That(peer.GetTransceivers(), Is.Empty);
            var transceiver = peer.AddTransceiver(track);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.Mid, Is.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);
            RTCRtpSender sender = transceiver.Sender;
            Assert.That(sender, Is.Not.Null);
            Assert.That(track, Is.EqualTo(sender.Track));

            RTCRtpSendParameters parameters = sender.GetParameters();
            Assert.That(parameters, Is.Not.Null);
            Assert.That(parameters.encodings[0].active, Is.True);
            Assert.That(parameters.transactionId, Is.Not.Empty);
            Assert.That(peer.GetTransceivers(), Has.Count.EqualTo(1));
            Assert.That(peer.GetTransceivers().First(), Is.Not.Null);
            Assert.That(parameters.codecs, Is.Empty);
            Assert.That(parameters.rtcp, Is.Not.Null);

            // Some platforms return an empty list
            Assert.That(parameters.headerExtensions, Is.Not.Null);
            foreach (var extension in parameters.headerExtensions)
            {
                Assert.That(extension, Is.Not.Null);
                Assert.That(extension.uri, Is.Not.Empty);
            }

            track.Dispose();
            peer.Dispose();
            Object.DestroyImmediate(rt);
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void GetTransceiversReturnsNotEmptyAfterDisposingTransceiver()
        {
            // `RTCPeerConnection.AddTransceiver` method is not intuitive. Moreover, we don't have the API to remove
            // the transceiver from RTCPeerConnection directly.
            var peer = new RTCPeerConnection();
            var transceiver = peer.AddTransceiver(TrackKind.Video);
            Assert.That(peer.GetTransceivers(), Has.Count.EqualTo(1));
            transceiver.Dispose();
            Assert.That(peer.GetTransceivers(), Has.Count.EqualTo(1));
            peer.Dispose();
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void GetTransceiversReturnsNotEmptyAfterCallingRemoveTrack()
        {
            // Also, `RTCPeerConnection.AddTrack` and `RTCPeerConnection.RemoveTrack` method is not intuitive.
            var peer = new RTCPeerConnection();
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack(rt);
            var sender = peer.AddTrack(track);
            Assert.That(peer.GetTransceivers(), Has.Count.EqualTo(1));
            Assert.That(peer.RemoveTrack(sender), Is.EqualTo(RTCErrorType.None));
            Assert.That(peer.GetTransceivers(), Has.Count.EqualTo(1));
            peer.Dispose();
        }


        [Test]
        public void AddTransceiverThrowException()
        {
            var peer = new RTCPeerConnection();
            Assert.Throws<ArgumentNullException>(() => peer.AddTransceiver(null));
        }


        [Test]
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
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
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
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void AddTransceiverWithInit()
        {
            var peer = new RTCPeerConnection();
            var stream = new MediaStream();
            var direction = RTCRtpTransceiverDirection.SendOnly;
            var width = 256;
            var height = 256;
            var format = WebRTC.GetSupportedRenderTextureFormat(UnityEngine.SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, 0, format);
            rt.Create();
            var track = new VideoStreamTrack(rt);
            var init = new RTCRtpTransceiverInit()
            {
                direction = direction,
                sendEncodings = new RTCRtpEncodingParameters[] {
                    new RTCRtpEncodingParameters { maxFramerate = 30 }
                },
                streams = new MediaStream[] { stream }
            };
            var transceiver = peer.AddTransceiver(track, init);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);
            Assert.That(transceiver.Direction, Is.EqualTo(RTCRtpTransceiverDirection.SendOnly));
            Assert.That(transceiver.Sender, Is.Not.Null);

            var parameters = transceiver.Sender.GetParameters();
            Assert.That(parameters, Is.Not.Null);
            Assert.That(parameters.codecs, Is.Not.Null.And.Empty);
            peer.Dispose();
        }

        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public void AddTransceiverWithKindAndInit()
        {
            var peer = new RTCPeerConnection();
            var stream = new MediaStream();
            var direction = RTCRtpTransceiverDirection.SendOnly;
            var init = new RTCRtpTransceiverInit()
            {
                direction = direction,
                sendEncodings = new RTCRtpEncodingParameters[] {
                    new RTCRtpEncodingParameters { maxFramerate = 30 }
                },
                streams = new MediaStream[] { stream }
            };
            var transceiver = peer.AddTransceiver(TrackKind.Video, init);
            Assert.That(transceiver, Is.Not.Null);
            Assert.That(transceiver.CurrentDirection, Is.Null);
            Assert.That(transceiver.Direction, Is.EqualTo(RTCRtpTransceiverDirection.SendOnly));
            Assert.That(transceiver.Sender, Is.Not.Null);

            var parameters = transceiver.Sender.GetParameters();
            Assert.That(parameters, Is.Not.Null);
            Assert.That(parameters.codecs, Is.Not.Null.And.Empty);

            var init2 = new RTCRtpTransceiverInit()
            {
                direction = null,
                sendEncodings = null,
                streams = null
            };
            var transceiver2 = peer.AddTransceiver(TrackKind.Video, init2);
            Assert.That(transceiver2, Is.Not.Null);
            Assert.That(transceiver2.CurrentDirection, Is.Null);
            Assert.That(transceiver2.Direction, Is.EqualTo(RTCRtpTransceiverDirection.SendRecv));
            Assert.That(transceiver2.Sender, Is.Not.Null);
            peer.Dispose();
        }


        [Test]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
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
        public void GetTransceivers()
        {
            var peer = new RTCPeerConnection();
            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var track = new AudioStreamTrack(source);

            var sender = peer.AddTrack(track);
            Assert.That(peer.GetTransceivers().ToList(), Has.Count.EqualTo(1));
            Assert.That(peer.GetTransceivers().Select(t => t.Sender).ToList(), Has.Member(sender));

            track.Dispose();
            peer.Dispose();
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(1000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.OSXPlayer })]
        public IEnumerator CurrentDirection()
        {
            var config = GetDefaultConfiguration();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var audioTrack = new AudioStreamTrack(source);

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
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }


        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator TransceiverReturnsSender()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            var obj1 = new GameObject("audio1");
            var source1 = obj1.AddComponent<AudioSource>();
            source1.clip = AudioClip.Create("test1", 480, 2, 48000, false);
            AudioStreamTrack track1 = new AudioStreamTrack(source1);
            peer1.AddTrack(track1);

            yield return SignalingOffer(peer1, peer2);

            Assert.That(peer2.GetTransceivers().Count(), Is.EqualTo(1));
            RTCRtpSender sender1 = peer2.GetTransceivers().First().Sender;
            Assert.That(sender1, Is.Not.Null);

            var obj2 = new GameObject("audio2");
            var source2 = obj2.AddComponent<AudioSource>();
            source2.clip = AudioClip.Create("test2", 480, 2, 48000, false);
            AudioStreamTrack track2 = new AudioStreamTrack(source2);
            RTCRtpSender sender2 = peer2.AddTrack(track2);
            Assert.That(sender2, Is.Not.Null);
            Assert.That(sender1, Is.EqualTo(sender2));

            track1.Dispose();
            track2.Dispose();
            peer1.Dispose();
            peer2.Dispose();
            Object.DestroyImmediate(source1.clip);
            Object.DestroyImmediate(source2.clip);
            Object.DestroyImmediate(obj1);
            Object.DestroyImmediate(obj2);
        }

        [UnityTest]
        [Timeout(1000)]
        public IEnumerator CreateOffer()
        {
            var config = GetDefaultConfiguration();
            var peer = new RTCPeerConnection(ref config);
            var op = peer.CreateOffer(ref RTCOfferAnswerOptions.Default);

            yield return op;
            Assert.True(op.IsDone);
            Assert.False(op.IsError);

            peer.Close();
            peer.Dispose();
        }

        [UnityTest]
        [Timeout(1000)]
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
            var op4 = peer2.CreateAnswer(ref RTCOfferAnswerOptions.Default);
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

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SetDescriptionInParallel()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            var neededNegotiationPeer1 = false;
            var neededNegotiationPeer2 = false;
            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };
            peer1.OnNegotiationNeeded += () => neededNegotiationPeer1 = true;
            peer2.OnNegotiationNeeded += () => neededNegotiationPeer2 = true;

            peer1.AddTransceiver(TrackKind.Audio);

            yield return new WaitUntil(() => neededNegotiationPeer1);

            var op1 = peer1.SetLocalDescription();
            yield return op1;
            RTCSessionDescription desc = peer1.LocalDescription;
            Assert.That(desc.type, Is.EqualTo(RTCSdpType.Offer));
            Assert.That(peer1.SignalingState, Is.EqualTo(RTCSignalingState.HaveLocalOffer));

            peer2.AddTransceiver(TrackKind.Audio);
            yield return new WaitUntil(() => neededNegotiationPeer2);

            var op2 = peer2.SetLocalDescription();
            var op3 = peer2.SetRemoteDescription(ref desc);

            yield return op2;
            yield return op3;
            Assert.That(peer2.RemoteDescription.type, Is.EqualTo(RTCSdpType.Offer));
            Assert.That(peer2.SignalingState, Is.EqualTo(RTCSignalingState.HaveRemoteOffer));

            var op4 = peer2.SetLocalDescription();
            yield return op4;
            desc = peer2.LocalDescription;
            Assert.That(desc.type, Is.EqualTo(RTCSdpType.Answer));
            Assert.That(peer2.SignalingState, Is.EqualTo(RTCSignalingState.Stable));

            var op5 = peer1.SetRemoteDescription(ref desc);
            yield return op5;

            peer1.Close();
            peer2.Close();
        }


        [UnityTest]
        [Timeout(5000)]
        public IEnumerator CreateDescriptionInParallel()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            var neededNegotiationPeer1 = false;
            var neededNegotiationPeer2 = false;
            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };
            peer1.OnNegotiationNeeded += () => neededNegotiationPeer1 = true;
            peer2.OnNegotiationNeeded += () => neededNegotiationPeer2 = true;

            peer1.AddTransceiver(TrackKind.Audio);

            yield return new WaitUntil(() => neededNegotiationPeer1);

            var op1 = peer1.SetLocalDescription();
            yield return op1;
            RTCSessionDescription desc = peer1.LocalDescription;
            Assert.That(desc.type, Is.EqualTo(RTCSdpType.Offer));
            Assert.That(peer1.SignalingState, Is.EqualTo(RTCSignalingState.HaveLocalOffer));

            peer2.AddTransceiver(TrackKind.Audio);
            yield return new WaitUntil(() => neededNegotiationPeer2);

            var op2 = peer2.SetRemoteDescription(ref desc);
            yield return op2;
            Assert.That(peer2.RemoteDescription.type, Is.EqualTo(RTCSdpType.Offer));
            Assert.That(peer2.SignalingState, Is.EqualTo(RTCSignalingState.HaveRemoteOffer));

            var op3 = peer2.CreateOffer();
            var op4 = peer2.CreateAnswer();
            yield return op3;
            yield return op4;
            desc = op4.Desc;
            var op5 = peer2.SetLocalDescription(ref desc);
            yield return op5;
            Assert.That(desc.type, Is.EqualTo(RTCSdpType.Answer));
            Assert.That(peer2.SignalingState, Is.EqualTo(RTCSignalingState.Stable));

            var op6 = peer1.SetRemoteDescription(ref desc);
            yield return op6;

            peer1.Close();
            peer2.Close();
        }

        [Test]
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
        public IEnumerator SetLocalDescriptionFailed()
        {
            var peer = new RTCPeerConnection();
            var stream = new MediaStream();
            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var track = new AudioStreamTrack(source);
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
            Assert.That(peer.RemoveTrack(sender), Is.EqualTo(RTCErrorType.None));
            track.Dispose();
            stream.Dispose();
            peer.Close();
            peer.Dispose();
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(1000)]
        public IEnumerator SetRemoteDescriptionFailed()
        {
            var config = GetDefaultConfiguration();
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            var stream = new MediaStream();
            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var track = new AudioStreamTrack(source);
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
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator IceConnectionStateChange()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var track = new AudioStreamTrack(source);
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
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator AddIceCandidate()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            var peer1ReceiveCandidateQueue = new Queue<RTCIceCandidate>();
            var peer2ReceiveCandidateQueue = new Queue<RTCIceCandidate>();

            peer1.OnIceCandidate = candidate => { peer2ReceiveCandidateQueue.Enqueue(candidate); };
            peer2.OnIceCandidate = candidate => { peer1ReceiveCandidateQueue.Enqueue(candidate); };

            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var track = new AudioStreamTrack(source);
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
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator MediaStreamTrackThrowExceptionAfterPeerDisposed()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            AudioStreamTrack track = new AudioStreamTrack(source);
            peer1.AddTrack(track);

            MediaStreamTrack track1 = null;
            peer2.OnTrack = e => { track1 = e.Track; };

            yield return SignalingOffer(peer1, peer2);

            Assert.That(track1, Is.Not.Null);
            peer2.Dispose();
            track.Dispose();
            track1.Dispose();
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
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

            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            AudioStreamTrack track1 = new AudioStreamTrack(source);
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

            var op7 = new WaitUntilWithTimeout(() =>
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
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(5000)]
        [ConditionalIgnore(ConditionalIgnore.UnsupportedPlatformOpenGL, "Not support VideoStreamTrack for OpenGL")]
        public IEnumerator GetStatsReturnsReport()
        {
            var go1 = new GameObject("Test1");
            var source1 = go1.AddComponent<AudioSource>();
            source1.clip = AudioClip.Create("test1", 480, 2, 48000, false);
            var track1 = new AudioStreamTrack(source1);

            var go2 = new GameObject("Test2");
            var source2 = go2.AddComponent<AudioSource>();
            source2.clip = AudioClip.Create("test2", 480, 2, 48000, false);
            var track2 = new AudioStreamTrack(source2);

            var test = new MonoBehaviourTest<SignalingPeers>();
            test.component.AddTransceiver(0, track1);
            test.component.AddTransceiver(1, track2);
            yield return test;
            test.component.CoroutineUpdate();

            var op1 = test.component.GetSenderStats(0, 0);
            var op2 = test.component.GetReceiverStats(0, 0);
            var op3 = test.component.GetSenderStats(1, 0);
            var op4 = test.component.GetReceiverStats(1, 0);

            var ops = new[] { op1, op2, op3, op4 };
            foreach (var op in ops)
            {
                yield return op;
            }
            foreach (var op in ops)
            {
                Assert.That(op.IsDone, Is.True);
                Assert.That(op.Value, Is.Not.Null);
                Assert.That(op.Value.Stats, Is.Not.Null);
                op.Value.Dispose();
            }
            test.component.Dispose();
            track1.Dispose();
            track2.Dispose();
            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator RestartIceInvokeOnNegotiationNeeded()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            AudioStreamTrack track = new AudioStreamTrack(source);
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
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator RemoteOnRemoveTrack()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };

            var stream = new MediaStream();
            MediaStream receiveStream = null;

            var obj = new GameObject("audio");
            var source = obj.AddComponent<AudioSource>();
            source.clip = AudioClip.Create("test", 480, 2, 48000, false);
            var track = new AudioStreamTrack(source);
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
            Object.DestroyImmediate(source.clip);
            Object.DestroyImmediate(obj);
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
