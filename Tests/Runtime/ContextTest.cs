﻿using NUnit.Framework;
using Unity.WebRTC;

class ContextTest
{
    [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
    static void DebugLog(string str)
    {
        UnityEngine.Debug.Log(str);
    }

    [SetUp]
    public void SetUp()
    {
        NativeMethods.RegisterDebugLog(DebugLog);
    }

    [TearDown]
    public void TearDown()
    {
        NativeMethods.RegisterDebugLog(null);
    }

    [Test]
    [Category("Context")]
    public void Context_CreateAndDelete()
    {
    	Context.Create();
    }

    [Test]
    [Category("Context")]
    public void Context_CreateAndDeletePeerConnection()
    {
        var context = Context.Create();
        var peerPtr = context.CreatePeerConnection();
        context.DeletePeerConnection(peerPtr);
    }

    [Test]
    [Category("Context")]
    public void Context_CreateAndDeleteDataChannel()
    {
        var context = Context.Create();
        var peerPtr = context.CreatePeerConnection();
        var init = new RTCDataChannelInit(true);
        var channelPtr = context.CreateDataChannel(peerPtr, "test", ref init);
        context.DeleteDataChannel(channelPtr);
        context.DeletePeerConnection(peerPtr);
    }
}
