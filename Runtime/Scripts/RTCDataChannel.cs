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
        private DelegateOnMessage onMessage;
        private DelegateOnOpen onOpen;
        private DelegateOnClose onClose;

        private int id;
        private bool disposed;


        public DelegateOnMessage OnMessage
        {
            get { return onMessage; }
            set
            {
                onMessage = value;
            }
        }

        public DelegateOnOpen OnOpen
        {
            get { return onOpen; }
            set
            {
                onOpen = value;
            }
        }
        public DelegateOnClose OnClose
        {
            get { return onClose; }
            set
            {
                onClose = value;
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
            Label = NativeMethods.DataChannelGetLabel(self).AsAnsiStringWithFreeMem();

            NativeMethods.DataChannelRegisterOnMessage(self, DataChannelNativeOnMessage);
            NativeMethods.DataChannelRegisterOnOpen(self, DataChannelNativeOnOpen);
            NativeMethods.DataChannelRegisterOnClose(self, DataChannelNativeOnClose);
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
