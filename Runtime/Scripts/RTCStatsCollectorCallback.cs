using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    internal class RTCStatsCollectorCallback : SafeHandle
    {
        public Action<RTCStatsReport> onStatsDelivered;

        private RTCStatsCollectorCallback()
            : base(IntPtr.Zero, true)
        {
        }

        public void Invoke(RTCStatsReport report)
        {
            onStatsDelivered?.Invoke(report);
        }

        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }

        protected override bool ReleaseHandle()
        {
            WebRTC.Context.DeleteRefPtr(handle);
            onStatsDelivered = null;
            return true;
        }
    }
}
