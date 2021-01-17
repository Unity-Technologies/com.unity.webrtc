#if UNITY_WEBRTC_ENABLE_INPUT_SYSTEM
using System;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem;

namespace Unity.WebRTC.InputSystem
{
    /// <summary>
    /// 
    /// </summary>
    interface IInputManager
    {
        event Action<InputRemoting.Message> onMessage;
        event Action<InputEventPtr, InputDevice> onEvent;
        event Action<InputDevice, InputDeviceChange> onDeviceChange;
        event Action<string, InputControlLayoutChange> onLayoutChange;

        ReadOnlyArray<InputDevice> devices { get; }
        InputDevice GetDeviceById(int deviceId);
        InputDevice AddDevice(string layout, string name = null, string variants = null);
        void RemoveDevice(InputDevice device);
        void SetDeviceUsage(InputDevice device, string usage);
        InputControlLayout LoadLayout(string name);
        void RegisterLayout(string json, string name = null, InputDeviceMatcher? matches = null);
        void RemoveLayout(string name);
        void QueueEvent(InputEventPtr eventPtr);
    }
}
#endif
