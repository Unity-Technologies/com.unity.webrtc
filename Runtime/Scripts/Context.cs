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

        public static Context Create(int id = 0, EncoderType encoderType = EncoderType.Hardware)
        {
            var ptr = NativeMethods.ContextCreate(id, encoderType);
            return new Context(ptr, id);
        }

        public bool IsNull
        {
            get { return self == IntPtr.Zero; }
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

        public void PeerConnectionSetLocalDescription(IntPtr ptr, ref RTCSessionDescription desc)
        {
            NativeMethods.PeerConnectionSetLocalDescription(self, ptr, ref desc);
        }
        public void PeerConnectionSetRemoteDescription(IntPtr ptr, ref RTCSessionDescription desc)
        {
            NativeMethods.PeerConnectionSetRemoteDescription(self, ptr, ref desc);
        }

        public void PeerConnectionRegisterOnSetSessionDescSuccess(IntPtr ptr, DelegateNativePeerConnectionSetSessionDescSuccess callback)
        {
            NativeMethods.PeerConnectionRegisterOnSetSessionDescSuccess(self, ptr, callback);
        }

        public void PeerConnectionRegisterOnSetSessionDescFailure(IntPtr ptr, DelegateNativePeerConnectionSetSessionDescFailure callback)
        {
            NativeMethods.PeerConnectionRegisterOnSetSessionDescFailure(self, ptr, callback);
        }

        public IntPtr CreateDataChannel(IntPtr ptr, string label, ref RTCDataChannelInit options)
        {
            return NativeMethods.ContextCreateDataChannel(self, ptr, label, ref options);
        }

        public void DeleteDataChannel(IntPtr ptr)
        {
            NativeMethods.ContextDeleteDataChannel(self, ptr);
        }

        public IntPtr CreateMediaStream(string label)
        {
            return NativeMethods.ContextCreateMediaStream(self, label);
        }

        public void DeleteMediaStream(MediaStream stream)
        {
            NativeMethods.ContextDeleteMediaStream(self, stream.self);
        }

        public void MediaStreamRegisterOnAddTrack(IntPtr stream, DelegateNativeMediaStreamOnAddTrack callback)
        {
            NativeMethods.MediaStreamRegisterOnAddTrack(self, stream, callback);
        }

        public void MediaStreamRegisterOnRemoveTrack(IntPtr stream, DelegateNativeMediaStreamOnRemoveTrack callback)
        {
            NativeMethods.MediaStreamRegisterOnRemoveTrack(self, stream, callback);
        }

        public IntPtr GetRenderEventFunc()
        {
            return NativeMethods.GetRenderEventFunc(self);
        }

        public IntPtr CreateAudioTrack(string label)
        {
            return NativeMethods.ContextCreateAudioTrack(self, label);
        }

        public IntPtr CreateVideoTrack(string label, IntPtr texturePtr)
        {
            return NativeMethods.ContextCreateVideoTrack(self, label, texturePtr);
        }

        public void StopMediaStreamTrack(IntPtr track)
        {
            NativeMethods.ContextStopMediaStreamTrack(self, track);
        }

        public void DeleteMediaStreamTrack(IntPtr track)
        {
            NativeMethods.ContextDeleteMediaStreamTrack(self, track);
        }

        public IntPtr CreateVideoRenderer()
        {
            return NativeMethods.CreateVideoRenderer(self);
        }

        public void DeleteStatsReport(IntPtr report)
        {
            NativeMethods.ContextDeleteStatsReport(self, report);
        }

        public void SetVideoEncoderParameter(IntPtr track, int width, int height)
        {
            NativeMethods.ContextSetVideoEncoderParameter(self, track, width, height);
        }

        public CodecInitializationResult GetInitializationResult(IntPtr track)
        {
            return NativeMethods.GetInitializationResult(self, track);
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
