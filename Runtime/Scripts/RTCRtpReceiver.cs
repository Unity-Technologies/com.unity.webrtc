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
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        ~RTCRtpReceiver()
        {
            WebRTC.Table.Remove(self);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static RTCRtpCapabilities GetCapabilities(TrackKind kind)
        {
            WebRTC.Context.GetReceiverCapabilities(kind, out IntPtr ptr);
            RTCRtpCapabilitiesInternal capabilitiesInternal =
                Marshal.PtrToStructure<RTCRtpCapabilitiesInternal>(ptr);
            RTCRtpCapabilities capabilities = new RTCRtpCapabilities(capabilitiesInternal);
            Marshal.FreeHGlobal(ptr);
            return capabilities;
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
