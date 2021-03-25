using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    internal class ConditionalIgnore
    {
        public const string UnsupportedHardwareForHardwareCodec = "IgnoreUnsupportedHardwareForHardwareCodec";
        public const string Direct3D12 = "IgnoreDirect3D12";

        [RuntimeInitializeOnLoadMethod]
        static void OnLoad()
        {
            var ignoreHardwareEncoderTest = !NativeMethods.GetHardwareEncoderSupport();
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(UnsupportedHardwareForHardwareCodec,
                ignoreHardwareEncoderTest);

            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(Direct3D12,
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12);
        }
    }
}
