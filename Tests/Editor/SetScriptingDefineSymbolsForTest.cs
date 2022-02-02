using System;
using UnityEditor;

namespace Unity.WebRTC.EditorTest
{
    [InitializeOnLoad]
    class SetScriptingDefineSymbolsForTest
    {
        static SetScriptingDefineSymbolsForTest()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("YAMATO_JOB_ID")))
            {
                var target = EditorUserBuildSettings.activeBuildTarget;
                var group = BuildPipeline.GetBuildTargetGroup(target);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, "WEBRTC_TEST_PROJECT");
            }
        }
    }
}
