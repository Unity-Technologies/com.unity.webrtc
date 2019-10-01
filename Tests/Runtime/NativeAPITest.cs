using NUnit.Framework;
using Unity.WebRTC;

class NativeAPITest
{
    [Test]
    public void RegisterDebugLog()
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
    public void CreateAndDeleteMediaStream()
    {
        var context = NativeMethods.ContextCreate(0);
        var peer = NativeMethods.ContextCreatePeerConnection(context);
        var width = 1280;
        var height = 720;
        var renderTexture = new UnityEngine.RenderTexture(width, height, 0, UnityEngine.RenderTextureFormat.BGRA32);
        var stream = NativeMethods.CreateMediaStream(context, "MediaStream");
        NativeMethods.ContextDeletePeerConnection(context, peer);
        NativeMethods.ContextDestroy(0);
    }
}
