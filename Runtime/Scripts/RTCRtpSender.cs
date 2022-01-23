using System;
using System.Runtime.InteropServices;
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

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
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
            NativeMethods.SenderGetParameters(GetSelfOrThrow(), out var ptr);
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
        public RTCError SetParameters(RTCRtpSendParameters parameters)
        {
            if (Track is VideoStreamTrack videoTrack)
            {
                foreach (var encoding in parameters.encodings)
                {
                    var scale = encoding.scaleResolutionDownBy;
                    if (!scale.HasValue)
                    {
                        continue;
                    }

                    var error = WebRTC.ValidateTextureSize((int)(videoTrack.Texture.width / scale),
                        (int)(videoTrack.Texture.height / scale), Application.platform, WebRTC.GetEncoderType());
                    if (error.errorType != RTCErrorType.None)
                    {
                        return error;
                    }
                }
            }

            parameters.CreateInstance(out RTCRtpSendParametersInternal instance);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(instance));
            Marshal.StructureToPtr(instance, ptr, false);
            RTCErrorType type = NativeMethods.SenderSetParameters(GetSelfOrThrow(), ptr);
            Marshal.FreeCoTaskMem(ptr);
            return new RTCError {errorType = type};
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
