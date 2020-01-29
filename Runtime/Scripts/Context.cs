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
        private IntPtr renderFunction;

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

        public static Context Create(int id = 0, EncoderType encoderType = EncoderType.Hardware)
        {
            var ptr = NativeMethods.ContextCreate(id, encoderType);
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
            GC.SuppressFinalize(this);
        }

        public CodecInitializationResult GetCodecInitializationResult()
        {
            return NativeMethods.ContextGetCodecInitializationResult(self);
        }
        
        public EncoderType GetEncoderType()
        {
            return NativeMethods.ContextGetEncoderType(self);
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

        // TODO:: Fix API design for multi tracks
        public IntPtr CaptureVideoStream(IntPtr rt, int width, int height)
        {
            return NativeMethods.ContextCreateVideoStream(self, rt, width, height);
        }

        // TODO:: Fix API design for multi tracks
        public void DeleteVideoStream(IntPtr stream)
        {
            NativeMethods.ContextDeleteVideoStream(self, stream);
        }

        // TODO:: Fix API design for multi tracks
        public IntPtr CreateAudioStream()
        {
            return NativeMethods.ContextCreateAudioStream(self);
        }

        // TODO:: Fix API design for multi tracks
        public void DeleteAudioStream(IntPtr stream)
        {
            NativeMethods.ContextDeleteAudioStream(self, stream);
        }

        public IntPtr GetRenderEventFunc()
        {
            return NativeMethods.GetRenderEventFunc(self);
        }

        public void StopMediaStreamTrack(IntPtr track)
        {
            NativeMethods.StopMediaStreamTrack(self, track);
        }

        internal void InitializeEncoder()
        {
            renderFunction = renderFunction == IntPtr.Zero ? GetRenderEventFunc() : renderFunction;
            VideoEncoderMethods.InitializeEncoder(renderFunction);
        }

        internal void FinalizeEncoder()
        {
            renderFunction = renderFunction == IntPtr.Zero ? GetRenderEventFunc() : renderFunction;
            VideoEncoderMethods.FinalizeEncoder(renderFunction);
        }

        internal void Encode()
        {
            renderFunction = renderFunction == IntPtr.Zero ? GetRenderEventFunc() : renderFunction;
            VideoEncoderMethods.Encode(renderFunction);
        }
    }
}
