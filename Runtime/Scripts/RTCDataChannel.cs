using System.Runtime.InteropServices;
using System;

namespace Unity.WebRTC
{
    public delegate void DelegateOnOpen();
    public delegate void DelegateOnClose();
    public delegate void DelegateOnMessage(byte[] bytes);
    public delegate void DelegateOnDataChannel(RTCDataChannel channel);

    public class RTCDataChannel : IDisposable
    {
        private IntPtr self;
        private RTCPeerConnection peerConnection;
        private DelegateOnMessage onMessage;
        private DelegateOnOpen onOpen;
        private DelegateOnClose onClose;

        private DelegateNativeOnMessage selfOnMessage;
        private DelegateNativeOnOpen selfOnOpen;
        private DelegateNativeOnClose selfOnClose;
        private int id;
        private bool disposed;


        public DelegateOnMessage OnMessage
        {
            get { return onMessage; }
            set
            {
                onMessage = value;
                selfOnMessage = new DelegateNativeOnMessage(DataChannelNativeOnMessage);
                NativeMethods.DataChannelRegisterOnMessage(self, selfOnMessage);
            }
        }

        public DelegateOnOpen OnOpen
        {
            get { return onOpen; }
            set
            {
                onOpen = value;
                selfOnOpen = new DelegateNativeOnOpen(DataChannelNativeOnOpen);
                NativeMethods.DataChannelRegisterOnOpen(self, selfOnOpen);
            }
        }
        public DelegateOnClose OnClose
        {
            get { return onClose; }
            set
            {
                onClose = value;
                selfOnClose = new DelegateNativeOnClose(DataChannelNativeOnClose);
                NativeMethods.DataChannelRegisterOnClose(self, selfOnClose);
            }
        }

        public int Id
        {
            get => NativeMethods.DataChannelGetID(self);
        }
        public string Label { get; private set; }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnMessage))]
        static void DataChannelNativeOnMessage(IntPtr ptr, byte[] msg, int len)
        {
            WebRTC.SyncContext.Post(_ =>
            {
                var channel = WebRTC.Table[ptr] as RTCDataChannel;
                channel.onMessage(msg);
            }, null);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnOpen))]
        static void DataChannelNativeOnOpen(IntPtr ptr)
        {
            WebRTC.SyncContext.Post(_ =>
            {
                var channel = WebRTC.Table[ptr] as RTCDataChannel;
                channel.onOpen();
            }, null);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeOnClose))]
        static void DataChannelNativeOnClose(IntPtr ptr)
        {
            WebRTC.SyncContext.Post(_ =>
            {
                if (null == WebRTC.Table)
                    return;

                var channel = WebRTC.Table[ptr] as RTCDataChannel;
                channel.onClose();
            }, null);
        }
        internal RTCDataChannel(IntPtr ptr, RTCPeerConnection peerConnection)
        {
            self = ptr;
            this.peerConnection = peerConnection;
            WebRTC.Table.Add(self, this);
            var labelPtr = NativeMethods.DataChannelGetLabel(self);
            Label = Marshal.PtrToStringAnsi(labelPtr);
        }

        ~RTCDataChannel()
        {
            this.Dispose();
            WebRTC.Table.Remove(self);
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
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public void Send(string msg)
        {
            NativeMethods.DataChannelSend(self, msg);
        }

        public void Send(byte[] msg)
        {
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
