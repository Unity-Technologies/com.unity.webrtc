using System;
using NUnit.Framework;

namespace Unity.WebRTC.RuntimeTest
{
    class IceCandidateTest
    {
        [Test]
        public void Construct()
        {
            Assert.That(() => new RTCIceCandidate(), Throws.Nothing);
        }

        [Test]
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
            Assert.That(candidate.Candidate, Is.Not.Empty);
            Assert.That(candidate.Candidate, Is.EqualTo(option.candidate));
            Assert.That(candidate.SdpMLineIndex, Is.EqualTo(option.sdpMLineIndex));
            Assert.That(candidate.SdpMid, Is.EqualTo(option.sdpMid));
            Assert.That(candidate.Component, Is.EqualTo(RTCIceComponent.Rtp));
            Assert.That(candidate.Foundation, Is.Not.Empty);
            Assert.That(candidate.Port, Is.Not.Null);
            Assert.That(candidate.Priority, Is.Not.Null);
            Assert.That(candidate.Address, Is.Not.Empty);
            Assert.That(candidate.Protocol, Is.Not.Null);
            Assert.That(candidate.RelatedAddress, Is.Not.Empty);
            Assert.That(candidate.RelatedPort, Is.Not.Null);
            Assert.That(candidate.SdpMid, Is.Not.Empty);
            Assert.That(candidate.SdpMLineIndex, Is.Not.Null);
            Assert.That(candidate.Type, Is.Not.Null);
            Assert.That(candidate.TcpType, Is.Null);
            Assert.That(candidate.UserNameFragment, Is.Not.Empty);
        }

        [Test]
        public void ConstructWithEndOfCandidate()
        {
            var option = new RTCIceCandidateInit
            {
                sdpMid = "0",
                sdpMLineIndex = 0,
                candidate = ""
            };
            var candidate = new RTCIceCandidate(option);
            Assert.That(candidate.Candidate, Is.Empty);
            Assert.That(candidate.Candidate, Is.Empty);
            Assert.That(candidate.SdpMLineIndex, Is.Null);
            Assert.That(candidate.SdpMid, Is.Empty);
            Assert.That(candidate.Component, Is.Null);
            Assert.That(candidate.Foundation, Is.Empty);
            Assert.That(candidate.Port, Is.Zero);
            Assert.That(candidate.Priority, Is.Zero);
            Assert.That(candidate.Address, Is.EqualTo(":0"));
            Assert.That(candidate.Protocol, Is.Null);
            Assert.That(candidate.RelatedAddress, Is.EqualTo(":0"));
            Assert.That(candidate.RelatedPort, Is.Zero);
            Assert.That(candidate.SdpMid, Is.Empty);
            Assert.That(candidate.SdpMLineIndex, Is.Null);
            Assert.That(candidate.Type, Is.Null);
            Assert.That(candidate.TcpType, Is.Null);
            Assert.That(candidate.UserNameFragment, Is.Empty);
        }
    }
}
