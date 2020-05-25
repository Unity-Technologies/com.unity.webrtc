using UnityEngine;

namespace Unity.WebRTC
{

    /// <inheritdoc />
    /// <summary>
    /// </summary>
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
