using UnityEngine;

namespace Unity.WebRTC.RuntimeTest
{
    internal class WaitUntilWithTimeout : CustomYieldInstruction
    {
        public bool IsCompleted { get; private set; }

        private readonly float timeoutTime;

        private readonly System.Func<bool> predicate;

        public override bool keepWaiting
        {
            get
            {
                IsCompleted = predicate();
                if (IsCompleted)
                {
                    return false;
                }

                return !(Time.realtimeSinceStartup >= timeoutTime);
            }
        }

        public WaitUntilWithTimeout(System.Func<bool> predicate, int timeout)
        {
            this.timeoutTime = Time.realtimeSinceStartup + timeout * 0.001f;
            this.predicate = predicate;
        }
    }
}
