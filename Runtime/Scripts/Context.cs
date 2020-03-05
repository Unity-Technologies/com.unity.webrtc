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
            UnityEngine.Debug.Log("Context Dispose");
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
            UnityEngine.Debug.Log("Context Dispose end");
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
        /*
        public IntPtr CaptureVideoStream(IntPtr rt, int width, int height)
        {
            return NativeMethods.ContextCreateVideoStream(self, rt, width, height);
        }

        // TODO:: Fix API design for multi tracks
        public void DeleteVideoStream(IntPtr stream)
        {
            NativeMethods.ContextDeleteVideoStream(self, stream);
        }
        */

        public IntPtr CreateMediaStream(string label)
        {
            return NativeMethods.ContextCreateMediaStream(self, label);
        }

        public void DeleteMediaStream(MediaStream stream)
        {
            NativeMethods.ContextDeleteMediaStream(self, stream.self);
        }

        /*
        public MediaStream CreateAudioStream()
        {
            var stream = CreateMediaStream("audiostream");
            var track = CreateAudioTrack("audio");
            stream.AddTrack(track);
            return stream;
        }

        public MediaStream CreateVideoStream(IntPtr rt, int width, int height, int bitrate)
        {
            var stream = CreateMediaStream("videostream");
            var track = CreateVideoTrack("video", rt, width, height, bitrate);
            NativeMethods.MediaStreamAddTrack()
            stream.AddTrack(track);
            return stream;
        }
        */


        public IntPtr GetRenderEventFunc()
        {
            return NativeMethods.GetRenderEventFunc(self);
        }

        public IntPtr CreateAudioTrack(string label)
        {
            return NativeMethods.ContextCreateAudioTrack(self, label);
        }

        public IntPtr CreateVideoTrack(string label, IntPtr rt, int width, int height, int bitrate)
        {
            return NativeMethods.ContextCreateVideoTrack(self, label, rt, width, height, bitrate);
        }

        public void StopMediaStreamTrack(IntPtr track)
        {
            NativeMethods.ContextStopMediaStreamTrack(self, track);
        }

        public void DeleteMediaStreamTrack(IntPtr track)
        {
            NativeMethods.ContextDeleteMediaStreamTrack(self, track);
        }

        internal void InitializeEncoder(IntPtr track)
        {
            renderFunction = renderFunction == IntPtr.Zero ? GetRenderEventFunc() : renderFunction;
            VideoEncoderMethods.InitializeEncoder(renderFunction, track);
        }

        internal void FinalizeEncoder(IntPtr track)
        {
            renderFunction = renderFunction == IntPtr.Zero ? GetRenderEventFunc() : renderFunction;
            VideoEncoderMethods.FinalizeEncoder(renderFunction, track);
        }

        internal void Encode(IntPtr track)
        {
            renderFunction = renderFunction == IntPtr.Zero ? GetRenderEventFunc() : renderFunction;
            VideoEncoderMethods.Encode(renderFunction, track);
        }
    }
}
