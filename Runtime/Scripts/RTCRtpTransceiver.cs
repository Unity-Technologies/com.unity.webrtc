using System;

namespace Unity.WebRTC
{
    public enum RTCRtpTransceiverDirection
    {
        SendRecv = 0,
        SendOnly = 1,
        RecvOnly = 2,
        Inactive = 3,
        Stopped  = 4
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
        private RTCPeerConnection peer;

        internal RTCRtpTransceiver(IntPtr ptr, RTCPeerConnection peer)
        {
            self = ptr;
            this.peer = peer;
        }

        /// <summary>
        /// This is used to set the transceiver's desired direction
        /// and will be used in calls to CreateOffer and CreateAnswer.
        /// </summary>
        public RTCRtpTransceiverDirection Direction
        {
            get { return NativeMethods.TransceiverGetDirection(self); }
            set { NativeMethods.TransceiverSetDirection(self, value); }
        }

        /// <summary>
        /// This property indicates the transceiver's current directionality,
        /// or null if the transceiver is stopped or has never participated in an exchange of offers and answers.
        /// To change the transceiver's directionality, set the value of the <see cref="Direction"/> property.
        /// </summary>
        public RTCRtpTransceiverDirection? CurrentDirection
        {
            get
            {
                var direction = RTCRtpTransceiverDirection.RecvOnly;
                if (NativeMethods.TransceiverGetCurrentDirection(self, ref direction))
                {
                    return direction;
                }

                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public RTCRtpReceiver Receiver
        {
            get { return new RTCRtpReceiver(NativeMethods.TransceiverGetReceiver(self), peer); }
        }

        /// <summary>
        ///
        /// </summary>
        public RTCRtpSender Sender
        {
            get { return new RTCRtpSender(NativeMethods.TransceiverGetSender(self), peer); }
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
