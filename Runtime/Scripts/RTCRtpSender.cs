using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    public class RTCRtpSender : RefCountedObject
    {
        private RTCPeerConnection peer;

        internal RTCRtpSender(IntPtr ptr, RTCPeerConnection peer) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        ~RTCRtpSender()
        {
            this.Dispose();
        }

        internal IntPtr GetSelfOrThrow()
        {
            if (self == IntPtr.Zero)
            {
                throw new InvalidOperationException("This instance has been disposed.");
            }
            return self;
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
#if UNITY_WEBGL
                NativeMethods.DeleteSender(self);
#endif
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static RTCRtpCapabilities GetCapabilities(TrackKind kind)
        {

#if !UNITY_WEBGL
            WebRTC.Context.GetSenderCapabilities(kind, out IntPtr ptr);
            RTCRtpCapabilitiesInternal capabilitiesInternal =
                Marshal.PtrToStructure<RTCRtpCapabilitiesInternal>(ptr);
            RTCRtpCapabilities capabilities = new RTCRtpCapabilities(capabilitiesInternal);
            Marshal.FreeHGlobal(ptr);
            return capabilities;
#else
            return WebRTC.Context.GetSenderCapabilities(kind);
#endif
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
                IntPtr ptr = NativeMethods.SenderGetTrack(GetSelfOrThrow());
                if (ptr == IntPtr.Zero)
                    return null;
                return WebRTC.FindOrCreate(ptr, MediaStreamTrack.Create);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public RTCRtpSendParameters GetParameters()
        {

#if !UNITY_WEBGL
            NativeMethods.SenderGetParameters(GetSelfOrThrow(), out var ptr);
            RTCRtpSendParametersInternal parametersInternal = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            RTCRtpSendParameters parameters = new RTCRtpSendParameters(ref parametersInternal);
            Marshal.FreeHGlobal(ptr);
            return parameters;
#else
            string json = NativeMethods.SenderGetParameters(self);
            return JsonConvert.DeserializeObject<RTCRtpSendParameters>(json);
#endif
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public RTCErrorType SetParameters(RTCRtpSendParameters parameters)
        {
#if !UNITY_WEBGL
            parameters.CreateInstance(out RTCRtpSendParametersInternal instance);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(instance));
            Marshal.StructureToPtr(instance, ptr, false);
            RTCErrorType error = NativeMethods.SenderSetParameters(GetSelfOrThrow(), ptr);
            Marshal.FreeCoTaskMem(ptr);
            return error;
#else
            string json = JsonConvert.SerializeObject(parameters, Formatting.None, new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore});

            //TODO Get correct RTCErrorType from jslib.
            NativeMethods.SenderSetParameters(self, json);
            return RTCErrorType.None;
#endif
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public bool ReplaceTrack(MediaStreamTrack track)
        {
            IntPtr trackPtr = track?.GetSelfOrThrow() ?? IntPtr.Zero;
            return NativeMethods.SenderReplaceTrack(GetSelfOrThrow(), trackPtr);
        }
    }
}
