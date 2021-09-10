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
        public IEnumerator SendAndReceiveMessage()
        {
            var test = new MonoBehaviourTest<SignalingPeers>();
            var label = "test";

            RTCDataChannel channel1 = test.component.CreateDataChannel(0, label);
            Assert.That(channel1, Is.Not.Null);
            yield return test;

            var op1 = new WaitUntilWithTimeout(() => test.component.GetDataChannelList(1).Count > 0, 5000);
            yield return op1;
            RTCDataChannel channel2 = test.component.GetDataChannelList(1)[0];
            Assert.That(channel2, Is.Not.Null);

            Assert.That(channel1.ReadyState, Is.EqualTo(RTCDataChannelState.Open));
            Assert.That(channel2.ReadyState, Is.EqualTo(RTCDataChannelState.Open));
            Assert.That(channel1.Label, Is.EqualTo(channel2.Label));
            Assert.That(channel1.Id, Is.EqualTo(channel2.Id));

            // send string
            const string message1 = "hello";
            string message2 = null;
            channel2.OnMessage = bytes => { message2 = System.Text.Encoding.UTF8.GetString(bytes); };
            channel1.Send(message1);
            var op10 = new WaitUntilWithTimeout(() => !string.IsNullOrEmpty(message2), 5000);
            yield return op10;
            Assert.That(op10.IsCompleted, Is.True);
            Assert.That(message1, Is.EqualTo(message2));

            // send byte array
            byte[] message3 = {1, 2, 3};
            byte[] message4 = null;
            channel2.OnMessage = bytes => { message4 = bytes; };
            channel1.Send(message3);
            var op11 = new WaitUntilWithTimeout(() => message4 != null, 5000);
            yield return op11;
            Assert.That(op11.IsCompleted, Is.True);
            Assert.That(message3, Is.EqualTo(message4));
            
            // todo:: native array

            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SendAndReceiveMessageWithExecuteTasks()
        {
            var test = new MonoBehaviourTest<SignalingPeers>();
            var label = "test";

            RTCDataChannel channel1 = test.component.CreateDataChannel(0, label);
            Assert.That(channel1, Is.Not.Null);
            yield return test;

            var op1 = new WaitUntilWithTimeout(() => test.component.GetDataChannelList(1).Count > 0, 5000);
            yield return op1;
            RTCDataChannel channel2 = test.component.GetDataChannelList(1)[0];
            Assert.That(channel2, Is.Not.Null);

            Assert.That(channel1.ReadyState, Is.EqualTo(RTCDataChannelState.Open));
            Assert.That(channel2.ReadyState, Is.EqualTo(RTCDataChannelState.Open));
            Assert.That(channel1.Label, Is.EqualTo(channel2.Label));
            Assert.That(channel1.Id, Is.EqualTo(channel2.Id));

            // send string
            const int millisecondTimeout = 1000;
            const string message1 = "hello";
            string message2 = null;
            channel2.OnMessage = bytes => { message2 = System.Text.Encoding.UTF8.GetString(bytes); };
            channel1.Send(message1);

            while (message2 == null)
            {
                Assert.That(WebRTC.ExecutePendingTasks(millisecondTimeout), Is.True);
            }
            Assert.That(message1, Is.EqualTo(message2));

            // send byte array
            byte[] message3 = { 1, 2, 3 };
            byte[] message4 = null;
            channel2.OnMessage = bytes => { message4 = bytes; };
            channel1.Send(message3);

            while(message4 == null)
            {
                Assert.That(WebRTC.ExecutePendingTasks(millisecondTimeout), Is.True);
            }
            Assert.That(message3, Is.EqualTo(message4));

            // todo:: native array

            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }
    }
}
