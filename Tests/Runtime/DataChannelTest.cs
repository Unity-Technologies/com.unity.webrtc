using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Unity.WebRTC.RuntimeTest
{
    class DataChannelTest
    {
        [Test]
        public void CreateDataChannel()
        {
            RTCConfiguration config = default;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } };
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
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator SendThrowsException()
        {
            byte[] message1 = { 1, 2, 3 };
            string message2 = "123";
            NativeArray<byte> message3 = new NativeArray<byte>(message1, Allocator.Persistent);
            NativeArray<byte>.ReadOnly message4 = message3.AsReadOnly();
            NativeSlice<byte> message5 = message3.Slice();

            var test = new MonoBehaviourTest<SignalingPeers>();
            RTCDataChannel channel = test.component.CreateDataChannel(0, "test");

            // Throws exception before opening channel
            Assert.That(() => channel.Send(message1), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message2), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message3), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message4), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message5), Throws.TypeOf<InvalidOperationException>());

            yield return test;

            var op1 = new WaitUntilWithTimeout(() => channel.ReadyState == RTCDataChannelState.Open, 5000);
            yield return op1;
            Assert.That(op1.IsCompleted, Is.True);
            Assert.That(() => channel.Send(message1), Throws.Nothing);
            Assert.That(() => channel.Send(message2), Throws.Nothing);
            Assert.That(() => channel.Send(message3), Throws.Nothing);
            Assert.That(() => channel.Send(message4), Throws.Nothing);
            Assert.That(() => channel.Send(message5), Throws.Nothing);
            channel.Close();

            // Throws exception after closing
            Assert.That(() => channel.Send(message1), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message2), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message3), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message4), Throws.TypeOf<InvalidOperationException>());
            Assert.That(() => channel.Send(message5), Throws.TypeOf<InvalidOperationException>());

            message3.Dispose();
            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator CreateDataChannelMultiple()
        {
            var test = new MonoBehaviourTest<SignalingPeers>();
            var label = "test";

            RTCDataChannel channel1 = test.component.CreateDataChannel(0, label);
            RTCDataChannel channel2 = test.component.CreateDataChannel(1, label);
            Assert.That(channel1, Is.Not.Null);
            yield return test;

            var op1 = new WaitUntilWithTimeout(() => test.component.GetDataChannelList(1).Count > 0, 5000);
            yield return op1;
            Assert.That(test.component.GetDataChannelList(1).Count, Is.GreaterThan(0));

            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
        public IEnumerator CreateAndClose()
        {
            var test = new MonoBehaviourTest<SignalingPeers>();
            var label = "test";
            bool closed = false;

            RTCDataChannel channel1 = test.component.CreateDataChannel(0, label);
            Assert.That(channel1, Is.Not.Null);
            yield return test;

            var op1 = new WaitUntilWithTimeout(() => test.component.GetDataChannelList(1).Count > 0, 5000);
            yield return op1;
            RTCDataChannel channel2 = test.component.GetDataChannelList(1)[0];
            Assert.That(channel2, Is.Not.Null);
            channel2.OnClose = () => { closed = true; };

            channel1.Close();

            var op2 = new WaitUntilWithTimeout(() => closed, 5000);
            yield return op2;
            Assert.That(closed, Is.True);

            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
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
            byte[] message3 = { 1, 2, 3 };
            byte[] message4 = null;
            channel2.OnMessage = bytes => { message4 = bytes; };
            channel1.Send(message3);
            var op11 = new WaitUntilWithTimeout(() => message4 != null, 5000);
            yield return op11;
            Assert.That(op11.IsCompleted, Is.True);
            Assert.That(message3, Is.EqualTo(message4));

            // Native Array

            // Native Arrays that are declared in tests that use IEnumerator seem to have some oddities about them
            // they tend to dispose themselves on yields so we recreate the array as needed.

            byte[] comparisonBuffer = { 1, 2, 3 };
            var nativeArrayTestMessageReceiver = default(byte[]);

            using (var message5 = new NativeArray<byte>(comparisonBuffer, Allocator.Temp))
            {
                Assert.That(message5.IsCreated, Is.True);
                // Only needs to be set once as it will be reused.
                channel2.OnMessage = bytes => { nativeArrayTestMessageReceiver = bytes; };
                channel1.Send(message5);
            }
            var op12 = new WaitUntilWithTimeout(() => nativeArrayTestMessageReceiver != null, 5000);
            yield return op12;
            Assert.That(op12.IsCompleted, Is.True);
            Assert.That(comparisonBuffer, Is.EqualTo(nativeArrayTestMessageReceiver));

            // Native Slice
            using (var nativeArray = new NativeArray<byte>(comparisonBuffer, Allocator.Temp))
            {
                Assert.That(nativeArray.IsCreated, Is.True);
                var message6 = nativeArray.Slice();
                nativeArrayTestMessageReceiver = null;
                channel1.Send(message6);
            }
            var op13 = new WaitUntilWithTimeout(() => nativeArrayTestMessageReceiver != null, 5000);
            yield return op13;
            Assert.That(op13.IsCompleted, Is.True);
            Assert.That(comparisonBuffer, Is.EqualTo(nativeArrayTestMessageReceiver));

#if UNITY_2021_1_OR_NEWER
            // NativeArray.ReadOnly
            using (var nativeArray = new NativeArray<byte>(comparisonBuffer, Allocator.Temp))
            {
                Assert.That(nativeArray.IsCreated, Is.True);
                var message7 = nativeArray.AsReadOnly();
                nativeArrayTestMessageReceiver = null;
                channel1.Send(message7);
            }
            var op14 = new WaitUntilWithTimeout(() => nativeArrayTestMessageReceiver != null, 5000);
            yield return op14;
            Assert.That(op14.IsCompleted, Is.True);
            Assert.That(comparisonBuffer, Is.EqualTo(nativeArrayTestMessageReceiver));
#endif // UNITY_2020_1_OR_NEWER

            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }

        [UnityTest]
        [Timeout(5000)]
        [UnityPlatform(exclude = new[] { RuntimePlatform.IPhonePlayer })]
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
            const int millisecondTimeout = 5000;
            const string message1 = "hello";
            string message2 = null;
            channel2.OnMessage = bytes => { message2 = System.Text.Encoding.UTF8.GetString(bytes); };
            channel1.Send(message1);
            ExecutePendingTasksWithTimeout(ref message2, millisecondTimeout);
            Assert.That(message1, Is.EqualTo(message2));

            // send byte array
            byte[] message3 = { 1, 2, 3 };
            byte[] message4 = null;
            channel2.OnMessage = bytes => { message4 = bytes; };
            channel1.Send(message3);
            ExecutePendingTasksWithTimeout(ref message4, millisecondTimeout);
            Assert.That(message3, Is.EqualTo(message4));

            // Native Collections Tests
            Vector3[] structData = { Vector3.one, Vector3.zero, Vector3.up, Vector3.down };
            using (var nativeArray = new NativeArray<Vector3>(structData, Allocator.Temp))
            {
                var nativeArrayTestMessageReceiver = default(byte[]);
                channel2.OnMessage = bytes => { nativeArrayTestMessageReceiver = bytes; };

                // Native Array
                var message5 = nativeArray;
                Assert.That(message5.IsCreated, Is.True);
                nativeArrayTestMessageReceiver = null;
                channel1.Send(message5);
                ExecutePendingTasksWithTimeout(ref nativeArrayTestMessageReceiver, millisecondTimeout);
                Assert.That(NativeArrayMemCmp(message5, nativeArrayTestMessageReceiver), Is.True, "Elements of the received message are not the same as the original message.");

                // Native Slice
                var message6 = nativeArray.Slice();
                nativeArrayTestMessageReceiver = null;
                channel1.Send(message6);
                ExecutePendingTasksWithTimeout(ref nativeArrayTestMessageReceiver, millisecondTimeout);
                Assert.That(NativeArrayMemCmp(message6, nativeArrayTestMessageReceiver), Is.True, "Elements of the received message are not the same as the original message.");

#if UNITY_2021_1_OR_NEWER
                // NativeArray.ReadOnly
                var message7 = nativeArray.AsReadOnly();
                nativeArrayTestMessageReceiver = null;
                channel1.Send(message7);
                ExecutePendingTasksWithTimeout(ref nativeArrayTestMessageReceiver, millisecondTimeout);
                Assert.That(NativeArrayMemCmp(message7, nativeArrayTestMessageReceiver), Is.True, "Elements of the received message are not the same as the original message.");
#endif // UNITY_2021_1_OR_NEWER
            }

            test.component.Dispose();
            Object.DestroyImmediate(test.gameObject);
        }

        static void ExecutePendingTasksWithTimeout(ref string message, int timeoutInMilliseconds)
        {
            Stopwatch watchdog = Stopwatch.StartNew();
            while (watchdog.ElapsedMilliseconds < timeoutInMilliseconds && message == null)
            {
                WebRTC.ExecutePendingTasks(timeoutInMilliseconds);
            }
            Assert.That(message, Is.Not.Null, "Message was not received in the allotted time!");
        }

        static void ExecutePendingTasksWithTimeout(ref byte[] message, int timeoutInMilliseconds)
        {
            Stopwatch watchdog = Stopwatch.StartNew();
            while (watchdog.ElapsedMilliseconds < timeoutInMilliseconds && message == null)
            {
                WebRTC.ExecutePendingTasks(timeoutInMilliseconds);
            }
            Assert.That(message, Is.Not.Null, "Message was not received in the allotted time!");
        }

        static unsafe bool NativeArrayMemCmp<T>(NativeArray<T> array, byte[] buffer)
            where T : struct
        {
            if (array.Length * UnsafeUtility.SizeOf<T>() == buffer.Length)
            {
                var nativeArrayIntPtr = new IntPtr(array.GetUnsafeReadOnlyPtr());
                return IntPtrMemCmp(nativeArrayIntPtr, buffer);
            }

            return false;
        }

        static unsafe bool NativeArrayMemCmp<T>(NativeSlice<T> array, byte[] buffer)
            where T : struct
        {
            if (array.Length * UnsafeUtility.SizeOf<T>() == buffer.Length)
            {
                var nativeArrayIntPtr = new IntPtr(array.GetUnsafeReadOnlyPtr());
                return IntPtrMemCmp(nativeArrayIntPtr, buffer);
            }

            return false;
        }

#if UNITY_2021_1_OR_NEWER
        static unsafe bool NativeArrayMemCmp<T>(NativeArray<T>.ReadOnly array, byte[] buffer)
            where T : struct
        {
            if (array.Length * UnsafeUtility.SizeOf<T>() == buffer.Length)
            {
                var nativeArrayIntPtr = new IntPtr(array.GetUnsafeReadOnlyPtr());
                return IntPtrMemCmp(nativeArrayIntPtr, buffer);
            }

            return false;
        }
#endif // UNITY_2021_1_OR_NEWER

        static unsafe bool IntPtrMemCmp(IntPtr ptr, byte[] buffer)
        {
            fixed (byte* bufPtr = buffer)
            {
                var bufIntPtr = new IntPtr(bufPtr);
                return UnsafeUtility.MemCmp((void*)ptr, (void*)bufIntPtr, buffer.Length) == 0;
            }
        }
    }
}
