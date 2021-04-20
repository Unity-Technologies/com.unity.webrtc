using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.WebRTC.Editor
{
    /// <summary>
    /// This type express a return value of "PlayerSettings.GetArchitecture(BuildTargetGroup.iOS)"
    /// </summary>
    public enum iOSArchitecture
    {
        ARMv7 = 0,
        ARM64 = 1,
        Universal = 2
    }

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
        public const AndroidSdkVersions RequiredAndroidSdkVersion = AndroidSdkVersions.AndroidApiLevel21;

        /// <summary>
        ///
        /// </summary>
        public const AndroidArchitecture RequiredAndroidArchitectures = AndroidArchitecture.ARM64;

        /// <summary>
        ///
        /// </summary>
        public const iOSArchitecture RequiredIOSArchitectures = iOSArchitecture.ARM64;

        /// <summary>
        ///
        /// </summary>
        /// <param name="report"></param>
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            switch (report.summary.platform)
            {
                case BuildTarget.Android:
                    EnsureAndroidSdkVersion();
                    EnsureAndroidArchitecture();
                    break;
                case BuildTarget.iOS:
                    EnsureIOSArchitecture();
                    break;
                case BuildTarget.StandaloneOSX:
                    EnsureOSXArchitecture();
                    break;
            }
        }

        /// <summary>
        ///
        /// </summary>
        static void EnsureAndroidSdkVersion()
        {
            if ((int)PlayerSettings.Android.minSdkVersion < (int)RequiredAndroidSdkVersion)
            {
                Debug.LogWarning(
                    $"WebRTC apps require a minimum SDK version of {RequiredAndroidSdkVersion}. " +
                    $"Currently set to {PlayerSettings.Android.minSdkVersion}");
            }
        }

        /// <summary>
        ///
        /// </summary>
        static void EnsureAndroidArchitecture()
        {
            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                Debug.LogWarning(
                    $"WebRTC apps require a target architecture to be set {RequiredAndroidArchitectures}. " +
                    $"Currently set to {PlayerSettings.Android.targetArchitectures}");
            }
        }

        /// <summary>
        ///
        /// </summary>
        static void EnsureIOSArchitecture()
        {
            // Architecture value is ignored when using SimulatorSDK
            if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK)
                return;

            var architecture = (iOSArchitecture)PlayerSettings.GetArchitecture(BuildTargetGroup.iOS);

            if(architecture != iOSArchitecture.ARM64)
            {
                Debug.LogWarning(
                    $"WebRTC apps require a target architecture to be set {RequiredIOSArchitectures}. " +
                    $"Currently set to {architecture}");
            }
        }

        /// <summary>
        ///
        /// </summary>
        static void EnsureOSXArchitecture()
        {
            var platformName = BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneOSX);
            var architecture = EditorUserBuildSettings.GetPlatformSettings(platformName, "Architecture");

            // throws exception when selecting  "ARM64" or "x64ARM64" for the architecture
            if (architecture != "x64")
            {
                Debug.LogWarning(
                    $"Apple Silicon architecture is not supported by WebRTC package" +
                    $"Currently set to {architecture}");
            }
        }
    }
}
