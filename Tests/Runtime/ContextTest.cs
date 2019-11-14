using NUnit.Framework;
using Unity.WebRTC;

class ContextTest
{
    [Test]
    [Category("Context")]
    public void Context_CreateAndDelete()
    {
    	Context.Create();
    }

    [Test]
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
