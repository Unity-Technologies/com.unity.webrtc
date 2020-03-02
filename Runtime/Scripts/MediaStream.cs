using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public class MediaStream : IDisposable
    {
        enum MediaStreamType
        {
            Video,
            Audio
        }

        internal IntPtr self;
        private readonly string id;
        private bool disposed;
        private List<MediaStreamTrack> tracks = new List<MediaStreamTrack>();


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
            UnityEngine.Debug.Log("MediaStream Dispose");

            if (this.disposed)
            {
                UnityEngine.Debug.Log("MediaStream already Disposed");
                return;
            }
            if(self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteMediaStream(this);
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);

            UnityEngine.Debug.Log("MediaStream Dispose end");
        }

        public void FinalizeEncoder()
        {
            WebRTC.Context.FinalizeEncoder();
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
            return tracks.Where(_ => _.Kind == TrackKind.Video);
        }
        public IEnumerable<MediaStreamTrack> GetAudioTracks()
        {
            return tracks.Where(_ => _.Kind == TrackKind.Audio);
        }
        public IEnumerable<MediaStreamTrack> GetTracks()
        {
            return tracks;
        }

        public bool AddTrack(MediaStreamTrack track)
        {
            if(NativeMethods.MediaStreamAddTrack(self, track.self))
            {
                tracks.Add(track);
                return true;
            }
            return false;
        }
        public bool RemoveTrack(MediaStreamTrack track)
        {
            if(NativeMethods.MediaStreamRemoveTrack(self, track.self))
            {
                tracks.Remove(track);
                return true;
            }
            return false;
        }

        internal MediaStream(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            id = Marshal.PtrToStringAnsi(NativeMethods.MediaStreamGetID(self));
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
        internal static List<RenderTexture[]> camCopyRts = new List<RenderTexture[]>();
        internal static bool started = false;
        public static MediaStream CaptureStream(this Camera cam, int width, int height, RenderTextureDepth depth = RenderTextureDepth.DEPTH_24)
        {
            switch (depth)
            {
                case RenderTextureDepth.DEPTH_16:
                case RenderTextureDepth.DEPTH_24:
                case RenderTextureDepth.DEPTH_32:
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(depth), (int) depth, typeof(RenderTextureDepth));
            }

            int depthValue = (int)depth;

            var rts = new RenderTexture[2];
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            //rts[0] for render target, rts[1] for flip and WebRTC source
            rts[0] = new RenderTexture(width, height, depthValue, format);
            rts[1] = new RenderTexture(width, height,  0, format);
            rts[0].Create();
            rts[1].Create();
            camCopyRts.Add(rts);
            cam.targetTexture = rts[0];
            cam.gameObject.AddCleanerCallback(() =>
            {
                RemoveRt(rts);
                rts[0].Release();
                rts[1].Release();
                UnityEngine.Object.Destroy(rts[0]);
                UnityEngine.Object.Destroy(rts[1]);
            });
            started = true;

            var stream = new MediaStream(WebRTC.Context.CreateMediaStream("videostream"));
            var track = new MediaStreamTrack(WebRTC.Context.CreateVideoTrack("video", rts[1].GetNativeTexturePtr(), width, height, 1000000));
            stream.AddTrack(track);

            // TODO::
            // You should initialize encoder after create stream instance.
            // This specification will change in the future.
            WebRTC.Context.InitializeEncoder();
            return stream;
        }
        public static void RemoveRt(RenderTexture[] rts)
        {
            camCopyRts.Remove(rts);
            if (camCopyRts.Count == 0)
            {
                started = false;
            }
        }

    }

    public static class Audio
    {
        private static bool started = false;
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
