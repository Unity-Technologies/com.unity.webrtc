using System;
using UnityEditor;

namespace Unity.WebRTC.EditorTest
{
    [InitializeOnLoad]
    class SetScriptingDefineSymbolsForTest
    {
        static SetScriptingDefineSymbolsForTest()
        {
            // Opt-in for tests that need real devices (e.g. a webcam), which CI runners do not have.
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBRTC_ENABLE_DEVICE_TESTS")))
            {
                var target = EditorUserBuildSettings.activeBuildTarget;
                var group = BuildPipeline.GetBuildTargetGroup(target);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, "WEBRTC_TEST_PROJECT");
            }
        }
    }
}
