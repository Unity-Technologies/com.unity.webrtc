using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.WebRTC
{
    /// <summary>
    /// Types of encoded video frames.
    /// </summary>
    public enum RTCEncodedVideoFrameType
    {
        /// <summary>
        /// No frame data.
        /// </summary>
        Empty,

        /// <summary>
        /// Key frame for video decoding.
        /// </summary>
        Key,

        /// <summary>
        /// Delta frame containing changes.
        /// </summary>
        Delta
    }

    /// <summary>
    /// Metadata for an encoded video frame.
    /// </summary>
    public class RTCEncodedVideoFrameMetadata
    {
        /// <summary>
        /// Unique identifier for the frame.
        /// </summary>
        public readonly long? frameId;

        /// <summary>
        /// Frame width in pixels.
        /// </summary>
        public readonly ushort width;

        /// <summary>
        /// Frame height in pixels.
        /// </summary>
        public readonly ushort height;

        /// <summary>
        /// Simulcast stream index.
        /// </summary>
        public readonly int simulcastIndex;

        /// <summary>
        /// Temporal layer index.
        /// </summary>
        public readonly long temporalIndex;

        /// <summary>
        /// An Array of positive integers indicating the frameIds of frames on which this frame depends.
        /// </summary>
        public readonly long[] dependencies;

        internal RTCEncodedVideoFrameMetadata(
            RTCEncodedVideoFrameMetadataInternal data)
        {
            frameId = data.frameId;
            width = data.width;
            height = data.height;
            simulcastIndex = data.simulcastIndex;
            temporalIndex = data.temporalIndex;
            dependencies = data.dependencies.ToArray();
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCEncodedVideoFrameMetadataInternal
    {
        public OptionalLong frameId;
        public ushort width;
        public ushort height;
        public int simulcastIndex;
        public int temporalIndex;
        public MarshallingArray<long> dependencies;

        public void Dispose()
        {
            dependencies.Dispose();
        }
    }

    /// <summary>
    /// Represents an encoded RTP frame.
    /// </summary>
    public class RTCEncodedFrame
    {
        internal IntPtr self;

        /// <summary>
        /// Timestamp of the frame.
        /// </summary>
        public uint Timestamp => NativeMethods.FrameGetTimestamp(self);
        /// <summary>
        /// SSRC identifier for the frame.
        /// </summary>
        public uint Ssrc => NativeMethods.FrameGetSsrc(self);

        /// <summary>
        /// Gets the encoded frame data as a read-only array.
        /// </summary>
        /// <returns>Read-only byte array of frame data.</returns>
#if UNITY_ANDROID
        // todo: Optimizing for Android platform leads a crash issue.
        [MethodImpl(MethodImplOptions.NoOptimization)]
#endif
        public NativeArray<byte>.ReadOnly GetData()
        {
            NativeMethods.FrameGetData(self, out var data, out var size);

            unsafe
            {
                var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                    data.ToPointer(), size, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.Create());
#endif
                return arr.AsReadOnly();
            }
        }

        /// <summary>
        /// Sets the frame data from a read-only array.
        /// </summary>
        /// <param name="data">Read-only byte array.</param>
        public void SetData(NativeArray<byte>.ReadOnly data)
        {
            unsafe
            {
                NativeMethods.FrameSetData(self, new IntPtr(data.GetUnsafeReadOnlyPtr()), data.Length);
            }
        }

        /// <summary>
        /// Sets a portion of the frame data.
        /// </summary>
        /// <param name="data">Read-only byte array.</param>
        /// <param name="startIndex">Start index in array.</param>
        /// <param name="length">Number of bytes to set.</param>
        public void SetData(NativeArray<byte>.ReadOnly data, int startIndex, int length)
        {
            unsafe
            {
                NativeMethods.FrameSetData(self, IntPtr.Add(new IntPtr(data.GetUnsafeReadOnlyPtr()), startIndex), length);
            }
        }

        /// <summary>
        /// Sets the frame data from a native slice.
        /// </summary>
        /// <param name="data">Native slice of bytes.</param>
        public void SetData(NativeSlice<byte> data)
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
    /// Encoded audio frame for RTP transform.
    /// </summary>
    public class RTCEncodedAudioFrame : RTCEncodedFrame
    {
        internal RTCEncodedAudioFrame(IntPtr ptr) : base(ptr) { }
    }

    /// <summary>
    /// Encoded video frame for RTP transform.
    /// </summary>
    public class RTCEncodedVideoFrame : RTCEncodedFrame
    {
        /// <summary>
        /// Type of the encoded video frame.
        /// </summary>
        public RTCEncodedVideoFrameType Type
        {
            get
            {
                var isKeyFrame = NativeMethods.VideoFrameIsKeyFrame(self);
                return isKeyFrame ? RTCEncodedVideoFrameType.Key : RTCEncodedVideoFrameType.Delta;
            }
        }

        /// <summary>
        /// Gets metadata for the encoded video frame.
        /// </summary>
        /// <returns>Metadata object.</returns>
        public RTCEncodedVideoFrameMetadata GetMetadata()
        {
            IntPtr ptr = NativeMethods.VideoFrameGetMetadata(self);
            RTCEncodedVideoFrameMetadataInternal data =
                Marshal.PtrToStructure<RTCEncodedVideoFrameMetadataInternal>(ptr);
            Marshal.FreeHGlobal(ptr);
            return new RTCEncodedVideoFrameMetadata(data);
        }

        internal RTCEncodedVideoFrame(IntPtr ptr) : base(ptr) { }
    };

    /// <summary>
    /// RTP transform for encoded frames.
    /// </summary>
    public class RTCRtpTransform : RefCountedObject
    {
        /// <summary>
        /// Track kind for the transform.
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

        /// <summary>
        /// Releases resources used by the transform.
        /// </summary>
        ~RTCRtpTransform()
        {
            this.Dispose();
        }

        /// <summary>
        /// Writes an encoded frame to the transform sink.
        /// </summary>
        /// <param name="frame">Encoded frame to write.</param>
        public void Write(RTCEncodedFrame frame)
        {
            NativeMethods.FrameTransformerSendFrameToSink(self, frame.self);
        }

        /// <summary>
        /// Releases resources used by the transform.
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
    /// Event for transformed RTP frames.
    /// </summary>
    public class RTCTransformEvent
    {
        /// <summary>
        /// Transformed encoded frame.
        /// </summary>
        public RTCEncodedFrame Frame { get; }

        internal RTCTransformEvent(RTCEncodedFrame frame)
        {
            Frame = frame;
        }
    }

    /// <summary>
    /// Callback for transformed RTP frames.
    /// </summary>
    /// <param name="e">Transform event argument.</param>
    public delegate void TransformedFrameCallback(RTCTransformEvent e);

    /// <summary>
    /// Script-based RTP transform for encoded frames.
    /// </summary>
    public class RTCRtpScriptTransform : RTCRtpTransform
    {
        /// <summary>
        /// Constructor for RTCRtpScriptTransform.
        /// </summary>
        /// <param name="kind">Track kind for the transform.</param>
        /// <param name="callback">Callback to invoke for transformed frames.</param>
        public RTCRtpScriptTransform(TrackKind kind, TransformedFrameCallback callback)
            : base(kind, callback)
        {
        }
    }
}
