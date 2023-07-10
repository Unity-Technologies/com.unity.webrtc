using System.Diagnostics;    // Process
using System.IO;
using System.Linq;
using NUnit.Framework;       // Assert
using UnityEditor;
using UnityEngine;           // RuntimePlatform
using UnityEngine.TestTools; // UnityPlatform

namespace Unity.WebRTC.EditorTest
{

    class PluginTest
    {

        /// <todo>
        /// This test is only supported on Linux Editor, OSX Editor
        /// not found "libwebrtc.so" on Linux Editor.
        /// </todo>
        [Test]
        [UnityPlatform(RuntimePlatform.WindowsEditor)]
        public static void IsPluginLoaded()
        {


            // Get the current process.
            Process currentProcess = Process.GetCurrentProcess();
            var names = currentProcess.Modules
                .Cast<ProcessModule>()
                .Where(_ => _ != null)
                .Select(_ => Path.GetFileNameWithoutExtension(_.ModuleName));

            Assert.That(names, Contains.Item(WebRTC.GetModuleName()));
        }

        //----------------------------------------------------------------------------------------------------------------------
        [Test]
        public static void CheckPluginImportSettings()
        {

            string[] guids = AssetDatabase.FindAssets("", new[] { "Packages/com.unity.webrtc/Runtime/Plugins" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                Assert.IsNotNull(assetImporter);
                if (assetImporter.GetType() != typeof(PluginImporter))
                    continue;
                PluginImporter pluginImporter = assetImporter as PluginImporter;
                Assert.That(pluginImporter, Is.Not.Null);
                Assert.That(pluginImporter.isPreloaded, Is.True);
            }

        }

    }

} //namespace Unity.WebRTC.EditorTest
