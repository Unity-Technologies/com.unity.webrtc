using System;
using System.Collections;

namespace Unity.WebRTC
{
    internal class Context : IDisposable
    {
        internal IntPtr self;
        internal Hashtable table;

        private int id;
        private bool disposed;

        public bool IsNull
        {
            get { return self == IntPtr.Zero; }
        }

        public static implicit operator bool(Context v)
        {
            return v.self != IntPtr.Zero;
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
            self = ptr;
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
            if (self != IntPtr.Zero)
            {
                foreach(var value in table.Values)
                {
                    var disposable = value as IDisposable;
                    disposable.Dispose();
                }
                table.Clear();

                NativeMethods.ContextDestroy(id);
                self = IntPtr.Zero;
            }
            this.disposed = true;
        }

        public static CodecInitializationResult GetCodecInitializationResult()
        {
            return NativeMethods.GetCodecInitializationResult();
        }

        public IntPtr CreatePeerConnection()
        {
            return NativeMethods.ContextCreatePeerConnection(self);
        }

        public IntPtr CreatePeerConnection(string conf)
        {
            return NativeMethods.ContextCreatePeerConnectionWithConfig(self, conf);
        }

        public void DeletePeerConnection(IntPtr ptr)
        {
            NativeMethods.ContextDeletePeerConnection(self, ptr);
        }

        public IntPtr CreateDataChannel(IntPtr ptr, string label, ref RTCDataChannelInit options)
        {
            return NativeMethods.ContextCreateDataChannel(self, ptr, label, ref options);
        }

        public void DeleteDataChannel(IntPtr ptr)
        {
            NativeMethods.ContextDeleteDataChannel(self, ptr);
        }

        public IntPtr CaptureVideoStream(IntPtr rt, int width, int height)
        {
            return NativeMethods.ContextCaptureVideoStream(self, rt, width, height);
        }

        public void DeleteVideoStream(IntPtr stream)
        {
            NativeMethods.ContextDeleteVideoStream(self, stream);
        }

        public IntPtr CaptureAudioStream()
        {
            return NativeMethods.ContextCaptureAudioStream(self);
        }

        public IntPtr GetRenderEventFunc()
        {
            return NativeMethods.GetRenderEventFunc(self);
        }

        public void StopMediaStreamTrack(IntPtr track)
        {
            NativeMethods.StopMediaStreamTrack(self, track);
        }
    }
}
