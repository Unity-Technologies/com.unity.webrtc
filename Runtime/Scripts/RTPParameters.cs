

namespace Unity.WebRTC
{
    internal struct RTCRtpSendParameters
    {
        public RTCRtpCodecParameters[] codecs;
        public RTCRtpEncodingParameters[] encodings;
        public string transactionId;
    }

    internal struct RTCRtpEncodingParameters
    {
        public bool active;
        public ulong maxBitrate;
        public string rid;
    }

    internal struct RTCRtpCodecParameters
    {
    }
}
