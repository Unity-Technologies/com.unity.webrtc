using UnityEngine;

namespace Unity.WebRTC
{
    public class AsyncOperationBase : CustomYieldInstruction
    {
        public RTCError Error { get; internal set; }

        public bool IsError { get; internal set; }
        public bool IsDone { get; internal set; }

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

    public class RTCStatsReportAsyncOperation : AsyncOperationBase
    {
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

    public class RTCSessionDescriptionAsyncOperation : AsyncOperationBase
    {
        public RTCSessionDescription Desc { get; internal set; }
    }

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
