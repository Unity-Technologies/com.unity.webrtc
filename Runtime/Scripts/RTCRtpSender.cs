using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public class RTCRtpSender
    {
        internal IntPtr self;
        private RTCPeerConnection peer;

        internal RTCRtpSender(IntPtr ptr, RTCPeerConnection peer)
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
                IntPtr ptr = NativeMethods.SenderGetTrack(self);
                return WebRTC.FindOrCreate(ptr, MediaStreamTrack.Create);
            }
        }

        public RTCRtpSendParameters GetParameters()
        {
            NativeMethods.SenderGetParameters(self, out var ptr);
            RTCRtpSendParametersInternal parametersInternal = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            RTCRtpSendParameters parameters = new RTCRtpSendParameters(parametersInternal);
            Marshal.FreeHGlobal(ptr);
            return parameters;
        }

        public RTCErrorType SetParameters(RTCRtpSendParameters parameters)
        {
            IntPtr ptr = parameters.CreatePtr();
            RTCErrorType error = NativeMethods.SenderSetParameters(self, ptr);
            RTCRtpSendParameters.DeletePtr(ptr);

            return error;
        }
    }
}
