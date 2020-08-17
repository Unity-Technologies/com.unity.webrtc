using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public class RTCRtpReceiver
    {
        internal IntPtr self;
        private RTCPeerConnection peer;

        internal RTCRtpReceiver(IntPtr ptr, RTCPeerConnection peer)
        {
            self = ptr;
            this.peer = peer;
        }

        public RTCStatsReportAsyncOperation GetStats()
        {
            return peer.GetStats(this);
        }
    }
}
