#if UNITY_WEBRTC_ENABLE_INPUT_SYSTEM
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Unity.WebRTC.InputSystem
{
    class Sender : InputManager
    {
        public override event Action<InputEventPtr, InputDevice> onEvent;
        public override event Action<InputDevice, InputDeviceChange> onDeviceChange;
        public override event Action<string, InputControlLayoutChange> onLayoutChange;

        public Sender()
        {
            UnityEngine.InputSystem.InputSystem.onEvent += OnEvent;
            UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
            UnityEngine.InputSystem.InputSystem.onLayoutChange += OnLayoutChange;
        }

        ~Sender()
        {
            UnityEngine.InputSystem.InputSystem.onEvent -= OnEvent;
            UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
            UnityEngine.InputSystem.InputSystem.onLayoutChange -= OnLayoutChange;
        }

        public override ReadOnlyArray<InputDevice> devices
        {
            get
            {
                // note:: InputRemoting class rejects remote devices when sending device information to the remote peer.
                // Avoid to get assert "Device being sent to remotes should be a local device, not a remote one"
                var localDevices = UnityEngine.InputSystem.InputSystem.devices.Where(device => !device.remote);
                return new ReadOnlyArray<InputDevice>(localDevices.ToArray());
            }
        }

        public override IEnumerable<string> layouts
        {
            get
            {
                // todo(kazuki):: filter layout
                return UnityEngine.InputSystem.InputSystem.ListLayouts();
            }
        }
            
        private void OnEvent(InputEventPtr ptr, InputDevice device)
        {
            onEvent?.Invoke(ptr, device);
        }
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            onDeviceChange?.Invoke(device, change);
        }
        private void OnLayoutChange(string name, InputControlLayoutChange change)
        {
            onLayoutChange?.Invoke(name, change);
        }
    }

    /// <summary>
    ///
    /// </summary>
    class Observer : IObserver<InputRemoting.Message>
    {
        private RTCDataChannel _channel;
        private bool _isOpen;
        public Observer(RTCDataChannel channel)
        {
            _channel = channel ?? throw new ArgumentNullException("channel is null");
            _channel.OnOpen += () => { _isOpen = true; };
            _channel.OnClose += () => { _isOpen = false; };
            _isOpen = _channel.ReadyState == RTCDataChannelState.Open;
        }
        public void OnNext(InputRemoting.Message value)
        {
            if (!_isOpen)
                return;
            byte[] bytes = MessageSerializer.Serialize(ref value);
            _channel.Send(bytes);
        }

        public void OnCompleted()
        {
        }
        public void OnError(Exception error)
        {
        }
    }
}
#endif
