using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    internal class ConditionalIgnore
    {
        public const string UnsupportedHardwareForHardwareCodec = "IgnoreUnsupportedHardwareForHardwareCodec";
        public const string Direct3D12 = "IgnoreDirect3D12";
        public const string OpenGLOnLinux = "IgnoreOpenGLOnLinux";

        [RuntimeInitializeOnLoadMethod]
        static void OnLoad()
        {
            var ignoreHardwareEncoderTest = !NativeMethods.GetHardwareEncoderSupport();
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(UnsupportedHardwareForHardwareCodec,
                ignoreHardwareEncoderTest);

            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(Direct3D12,
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12);

            bool isLinux = Application.platform == RuntimePlatform.LinuxEditor ||
                           Application.platform == RuntimePlatform.LinuxPlayer;
            bool isOpenGL = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore ||
                            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
                            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3;

            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(OpenGLOnLinux, isOpenGL && isLinux);
        }
    }
}
