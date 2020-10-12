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

        public MediaStreamTrack Track
        {
            get
            {
                IntPtr ptrTrack = NativeMethods.ReceiverGetTrack(self);
                return WebRTC.FindOrCreate(ptrTrack, MediaStreamTrack.Create);
            }
        }
    }
}
