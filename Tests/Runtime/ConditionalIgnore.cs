using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    internal class ConditionalIgnore
    {
        public const string UnsupportedPlatformVideoDecoder = "IgnoreUnsupportedPlatformVideoDecoder";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnLoad()
        {
#if !UNITY_2020_1_OR_NEWER
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(UnsupportedPlatformVideoDecoder,
                SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12);
#endif
        }
    }
}
