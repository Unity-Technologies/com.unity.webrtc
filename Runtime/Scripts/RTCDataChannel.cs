using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.WebRTC
{
    /// <summary>
    /// Represents argument parameters for RTCPeerConnection.CreateDataChannel()
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
    /// Represents a network channel which can be used for bidirectional peer-to-peer transfers of arbitrary data.
    /// </summary>
    /// <remarks>
    /// RTCDataChannel interface represents a network channel which can be used for bidirectional peer-to-peer transfers of arbitrary data.
    /// Every data channel is associated with an RTCPeerConnection, and each peer connection can have up to a theoretical maximum of 65,534 data channels.
    ///
    /// To create a data channel and ask a remote peer to join you, call the RTCPeerConnection's createDataChannel() method.
    /// The peer being invited to exchange data receives a datachannel event (which has type RTCDataChannelEvent) to let it know the data channel has been added to the connection.
    /// </remarks>
    /// <example>
    ///     <code lang="cs"><![CDATA[
    ///         var pc = new RTCPeerConnection();
    ///         var dc = pc.createDataChannel("my channel");
    ///
    ///         dc.OnMessage = (event) => {
    ///             Debug.LogFormat("received: {0}"`",${event.data});
    ///         };
    ///
    ///         dc.OnOpen = () => {
    ///             Debug.Log("datachannel open");
    ///         };
    ///
    ///         dc.OnClose = () => {
    ///             Debug.Log("datachannel close");
    ///         };
    ///     ]]></code>
    /// </example>
    /// <seealso cref="RTCPeerConnection.CreateDataChannel(string, RTCDataChannelInit)"/>
    public class RTCDataChannel : RefCountedObject
    {
        private DelegateOnMessage onMessage;
        private DelegateOnOpen onOpen;
        private DelegateOnClose onClose;
        private DelegateOnError onError;

        /// <summary>
        /// Delegate to be called when a message has been received from the remote peer.
        /// </summary>
        public DelegateOnMessage OnMessage
        {
            get => onMessage;
            set => onMessage = value;
        }

        /// <summary>
        /// Delegate to be called when the data channel's messages is opened or reopened.
        /// </summary>
        public DelegateOnOpen OnOpen
        {
            get => onOpen;
            set => onOpen = value;
        }

        /// <summary>
        /// Delegate to be called when the data channel's messages is closed.
        /// </summary>
        public DelegateOnClose OnClose
        {
            get => onClose;
            set => onClose = value;
        }

        /// <summary>
        /// Delegate to be called when the errors occur.
        /// </summary>
        public DelegateOnError OnError
        {
            get => onError;
            set => onError = value;
        }

        /// <summary>
        /// Returns an ID number (between 0 and 65,534) which uniquely identifies the RTCDataChannel.
        /// </summary>
        public int Id => NativeMethods.DataChannelGetID(GetSelfOrThrow());

        /// <summary>
        /// Returns a string containing a name describing the data channel which are not required to be unique.
        /// </summary>
        public string Label => NativeMethods.DataChannelGetLabel(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        /// <summary>
        /// Returns a string containing the name of the sub protocol in use. If no protocol was specified when the data channel was created, then this property's value is the empty string ("").
        /// </summary>
        public string Protocol => NativeMethods.DataChannelGetProtocol(GetSelfOrThrow()).AsAnsiStringWithFreeMem();

        /// <summary>
        /// Returns the maximum number of times the browser should try to retransmit a message before giving up.
        /// </summary>
        public ushort MaxRetransmits => NativeMethods.DataChannelGetMaxRetransmits(GetSelfOrThrow());

        /// <summary>
        /// Returns the amount of time, in milliseconds, the browser is allowed to take to attempt to transmit a message, as set when the data channel was created, or null.
        /// </summary>
        public ushort MaxRetransmitTime => NativeMethods.DataChannelGetMaxRetransmitTime(GetSelfOrThrow());

        /// <summary>
        /// Indicates whether or not the data channel guarantees in-order delivery of messages.
        /// </summary>
        public bool Ordered => NativeMethods.DataChannelGetOrdered(GetSelfOrThrow());

        /// <summary>
        /// Returns the number of bytes of data currently queued to be sent over the data channel.
        /// </summary>
        public ulong BufferedAmount => NativeMethods.DataChannelGetBufferedAmount(GetSelfOrThrow());

        /// <summary>
        ///
        /// </summary>
        public bool Negotiated => NativeMethods.DataChannelGetNegotiated(GetSelfOrThrow());

        /// <summary>
        /// Returns an enum of the <c>RTCDataChannelState</c> which shows
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
        /// Release all the resources RTCDataChannel instance has allocated.
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
        /// Sends data across the data channel to the remote peer.
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
        /// Sends data across the data channel to the remote peer.
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
        /// Sends data across the data channel to the remote peer.
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
        /// Sends data across the data channel to the remote peer.
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
        /// Sends data across the data channel to the remote peer.
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
        /// Sends data across the data channel to the remote peer.
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
        /// Sends data across the data channel to the remote peer.
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
        /// Closes the RTCDataChannel. Either peer is permitted to call this method to initiate closure of the channel.
        /// </summary>
        public void Close()
        {
            NativeMethods.DataChannelClose(GetSelfOrThrow());
        }
    }
}
