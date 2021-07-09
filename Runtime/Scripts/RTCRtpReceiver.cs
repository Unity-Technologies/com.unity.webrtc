using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpReceiver : IDisposable
    {
        internal IntPtr self;
        private RTCPeerConnection peer;
        private bool disposed;

        internal RTCRtpReceiver(IntPtr ptr, RTCPeerConnection peer)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        ~RTCRtpReceiver()
        {
            this.Dispose();
        }

        public virtual void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
#if UNITY_WEBGL
                NativeMethods.DeleteReceiver(self);
#endif
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static RTCRtpCapabilities GetCapabilities(TrackKind kind)
        {

#if !UNITY_WEBGL
            WebRTC.Context.GetReceiverCapabilities(kind, out IntPtr ptr);
            RTCRtpCapabilitiesInternal capabilitiesInternal =
                Marshal.PtrToStructure<RTCRtpCapabilitiesInternal>(ptr);
            RTCRtpCapabilities capabilities = new RTCRtpCapabilities(capabilitiesInternal);
            Marshal.FreeHGlobal(ptr);
            return capabilities;
#else
            return WebRTC.Context.GetReceiverCapabilities(kind);
#endif

        }

        public RTCStatsReportAsyncOperation GetStats()
        {
            return peer.GetStats(this);
        }

        public MediaStreamTrack Track
        {
            get
            {
                IntPtr ptr = NativeMethods.ReceiverGetTrack(self);
                if (ptr == IntPtr.Zero)
                    return null;
                return WebRTC.FindOrCreate(ptr, MediaStreamTrack.Create);
            }
        }

        public IEnumerable<MediaStream> Streams
        {
            get
            {
                IntPtr ptrStreams = NativeMethods.ReceiverGetStreams(self, out ulong length);
                return WebRTC.Deserialize(ptrStreams, (int)length, ptr => new MediaStream(ptr));
            }
        }
    }
}
