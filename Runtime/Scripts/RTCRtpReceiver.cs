using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    /// <summary>
    /// Represents a contributing source for RTP streams.
    /// </summary>
    public class RTCRtpContributingSource
    {
        /// <summary>
        /// The audio level of the source, ranging from 0.0 to 1.0.
        /// </summary>
        public double? audioLevel { get; private set; }

        /// <summary>
        /// The RTP timestamp of the source.
        /// </summary>
        public long? rtpTimestamp { get; private set; }

        /// <summary>
        /// The SSRC of the source.
        /// </summary>
        public uint? source { get; private set; }

        /// <summary>
        /// The timestamp of the source.
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
    /// Represents a receiver for RTP streams.
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
        /// Finalizer for RTCRtpReceiver.
        /// </summary>
        ~RTCRtpReceiver()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases resources used by the object.
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
        /// Gets the capabilities of the RTP receiver.
        /// </summary>
        /// <param name="kind">The type of media track (audio or video).</param>
        /// <returns>Capabilities supported by the receiver.</returns>
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
        /// Gets the statistics report for the receiver.
        /// </summary>
        /// <returns>Returns an asynchronous operation for retrieving receiver statistics.</returns>
        public RTCStatsReportAsyncOperation GetStats()
        {
            return peer.GetStats(this);
        }

        /// <summary>
        /// Gets the contributing sources for the receiver.
        /// </summary>
        /// <returns>Returns an array of contributing sources.</returns>
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
        /// Gets the synchronization sources for the receiver.
        /// </summary>
        /// <returns>Returns an array of synchronization sources.</returns>
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

        /// <summary>
        /// Gets the media stream track associated with the receiver.
        /// </summary>
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
        /// Gets or sets the RTP transform for the receiver.
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
        /// Gets the media streams associated with the receiver.
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
