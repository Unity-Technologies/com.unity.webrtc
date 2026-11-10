using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.WebRTC.RuntimeTest
{
    class ExecutableUnitySynchronizationContextTest
    {
        static ExecutableUnitySynchronizationContext GetContext()
        {
            // Use the running pump rather than registering another recurring pump.
            var field = typeof(WebRTC).GetField("s_syncContext", BindingFlags.Static | BindingFlags.NonPublic);
            var context = field.GetValue(null) as ExecutableUnitySynchronizationContext;
            Assert.That(context, Is.Not.Null);
            return context;
        }

        [UnityTest]
        public IEnumerator CallbacksContinueAfterException()
        {
            var context = GetContext();
            bool sameBatchRan = false;
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: callback failure regression"));
            context.Post(_ => throw new InvalidOperationException("callback failure regression"), null);
            context.Post(_ => sameBatchRan = true, null);

            yield return new WaitUntilWithTimeout(() => sameBatchRan, 2000);
            Assert.That(sameBatchRan, Is.True, "A throwing callback stopped the current batch.");

            // The automatic pump must also have scheduled itself for later frames.
            bool laterBatchRan = false;
            context.Post(_ => laterBatchRan = true, null);
            yield return new WaitUntilWithTimeout(() => laterBatchRan, 2000);
            Assert.That(laterBatchRan, Is.True, "A throwing callback stopped subsequent WebRTC events.");
        }

        [UnityTest]
        public IEnumerator WaitingSenderIsReleasedAfterCallbackException()
        {
            var context = GetContext();
            int completed = 0;
            Exception workerException = null;
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: waiting sender regression"));
            var worker = new Thread(() =>
            {
                try
                {
                    context.Send(_ => throw new InvalidOperationException("waiting sender regression"), null);
                }
                catch (Exception exception)
                {
                    workerException = exception;
                }
                finally
                {
                    Volatile.Write(ref completed, 1);
                }
            }) { IsBackground = true };

            worker.Start();
            yield return new WaitUntilWithTimeout(() => Volatile.Read(ref completed) != 0, 2000);
            // Drain a late enqueue before checking the worker, including on failure.
            context.ExecutePendingTasks(100);
            Assert.That(worker.Join(2000), Is.True, "The throwing callback did not signal its waiting sender.");
            Assert.That(workerException, Is.Null);
        }
    }
}
