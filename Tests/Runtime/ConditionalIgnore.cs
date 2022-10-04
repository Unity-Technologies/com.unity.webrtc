using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Rendering;

namespace Unity.WebRTC.RuntimeTest
{
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnLoad()
        {
#if !UNITY_2020_1_OR_NEWER
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(UnsupportedPlatformVideoDecoder,
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12);
#endif
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(UnsupportedPlatformOpenGL,
                (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3) &&
                (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.OSXPlayer));
        }
    }
}
