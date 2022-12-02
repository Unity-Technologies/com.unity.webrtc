using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Unity.WebRTC
{
    /// <summary>
    /// An executable (xref: System.Threading.SynchronizationContext) designed to wrap the current main thread
    /// (xref: System.Threading.SynchronizationContext).
    /// </summary>
    /// <remarks>
    /// Functions similarly to the UnitySynchronizationContext except it allows task execution on the main thread to be
    /// invoked manually.
    /// </remarks>
    class ExecutableUnitySynchronizationContext : SynchronizationContext
    {
        const int k_AwqInitialCapacity = 20;

        static SynchronizationContext s_MainThreadContext;

        readonly List<WorkRequest> m_AsyncWorkQueue;
        readonly List<WorkRequest> m_CurrentFrameWork = new List<WorkRequest>(k_AwqInitialCapacity);
        readonly int m_MainThreadID;
        int m_TrackedCount;

        internal ExecutableUnitySynchronizationContext(SynchronizationContext context)
        {
            if (s_MainThreadContext == null)
            {
                s_MainThreadContext = context;
            }

            if (s_MainThreadContext == null || s_MainThreadContext != context)
            {
                throw new InvalidOperationException("Unable to create executable synchronization context without a valid synchronization context.");
            }

            // Set the thread ID to the current thread sync context.  It is assumed to be the main thread sync context.
            m_MainThreadID = Thread.CurrentThread.ManagedThreadId;
            m_AsyncWorkQueue = new List<WorkRequest>();

            // Queue up and Execute work request with the synchronization context.
            s_MainThreadContext.Post(SendOrPostCallback, new CallbackObject(ExecuteAndAppendNextExecute));
        }

        ExecutableUnitySynchronizationContext(List<WorkRequest> queue, int mainThreadID)
        {
            m_AsyncWorkQueue = queue;
            m_MainThreadID = mainThreadID;
        }

        static void SendOrPostCallback(object state)
        {
            var obj = state as CallbackObject;
            obj?.callback();
        }

        public override void Send(SendOrPostCallback callback, object state)
        {
            // Send will process the call synchronously. If the call is processed on the main thread, we'll invoke it
            // directly here. If the call is processed on another thread it will be queued up like POST to be executed
            // on the main thread and it will wait. Once the main thread processes the work we can continue
            if (m_MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                callback(state);
            }
            else
            {
                using (var waitHandle = new ManualResetEvent(false))
                {
                    lock (m_AsyncWorkQueue)
                    {
                        m_AsyncWorkQueue.Add(new WorkRequest(callback, state, waitHandle));
                    }
                    waitHandle.WaitOne();
                }
            }
        }

        public override void OperationStarted() { Interlocked.Increment(ref m_TrackedCount); }

        public override void OperationCompleted() { Interlocked.Decrement(ref m_TrackedCount); }

        // Post will add the call to a task list to be executed later on the main thread then work will continue asynchronously
        public override void Post(SendOrPostCallback callback, object state)
        {
            lock (m_AsyncWorkQueue)
            {
                m_AsyncWorkQueue.Add(new WorkRequest(callback, state));
            }
        }

        // CreateCopy returns a new ExecutableUnitySynchronizationContext object, but the queue is still shared with the original
        public override SynchronizationContext CreateCopy()
        {
            lock (m_AsyncWorkQueue)
            {
                return new ExecutableUnitySynchronizationContext(m_AsyncWorkQueue, m_MainThreadID);
            }
        }

        /// <summary>
        /// Executes the current set of pending tasks with a timeout that prevents the execution from continuously adding
        /// and executing work. If the current set finishes and there is time left then take the next batch.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The timeout in milliseconds that the execute can take.
        /// </param>
        /// <returns>
        /// <c>true</c> if all tasks have been executed and completed and <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        /// The timeout is there to guard against tasks that spawn other tasks and threads adding jobs during processing.
        /// It will not cease execution on the current set of tasks, just prevent execution of another set of tasks.
        /// </remarks>
        internal bool ExecutePendingTasks(long millisecondsTimeout)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (HasPendingTasks())
            {
                // To prevent work from posting more work indefinitely, stop processing after timeout.
                // Does not prevent a given work queue from going over the time allotment
                if (stopwatch.ElapsedMilliseconds > millisecondsTimeout)
                {
                    break;
                }

                Execute();
                Thread.Sleep(1);
            }
            stopwatch.Stop();

            return !HasPendingTasks();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasPendingTasks()
        {
            lock (m_AsyncWorkQueue)
            {
                return m_AsyncWorkQueue.Count != 0 || m_TrackedCount != 0;
            }
        }

        /// <summary>
        /// Executes the current set of pending tasks.
        /// </summary>
        /// <remarks>
        /// This will take the complete set of pending tasks in the work queue.
        /// </remarks>
        void Execute()
        {
            // Enforce all job execution completion on the main thread.
            if (m_MainThreadID == Thread.CurrentThread.ManagedThreadId)
            {
                // The following is the same behavior as UnitySynchronizationContext
                lock (m_AsyncWorkQueue)
                {
                    m_CurrentFrameWork.AddRange(m_AsyncWorkQueue);
                    m_AsyncWorkQueue.Clear();
                }

                while (m_CurrentFrameWork.Count > 0)
                {
                    var work = m_CurrentFrameWork[0];
                    m_CurrentFrameWork.RemoveAt(0);
                    work.Invoke();
                }
            }
        }

        /// <summary>
        /// Executes the current set of pending tasks and then (xref: System.Threading.SynchronizationContext.Post)
        /// another <see cref='CallbackObject'> with another ExecuteAndAppendNextExecute action to the (xref: UnityEngine.UnitySyncrhonizationContext)
        /// </summary>
        /// <remarks>
        /// This method is intended to only be used to hook into the (xref: UnityEngine.UnitySyncrhonizationContext) pending task processing callback.
        /// </remarks>
        void ExecuteAndAppendNextExecute()
        {
            // Stop infinity loop when application is stopped.
            if (!UnityEngine.Application.isPlaying)
                return;

            Execute();

            // UnitySynchronizationContext works by performing work in batches so as not to stall the main thread
            // forever. Therefore it is safe to re-add ourselves to the delayed work queue. This is how we hook into
            // the main thread delayed tasks.
            s_MainThreadContext.Post(SendOrPostCallback, new CallbackObject(ExecuteAndAppendNextExecute));
        }

        class CallbackObject
        {
            public readonly Action callback;

            public CallbackObject(Action callback)
            {
                this.callback = callback;
            }
        }

        readonly struct WorkRequest
        {
            readonly SendOrPostCallback m_DelegateCallback;
            readonly object m_DelegateState;
            readonly ManualResetEvent m_WaitHandle;

            public WorkRequest(SendOrPostCallback callback, object state, ManualResetEvent waitHandle = null)
            {
                m_DelegateCallback = callback;
                m_DelegateState = state;
                m_WaitHandle = waitHandle;
            }

            public void Invoke()
            {
                try
                {
                    m_DelegateCallback(m_DelegateState);
                }
                finally
                {
                    m_WaitHandle?.Set();
                }
            }
        }
    }
}
