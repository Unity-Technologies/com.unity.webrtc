using UnityEngine;           // RuntimePlatform
using System.Diagnostics;    // Process
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
        // Get the current process.
        Process currentProcess = Process.GetCurrentProcess();
        bool found = false;
        var enumerator = currentProcess.Modules.GetEnumerator();
        while (enumerator.MoveNext()) {
            ProcessModule module = enumerator.Current as ProcessModule;
            if (null != module && module.ModuleName == WebRTC.Lib)
                found = true;

        }
        Assert.True(found);
    }
}

} //namespace Unity.WebRTC.Editor
