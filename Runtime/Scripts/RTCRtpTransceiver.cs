using System;

namespace Unity.WebRTC
{
    /// <summary>
    ///     Describes a permanent pairing of an RTCRtpSender and an RTCRtpReceiver.
    /// </summary>
    /// <remarks>
    ///     `RTCRtpTransceiver` class is used to represent a permanent pairing of an `RTCRtpSender` and an `RTCRtpReceiver`, along with shared state.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
    ///         RTCRtpTransceiver transceiver = peerConnection.GetTransceivers().First();
    ///         RTCErrorType error = transceiver.SetCodecPreferences(capabilities.codecs);
    ///         if (error.errorType != RTCErrorType.None)
    ///         {
    ///             Debug.LogError($"Failed to set codec preferences: {error.message}");
    ///         }
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCPeerConnection"/>
    public class RTCRtpTransceiver : RefCountedObject
    {
        private RTCPeerConnection peer;

        internal RTCRtpTransceiver(IntPtr ptr, RTCPeerConnection peer) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        /// <summary>
        ///     Finalizer for RTCRtpTransceiver.
        /// </summary>
        /// <remarks>
        ///     Ensures that resources are released by calling the `Dispose` method
        /// </remarks>
        ~RTCRtpTransceiver()
        {
            this.Dispose();
        }

        /// <summary>
        ///     Disposes of RTCRtpTransceiver.
        /// </summary>
        /// <remarks>
        ///     `Dispose` method disposes of the `RTCRtpTransceiver` and releases the associated resources. 
        /// </remarks>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         transceiver.Dispose();
        ///     ]]></code>
        /// </example>
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
        ///     Specifies the negotiated media ID (mid) that uniquely identifies the pairing of the sender and receiver agreed upon by local and remote peers.
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
        ///     This is used to set the transceiver's desired direction and will be used in calls to CreateOffer and CreateAnswer.
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
        ///     This property indicates the transceiver's current directionality,
        ///     or null if the transceiver is stopped or has never participated in an exchange of offers and answers.
        ///     To change the transceiver's directionality, set the value of the Direction property.
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
        ///     RTCRtpReceiver object that handles receiving and decoding incoming media data for the transceiver's stream.
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
        ///     RTCRtpSender object that handles encoding and sending outgoing media data for the transceiver's stream.
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
        ///     Specifies the codecs for decoding received data on this transceiver, arranged in order of decreasing preference.
        /// </summary>
        /// <remarks>
        ///     `SetCodecPreferences` method sets codec preferences for negotiating data encoding with a remote peer.
        ///     It requires reordering supported codecs by preference and applying them to influence the negotiation process.
        ///     When initiating an `RTCPeerConnection`, set codecs before calling `CreateOffer` or `CreateAnswer`.
        ///     Changing codecs during a session requires a new negotiation but does not automatically trigger the `OnNegotiationNeeded` event.
        /// </remarks>
        /// <param name="codecs">
        ///     An array of `RTCRtpCodecCapability` objects arranged by preference to determine the codecs used for receiving and sending data streams.
        ///     If it is empty, the codec configurations are all reset to the defaults.
        /// </param>
        /// <returns>`RTCErrorType` value.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCRtpCapabilities capabilities = RTCRtpSender.GetCapabilities(TrackKind.Video);
        ///         RTCRtpTransceiver transceiver = peerConnection.GetTransceivers().First();
        ///         RTCErrorType error = transceiver.SetCodecPreferences(capabilities.codecs);
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to set codec preferences: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="RTCPeerConnection"/>
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
        ///     Stops the transceiver permanently by stopping both the associated RTCRtpSender and RTCRtpReceiver.
        /// </summary>
        /// <remarks>
        ///     Calling `Stop` method stops the sender from sending media immediately, closes RTP streams with an RTCP "BYE" message, and the receiver stops receiving media.
        ///     The receiver's track ceases, and the transceiver's direction changes to `Stopped`.
        /// </remarks>
        /// <returns>`RTCErrorType` value.</returns>
        /// <example>
        ///     <code lang="cs"><![CDATA[
        ///         RTCErrorType error = transceiver.Stop();
        ///         if (error.errorType != RTCErrorType.None)
        ///         {
        ///             Debug.LogError($"Failed to stop transceiver: {error.message}");
        ///         }
        ///     ]]></code>
        /// </example>
        public RTCErrorType Stop()
        {
            return NativeMethods.TransceiverStop(self);
        }
    }
}
