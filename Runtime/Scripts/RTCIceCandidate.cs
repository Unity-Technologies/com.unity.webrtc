using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public class RTCIceCandidateInit
    {
        /// <summary>
        /// 
        /// </summary>
        public string candidate;
        /// <summary>
        /// 
        /// </summary>
        public string sdpMid;
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        Rtp = 1,
        /// <summary>
        /// 
        /// </summary>
        Rtcp = 2,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RTCIceProtocol : int
    {
        /// <summary>
        /// 
        /// </summary>
        Udp = 1,
        /// <summary>
        /// 
        /// </summary>
        Tcp = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RTCIceCandidateType
    {
        /// <summary>
        /// 
        /// </summary>
        Host,
        /// <summary>
        /// 
        /// </summary>
        Srflx,
        /// <summary>
        /// 
        /// </summary>
        Prflx,
        /// <summary>
        /// 
        /// </summary>
        Relay
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RTCIceTcpCandidateType
    {
        /// <summary>
        /// 
        /// </summary>
        Active,
        /// <summary>
        /// 
        /// </summary>
        Passive,
        /// <summary>
        /// 
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
    /// 
    /// </summary>
    public class RTCIceCandidate : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public string Candidate => NativeMethods.IceCandidateGetSdp(self);
        /// <summary>
        /// 
        /// </summary>
        public string SdpMid => NativeMethods.IceCandidateGetSdpMid(self);
        /// <summary>
        /// 
        /// </summary>
        public int? SdpMLineIndex => NativeMethods.IceCandidateGetSdpLineIndex(self);
        /// <summary>
        /// 
        /// </summary>
        public string Foundation => _candidate.foundation;
        /// <summary>
        /// 
        /// </summary>
        public RTCIceComponent? Component => _candidate.component;
        /// <summary>
        /// 
        /// </summary>
        public uint Priority => _candidate.priority;
        /// <summary>
        /// 
        /// </summary>
        public string Address => _candidate.address;
        /// <summary>
        /// 
        /// </summary>
        public RTCIceProtocol? Protocol => _candidate.protocol.ParseRTCIceProtocol();
        /// <summary>
        /// 
        /// </summary>
        public ushort? Port => _candidate.port;
        /// <summary>
        /// 
        /// </summary>
        public RTCIceCandidateType? Type => _candidate.type.ParseRTCIceCandidateType();
        /// <summary>
        /// 
        /// </summary>
        public RTCIceTcpCandidateType? TcpType => _candidate.tcpType.ParseRTCIceTcpCandidateType();
        /// <summary>
        /// 
        /// </summary>
        public string RelatedAddress => _candidate.relatedAddress;
        /// <summary>
        /// 
        /// </summary>
        public ushort? RelatedPort => _candidate.relatedPort;
        /// <summary>
        /// 
        /// </summary>
        public string UserNameFragment => _candidate.usernameFragment;


        internal IntPtr self;
        private CandidateInternal _candidate;
        private bool disposed;

        /// <summary>
        /// 
        /// </summary>
        ~RTCIceCandidate()
        {
            this.Dispose();
        }

        /// <summary>
        ///
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
        /// 
        /// </summary>
        /// <param name="candidateInfo"></param>
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
