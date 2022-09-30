using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    class SetSessionDescriptionObserver : SafeHandle
    {
        public Action<RTCErrorType, string> onSetSessionDescription;

        private SetSessionDescriptionObserver()
            : base(IntPtr.Zero, true)
        {
        }

        public void Invoke(RTCErrorType type, string message)
        {
            onSetSessionDescription?.Invoke(type, message);
        }

        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        protected override bool ReleaseHandle()
        {
            onSetSessionDescription = null;
            return true;
        }
    }
}
