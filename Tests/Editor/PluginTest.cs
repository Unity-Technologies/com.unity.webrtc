using UnityEngine;           // RuntimePlatform
using System.Diagnostics;    // Process
using System.Linq;
using UnityEngine.TestTools; // UnityPlatform
using NUnit.Framework;       // Assert

namespace Unity.WebRTC.Editor {

public class PluginTest {

    /// <todo>
    /// This test is only supported on Linux Editor, OSX Editor
    /// not found "libwebrtc.so" on Linux Editor.
    /// </todo>
    [Test]
    [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor)]
    public static void IsPluginLoaded() {

        
#if !UNITY_EDITOR_OSX
        // Get the current process.
        Process currentProcess = Process.GetCurrentProcess();
        var names = currentProcess.Modules
            .Cast<ProcessModule>()
            .Where(_ => _ != null)
            .Select(_ => _.ModuleName)
            .ToArray();

        Assert.Contains(WebRTC.GetModuleName(), names);
#endif
    }
}

} //namespace Unity.WebRTC.Editor
