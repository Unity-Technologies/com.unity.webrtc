using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.WebRTC
{
    //public class TransformStreamDefaultController
    //{
    //    public double desiredSize;

    //    //undefined enqueue(optional any chunk);
    //    //undefined error(optional any reason);
    //    //undefined terminate();
    //};

    //public delegate void TransformerStartCallback(TransformStreamDefaultController controller);
    //public delegate void TransformerTransformCallback(TransformStreamDefaultController  controller);
    //public delegate void TransformerFlushCallback(TransformStreamDefaultController controller );

    //public class Transformer
    //{
    //    public TransformerStartCallback start;
    //    public TransformerTransformCallback transform;
    //    public TransformerFlushCallback flush;
    //    //any readableType;
    //    //any writableType;
    //};

    //public class RTCRtpScriptTransformer
    //{
    //    public readonly ReadableStream readable;
    //    public readonly WritableStream writable;
    //    // readonly public options;
    //}

    public enum RTCEncodedVideoFrameType
    {
        Empty,
        Key,
        Delta
    }

    public class RTCEncodedVideoFrameMetadata
    {
        public readonly long? frameId;
        public readonly ushort width;
        public readonly ushort height;
        public readonly int spatialIndex;
        public readonly long temporalIndex;
//        public readonly long synchronizationSource;
        public readonly long[] dependencies;
//        public readonly long[] contributingSources;

        internal RTCEncodedVideoFrameMetadata(
            RTCEncodedVideoFrameMetadataInternal data)
        {
            frameId = data.frameId;
            width = data.width;
            height = data.height;
            spatialIndex = data.spatialIndex;
            temporalIndex = data.temporalIndex;
            dependencies = data.dependencies.ToArray();
//            contributingSources = data.contributingSources.ToArray();
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCEncodedVideoFrameMetadataInternal
    {
        public OptionalLong frameId;
        public ushort width;
        public ushort height;
        public int spatialIndex;
        public int temporalIndex;
        public MarshallingArray<long> dependencies;
//        public MarshallingArray<long> contributingSources;

        public void Dispose()
        {
            dependencies.Dispose();
//            contributingSources.Dispose();
        }
    }

    public class RTCEncodedFrame
    {
        protected IntPtr self;

        /// <summary>
        /// 
        /// </summary>
        public uint Timestamp => NativeMethods.FrameGetTimestamp(self);
        /// <summary>
        /// 
        /// </summary>
        public uint Ssrc => NativeMethods.FrameGetSsrc(self);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NativeArray<byte>.ReadOnly GetData()
        {
            NativeMethods.FrameGetData(self, out var data, out var size);
            unsafe
            {
                var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                    data.ToPointer(), size, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.Create());
#endif
                return arr.AsReadOnly();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void SetData(NativeArray<byte>.ReadOnly data)
        {
            unsafe
            {
                NativeMethods.FrameSetData(self, new IntPtr(data.GetUnsafeReadOnlyPtr()), data.Length);
            }
        }

        internal RTCEncodedFrame(IntPtr ptr)
        {
            this.self = ptr;
        }
    }

    public class RTCEncodedAudioFrame : RTCEncodedFrame
    {
        public RTCEncodedAudioFrame(IntPtr ptr) : base(ptr) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCEncodedVideoFrame : RTCEncodedFrame
    {
        /// <summary>
        /// 
        /// </summary>
        public RTCEncodedVideoFrameType Type
        {
            get
            {
                if (!NativeMethods.VideoFrameIsKeyFrame(self, out var isKeyFrame))
                    return RTCEncodedVideoFrameType.Empty;
                return isKeyFrame ? RTCEncodedVideoFrameType.Key : RTCEncodedVideoFrameType.Delta;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RTCEncodedVideoFrameMetadata GetMetadata()
        {
            NativeMethods.VideoFrameGetMetadata(self, out IntPtr ptr);
            RTCEncodedVideoFrameMetadataInternal data =
                Marshal.PtrToStructure<RTCEncodedVideoFrameMetadataInternal>(ptr);
            return new RTCEncodedVideoFrameMetadata(data);
        }

        public RTCEncodedVideoFrame(IntPtr ptr) : base(ptr) {}
    };

    //public interface IReadableWritablePair
    //{
    //    public ReadableStream readable { get; }
    //    public WritableStream writable { get; }
    //}

    //public class TransformStream : IReadableWritablePair
    //{
    //    public ReadableStream readable { get; private set; }
    //    public WritableStream writable { get; private set; }

    //    private Transformer transformer_;

    //    public TransformStream(Transformer transformer = null)
    //    {
    //        transformer_ = transformer;
    //    }
    //}

    //public class MediaStreamTrackProcessor
    //{
    //    public MediaStreamTrackProcessor(MediaStreamTrack track)
    //    {

    //    }

    //    public ReadableStream readable
    //    {
    //        get
    //        {
    //            return null;
    //        }
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpTransform : RefCountedObject
    {
        /// <summary>
        /// 
        /// </summary>
        public TrackKind Kind { get; }

        internal TransformedFrameCallback callback_;

        internal RTCRtpTransform(TrackKind kind, TransformedFrameCallback callback)
            : base(WebRTC.Context.CreateFrameTransformer(OnSetTransformedFrame))
        {
            Kind = kind;
            callback_ = callback;
            WebRTC.Table.Add(self, this);
        }

        ~RTCRtpTransform()
        {
            this.Dispose();
        }

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

        [AOT.MonoPInvokeCallback(typeof(DelegateTransformedFrame))]
        static void OnSetTransformedFrame(IntPtr ptr, IntPtr frame)
        {
            // Run on worker thread, not on main thread.
            if(WebRTC.Table.TryGetValue(ptr, out RTCRtpScriptTransform transform))
            {
                RTCEncodedFrame frame_ = null;
                if (transform.Kind == TrackKind.Video)
                    frame_ = new RTCEncodedVideoFrame(frame);
                else
                    frame_ = new RTCEncodedAudioFrame(frame);
                transform.callback_(new RTCTransformEvent(frame_));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCTransformEvent
    {
        public RTCEncodedFrame Frame { get; }

        internal RTCTransformEvent(RTCEncodedFrame frame)
        {
            Frame = frame;
        }
    }

    public delegate void TransformedFrameCallback(RTCTransformEvent e);

    public class RTCRtpScriptTransform : RTCRtpTransform
    {
        public RTCRtpScriptTransform(TrackKind kind, TransformedFrameCallback callback)
            : base(kind, callback)
        {
        }
    }

    //public class SFrameTransform : RTCRtpTransform
    //{
    //    public SFrameTransform(TrackKind kind, TransformedFrameCallback callback)
    //        : base(kind, callback)
    //    {
    //    }
    //}

    //public class StreamPipeOptions {
    //    bool preventClose = false;
    //    bool preventAbort = false;
    //    bool preventCancel = false;
    //    //AbortSignal signal;
    //};

    //public class ReadableStream
    //{
    //    public ReadableStream PipeThorough(IReadableWritablePair trasform, StreamPipeOptions options = null)
    //    {
    //        return this;
    //    }
    //    public AsyncOperationBase PipeTo(WritableStream destination, StreamPipeOptions options = null)
    //    {
    //        return null;
    //    }
    //}

    //public class WritableStream
    //{

    //}


    //public class MediaStreamTrackGenerator
    //{
    //    public MediaStreamTrackGenerator(TrackKind kind)
    //    {

    //    }
    //}
}
