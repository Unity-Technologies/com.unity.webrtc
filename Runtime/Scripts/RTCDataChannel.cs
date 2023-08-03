using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.CreateDataChannel(string, RTCDataChannelInit)"/>
    public class RTCDataChannelInit
    {
        /// <summary>
        ///
        /// </summary>
        public bool? ordered;
        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// Cannot be set along with <see cref="RTCDataChannelInit.maxRetransmits"/>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelInit.maxRetransmits"/>
        public int? maxPacketLifeTime;
        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// Cannot be set along with <see cref="RTCDataChannelInit.maxPacketLifeTime"/>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelInit.maxPacketLifeTime"/>
        public int? maxRetransmits;
        /// <summary>
        ///
        /// </summary>
        public string protocol;
        /// <summary>
        ///
        /// </summary>
        public bool? negotiated;
        /// <summary>
        ///
        /// </summary>
        public int? id;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCDataChannelInitInternal
    {
        public OptionalBool ordered;
        public OptionalInt maxRetransmitTime;
        public OptionalInt maxRetransmits;
        [MarshalAs(UnmanagedType.LPStr)]
        public string protocol;
        public OptionalBool negotiated;
        public OptionalInt id;

        public static explicit operator RTCDataChannelInitInternal(RTCDataChannelInit origin)
        {
            RTCDataChannelInitInternal dst = new RTCDataChannelInitInternal
            {
                ordered = origin.ordered,
                maxRetransmitTime = origin.maxPacketLifeTime,
                maxRetransmits = origin.maxRetransmits,
                protocol = origin.protocol,
                negotiated = origin.negotiated,
                id = origin.id
            };
            return dst;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public delegate void DelegateOnOpen();
    /// <summary>
    /// 
    /// </summary>
    public delegate void DelegateOnClose();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bytes"></param>
    public delegate void DelegateOnMessage(byte[] bytes);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="channel"></param>
    public delegate void DelegateOnDataChannel(RTCDataChannel channel);

    /// <summary>
    /// 
    /// </summary>
    public delegate void DelegateOnError(RTCError error);

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="RTCPeerConnection.CreateDataChannel(string, RTCDataChannelInit)"/>
    public class RTCDataChannel : RefCountedObject
    {
        private DelegateOnMessage onMessage;
        private DelegateOnOpen onOpen;
        private DelegateOnClose onClose;
        private DelegateOnError onError;

        /// <summary>
        ///
        /// </summary>
        public DelegateOnMessage OnMessage
        {
            get => onMessage;
            set => onMessage = value;
        }

        /// <summary>
        ///
        /// </summary>
        public DelegateOnOpen OnOpen
        {
            get => onOpen;
            set => onOpen = value;
        }

        /// <summary>
        ///
        /// </summary>
        public DelegateOnClose OnClose
        {
            get => onClose;
            set => onClose = value;
        }

        /// <summary>
        ///
        /// </summary>
        public DelegateOnError OnError
        {
            get => onError;
            set => onError = value;
        }

        /// <summary>
        ///
        /// </summary>
        public int Id => NativeMethods.DataChannelGetID(GetSelfOrThrow());

        /// <summary>
        ///
        /// </summary>
        public string Label => NativeMethods.DataChannelGetLabel(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        /// <summary>
        ///
        /// </summary>
        public string Protocol => NativeMethods.DataChannelGetProtocol(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        /// <summary>
        ///
        /// </summary>
        public ushort MaxRetransmits => NativeMethods.DataChannelGetMaxRetransmits(GetSelfOrThrow());

        /// <summary>
        ///
        /// </summary>
        public ushort MaxRetransmitTime => NativeMethods.DataChannelGetMaxRetransmitTime(GetSelfOrThrow());

        /// <summary>
        ///
        /// </summary>
        public bool Ordered => NativeMethods.DataChannelGetOrdered(GetSelfOrThrow());

        /// <summary>
        ///
        /// </summary>
        public ulong BufferedAmount => NativeMethods.DataChannelGetBufferedAmount(GetSelfOrThrow());

        /// <summary>
        ///
        /// </summary>
        public bool Negotiated => NativeMethods.DataChannelGetNegotiated(GetSelfOrThrow());

        /// <summary>
        /// The property returns an enum of the <c>RTCDataChannelState</c> which shows
        /// the state of the channel.
        /// </summary>
        /// <remarks>
        /// <see cref="Send(string)"/> method must be called when the state is <b>Open</b>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelState"/>
        public RTCDataChannelState ReadyState => NativeMethods.DataChannelGetReadyState(GetSelfOrThrow());

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnMessage))]
        static void DataChannelNativeOnMessage(IntPtr ptr, byte[] msg, int size)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCDataChannel channel)
                {
                    channel.onMessage?.Invoke(msg);
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnOpen))]
        static void DataChannelNativeOnOpen(IntPtr ptr)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCDataChannel channel)
                {
                    channel.onOpen?.Invoke();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnClose))]
        static void DataChannelNativeOnClose(IntPtr ptr)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCDataChannel channel)
                {
                    channel.onClose?.Invoke();
                }
            });
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnError))]
        static void DataChannelNativeOnError(IntPtr ptr, RTCErrorType errorType, byte[] message, int size)
        {
            WebRTC.Sync(ptr, () =>
            {
                if (WebRTC.Table[ptr] is RTCDataChannel channel)
                {
                    channel.onError?.Invoke(new RTCError() { errorType = errorType, message = System.Text.Encoding.UTF8.GetString(message) });
                }
            });
        }


        internal RTCDataChannel(IntPtr ptr, RTCPeerConnection peerConnection)
            : base(ptr)
        {
            WebRTC.Table.Add(self, this);
            WebRTC.Context.DataChannelRegisterOnMessage(self, DataChannelNativeOnMessage);
            WebRTC.Context.DataChannelRegisterOnOpen(self, DataChannelNativeOnOpen);
            WebRTC.Context.DataChannelRegisterOnClose(self, DataChannelNativeOnClose);
            WebRTC.Context.DataChannelRegisterOnError(self, DataChannelNativeOnError);
        }

        /// <summary>
        /// 
        /// </summary>
        ~RTCDataChannel()
        {
            this.Dispose();
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
                Close();
                WebRTC.Context.DeleteDataChannel(self);
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }

        /// <summary>
        /// The method Sends data across the data channel to the remote peer.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>
        /// <param name="msg"></param>
        /// <seealso cref="ReadyState"/>
        public void Send(string msg)
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSend(GetSelfOrThrow(), msg);
        }

        /// <summary>
        /// The method Sends data across the data channel to the remote peer.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The method throws <c>InvalidOperationException</c> when <see cref="ReadyState"/>
        ///  is not <b>Open</b>.
        /// </exception>
        /// <param name="msg"></param>
        /// <seealso cref="ReadyState"/>
        public void Send(byte[] msg)
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSendBinary(GetSelfOrThrow(), msg, msg.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        public unsafe void Send<T>(NativeArray<T> msg)
            where T : struct
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            if (!msg.IsCreated)
            {
                throw new ArgumentException("Message array has not been created.", nameof(msg));
            }
            NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), new IntPtr(msg.GetUnsafeReadOnlyPtr()), msg.Length * UnsafeUtility.SizeOf<T>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        public unsafe void Send<T>(NativeSlice<T> msg)
            where T : struct
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), new IntPtr(msg.GetUnsafeReadOnlyPtr()), msg.Length * UnsafeUtility.SizeOf<T>());
        }

#if UNITY_2020_1_OR_NEWER // ReadOnly support was introduced in 2020.1

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        public unsafe void Send<T>(NativeArray<T>.ReadOnly msg)
            where T : struct
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), new IntPtr(msg.GetUnsafeReadOnlyPtr()), msg.Length * UnsafeUtility.SizeOf<T>());
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgPtr"></param>
        /// <param name="length"></param>
        public unsafe void Send(void* msgPtr, int length)
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), new IntPtr(msgPtr), length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msgPtr"></param>
        /// <param name="length"></param>
        public void Send(IntPtr msgPtr, int length)
        {
            if (ReadyState != RTCDataChannelState.Open)
            {
                throw new InvalidOperationException("DataChannel is not open");
            }
            if (msgPtr != IntPtr.Zero && length > 0)
            {
                NativeMethods.DataChannelSendPtr(GetSelfOrThrow(), msgPtr, length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            NativeMethods.DataChannelClose(GetSelfOrThrow());
        }
    }
}
