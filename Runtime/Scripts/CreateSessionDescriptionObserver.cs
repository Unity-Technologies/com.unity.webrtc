using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    class CreateSessionDescriptionObserver : SafeHandle
    {
        public Action<RTCSdpType, string, RTCErrorType, string> onCreateSessionDescription;

        private CreateSessionDescriptionObserver()
            : base(IntPtr.Zero, true)
        {
        }

        public void Invoke(RTCSdpType type, string sdp, RTCErrorType errorType, string message)
        {
            onCreateSessionDescription?.Invoke(type, sdp, errorType, message);
        }

        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        protected override bool ReleaseHandle()
        {
            onCreateSessionDescription = null;
            return true;
        }
    }
}
