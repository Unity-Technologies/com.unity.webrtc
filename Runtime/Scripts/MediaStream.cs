using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public delegate void DelegateOnAddTrack(MediaStreamTrackEvent e);
    public delegate void DelegateOnRemoveTrack(MediaStreamTrackEvent e);

    public class MediaStream : IDisposable
    {
        private DelegateOnAddTrack onAddTrack;
        private DelegateNativeMediaStreamOnAddTrack selfOnAddTrack;
        private DelegateOnRemoveTrack onRemoveTrack;
        private DelegateNativeMediaStreamOnRemoveTrack selfOnRemoveTrack;

        internal IntPtr self;
        private readonly string id;
        private bool disposed;


        public string Id
        {
            get
            {
                return id;
            }
        }

        ~MediaStream()
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
            if(self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteMediaStream(this);
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public DelegateOnAddTrack OnAddTrack
        {
            get => onAddTrack;
            set
            {
                onAddTrack = value;
                selfOnAddTrack = MediaStreamOnAddTrack;
                WebRTC.Context.MediaStreamRegisterOnAddTrack(self, selfOnAddTrack);
            }
        }

        public DelegateOnRemoveTrack OnRemoveTrack
        {
            get => onRemoveTrack;
            set
            {
                onRemoveTrack = value;
                selfOnRemoveTrack = MediaStreamOnRemoveTrack;
                WebRTC.Context.MediaStreamRegisterOnRemoveTrack(self, selfOnRemoveTrack);
            }
        }

        private void StopTrack(MediaStreamTrack track)
        {

            if (track.Kind == TrackKind.Video)
            {
                WebRTC.Context.StopMediaStreamTrack(track.self);
            }
            else
            {
                Audio.Stop();
            }
        }

        public IEnumerable<MediaStreamTrack> GetVideoTracks()
        {
            int length = 0;
            var buf = NativeMethods.MediaStreamGetVideoTracks(self, ref length);
            return WebRTC.Deserialize(buf, length, ptr => new MediaStreamTrack(ptr));
        }

        public IEnumerable<MediaStreamTrack> GetAudioTracks()
        {
            int length = 0;
            var buf = NativeMethods.MediaStreamGetAudioTracks(self, ref length);
            return WebRTC.Deserialize(buf, length, ptr => new MediaStreamTrack(ptr));
        }

        public IEnumerable<MediaStreamTrack> GetTracks()
        {
            return GetAudioTracks().Concat(GetVideoTracks());
        }

        public bool AddTrack(MediaStreamTrack track)
        {
            return NativeMethods.MediaStreamAddTrack(self, track.self);
        }
        public bool RemoveTrack(MediaStreamTrack track)
        {
            return NativeMethods.MediaStreamRemoveTrack(self, track.self);
        }

        public MediaStream() : this(WebRTC.Context.CreateMediaStream(Guid.NewGuid().ToString()))
        {
        }

        internal MediaStream(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            id = Marshal.PtrToStringAnsi(NativeMethods.MediaStreamGetID(self));
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnAddTrack))]
        static void MediaStreamOnAddTrack(IntPtr ptr, IntPtr track)
        {
            WebRTC.SyncContext.Post(_ =>
            {
                if (WebRTC.Table[ptr] is MediaStream stream)
                {
                    stream.OnAddTrack(new MediaStreamTrackEvent(track));
                }
            }, null);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateNativeMediaStreamOnRemoveTrack))]
        static void MediaStreamOnRemoveTrack(IntPtr ptr, IntPtr track)
        {
            WebRTC.SyncContext.Post(_ =>
            {
                if (WebRTC.Table[ptr] is MediaStream stream)
                {
                    stream.OnRemoveTrack(new MediaStreamTrackEvent(track));
                }
            }, null);
        }
    }
    internal class Cleaner : MonoBehaviour
    {
        private Action onDestroy;
        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
        public static void AddCleanerCallback(GameObject obj, Action callback)
        {
            Cleaner cleaner = obj.GetComponent<Cleaner>();
            if (!cleaner)
            {
                cleaner = obj.AddComponent<Cleaner>();
                cleaner.hideFlags = HideFlags.HideAndDontSave;
            }
            cleaner.onDestroy += callback;
        }
    }
    internal static class CleanerExtensions
    {
        public static void AddCleanerCallback(this GameObject obj, Action callback)
        {
            Cleaner.AddCleanerCallback(obj, callback);
        }
    }

    public static class CameraExtension
    {
        public static VideoStreamTrack CaptureStreamTrack(this Camera cam, int width, int height, int bitrate, RenderTextureDepth depth = RenderTextureDepth.DEPTH_24)
        {
            switch (depth)
            {
                case RenderTextureDepth.DEPTH_16:
                case RenderTextureDepth.DEPTH_24:
                case RenderTextureDepth.DEPTH_32:
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(depth), (int)depth, typeof(RenderTextureDepth));
            }

            int depthValue = (int)depth;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new RenderTexture(width, height, depthValue, format);
            rt.Create();
            cam.targetTexture = rt;
            return new VideoStreamTrack(cam.name, rt, bitrate);
        }


        public static MediaStream CaptureStream(this Camera cam, int width, int height, int bitrate, RenderTextureDepth depth = RenderTextureDepth.DEPTH_24)
        {
            var stream = new MediaStream(WebRTC.Context.CreateMediaStream("videostream"));
            var track = cam.CaptureStreamTrack(width, height, bitrate, depth);
            stream.AddTrack(track);
            return stream;
        }
    }

    public static class Audio
    {
        private static bool started;
        public static MediaStream CaptureStream()
        {
            started = true;

            var stream = new MediaStream(WebRTC.Context.CreateMediaStream("audiostream"));
            var track = new MediaStreamTrack(WebRTC.Context.CreateAudioTrack("audio"));
            stream.AddTrack(track);
            return stream;
        }

        public static void Update(float[] audioData, int channels)
        {
            if (started)
            {
                NativeMethods.ProcessAudio(audioData, audioData.Length);
            }
        }
        public static void Stop()
        {
            if (started)
            {
                started = false;
            }
        }
    }
}
