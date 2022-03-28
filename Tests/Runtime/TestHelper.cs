using System.Linq;
using UnityEngine;

namespace Unity.WebRTC.RuntimeTest
{
    internal class TestHelper
    {
        private static readonly string[] excludeCodecMimeType = {"video/red", "video/ulpfec", "video/rtx"};

        public static bool CheckVideoSendRecvCodecSupport()
        {
            // hardware encoder is not supported
            if (!WebRTC.HardwareEncoderSupport())
                return false;

            WebRTC.Initialize();
            var capabilitiesSenderCodec = RTCRtpSender.GetCapabilities(TrackKind.Video)
                .codecs
                .Select(x => x.mimeType)
                .Except(excludeCodecMimeType);
            var capabilitiesReceiverCodec = RTCRtpReceiver.GetCapabilities(TrackKind.Video)
                .codecs
                .Select(x => x.mimeType)
                .Except(excludeCodecMimeType);
            var isSupported = capabilitiesSenderCodec.Any(x => capabilitiesReceiverCodec.Contains(x));
            WebRTC.Dispose();
            return isSupported;
        }
    }
}
