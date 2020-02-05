using UnityEngine;
using System;
using System.Collections;
using Unity.Collections;
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

        private IntPtr self;
        private string id;
        private bool disposed;
        private MediaStreamType _streamType;
        private Dictionary<MediaStreamTrack, RenderTexture[]> VideoTrackToRts;
        private List<MediaStreamTrack> AudioTracks;

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
                var tracks = GetTracks();
                foreach (var track in tracks)
                {
                    StopTrack(track);
                }
                switch (_streamType)
                {
                    case MediaStreamType.Video:
                        WebRTC.Context.DeleteVideoStream(self);
                        break;
                    case MediaStreamType.Audio:
                        WebRTC.Context.DeleteAudioStream(self);
                        break;
                }
                self = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
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
                RenderTexture[] rts = VideoTrackToRts[track];
                if (rts != null)
                {
                    CameraExtension.RemoveRt(rts);
                    rts[0].Release();
                    rts[1].Release();
                    UnityEngine.Object.Destroy(rts[0]);
                    UnityEngine.Object.Destroy(rts[1]);
                }
            }
            else
            {
                Audio.Stop();
            }
        }
        private RenderTexture[] GetRts(MediaStreamTrack track)
        {
            return VideoTrackToRts[track];
        }
        public MediaStreamTrack[] GetTracks()
        {
            MediaStreamTrack[] tracks = new MediaStreamTrack[VideoTrackToRts.Keys.Count + AudioTracks.Count];
            AudioTracks.CopyTo(tracks, 0);
            VideoTrackToRts.Keys.CopyTo(tracks, AudioTracks.Count);
            return tracks;
        }
        public MediaStreamTrack[] GetAudioTracks()
        {
            return AudioTracks.ToArray();
        }
        public MediaStreamTrack[] GetVideoTracks()
        {
            MediaStreamTrack[] tracks = new MediaStreamTrack[VideoTrackToRts.Keys.Count];
            VideoTrackToRts.Keys.CopyTo(tracks, 0);
            return tracks;
        }

        public void AddTrack(MediaStreamTrack track)
        {
            if(track.Kind == TrackKind.Video)
            {
                VideoTrackToRts[track] = track.getRts(track);
            }
            else
            {
                AudioTracks.Add(track);
            }
            NativeMethods.MediaStreamAddTrack(self, track.self);
        }
        public void RemoveTrack(MediaStreamTrack track)
        {
            NativeMethods.MediaStreamRemoveTrack(self, track.self);
        }
        //for camera CaptureStream
        internal MediaStream(RenderTexture[] rts, IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            id = Marshal.PtrToStringAnsi(NativeMethods.MediaStreamGetID(self));
            _streamType = MediaStreamType.Video;
            VideoTrackToRts = new Dictionary<MediaStreamTrack, RenderTexture[]>();
            AudioTracks = new List<MediaStreamTrack>();
            //get initial tracks
            int trackSize = 0;
            IntPtr tracksNativePtr = NativeMethods.MediaStreamGetVideoTracks(self, ref trackSize);
            IntPtr[] tracksPtr = new IntPtr[trackSize];
            Marshal.Copy(tracksNativePtr, tracksPtr, 0, trackSize);
            //TODO: Linux compatibility
            Marshal.FreeCoTaskMem(tracksNativePtr);
            for (int i = 0; i < trackSize; i++)
            {
                MediaStreamTrack track = new MediaStreamTrack(tracksPtr[i]);
                track.stopTrack += StopTrack;
                track.getRts += GetRts;
                VideoTrackToRts[track] = rts;
            }
        }
        //for audio CaptureStream
        internal MediaStream(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);
            id = Marshal.PtrToStringAnsi(NativeMethods.MediaStreamGetID(self));
            _streamType = MediaStreamType.Audio;
            VideoTrackToRts = new Dictionary<MediaStreamTrack, RenderTexture[]>();
            AudioTracks = new List<MediaStreamTrack>();
            //get initial tracks
            int trackSize = 0;
            IntPtr trackNativePtr = NativeMethods.MediaStreamGetAudioTracks(self, ref trackSize);
            IntPtr[] tracksPtr = new IntPtr[trackSize];
            Marshal.Copy(trackNativePtr, tracksPtr, 0, trackSize);
            //TODO: Linux compatibility
            Marshal.FreeCoTaskMem(trackNativePtr);

            for (int i = 0; i < trackSize; i++)
            {
                MediaStreamTrack track = new MediaStreamTrack(tracksPtr[i]);
                track.stopTrack += StopTrack;
                track.getRts += GetRts;
                AudioTracks.Add(track);
            }
        }

    }
    internal class Cleaner : MonoBehaviour
    {
        private Action onDestroy;
        private void OnDestroy()
        {
            if (onDestroy != null)
            {
                onDestroy();
            }
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
            if (camCopyRts.Count > 0)
            {
                throw new NotImplementedException("Currently not allowed multiple MediaStream");
            }

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

            RenderTexture[] rts = new RenderTexture[2];
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
                CameraExtension.RemoveRt(rts);
                rts[0].Release();
                rts[1].Release();
                UnityEngine.Object.Destroy(rts[0]);
                UnityEngine.Object.Destroy(rts[1]);
            });
            started = true;

            var stream = WebRTC.Context.CaptureVideoStream(rts[1].GetNativeTexturePtr(), width, height);

            // TODO::
            // You should initialize encoder after create stream instance.
            // This specification will change in the future.
            WebRTC.Context.InitializeEncoder();

            return new MediaStream(rts, stream);
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
            return new MediaStream(WebRTC.Context.CreateAudioStream());
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
