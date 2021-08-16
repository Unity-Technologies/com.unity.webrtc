using System;
using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    class IceCandidateTest
    {
        [SetUp]
        public void SetUp()
        {
            var type = TestHelper.HardwareCodecSupport() ? EncoderType.Hardware : EncoderType.Software;
            WebRTC.Initialize(type: type, limitTextureSize: true, forTest: true);
        }

        [TearDown]
        public void TearDown()
        {
            WebRTC.Dispose();
        }

        [Test]
        [Category("IceCandidate")]
        public void Construct()
        {
            Assert.Throws<ArgumentException>(() => new RTCIceCandidate());
        }

        [Test]
        [Category("IceCandidate")]
        public void ConstructWithOption()
        {
            var option = new RTCIceCandidateInit
            {
                sdpMid = "0",
                sdpMLineIndex = 0,
                candidate =
                    "candidate:102362043 1 udp 2122262783 240b:10:2fe0:4900:3cbd:7306:63c4:a8e1 50241 typ host generation 0 ufrag DEIG network-id 5"
            };
            var candidate = new RTCIceCandidate(option);
            Assert.IsNotEmpty(candidate.Candidate);
            Assert.AreEqual(candidate.Candidate, option.candidate);
            Assert.AreEqual(candidate.SdpMLineIndex, option.sdpMLineIndex);
            Assert.AreEqual(candidate.SdpMid, option.sdpMid);
            Assert.AreEqual(RTCIceComponent.Rtp, candidate.Component);
            Assert.IsNotEmpty(candidate.Foundation);
            Assert.NotNull(candidate.Port);
            Assert.NotNull(candidate.Priority);
            Assert.IsNotEmpty(candidate.Address);
            Assert.NotNull(candidate.Protocol);
            Assert.IsNotEmpty(candidate.RelatedAddress);
            Assert.NotNull(candidate.RelatedPort);
            Assert.IsNotEmpty(candidate.SdpMid);
            Assert.NotNull(candidate.SdpMLineIndex);
            Assert.NotNull(candidate.Type);
            Assert.Null(candidate.TcpType);
            Assert.IsNotEmpty(candidate.UserNameFragment);
        }
    }
}
