using System;

namespace Unity.WebRTC
{
    public class RTCRtpTransceiver : RefCountedObject
    {
        private RTCPeerConnection peer;

        internal RTCRtpTransceiver(IntPtr ptr, RTCPeerConnection peer) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        ~RTCRtpTransceiver()
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

        internal IntPtr GetSelfOrThrow()
        {
            if (self == IntPtr.Zero)
            {
                throw new InvalidOperationException("This instance has been disposed.");
            }
            return self;
        }

        /// <summary>
        /// This is used to set the transceiver's desired direction
        /// and will be used in calls to CreateOffer and CreateAnswer.
        /// </summary>
        public RTCRtpTransceiverDirection Direction
        {
            get { return NativeMethods.TransceiverGetDirection(GetSelfOrThrow()); }
            set { NativeMethods.TransceiverSetDirection(GetSelfOrThrow(), value); }
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
                if (NativeMethods.TransceiverGetCurrentDirection(GetSelfOrThrow(), ref direction))
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
            get
            {
                IntPtr receiverPtr = NativeMethods.TransceiverGetReceiver(GetSelfOrThrow());
                return WebRTC.FindOrCreate(receiverPtr, ptr => new RTCRtpReceiver(ptr, peer));
            }
        }

        /// <summary>
        ///
        /// </summary>
        public RTCRtpSender Sender
        {
            get
            {
                IntPtr senderPtr = NativeMethods.TransceiverGetSender(GetSelfOrThrow());
                return WebRTC.FindOrCreate(senderPtr, ptr => new RTCRtpSender(ptr, peer));
            }
        }

        public RTCErrorType SetCodecPreferences(RTCRtpCodecCapability[] codecs)
        {
            RTCRtpCodecCapabilityInternal[] array = Array.ConvertAll(codecs, v => v.Cast());
            MarshallingArray<RTCRtpCodecCapabilityInternal> instance = array;
            RTCErrorType error = NativeMethods.TransceiverSetCodecPreferences(GetSelfOrThrow(), instance.ptr, instance.length);
            foreach (var v in array)
            {
                v.Dispose();
            }
            instance.Dispose();
            return error;
        }

        public RTCErrorType Stop()
        {
            return NativeMethods.TransceiverStop(self);
        }
    }
}
