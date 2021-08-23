using System;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Object = UnityEngine.Object;

namespace Unity.WebRTC.RuntimeTest
{
    class DataChannelTest
    {
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
        public void CreateDataChannel()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};
            var peer = new RTCPeerConnection(ref config);
            var channel1 = peer.CreateDataChannel("test1");
            Assert.AreEqual("test1", channel1.Label);

            // It is return -1 when channel is not connected.
            Assert.AreEqual(channel1.Id, -1);

            channel1.Close();
            peer.Close();
        }

        [Test]
        public void CreateDataChannelWithOption()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer = new RTCPeerConnection(ref config);
            var options = new RTCDataChannelInit
            {
                id = 231,
                maxRetransmits = 1,
                maxPacketLifeTime = null,
                negotiated = false,
                ordered = false,
                protocol = ""
            };
            var channel1 = peer.CreateDataChannel("test1", options);
            Assert.AreEqual("test1", channel1.Label);
            Assert.AreEqual("", channel1.Protocol);
            Assert.NotZero(channel1.MaxRetransmitTime);
            Assert.NotZero(channel1.MaxRetransmits);
            Assert.False(channel1.Ordered);
            Assert.False(channel1.Negotiated);

            // It is return -1 when channel is not connected.
            Assert.AreEqual(channel1.Id, -1);

            channel1.Close();
            peer.Close();
        }

        [Test]
        public void CreateDataChannelFailed()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
            var peer = new RTCPeerConnection(ref config);

            // Cannot be set along with "maxRetransmits" and "maxPacketLifeTime"
            var options = new RTCDataChannelInit
            {
                id = 231,
                maxRetransmits = 1,
                maxPacketLifeTime = 1,
                negotiated = false,
                ordered = false,
                protocol = ""
            };
            Assert.Throws<ArgumentException>(() => peer.CreateDataChannel("test1", options));
            peer.Close();
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SendThrowsExceptionAfterClose()
        {
            var test = new MonoBehaviourTest<SignalingPeers>();
            RTCDataChannel channel = test.component.CreateDataChannel(0, "test");
            yield return test;
            byte[] message1 = { 1, 2, 3 };
            string message2 = "123";

            var op1 = new WaitUntilWithTimeout(() => channel.ReadyState == RTCDataChannelState.Open, 5000);
            yield return op1;
            Assert.That(op1.IsCompleted, Is.True);
            Assert.That(() => channel.Send(message1), Throws.Nothing);
            Assert.That(() => channel.Send(message2), Throws.Nothing);
            channel.Close();
            Assert.That(() => channel.Send(message1), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message2), Throws.TypeOf<InvalidOperationException>());
            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator EventsAreSentToOther()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] {new RTCIceServer {urls = new[] {"stun:stun.l.google.com:19302"}}};
            var peer1 = new RTCPeerConnection(ref config);
            var peer2 = new RTCPeerConnection(ref config);
            RTCDataChannel channel2 = null;

            peer1.OnIceCandidate = candidate => { peer2.AddIceCandidate(candidate); };
            peer2.OnIceCandidate = candidate => { peer1.AddIceCandidate(candidate); };
            peer2.OnDataChannel = channel => { channel2 = channel; };

            var channel1 = peer1.CreateDataChannel("data");
            bool channel1Opened = false;
            bool channel1Closed = false;
            channel1.OnOpen = () => { channel1Opened = true; };
            channel1.OnClose = () => { channel1Closed = true; };

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
            var op9 = new WaitUntilWithTimeout(() => channel2 != null, 5000);
            yield return op9;
            Assert.True(op9.IsCompleted);

            Assert.True(channel1Opened);
            Assert.AreEqual(channel1.Label, channel2.Label);
            Assert.AreEqual(channel1.Id, channel2.Id);

            const string message1 = "hello";
            string message2 = null;
            channel2.OnMessage = bytes => { message2 = System.Text.Encoding.UTF8.GetString(bytes); };
            channel1.Send(message1);
            var op10 = new WaitUntilWithTimeout(() => !string.IsNullOrEmpty(message2), 5000);
            yield return op10;
            Assert.True(op10.IsCompleted);
            Assert.AreEqual(message1, message2);

            byte[] message3 = {1, 2, 3};
            byte[] message4 = null;
            channel2.OnMessage = bytes => { message4 = bytes; };
            channel1.Send(message3);
            var op11 = new WaitUntilWithTimeout(() => message4 != null, 5000);
            yield return op11;
            Assert.True(op11.IsCompleted);
            Assert.AreEqual(message3, message4);

            channel1.Close();
            var op12 = new WaitUntilWithTimeout(() => channel1Closed, 5000);
            yield return op12;
            Assert.True(op12.IsCompleted);

            channel2.Close();
            peer1.Close();
            peer2.Close();
        }
    }
}
