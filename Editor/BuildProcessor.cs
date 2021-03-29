using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Unity.WebRTC.Editor
{
    public class PreprocessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public const int RequiredMinSdkVersion = 21;

        public const AndroidArchitecture RequiredTargetArchitectures = AndroidArchitecture.ARM64;


        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                EnsureMinSdkVersion();
                EnsureArchitecture();
            }
        }


        static void EnsureMinSdkVersion()
        {
            if ((int)PlayerSettings.Android.minSdkVersion < RequiredMinSdkVersion)
            {
                throw new BuildFailedException(
                    $"WebRTC apps require a minimum SDK version of {RequiredMinSdkVersion}. " +
                    $"Currently set to {PlayerSettings.Android.minSdkVersion}");
            }
        }

        static void EnsureArchitecture()
        {
            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                throw new BuildFailedException(
                    $"WebRTC apps require a target architecture to be set {RequiredTargetArchitectures}. " +
                    $"Currently set to {PlayerSettings.Android.targetArchitectures}");
            }
        }
    }
}
