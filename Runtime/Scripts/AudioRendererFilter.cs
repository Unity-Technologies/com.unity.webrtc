using System;
using UnityEngine;
using Unity.Collections;

namespace Unity.WebRTC
{
    static class NativeArrayExtension
    {
        public static int CircleBufferPushBack<T>(
            this NativeArray<T> dst, int dstIndex, T[] src) where T : struct
        {
            if (src.Length == 0)
                return dstIndex;

            if (src.Length > dst.Length)
                throw new ArgumentException("dst buffer should be greater than src buffer.");

            int length = Math.Min(src.Length, dst.Length - dstIndex);
            NativeArray<T>.Copy(src, 0, dst, dstIndex, length);

            if (src.Length <= length)
                return (dst.Length - dstIndex) == length ? 0 : dstIndex + length;

            NativeArray<T>.Copy(src, length, dst, 0, src.Length - length);
            return length - 1;
        }

        public static int CircleBufferCopyTo<T>(
            this NativeArray<T> src, int srcIndex, T[] dst) where T : struct
        {
            if (dst.Length == 0)
                return srcIndex;

            if (src.Length < dst.Length)
                throw new ArgumentException("src buffer should be greater than dst buffer.");

            int length = Math.Min(src.Length - srcIndex, dst.Length);
            NativeArray<T>.Copy(src, srcIndex, dst, 0, length);

            if (dst.Length <= length)
                return (src.Length - srcIndex) == length ? 0 : srcIndex + length;

            NativeArray<T>.Copy(src, 0, dst, length, dst.Length - length);
            return (dst.Length - length);
        }

    }

    /// <summary>
    ///
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    internal class AudioRendererFilter : MonoBehaviour
    {
        private NativeArray<float> queue;
        private int pushIndex;
        private int head;
        private object mutex_ = new object();

        long totalBufferSize = 0;
        long totalRenderSize = 0;

        private void OnDestroy()
        {
            queue.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <param name="channel"></param>
        public void AllocateBuffer(int sampleRate, int channels)
        {
            lock (mutex_)
            {
                if (queue.IsCreated)
                    queue.Dispose();
                queue = new NativeArray<float>(sampleRate * channels, Allocator.Persistent);
                pushIndex = 0;
                head = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void SetData(float[] data)
        {
            lock(mutex_)
            {
                pushIndex = queue.CircleBufferPushBack(pushIndex, data);
                totalBufferSize += (long)data.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <note>
        /// Call on the audio thread, not main thread.
        /// </note>
        /// <param name="data"></param>
        /// <param name="channels"></param>
        void OnAudioFilterRead(float[] data, int channels)
        {
            lock (mutex_)
            {
                // skip reading buffer if head position
                long next = totalRenderSize + data.Length;
                if (totalBufferSize < next) {
                    return;
                }
                // move head to close buffer position
                int limit = (int)(queue.Length / 2.0);
                if (totalBufferSize - next > limit) {
                    head = (head + limit) % queue.Length;
                    next = totalRenderSize + limit;
                }
                head = queue.CircleBufferCopyTo(head, data);
                totalRenderSize = next;
            }
        }
    }
}
