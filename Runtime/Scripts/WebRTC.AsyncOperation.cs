using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public class AsyncOperationBase : CustomYieldInstruction
    {
        /// <summary>
        /// 
        /// </summary>
        public RTCError Error { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsError { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDone { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public override bool keepWaiting
        {
            get
            {
                if (IsDone)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        internal void Done()
        {
            IsDone = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCStatsReportAsyncOperation : AsyncOperationBase
    {
        /// <summary>
        /// 
        /// </summary>
        public RTCStatsReport Value { get; private set; }

        internal RTCStatsReportAsyncOperation(RTCPeerConnection connection)
        {
            NativeMethods.PeerConnectionGetStats(connection.GetSelfOrThrow());

            connection.OnStatsDelivered = ptr =>
            {
                Value = WebRTC.FindOrCreate(ptr, ptr_ => new RTCStatsReport(ptr_));
                IsError = false;
                this.Done();
            };
        }

        internal RTCStatsReportAsyncOperation(RTCPeerConnection connection, RTCRtpSender sender)
        {
            NativeMethods.PeerConnectionSenderGetStats(connection.GetSelfOrThrow(), sender.self);

            connection.OnStatsDelivered = ptr =>
            {
                Value = WebRTC.FindOrCreate(ptr, ptr_ => new RTCStatsReport(ptr_));
                IsError = false;
                this.Done();
            };
        }
        internal RTCStatsReportAsyncOperation(RTCPeerConnection connection, RTCRtpReceiver receiver)
        {
            NativeMethods.PeerConnectionReceiverGetStats(connection.GetSelfOrThrow(), receiver.self);

            connection.OnStatsDelivered = ptr =>
            {
                Value = WebRTC.FindOrCreate(ptr, ptr_ => new RTCStatsReport(ptr_));
                IsError = false;
                this.Done();
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCSessionDescriptionAsyncOperation : AsyncOperationBase
    {
        /// <summary>
        /// 
        /// </summary>
        public RTCSessionDescription Desc { get; internal set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCSetSessionDescriptionAsyncOperation : AsyncOperationBase
    {
        internal RTCSetSessionDescriptionAsyncOperation(RTCPeerConnection connection)
        {
            connection.OnSetSessionDescriptionSuccess = () =>
            {
                IsError = false;
                this.Done();
            };
            connection.OnSetSessionDescriptionFailure = (error) =>
            {
                IsError = true;
                Error = error;
                this.Done();
            };
        }
    }
}
