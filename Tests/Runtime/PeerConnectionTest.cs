using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unity.WebRTC;

class PeerConnectionTest
{
    static RTCConfiguration GetConfiguration()
    {
        RTCConfiguration config = default;
        config.iceServers = new RTCIceServer[]
        {
            new RTCIceServer
            {
                urls = new string[] { "stun:stun.l.google.com:19302" },
                username = "",
                credential = "",
                credentialType = RTCIceCredentialType.Password
            }
        };
        return config;
    }

    [SetUp]
    public void SetUp()
    {
        WebRTC.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        WebRTC.Finalize();
    }

    [Test]
    [Category("PeerConnection")]
    public void PeerConnection_Construct()
    {
        var peer = new RTCPeerConnection();
        peer.Close();
    }

    [Test]
    [Category("PeerConnection")]
    public void PeerConnection_ConstructWithConfig()
    {
        var config = GetConfiguration();
        var peer = new RTCPeerConnection(ref config);

        var config2 = peer.GetConfiguration();
        Assert.NotNull(config.iceServers);
        Assert.NotNull(config2.iceServers);
        Assert.AreEqual(config.iceServers.Length, config2.iceServers.Length);
        Assert.AreEqual(config.iceServers[0].username, config2.iceServers[0].username);
        Assert.AreEqual(config.iceServers[0].credential, config2.iceServers[0].credential);
        Assert.AreEqual(config.iceServers[0].urls, config2.iceServers[0].urls);

        peer.Close();
    }

    [Test]
    [Category("PeerConnection")]
    public void PeerConnection_SetConfiguration()
    {
        var peer = new RTCPeerConnection();
        var config = GetConfiguration();
        var result = peer.SetConfiguration(ref config);
        Assert.AreEqual(RTCErrorType.None, result);
    }

    [Test]
    [Category("PeerConnection")]
    public void PeerConnection_SetCallback()
    {
        var peer = new RTCPeerConnection();
        var config = GetConfiguration();
        var result = peer.SetConfiguration(ref config);

        var pc1OnIceCandidate = new DelegateOnIceCandidate(candidate => { ; });
        var pc1OnIceConnectionChange = new DelegateOnIceConnectionChange(candidate => { ; });
        var pc1OnNegotiationNeeded = new DelegateOnNegotiationNeeded(() => { ; });

        peer.OnIceCandidate = pc1OnIceCandidate;
        peer.OnIceConnectionChange = pc1OnIceConnectionChange;
        peer.OnNegotiationNeeded = pc1OnNegotiationNeeded;
    }

    [UnityTest]
    [Category("PeerConnection")]

    public IEnumerator PeerConnection_CreateOffer()
    {
        var config = GetConfiguration();
        var peer = new RTCPeerConnection(ref config);
        RTCOfferOptions options = default;
        var op = peer.CreateOffer(ref options);

        yield return op;
        Assert.True(op.isDone);
        Assert.False(op.isError);

        peer.Close();
    }

    [UnityTest]
    [Category("PeerConnection")]

    public IEnumerator PeerConnection_SetLocalDescription()
    {
        var peer = new RTCPeerConnection();
        RTCOfferOptions options = default;
        var op = peer.CreateOffer(ref options);
        yield return op;
        Assert.True(op.isDone);
        Assert.False(op.isError);
        var op2 = peer.SetLocalDescription(ref op.desc);
        yield return op2;
        Assert.True(op2.isDone);
        Assert.False(op2.isError);
        peer.Close();
    }

    [UnityTest]
    [Timeout(5000)]
    [Category("PeerConnection")]
    public IEnumerator PeerConnection_OnNegotiationNeeded()
    {
        var peer = new RTCPeerConnection();
        bool isDone = false;
        peer.OnNegotiationNeeded = new DelegateOnNegotiationNeeded(() => { isDone = true; });
        RTCOfferOptions options = default;
        var op = peer.CreateOffer(ref options);
        yield return op;
        Assert.True(op.isDone);
        Assert.False(op.isError);
        var op2 = peer.SetLocalDescription(ref op.desc);
        yield return op2;
        Assert.True(op2.isDone);
        Assert.False(op2.isError);

        var conf = new RTCDataChannelInit(true);
        var channel = peer.CreateDataChannel("data", ref conf);

        yield return new WaitUntil(() => isDone);
        peer.Close();
    }

    [UnityTest]
    [Timeout(5000)]
    [Category("PeerConnection")]
    public IEnumerator PeerConnection_OnSignalingChange()
    {
        var config = GetConfiguration();
        var peer1 = new RTCPeerConnection(ref config);
        var peer2 = new RTCPeerConnection(ref config);

        peer1.OnIceCandidate = new DelegateOnIceCandidate(candidate => { peer2.AddIceCandidate(ref candidate); });
        peer2.OnIceCandidate = new DelegateOnIceCandidate(candidate => { peer1.AddIceCandidate(ref candidate); });

        var state = SignalingState.Closed;
        peer1.OnSignalingChange = new DelegateOnSignalingChange(_state => { state = _state; });

        var conf = new RTCDataChannelInit(true);
        var channel1 = peer1.CreateDataChannel("data", ref conf);

        RTCOfferOptions options1 = default;
        RTCAnswerOptions options2 = default;
        var op1 = peer1.CreateOffer(ref options1);
        yield return op1;
        var op2 = peer1.SetLocalDescription(ref op1.desc);
        yield return op2;
        var op3 = peer2.SetRemoteDescription(ref op1.desc);
        yield return op3;
        var op4 = peer2.CreateAnswer(ref options2);
        yield return op4;
        var op5 = peer2.SetLocalDescription(ref op4.desc);
        yield return op5;
        var op6 = peer1.SetRemoteDescription(ref op4.desc);
        yield return op6;

        yield return new WaitUntil(() => state == SignalingState.Stable);
    }

    [UnityTest]
    [Timeout(5000)]
    [Category("PeerConnection")]
    public IEnumerator PeerConnection_CreateDataChannel()
    {
        var config = GetConfiguration();
        var peer1 = new RTCPeerConnection(ref config);
        var peer2 = new RTCPeerConnection(ref config);

        peer1.OnIceCandidate = new DelegateOnIceCandidate(candidate => { peer2.AddIceCandidate(ref candidate); });
        peer2.OnIceCandidate = new DelegateOnIceCandidate(candidate => { peer1.AddIceCandidate(ref candidate); });

        var conf = new RTCDataChannelInit(true);
        var channel1 = peer1.CreateDataChannel("data", ref conf);

        RTCOfferOptions options1 = default;
        RTCAnswerOptions options2 = default;
        var op1 = peer1.CreateOffer(ref options1);
        yield return op1;
        var op2 = peer1.SetLocalDescription(ref op1.desc);
        yield return op2;
        var op3 = peer2.SetRemoteDescription(ref op1.desc);
        yield return op3;
        var op4 = peer2.CreateAnswer(ref options2);
        yield return op4;
        var op5 = peer2.SetLocalDescription(ref op4.desc);
        yield return op5;
        var op6 = peer1.SetRemoteDescription(ref op4.desc);
        yield return op6;

        yield return new WaitUntil(() => peer1.IceConnectionState == RTCIceConnectionState.Connected || peer1.IceConnectionState == RTCIceConnectionState.Completed);
        yield return new WaitUntil(() => peer2.IceConnectionState == RTCIceConnectionState.Connected || peer2.IceConnectionState == RTCIceConnectionState.Completed);
    }
}
