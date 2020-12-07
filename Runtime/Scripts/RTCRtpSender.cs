using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpSender
    {
        internal IntPtr self;
        private RTCPeerConnection peer;

        internal RTCRtpSender(IntPtr ptr, RTCPeerConnection peer)
        {
            self = ptr;
            this.peer = peer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static RTCRtpCapabilities GetCapabilities(TrackKind kind)
        {
            WebRTC.Context.GetSenderCapabilities(kind, out IntPtr ptr);
            RTCRtpCapabilitiesInternal capabilitiesInternal =
                Marshal.PtrToStructure<RTCRtpCapabilitiesInternal>(ptr);
            RTCRtpCapabilities capabilities = new RTCRtpCapabilities(capabilitiesInternal);
            Marshal.FreeHGlobal(ptr);
            return capabilities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RTCStatsReportAsyncOperation GetStats()
        {
            return peer.GetStats(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public MediaStreamTrack Track
        {
            get
            {
                IntPtr ptr = NativeMethods.SenderGetTrack(self);
                return WebRTC.FindOrCreate(ptr, MediaStreamTrack.Create);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RTCRtpSendParameters GetParameters()
        {
            NativeMethods.SenderGetParameters(self, out var ptr);
            RTCRtpSendParametersInternal parametersInternal = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            RTCRtpSendParameters parameters = new RTCRtpSendParameters(ref parametersInternal);
            Marshal.FreeHGlobal(ptr);
            return parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public RTCErrorType SetParameters(RTCRtpSendParameters parameters)
        {
            parameters.CreateInstance(out RTCRtpSendParametersInternal instance);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(instance));
            Marshal.StructureToPtr(instance, ptr, false);
            RTCErrorType error = NativeMethods.SenderSetParameters(self, ptr);
            Marshal.FreeCoTaskMem(ptr);
            return error;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public bool ReplaceTrack(MediaStreamTrack track)
        {
            return NativeMethods.SenderReplaceTrack(self, track.self);
        }
    }
}
