using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Unity.WebRTC.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class PreprocessBuild : IPreprocessBuildWithReport
    {
        /// <summary>
        /// 
        /// </summary>
        int IOrderedCallback.callbackOrder => 1;

        /// <summary>
        /// 
        /// </summary>
        public const AndroidSdkVersions RequiredMinSdkVersion = AndroidSdkVersions.AndroidApiLevel21;

        /// <summary>
        /// 
        /// </summary>
        public const AndroidArchitecture RequiredTargetArchitectures = AndroidArchitecture.ARM64;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                EnsureMinSdkVersion();
                EnsureArchitecture();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void EnsureMinSdkVersion()
        {
            if ((int)PlayerSettings.Android.minSdkVersion < (int)RequiredMinSdkVersion)
            {
                throw new BuildFailedException(
                    $"WebRTC apps require a minimum SDK version of {RequiredMinSdkVersion}. " +
                    $"Currently set to {PlayerSettings.Android.minSdkVersion}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
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
