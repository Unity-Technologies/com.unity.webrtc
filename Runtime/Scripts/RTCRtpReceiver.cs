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
        /// 
        /// </summary>
        public float? audioLevel { get; private set; }

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


        internal RTCRtpContributingSource(ref RTCRtpContributingSourceInternal data)
        {
            audioLevel = data.audioLevel;
            rtpTimestamp = data.rtpTimestamp;
            source = data.source;
            timestamp = data.timestamp;
        }
    }

    internal struct RTCRtpContributingSourceInternal
    {
        public OptionalByte audioLevel;
        public uint rtpTimestamp;
        public uint source;
        public long timestamp;
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCRtpReceiver : RefCountedObject
    {
        private RTCPeerConnection peer;

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
                if (WebRTC.Table.TryGetValue(self, out object value) && value == this)
                {
                    WebRTC.Table.Remove(self);
                }
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
                sources[i] = new RTCRtpContributingSource(ref array[i]);
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
