using System;
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

        internal RTCStatsReportAsyncOperation(RTCStatsCollectorCallback callback)
        {
            callback.onStatsDelivered = OnStatsDelivered;
        }

        void OnStatsDelivered(RTCStatsReport report)
        {
            Value = report;
            IsError = false;
            this.Done();
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

        internal RTCSessionDescriptionAsyncOperation(CreateSessionDescriptionObserver observer)
        {
            observer.onCreateSessionDescription = OnCreateSessionDescription;
        }

        void OnCreateSessionDescription(RTCSdpType type, string sdp, RTCErrorType errorType, string error)
        {
            IsError = errorType != RTCErrorType.None;
            Error = new RTCError() { errorType = errorType, message = error };
            Desc = new RTCSessionDescription() { type = type, sdp = sdp };
            this.Done();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCSetSessionDescriptionAsyncOperation : AsyncOperationBase
    {
        internal RTCSetSessionDescriptionAsyncOperation(SetSessionDescriptionObserver observer)
        {
            observer.onSetSessionDescription = OnSetSessionDescription;
        }

        void OnSetSessionDescription(RTCErrorType errorType, string error)
        {
            IsError = errorType != RTCErrorType.None;
            Error = new RTCError() { errorType = errorType, message = error };
            this.Done();
        }
    }
}
