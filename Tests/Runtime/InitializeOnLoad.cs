using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    public class InitializeOnLoad
    {
        [RuntimeInitializeOnLoadMethod]
        static void OnLoad()
        {
            var ignoreHardwareEncoderTest = !NativeMethods.GetHardwareEncoderSupport();
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping("IgnoreHardwareEncoderTest", ignoreHardwareEncoderTest);
        }
    }
}
