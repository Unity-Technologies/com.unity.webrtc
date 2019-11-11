using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unity.WebRTC;

class ContextTest
{
    [Test]
    [Category("Context")]
    public void Context_Create()
    {
    	Context.Create();
    }
}