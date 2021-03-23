using Unity.WebRTC.EditorTest;
using UnityEditor;
using UnityEditor.TestTools;

[assembly: TestPlayerBuildModifier(typeof(BuildModifier))]

namespace Unity.WebRTC.EditorTest
{

    public class BuildModifier : ITestPlayerBuildModifier
    {
        public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
        {
            if (playerOptions.target == BuildTarget.StandaloneOSX)
            {
                // ToDo: Need build WebRTC Plugin for Apple Silicon
                // Currently, Not compatible with Apple Silicon on WebRTC Plugin.
                // So, force set Architecture to only x64.
                EditorUserBuildSettings.SetPlatformSettings(
                    "Standalone",
                    "OSXUniversal",
                    "Architecture",
                    "x64"
                );
            }

            return playerOptions;
        }
    }
}
