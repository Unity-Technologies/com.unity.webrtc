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
#if UNITY_2021
        public const AndroidSdkVersions RequiredAndroidSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
#else
        public const AndroidSdkVersions RequiredAndroidSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
#endif
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
                    EnsureAndroidInternetPermission();
                    break;
                case BuildTarget.iOS:
                    EnsureIOSArchitecture();
                    break;
                case BuildTarget.StandaloneOSX:
                    EnsureOSXArchitecture();
                    break;
                case BuildTarget.StandaloneWindows:
                    throw new BuildFailedException(
                        "Windows 32bit(x86) architecture is not supported by WebRTC package.");
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

        static void EnsureAndroidInternetPermission()
        {
            if (!PlayerSettings.Android.forceInternetPermission)
            {
                Debug.LogWarning(
                    $"WebRTC apps require a internet permission on Android devices." +
                    "Please check the \"Internet Access\" on your Build Settings.");
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
        }
    }
}
