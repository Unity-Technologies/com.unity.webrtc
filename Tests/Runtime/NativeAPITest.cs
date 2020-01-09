using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

class NativeAPITest
{
    [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
    static void DebugLog(string str)
    {
        Debug.Log(str);
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
    public void CreateAndDestroyContext()
    {
        var context = NativeMethods.ContextCreate(0);
        NativeMethods.ContextDestroy(0);
    }

    [Test]
    public void CreateAndDeletePeerConnection()
    {
        var context = NativeMethods.ContextCreate(0);
        var peer = NativeMethods.ContextCreatePeerConnection(context);
        NativeMethods.ContextDeletePeerConnection(context, peer);
        NativeMethods.ContextDestroy(0);
    }

    [Test]
    public void CreateAndDeleteDataChannel()
    {
        var context = NativeMethods.ContextCreate(0);
        var peer = NativeMethods.ContextCreatePeerConnection(context);
        var init = new RTCDataChannelInit(true);
        var channel = NativeMethods.ContextCreateDataChannel(context, peer, "test", ref init);
        NativeMethods.ContextDeleteDataChannel(context, channel);
        NativeMethods.ContextDeletePeerConnection(context, peer);
        NativeMethods.ContextDestroy(0);
    }
    [Test]
    public void CreateAndDeleteVideoStream()
    {
        var context = NativeMethods.ContextCreate(0);
        const int width = 1280;
        const int height = 720;
        var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
        var renderTexture = new RenderTexture(width, height, 0, format);
        renderTexture.Create();
        var stream = NativeMethods.ContextCreateVideoStream(context, renderTexture.GetNativeTexturePtr(), width, height);
        NativeMethods.ContextDeleteVideoStream(context, stream);
        NativeMethods.ContextDestroy(0);
    }

    [Test]
    public void CreateAndDeleteAudioStream()
    {
        var context = NativeMethods.ContextCreate(0);
        var stream = NativeMethods.ContextCreateAudioStream(context);
        NativeMethods.ContextDeleteAudioStream(context, stream);
        NativeMethods.ContextDestroy(0);
    }


    [Test]
    public void CallGetRenderEventFunc()
    {
        var context = NativeMethods.ContextCreate(0);
        var callback = NativeMethods.GetRenderEventFunc(context);
        Assert.AreNotEqual(callback, IntPtr.Zero);
        NativeMethods.ContextDestroy(0);
    }

    [UnityTest]
    public IEnumerator CallVideoEncoderMethods()
    {

        Assert.IsTrue(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal);

        var context = NativeMethods.ContextCreate(0);
        const int width = 1280;
        const int height = 720;
        var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
        var renderTexture = new RenderTexture(width, height, 0, format);
        renderTexture.Create();
        var stream = NativeMethods.ContextCreateVideoStream(context, renderTexture.GetNativeTexturePtr(), width, height);
        var callback = NativeMethods.GetRenderEventFunc(context);

        // TODO::
        // note:: You must call `InitializeEncoder` method after `NativeMethods.ContextCaptureVideoStream`
        VideoEncoderMethods.InitializeEncoder(callback);
        yield return new WaitForSeconds(1.0f);
        //VideoEncoderMethods.Encode(callback);
        //yield return new WaitForEndOfFrame();
        //VideoEncoderMethods.FinalizeEncoder(callback);
        //yield return new WaitForEndOfFrame();

        NativeMethods.ContextDeleteVideoStream(context, stream);
        NativeMethods.ContextDestroy(0);
        yield return null;
    }
}
