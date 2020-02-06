using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unity.WebRTC;

namespace Unity.WebRTC.RuntimeTest
{
    class PeerConnectionTest
    {
        static RTCConfiguration GetConfiguration()
        {
            RTCConfiguration config = default;
            config.iceServers = new RTCIceServer[]
            {
                new RTCIceServer
                {
                    urls = new string[] {"stun:stun.l.google.com:19302"},
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
            WebRTC.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Finalize();
        }

        [Test]
        [Category("PeerConnection")]
        public void PeerConnection_Construct()
        {
            var peer = new RTCPeerConnection();
            peer.Close();
        }

        [Test]
        [Category("PeerConnection")]
        public void PeerConnection_ConstructWithConfig()
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
        }

        [Test]
        [Category("PeerConnection")]
        public void PeerConnection_SetConfiguration()
        {
            var peer = new RTCPeerConnection();
            var config = GetConfiguration();
            var result = peer.SetConfiguration(ref config);
            Assert.AreEqual(RTCErrorType.None, result);
        }

        [UnityTest]
        [Category("PeerConnection")]

        public IEnumerator PeerConnection_CreateOffer()
        {
            var config = GetConfiguration();
            var peer = new RTCPeerConnection(ref config);
            RTCOfferOptions options = default;
            var op = peer.CreateOffer(ref options);

            yield return op;
            Assert.True(op.isDone);
            Assert.False(op.isError);

            peer.Close();
        }

        [UnityTest]
        [Category("PeerConnection")]

        public IEnumerator PeerConnection_SetLocalDescription()
        {
            var peer = new RTCPeerConnection();
            RTCOfferOptions options = default;
            var op = peer.CreateOffer(ref options);
            yield return op;
            Assert.True(op.isDone);
            Assert.False(op.isError);
            var op2 = peer.SetLocalDescription(ref op.desc);
            yield return op2;
            Assert.True(op2.isDone);
            Assert.False(op2.isError);
            peer.Close();
        }

        [UnityTest]
        [Category("PeerConnection")]
        public IEnumerator PeerConnection_SetRemoteDescription()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            RTCDataChannel channel1 = null;

            var conf = new RTCDataChannelInit(true);
            channel1 = peer1.CreateDataChannel("data", ref conf);

            RTCOfferOptions options1 = default;
            RTCAnswerOptions options2 = default;
            var op1 = peer1.CreateOffer(ref options1);
            yield return op1;
            var op2 = peer1.SetLocalDescription(ref op1.desc);
            yield return op2;
            var op3 = peer2.SetRemoteDescription(ref op1.desc);
            yield return op3;
            var op4 = peer2.CreateAnswer(ref options2);
            yield return op4;
            var op5 = peer2.SetLocalDescription(ref op4.desc);
            yield return op5;
            var op6 = peer1.SetRemoteDescription(ref op4.desc);
            yield return op6;

            channel1.Dispose();
            peer1.Dispose();
            peer2.Dispose();
        }
    }
}
