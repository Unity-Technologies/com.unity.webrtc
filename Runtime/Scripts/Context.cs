using System;
using System.Collections;

namespace Unity.WebRTC
{
    internal class Context : IDisposable
    {
        internal IntPtr ptrNativeObj;
        internal Hashtable table;

        private int id;
        private bool disposed;

        public bool IsNull
        {
            get { return ptrNativeObj == IntPtr.Zero; }
        }

        public static implicit operator bool(Context v)
        {
            return v.ptrNativeObj != IntPtr.Zero;
        }

        public static bool ToBool(Context v)
        {
            return v;
        }

        public static Context Create(int id = 0)
        {
            var ptr = NativeMethods.ContextCreate(id);
            return new Context(ptr, id);
        }

        private Context(IntPtr ptr, int id)
        {
            ptrNativeObj = ptr;
            this.id = id;
            this.table = new Hashtable();
        }

        ~Context()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (ptrNativeObj != IntPtr.Zero)
            {
                foreach(var value in table.Values)
                {
                    var disposable = value as IDisposable;
                    disposable.Dispose();
                }
                table.Clear();

                NativeMethods.ContextDestroy(id);
                ptrNativeObj = IntPtr.Zero;
            }
            this.disposed = true;
        }

        public static CodecInitializationResult GetCodecInitializationResult()
        {
            return NativeMethods.GetCodecInitializationResult();
        }

        public IntPtr CreatePeerConnection()
        {
            return NativeMethods.ContextCreatePeerConnection(ptrNativeObj);
        }

        public IntPtr CreatePeerConnection(string conf)
        {
            return NativeMethods.ContextCreatePeerConnectionWithConfig(ptrNativeObj, conf);
        }

        public void DeletePeerConnection(IntPtr ptr)
        {
            NativeMethods.ContextDeletePeerConnection(ptrNativeObj, ptr);
        }

        public IntPtr CreateDataChannel(IntPtr ptr, string label, ref RTCDataChannelInit options)
        {
            return NativeMethods.ContextCreateDataChannel(ptrNativeObj, ptr, label, ref options);
        }

        public void DeleteDataChannel(IntPtr ptr)
        {
            NativeMethods.ContextDeleteDataChannel(ptrNativeObj, ptr);
        }

        public IntPtr CreateMediaStream(string label)
        {
            return NativeMethods.CreateMediaStream(ptrNativeObj, label);
        }

        public IntPtr CreateVideoTrack(string label, IntPtr rt, int width, int height, int bitRate)
        {
            return NativeMethods.CreateVideoTrack(ptrNativeObj, label, rt, width, height, bitRate);
        }
        public IntPtr CreateAudioTrack(string label)
        {
            return NativeMethods.CreateAudioTrack(ptrNativeObj, label);
        }

        public IntPtr GetRenderEventFunc()
        {
            return NativeMethods.GetRenderEventFunc(ptrNativeObj);
        }

        public void StopMediaStreamTrack(IntPtr track)
        {
            NativeMethods.StopMediaStreamTrack(ptrNativeObj, track);
        }
    }
}
