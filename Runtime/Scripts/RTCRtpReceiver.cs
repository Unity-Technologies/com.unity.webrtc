using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    public class RTCRtpContributingSource
    {
        /// <summary>
        /// This value is in the range 0.0 to 1.0
        /// </summary>
        public double? audioLevel { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public long? rtpTimestamp { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public uint? source { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public long? timestamp { get; private set; }


        internal RTCRtpContributingSource(ref RTCRtpContributingSourceInternal data, RtpSourceType sourceType)
        {
            audioLevel = data.audioLevel.hasValue ? data.audioLevel.value / byte.MaxValue : (double?)null;
            rtpTimestamp = data.rtpTimestamp;
            source = data.sourceType == sourceType ? data.source : (uint?)null;
            timestamp = data.timestamp;
        }
    }

    internal enum RtpSourceType : byte
    {
        SSRC,
        CSRC
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpContributingSourceInternal
    {
        public OptionalByte audioLevel;
        public RtpSourceType sourceType;
        public uint source;
        public uint rtpTimestamp;
        public long timestamp;
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCRtpReceiver : RefCountedObject
    {
        private RTCPeerConnection peer;
        private RTCRtpTransform transform;

        internal RTCRtpReceiver(IntPtr ptr, RTCPeerConnection peer) : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            this.peer = peer;
        }

        /// <summary>
        ///
        /// </summary>
        ~RTCRtpReceiver()
        {
            this.Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static RTCRtpCapabilities GetCapabilities(TrackKind kind)
        {
            WebRTC.Context.GetReceiverCapabilities(kind, out IntPtr ptr);
            RTCRtpCapabilitiesInternal capabilitiesInternal =
                Marshal.PtrToStructure<RTCRtpCapabilitiesInternal>(ptr);
            RTCRtpCapabilities capabilities = new RTCRtpCapabilities(capabilitiesInternal);
            Marshal.FreeHGlobal(ptr);
            return capabilities;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public RTCStatsReportAsyncOperation GetStats()
        {
            return peer.GetStats(this);
        }

        /// <summary>
        ///
        /// </summary>
        public RTCRtpContributingSource[] GetContributingSources()
        {
            RTCRtpContributingSourceInternal[] array = NativeMethods.ReceiverGetSources(self, out var length).AsArray<RTCRtpContributingSourceInternal>((int)length);

            RTCRtpContributingSource[] sources = new RTCRtpContributingSource[length];
            for (int i = 0; i < (int)length; i++)
            {
                sources[i] = new RTCRtpContributingSource(ref array[i], RtpSourceType.CSRC);
            }
            return sources;
        }

        /// <summary>
        ///
        /// </summary>
        public RTCRtpContributingSource[] GetSynchronizationSources()
        {
            RTCRtpContributingSourceInternal[] array = NativeMethods.ReceiverGetSources(self, out var length).AsArray<RTCRtpContributingSourceInternal>((int)length);

            RTCRtpContributingSource[] sources = new RTCRtpContributingSource[length];
            for (int i = 0; i < (int)length; i++)
            {
                sources[i] = new RTCRtpContributingSource(ref array[i], RtpSourceType.SSRC);
            }
            return sources;
        }

        public MediaStreamTrack Track
        {
            get
            {
                IntPtr ptr = NativeMethods.ReceiverGetTrack(GetSelfOrThrow());
                if (ptr == IntPtr.Zero)
                    return null;
                return WebRTC.FindOrCreate(ptr, MediaStreamTrack.Create);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public RTCRtpTransform Transform
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // cache reference
                transform = value;
                NativeMethods.ReceiverSetTransform(GetSelfOrThrow(), value.self);
            }
            get
            {
                return transform;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public IEnumerable<MediaStream> Streams
        {
            get
            {
                IntPtr ptrStreams = NativeMethods.ReceiverGetStreams(GetSelfOrThrow(), out ulong length);
                return WebRTC.Deserialize(ptrStreams, (int)length, ptr => new MediaStream(ptr));
            }
        }
    }
}
