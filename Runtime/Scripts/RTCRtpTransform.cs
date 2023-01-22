using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public enum RTCEncodedVideoFrameType
    {
        /// <summary>
        /// 
        /// </summary>
        Empty,

        /// <summary>
        /// 
        /// </summary>
        Key,

        /// <summary>
        /// 
        /// </summary>
        Delta
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCEncodedVideoFrameMetadata
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly long? frameId;

        /// <summary>
        /// 
        /// </summary>
        public readonly ushort width;

        /// <summary>
        /// 
        /// </summary>
        public readonly ushort height;

        /// <summary>
        /// 
        /// </summary>
        public readonly int spatialIndex;

        /// <summary>
        /// 
        /// </summary>
        public readonly long temporalIndex;

//        public readonly long synchronizationSource;

        /// <summary>
        /// 
        /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    public class RTCEncodedFrame
    {
        internal IntPtr self;

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

    /// <summary>
    /// 
    /// </summary>
    public class RTCEncodedAudioFrame : RTCEncodedFrame
    {
        internal RTCEncodedAudioFrame(IntPtr ptr) : base(ptr) { }
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

        internal RTCEncodedVideoFrame(IntPtr ptr) : base(ptr) {}
    };

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
            : base(WebRTC.Context.CreateFrameTransformer())
        {
            Kind = kind;
            callback_ = callback;
            WebRTC.Table.Add(self, this);
        }

        ~RTCRtpTransform()
        {
            this.Dispose();
        }

        public void Write(RTCEncodedFrame frame)
        {
            NativeMethods.FrameTransformerSendFrameToSink(self, frame.self);
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
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCTransformEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public RTCEncodedFrame Frame { get; }

        internal RTCTransformEvent(RTCEncodedFrame frame)
        {
            Frame = frame;
        }
    }

    public delegate void TransformedFrameCallback(RTCTransformEvent e);

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpScriptTransform : RTCRtpTransform
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="callback"></param>
        public RTCRtpScriptTransform(TrackKind kind, TransformedFrameCallback callback)
            : base(kind, callback)
        {
        }
    }
}
