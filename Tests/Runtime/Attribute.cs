using System;
using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class LongRunningAttribute : CategoryAttribute
    {
    }
}
