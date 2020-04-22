using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.WebRTC
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RTCRtpSendParameters
    {
        internal int codecsLength;
        internal IntPtr codecs;
        internal int encodingsLength;
        internal IntPtr encodings;
        internal IntPtr transactionId;

        public RTCRtpEncodingParameters[] Codecs
        {
            get
            {
                return null;
            }
        }

        public RTCRtpCodecParameters[] Encodings
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public string TransactionId
        {
            get
            {
                //Debug.Log(transactionId == IntPtr.Zero);
                //return string.Empty;
                return Marshal.PtrToStringAnsi(transactionId);
            }
        }
    }

    public struct RTCRtpEncodingParameters
    {
        public bool active;
        public ulong maxBitrate;
        public IntPtr rid;
    }

    public struct RTCRtpCodecParameters
    {
    }
}
