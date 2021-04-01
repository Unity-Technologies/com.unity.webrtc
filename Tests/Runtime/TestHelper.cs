using System.Linq;
using UnityEngine;

namespace Unity.WebRTC.RuntimeTest
{
    internal class TestHelper
    {
        public static bool HardwareCodecSupport()
        {
            var value = NativeMethods.GetHardwareEncoderSupport();
            if (!value)
                return false;
            WebRTC.Initialize(EncoderType.Hardware);
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
    }
}
