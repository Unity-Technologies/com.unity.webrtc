using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    /// <summary>
    /// Initialization options for creating an ICE candidate.
    /// </summary>
    public class RTCIceCandidateInit
    {
        /// <summary>
        /// SDP string describing the candidate.
        /// </summary>
        public string candidate;
        /// <summary>
        /// The media stream identification for the candidate.
        /// </summary>
        public string sdpMid;
        /// <summary>
        /// The index of the m-line in SDP.
        /// </summary>
        public int? sdpMLineIndex;
    }

    /// <summary>
    /// Enumerated type to specify a ICE component.
    /// </summary>
    /// <seealso cref="RTCIceCandidate"/>
    public enum RTCIceComponent : int
    {
        /// <summary>
        /// RTP component.
        /// </summary>
        Rtp = 1,
        /// <summary>
        /// RTCP component.
        /// </summary>
        Rtcp = 2,
    }

    /// <summary>
    /// Indicates the transport protocol used by the ICE candidate.
    /// </summary>
    public enum RTCIceProtocol : int
    {
        /// <summary>
        /// UDP protocol.
        /// </summary>
        Udp = 1,
        /// <summary>
        /// TCP protocol.
        /// </summary>
        Tcp = 2
    }

    /// <summary>
    /// Specifies the type of ICE candidate.
    /// </summary>
    public enum RTCIceCandidateType
    {
        /// <summary>
        /// Host candidate type.
        /// </summary>
        Host,
        /// <summary>
        /// Server reflexive candidate type.
        /// </summary>
        Srflx,
        /// <summary>
        /// Peer reflexive candidate type.
        /// </summary>
        Prflx,
        /// <summary>
        /// Relay candidate type.
        /// </summary>
        Relay
    }

    /// <summary>
    /// Describes the TCP type for ICE candidates.
    /// </summary>
    public enum RTCIceTcpCandidateType
    {
        /// <summary>
        /// Active TCP candidate.
        /// </summary>
        Active,
        /// <summary>
        /// Passive TCP candidate.
        /// </summary>
        Passive,
        /// <summary>
        /// Simultaneous open TCP candidate.
        /// </summary>
        So
    }

    internal static class CandidateExtention
    {
        public static RTCIceProtocol ParseRTCIceProtocol(this string src)
        {
            switch (src)
            {
                case "udp":
                    return RTCIceProtocol.Udp;
                case "tcp":
                    return RTCIceProtocol.Tcp;
                default:
                    throw new ArgumentException($"Invalid parameter: {src}");
            }
        }

        public static RTCIceCandidateType ParseRTCIceCandidateType(this string src)
        {
            switch (src)
            {
                case "local":
                    return RTCIceCandidateType.Host;
                case "stun":
                    return RTCIceCandidateType.Srflx;
                case "prflx":
                    return RTCIceCandidateType.Prflx;
                case "relay":
                    return RTCIceCandidateType.Relay;
                default:
                    throw new ArgumentException($"Invalid parameter: {src}");
            }
        }


        public static RTCIceTcpCandidateType? ParseRTCIceTcpCandidateType(this string src)
        {
            if (string.IsNullOrEmpty(src))
                return null;
            switch (src)
            {
                case "active":
                    return RTCIceTcpCandidateType.Active;
                case "passive":
                    return RTCIceTcpCandidateType.Passive;
                case "so":
                    return RTCIceTcpCandidateType.So;
                default:
                    throw new ArgumentException($"Invalid parameter: {src}");
            }
        }
    }

    /// <summary>
    /// Represents an ICE candidate used for network connectivity checks.
    /// </summary>
    public class RTCIceCandidate : IDisposable
    {
        /// <summary>
        /// Returns the SDP string for this candidate.
        /// </summary>
        public string Candidate => NativeMethods.IceCandidateGetSdp(self);
        /// <summary>
        /// Returns the media stream identification.
        /// </summary>
        public string SdpMid => NativeMethods.IceCandidateGetSdpMid(self);
        /// <summary>
        /// Returns the index of the m-line in SDP.
        /// </summary>
        public int? SdpMLineIndex => NativeMethods.IceCandidateGetSdpLineIndex(self);
        /// <summary>
        /// Returns a string which uniquely identifies the candidate.
        /// </summary>
        public string Foundation => _candidate.foundation;
        /// <summary>
        /// Returns the ICE component type.
        /// </summary>
        public RTCIceComponent? Component => _candidate.component;
        /// <summary>
        /// Returns the priority value for this candidate.
        /// </summary>
        public uint Priority => _candidate.priority;
        /// <summary>
        /// Returns the IP address for this candidate.
        /// </summary>
        public string Address => _candidate.address;
        /// <summary>
        /// Returns the transport protocol for this candidate.
        /// </summary>
        public RTCIceProtocol? Protocol => _candidate.protocol.ParseRTCIceProtocol();
        /// <summary>
        /// Returns the port number for this candidate.
        /// </summary>
        public ushort? Port => _candidate.port;
        /// <summary>
        /// Returns the candidate type.
        /// </summary>
        public RTCIceCandidateType? Type => _candidate.type.ParseRTCIceCandidateType();
        /// <summary>
        /// Returns the TCP type for this candidate, if applicable.
        /// </summary>
        public RTCIceTcpCandidateType? TcpType => _candidate.tcpType.ParseRTCIceTcpCandidateType();
        /// <summary>
        /// Returns the related IP address for this candidate.
        /// </summary>
        public string RelatedAddress => _candidate.relatedAddress;
        /// <summary>
        /// Returns the related port number for this candidate.
        /// </summary>
        public ushort? RelatedPort => _candidate.relatedPort;
        /// <summary>
        /// Returns the username fragment for this candidate.
        /// </summary>
        public string UserNameFragment => _candidate.usernameFragment;


        internal IntPtr self;
        private CandidateInternal _candidate;
        private bool disposed;

        /// <summary>
        /// Finalizer for RTCIceCandidate to release resources.
        /// </summary>
        ~RTCIceCandidate()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases resources used by the ICE candidate.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero)
            {
                NativeMethods.DeleteIceCandidate(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a new RTCIceCandidate instance from initialization data.
        /// </summary>
        /// <param name="candidateInfo">Initialization data for the candidate.</param>
        public RTCIceCandidate(RTCIceCandidateInit candidateInfo = null)
        {
            candidateInfo = candidateInfo ?? new RTCIceCandidateInit();
            if (candidateInfo.sdpMLineIndex == null && candidateInfo.sdpMid == null)
                throw new ArgumentException("sdpMid and sdpMLineIndex are both null");

            RTCIceCandidateInitInternal option = (RTCIceCandidateInitInternal)candidateInfo;
            RTCErrorType error = NativeMethods.CreateIceCandidate(ref option, out self);
            if (error != RTCErrorType.None)
                throw new ArgumentException(
                        $"create candidate is failed. error type:{error}, " +
                        $"candidate:{candidateInfo.candidate}\n" +
                        $"sdpMid:{candidateInfo.sdpMid}\n" +
                        $"sdpMLineIndex:{candidateInfo.sdpMLineIndex}\n");

            NativeMethods.IceCandidateGetCandidate(self, out _candidate);
        }
    }

    internal struct RTCIceCandidateInitInternal
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string candidate;
        [MarshalAs(UnmanagedType.LPStr)]
        public string sdpMid;
        public int sdpMLineIndex;

        public static explicit operator RTCIceCandidateInitInternal(RTCIceCandidateInit origin)
        {
            RTCIceCandidateInitInternal dst = new RTCIceCandidateInitInternal
            {
                candidate = origin.candidate ?? "",
                sdpMid = origin.sdpMid ?? "0",
                sdpMLineIndex = origin.sdpMLineIndex.GetValueOrDefault(0)
            };
            return dst;
        }
    }

    internal struct CandidateInternal
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string candidate;
        public RTCIceComponent component;
        [MarshalAs(UnmanagedType.LPStr)]
        public string foundation;
        [MarshalAs(UnmanagedType.LPStr)]
        public string ip;
        public ushort port;
        public uint priority;
        [MarshalAs(UnmanagedType.LPStr)]
        public string address;
        [MarshalAs(UnmanagedType.LPStr)]
        public string protocol;
        [MarshalAs(UnmanagedType.LPStr)]
        public string relatedAddress;
        public ushort relatedPort;
        [MarshalAs(UnmanagedType.LPStr)]
        public string tcpType;
        [MarshalAs(UnmanagedType.LPStr)]
        public string type;
        [MarshalAs(UnmanagedType.LPStr)]
        public string usernameFragment;
    }
}
