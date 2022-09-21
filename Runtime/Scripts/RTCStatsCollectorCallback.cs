using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    internal class RTCStatsCollectorCallback : SafeHandle
    {
        public Action<IntPtr> onStatsDelivered;

        private RTCStatsCollectorCallback()
            : base(IntPtr.Zero, true)
        {
        }

        public void Invoke(IntPtr report)
        {
            onStatsDelivered?.Invoke(report);
        }

        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        protected override bool ReleaseHandle()
        {
            return true;
        }
    }
}
