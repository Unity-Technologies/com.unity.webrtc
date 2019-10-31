using System;
using NUnit.Framework;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.Rendering;

class NativeAPITest
{
    [AOT.MonoPInvokeCallback(typeof(DelegateDebugLog))]
    static void DebugLog(string str)
    {
        Debug.Log(str);
    }

    static RenderTextureFormat GetSupportedRenderTextureFormat(GraphicsDeviceType type)
    {
        switch (type)
        {
            case GraphicsDeviceType.Direct3D11:
            case GraphicsDeviceType.Direct3D12:
                return RenderTextureFormat.BGRA32;
            case GraphicsDeviceType.OpenGLCore:
            case GraphicsDeviceType.OpenGLES2:
            case GraphicsDeviceType.OpenGLES3:
                return RenderTextureFormat.ARGB32;
        }
        return RenderTextureFormat.Default;
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
        var format = GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
        var renderTexture = new RenderTexture(width, height, 0, format);
        renderTexture.Create();
        var stream = NativeMethods.ContextCaptureVideoStream(context, renderTexture.GetNativeTexturePtr(), width, height);
        NativeMethods.ContextDeleteVideoStream(context, stream);
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

    [Test]
    public void CallVideoEncoderMethods()
    {
        var context = NativeMethods.ContextCreate(0);
        const int width = 1280;
        const int height = 720;
        var format = GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
        var renderTexture = new RenderTexture(width, height, 0, format);
        renderTexture.Create();
        var stream = NativeMethods.ContextCaptureVideoStream(context, renderTexture.GetNativeTexturePtr(), width, height);
        var callback = NativeMethods.GetRenderEventFunc(context);

        // TODO::
        // note:: You must call `InitializeEncoder` method after `NativeMethods.ContextCaptureVideoStream`
        VideoEncoderMethods.InitializeEncoder(callback);
        VideoEncoderMethods.Encode(callback);
        VideoEncoderMethods.FinalizeEncoder(callback);

        NativeMethods.ContextDeleteVideoStream(context, stream);
        NativeMethods.ContextDestroy(0);
    }
}
