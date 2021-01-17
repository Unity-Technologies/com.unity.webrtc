using System;
using NUnit.Framework;
using UnityEngine.InputSystem;
using Unity.WebRTC.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

using Message = Unity.WebRTC.InputSystem.InputRemoting.Message;

namespace Unity.WebRTC.RuntimeTest
{
    class Receiver : InputManager
    {
        public override event Action<Message> onMessage;

        private RTCDataChannel channel;

        public Receiver(RTCDataChannel channel)
        {
            this.channel = channel;
            this.channel.OnMessage += OnMessage;
        }

        ~Receiver()
        {
            this.channel.OnMessage -= OnMessage;
        }

        private void OnMessage(byte[] bytes)
        {
            Message.Deserialize(bytes, out var message);
            onMessage?.Invoke(message);
        }
    }

    class Sender : InputManager
    {
        public override event Action<InputEventPtr, InputDevice> onEvent;
        public override event Action<InputDevice, InputDeviceChange> onDeviceChange;
        public override event Action<string, InputControlLayoutChange> onLayoutChange;

        public Sender()
        {
            UnityEngine.InputSystem.InputSystem.onEvent += onEvent;
            UnityEngine.InputSystem.InputSystem.onDeviceChange += onDeviceChange;
            UnityEngine.InputSystem.InputSystem.onLayoutChange += onLayoutChange;
        }

        ~Sender()
        {
            UnityEngine.InputSystem.InputSystem.onEvent -= onEvent;
            UnityEngine.InputSystem.InputSystem.onDeviceChange -= onDeviceChange;
            UnityEngine.InputSystem.InputSystem.onLayoutChange -= onLayoutChange;
        }
    }

    abstract class InputManager : IInputManager
    {
        public virtual event Action<Message> onMessage;
        public virtual event Action<InputEventPtr, InputDevice> onEvent;
        public virtual event Action<InputDevice, InputDeviceChange> onDeviceChange;
        public virtual event Action<string, InputControlLayoutChange> onLayoutChange;

        public ReadOnlyArray<InputDevice> devices
        {
            get { return UnityEngine.InputSystem.InputSystem.devices; }
        }

        public InputDevice GetDeviceById(int deviceId)
        {
            return UnityEngine.InputSystem.InputSystem.GetDeviceById(deviceId);
        }

        public InputDevice AddDevice(string layout, string name = null, string variants = null)
        {
            return UnityEngine.InputSystem.InputSystem.AddDevice(layout, name, variants);
        }

        public void RemoveDevice(InputDevice device)
        {
            UnityEngine.InputSystem.InputSystem.RemoveDevice(device);
        }

        public void SetDeviceUsage(InputDevice device, string usage)
        {
            UnityEngine.InputSystem.InputSystem.SetDeviceUsage(device, usage);
        }

        public InputControlLayout LoadLayout(string name)
        {
            return UnityEngine.InputSystem.InputSystem.LoadLayout(name);
        }

        public void RegisterLayout(string json, string name = null, InputDeviceMatcher? matches = null)
        {
            UnityEngine.InputSystem.InputSystem.RegisterLayout(json, name, matches);
        }

        public void RemoveLayout(string name)
        {
            UnityEngine.InputSystem.InputSystem.RemoveLayout(name);
        }

        public void QueueEvent(InputEventPtr eventPtr)
        {
            UnityEngine.InputSystem.InputSystem.QueueEvent(eventPtr);
        }
    }

    class Observer : IObserver<Message>
    {
        private RTCDataChannel channel = null;
        public Observer(RTCDataChannel channel)
        {
            this.channel = channel ?? throw new ArgumentNullException("channel is null");
        }
        public void OnNext(Message value)
        {
            channel.Send(value.Serialize());
        }
        public void OnCompleted()
        {
        }
        public void OnError(Exception error)
        {
        }
    }

    class InputRemotingTest
    {
        [Test]
        public void Test()
        {
            var sender = new Sender();
            var local = new Unity.WebRTC.InputSystem.InputRemoting(sender);
            var peer = new RTCPeerConnection();
            var channel = peer.CreateDataChannel("test");
            local.Subscribe(new Observer(channel));

            var peer2 = new RTCPeerConnection();
            RTCDataChannel channel2 = null;
            peer2.OnDataChannel = _channel => channel2 = _channel;
            var remote = new Unity.WebRTC.InputSystem.InputRemoting(new Receiver(channel2));
            remote.Subscribe(remote);
        }
    }
}
