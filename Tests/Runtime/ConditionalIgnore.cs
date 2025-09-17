using System;
using UnityEngine;
using UnityEngine.TestTools;

#if !UNITY_2020_1_OR_NEWER
using UnityEngine.Rendering;
#endif

namespace Unity.WebRTC.RuntimeTest
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    class ConditionalIgnoreMultipleAttribute : ConditionalIgnoreAttribute
    {
        public ConditionalIgnoreMultipleAttribute(string conditionKey, string ignoreReason)
            : base(conditionKey, ignoreReason)
        {
        }
    }


    internal class ConditionalIgnore
    {
        /// <summary>
        /// On Direct3D12 platform, CommandBuffer.IssuePluginCustomTextureUpdateV2 method doesn't work.
        /// </summary>
        public const string UnsupportedPlatformVideoDecoder = "IgnoreUnsupportedPlatformVideoDecoder";

        /// <summary>
        /// On Windows and macOS platform, VideoStreamTrack class doesn't work.
        /// </summary>
        public const string UnsupportedPlatformOpenGL = "IgnoreUnsupportedPlatformOpenGL";

        static bool s_loaded = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnLoad()
        {
            if (s_loaded)
                return;
            s_loaded = true;

#if !UNITY_2020_1_OR_NEWER
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(UnsupportedPlatformVideoDecoder,
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12);
#endif
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(UnsupportedPlatformOpenGL,
                !VideoStreamTrack.IsSupported(Application.platform, SystemInfo.graphicsDeviceType));
        }

        internal static bool IsUnstableOnGraphicsDevice(RuntimePlatform platform, string deviceName)
        {
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return deviceName.Contains("Microsoft Basic Render Driver");
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return deviceName.Contains("llvmpipe");
                default:
                    return false;
            }
        }
    }
}
