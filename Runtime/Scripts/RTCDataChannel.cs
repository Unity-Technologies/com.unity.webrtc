using System.Runtime.InteropServices;
using System;

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

        public static explicit operator RTCDataChannelInitInternal (RTCDataChannelInit origin)
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

    public delegate void DelegateOnOpen();
    public delegate void DelegateOnClose();
    public delegate void DelegateOnMessage(byte[] bytes);
    public delegate void DelegateOnDataChannel(RTCDataChannel channel);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="RTCPeerConnection.CreateDataChannel(string, RTCDataChannelInit)"/>
    public class RTCDataChannel : IDisposable
    {
        private IntPtr self;
        private DelegateOnMessage onMessage;
        private DelegateOnOpen onOpen;
        private DelegateOnClose onClose;

        private int id;
        private bool disposed;

        /// <summary>
        /// 
        /// </summary>
        public DelegateOnMessage OnMessage
        {
            get { return onMessage; }
            set
            {
                onMessage = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateOnOpen OnOpen
        {
            get { return onOpen; }
            set
            {
                onOpen = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateOnClose OnClose
        {
            get { return onClose; }
            set
            {
                onClose = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Id => NativeMethods.DataChannelGetID(self);

        /// <summary>
        /// 
        /// </summary>
        public string Label => NativeMethods.DataChannelGetLabel(self).AsAnsiStringWithFreeMem();

        /// <summary>
        /// 
        /// </summary>
        public string Protocol => NativeMethods.DataChannelGetProtocol(self).AsAnsiStringWithFreeMem();

        /// <summary>
        /// 
        /// </summary>
        public ushort MaxRetransmits => NativeMethods.DataChannelGetMaxRetransmits(self);

        /// <summary>
        /// 
        /// </summary>
        public ushort MaxRetransmitTime => NativeMethods.DataChannelGetMaxRetransmitTime(self);

        /// <summary>
        /// 
        /// </summary>
        public bool Ordered => NativeMethods.DataChannelGetOrdered(self);

        /// <summary>
        /// 
        /// </summary>
        public ulong BufferedAmount => NativeMethods.DataChannelGetBufferedAmount(self);

        /// <summary>
        /// 
        /// </summary>
        public bool Negotiated => NativeMethods.DataChannelGetNegotiated(self);

        /// <summary>
        /// The property returns an enum of the <c>RTCDataChannelState</c> which shows 
        /// the state of the channel.
        /// </summary>
        /// <remarks>
        /// <see cref="Send(string)"/> method must be called when the state is <b>Open</b>.
        /// </remarks>
        /// <seealso cref="RTCDataChannelState"/>
        public RTCDataChannelState ReadyState => NativeMethods.DataChannelGetReadyState(self);

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnMessage))]
        static void DataChannelNativeOnMessage(IntPtr ptr, byte[] msg, int len)
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

        internal RTCDataChannel(IntPtr ptr, RTCPeerConnection peerConnection)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            NativeMethods.DataChannelRegisterOnMessage(self, DataChannelNativeOnMessage);
            NativeMethods.DataChannelRegisterOnOpen(self, DataChannelNativeOnOpen);
            NativeMethods.DataChannelRegisterOnClose(self, DataChannelNativeOnClose);
        }

        ~RTCDataChannel()
        {
            this.Dispose();
        }

        public void Dispose()
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
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
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
            NativeMethods.DataChannelSend(self, msg);
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
            NativeMethods.DataChannelSendBinary(self, msg, msg.Length);
        }

        public void Close()
        {
            if (self != IntPtr.Zero)
            {
                NativeMethods.DataChannelClose(self);
            }
        }
    }
}
