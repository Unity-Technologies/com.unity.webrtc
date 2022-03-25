using System.Linq;
using UnityEngine;

namespace Unity.WebRTC.RuntimeTest
{
    internal class TestHelper
    {
        public static bool HardwareCodecSupport()
        {
            if (!WebRTC.HardwareEncoderSupport())
                return false;
            WebRTC.Initialize();
            var isSupported = CheckVideoCodecCapabilities();
            WebRTC.Dispose();
            return isSupported;
        }

        static bool CheckVideoCodecCapabilities()
        {
            var capabilitiesSender = RTCRtpSender.GetCapabilities(TrackKind.Video);
            if (!capabilitiesSender.codecs.Any())
                return false;
            var capabilitiesReceiver = RTCRtpReceiver.GetCapabilities(TrackKind.Video);
            if (!capabilitiesReceiver.codecs.Any())
                return false;
            return true;
        }

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
