using System;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpTransceiver : RefCountedObject
    {
        private RTCPeerConnection peer;

        internal RTCRtpTransceiver(IntPtr ptr, RTCPeerConnection peer) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        /// <summary>
        /// 
        /// </summary>
        ~RTCRtpTransceiver()
        {
            this.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
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
        public string Mid
        {
            get
            {
                IntPtr ptr = NativeMethods.TransceiverGetMid(GetSelfOrThrow());
                if (ptr == IntPtr.Zero)
                    return null;
                return ptr.AsAnsiStringWithFreeMem();
            }
        }

        /// <summary>
        /// This is used to set the transceiver's desired direction
        /// and will be used in calls to CreateOffer and CreateAnswer.
        /// </summary>
        public RTCRtpTransceiverDirection Direction
        {
            get { return NativeMethods.TransceiverGetDirection(GetSelfOrThrow()); }
            set
            {
                RTCErrorType errorType = NativeMethods.TransceiverSetDirection(GetSelfOrThrow(), value);
                if (errorType != RTCErrorType.None)
                {
                    var error = new RTCError { errorType = errorType };
                    throw new RTCErrorException(ref error);
                }
            }
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
                if (NativeMethods.TransceiverGetCurrentDirection(GetSelfOrThrow(), out var direction))
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codecs"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RTCErrorType Stop()
        {
            return NativeMethods.TransceiverStop(self);
        }
    }
}
