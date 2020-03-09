using System;

namespace Unity.WebRTC
{
    public enum RTCRtpTransceiverDirection
    {
        SendRecv,
        SendOnly,
        RecvOnly,
        Inactive
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    public struct RTCRtpCodecCapability
    {
    }

    public class RTCRtpTransceiver
    {
        internal IntPtr self;

        internal RTCRtpTransceiver(IntPtr ptr)
        {
            self = ptr;
        }

        /// <summary>
        /// 
        /// </summary>
        public RTCRtpTransceiverDirection CurrentDirection
        {
            get
            {
                var direction = RTCRtpTransceiverDirection.RecvOnly;
                if (NativeMethods.TransceiverGetCurrentDirection(self, ref direction))
                {
                    return direction;
                }
                throw new InvalidOperationException("Transceiver is not running");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RTCRtpReceiver Receiver
        {
            get { return new RTCRtpReceiver(NativeMethods.TransceiverGetReceiver(self)); }
        }

        /// <summary>
        /// 
        /// </summary>
        public RTCRtpSender Sender
        {
            get { return new RTCRtpSender(NativeMethods.TransceiverGetSender(self)); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        public void SetDirection(RTCRtpTransceiverDirection direction)
        {
            // TODO::
            throw new NotImplementedException();
        }


        public void SetCodecPreferences(RTCRtpCodecCapability[] capabilities)
        {
            throw new NotImplementedException("SetCodecPreferences is not implemented");
        }

        public void Stop()
        {
            NativeMethods.TransceiverStop(self);
        }
    }
}
