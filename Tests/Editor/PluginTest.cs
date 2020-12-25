using UnityEngine;           // RuntimePlatform
using System.Diagnostics;    // Process
using System.Linq;
using UnityEngine.TestTools; // UnityPlatform
using NUnit.Framework;       // Assert
using UnityEditor;

namespace Unity.WebRTC.EditorTest {

class PluginTest {

    /// <todo>
    /// This test is only supported on Linux Editor, OSX Editor
    /// not found "libwebrtc.so" on Linux Editor.
    /// </todo>
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor)]
    public static void IsPluginLoaded() {

        
        // Get the current process.
        Process currentProcess = Process.GetCurrentProcess();
        var names = currentProcess.Modules
            .Cast<ProcessModule>()
            .Where(_ => _ != null)
            .Select(_ => _.ModuleName)
            .ToArray();

        Assert.Contains(WebRTC.GetModuleName(), names);
    }

//----------------------------------------------------------------------------------------------------------------------
    [Test]
    public static void CheckPluginImportSettings() {

        string[] guids = AssetDatabase.FindAssets("", new[] {"Packages/com.unity.webrtc/Runtime/Plugins"});
        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetImporter assetImporter = AssetImporter.GetAtPath(path);
            Assert.IsNotNull(assetImporter);
            if (assetImporter.GetType() != typeof(PluginImporter))
                continue;
            PluginImporter pluginImporter = assetImporter as PluginImporter;
            Assert.IsNotNull(pluginImporter);
            Assert.IsTrue(pluginImporter.isPreloaded);
        }
       
    }

}

} //namespace Unity.WebRTC.EditorTest
