using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

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

#if UNITY_WEBGL
                NativeMethods.DeleteTransceiver(self);
#endif

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
#if !UNITY_WEBGL
                var direction = RTCRtpTransceiverDirection.RecvOnly;
                if (NativeMethods.TransceiverGetCurrentDirection(GetSelfOrThrow(), ref direction))
                {
                    return direction;
                }

                return null;
#else
                int currentDirection = NativeMethods.TransceiverGetCurrentDirection(GetSelfOrThrow());
                if (currentDirection == -1) return null;
                else return (RTCRtpTransceiverDirection) currentDirection;
#endif
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
#if !UNITY_WEBGL
            RTCRtpCodecCapabilityInternal[] array = Array.ConvertAll(codecs, v => v.Cast());
            MarshallingArray<RTCRtpCodecCapabilityInternal> instance = array;
            RTCErrorType error = NativeMethods.TransceiverSetCodecPreferences(GetSelfOrThrow(), instance.ptr, instance.length);
            foreach (var v in array)
            {
                v.Dispose();
            }
            instance.Dispose();
            return error;
#else
            string json = JsonConvert.SerializeObject(codecs, Formatting.None, new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore});

            //TODO Get correct RTCErrorType from jslib.
            NativeMethods.TransceiverSetCodecPreferences(self, json);
            return RTCErrorType.None;
#endif
        }

        public RTCErrorType Stop()
        {
            return NativeMethods.TransceiverStop(self);
        }
    }
}
