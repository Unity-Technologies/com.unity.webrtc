using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.WebRTC
{
    internal class Context : IDisposable
    {
        internal IntPtr self;
        internal WeakReferenceTable table;

        private int id;
        private bool disposed;
        private IntPtr renderFunction;
        private IntPtr textureUpdateFunction;

        public static Context Create(int id = 0, EncoderType encoderType = EncoderType.Hardware)
        {
            if (encoderType == EncoderType.Hardware && !NativeMethods.GetHardwareEncoderSupport())
            {
                throw new ArgumentException("Hardware encoder is not supported");
            }

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
            this.table = new WeakReferenceTable();
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
                foreach (var value in table.CopiedValues)
                {
                    if (value == null)
                        continue;
                    var disposable = value as IDisposable;
                    disposable?.Dispose();
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

        public RTCError PeerConnectionSetLocalDescription(
            IntPtr ptr, ref RTCSessionDescription desc)
        {
            IntPtr ptrError = IntPtr.Zero;
            RTCErrorType errorType = NativeMethods.PeerConnectionSetLocalDescription(
                self, ptr, ref desc, ref ptrError);
            string message = ptrError != IntPtr.Zero ? ptrError.AsAnsiStringWithFreeMem() : null;
            return new RTCError { errorType =  errorType, message = message};
        }

        public RTCError PeerConnectionSetLocalDescription(IntPtr ptr)
        {
            IntPtr ptrError = IntPtr.Zero;
            RTCErrorType errorType =
                NativeMethods.PeerConnectionSetLocalDescriptionWithoutDescription(self, ptr, ref ptrError);
            string message = ptrError != IntPtr.Zero ? ptrError.AsAnsiStringWithFreeMem() : null;
            return new RTCError {errorType = errorType, message = message};
        }

        public RTCError PeerConnectionSetRemoteDescription(
            IntPtr ptr, ref RTCSessionDescription desc)
        {
            IntPtr ptrError = IntPtr.Zero;
            RTCErrorType errorType = NativeMethods.PeerConnectionSetRemoteDescription(
                self, ptr, ref desc, ref ptrError);
            string message = ptrError != IntPtr.Zero ? ptrError.AsAnsiStringWithFreeMem() : null;
            return new RTCError { errorType =  errorType, message = message};
        }

        public void PeerConnectionRegisterOnSetSessionDescSuccess(IntPtr ptr, DelegateNativePeerConnectionSetSessionDescSuccess callback)
        {
            NativeMethods.PeerConnectionRegisterOnSetSessionDescSuccess(self, ptr, callback);
        }

        public void PeerConnectionRegisterOnSetSessionDescFailure(IntPtr ptr, DelegateNativePeerConnectionSetSessionDescFailure callback)
        {
            NativeMethods.PeerConnectionRegisterOnSetSessionDescFailure(self, ptr, callback);
        }

        public IntPtr CreateDataChannel(IntPtr ptr, string label, ref RTCDataChannelInitInternal options)
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
            NativeMethods.ContextDeleteMediaStream(self, stream.GetSelfOrThrow());
        }

        public void RegisterMediaStreamObserver(MediaStream stream)
        {
            NativeMethods.ContextRegisterMediaStreamObserver(self, stream.GetSelfOrThrow());
        }

        public void UnRegisterMediaStreamObserver(MediaStream stream)
        {
            NativeMethods.ContextRegisterMediaStreamObserver(self, stream.GetSelfOrThrow());
        }

        public void MediaStreamRegisterOnAddTrack(MediaStream stream, DelegateNativeMediaStreamOnAddTrack callback)
        {
            NativeMethods.MediaStreamRegisterOnAddTrack(self, stream.GetSelfOrThrow(), callback);
        }

        public void MediaStreamRegisterOnRemoveTrack(MediaStream stream, DelegateNativeMediaStreamOnRemoveTrack callback)
        {
            NativeMethods.MediaStreamRegisterOnRemoveTrack(self, stream.GetSelfOrThrow(), callback);
        }

        public IntPtr GetRenderEventFunc()
        {
            return NativeMethods.GetRenderEventFunc(self);
        }

        public IntPtr GetUpdateTextureFunc()
        {
            return NativeMethods.GetUpdateTextureFunc(self);
        }

        public IntPtr CreateAudioTrack(string label)
        {
            return NativeMethods.ContextCreateAudioTrack(self, label);
        }

        public IntPtr CreateVideoTrack(string label)
        {
            return NativeMethods.ContextCreateVideoTrack(self, label);
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

        public void DeleteVideoRenderer(IntPtr sink)
        {
            NativeMethods.DeleteVideoRenderer(self, sink);
        }

        public void DeleteStatsReport(IntPtr report)
        {
            NativeMethods.ContextDeleteStatsReport(self, report);
        }

        public void SetVideoEncoderParameter(IntPtr track, int width, int height, GraphicsFormat format, IntPtr texturePtr)
        {
            NativeMethods.ContextSetVideoEncoderParameter(self, track, width, height, format, texturePtr);
        }

        public CodecInitializationResult GetInitializationResult(IntPtr track)
        {
            return NativeMethods.GetInitializationResult(self, track);
        }

        public void GetSenderCapabilities(TrackKind kind, out IntPtr capabilities)
        {
            NativeMethods.ContextGetSenderCapabilities(self, kind, out capabilities);
        }

        public void GetReceiverCapabilities(TrackKind kind, out IntPtr capabilities)
        {
            NativeMethods.ContextGetReceiverCapabilities(self, kind, out capabilities);
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

        internal void UpdateRendererTexture(uint rendererId, UnityEngine.Texture texture)
        {
            textureUpdateFunction = textureUpdateFunction == IntPtr.Zero ? GetUpdateTextureFunc() : textureUpdateFunction;
            VideoDecoderMethods.UpdateRendererTexture(textureUpdateFunction, texture, rendererId);
        }
    }
}
