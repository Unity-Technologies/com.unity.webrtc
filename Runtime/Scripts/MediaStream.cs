using UnityEngine;
using System;
using Unity.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public class MediaStream : IDisposable
    {
        private IntPtr ptrNativeObj;
        private string id;
        protected List<MediaStreamTrack> mediaStreamTrackList = new List<MediaStreamTrack>();
        private static int sMediaStreamCount = 0;
        public string Id { get => id; private set { } }

        private bool disposed;

        public MediaStream()
        {
            sMediaStreamCount++;
            id = "MediaStream" + sMediaStreamCount;
            ptrNativeObj = WebRTC.Context.CreateMediaStream(id);
            WebRTC.Table.Add(ptrNativeObj, this);
        }

        public MediaStream(MediaStreamTrack[] tracks)
        {
            sMediaStreamCount++;
            id = "MediaStream" + sMediaStreamCount;
            ptrNativeObj = WebRTC.Context.CreateMediaStream(id);
            WebRTC.Table.Add(ptrNativeObj, this);

            foreach (var t in tracks)
            {
                AddTrack(t);
            }
        }

        ~MediaStream()
        {
            this.Dispose();
            WebRTC.Table.Remove(ptrNativeObj);
        }
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            if (ptrNativeObj != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteMediaStream(ptrNativeObj);
                ptrNativeObj = IntPtr.Zero;
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public void AddTrack(MediaStreamTrack track)
        {
            NativeMethods.MediaStreamAddTrack(ptrNativeObj, track.ptrNativeObj);
            mediaStreamTrackList.Add(track);
        }

        public MediaStreamTrack[] GetTracks()
        {
            return mediaStreamTrackList.ToArray();
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
            var cleaner = obj.GetComponent<Cleaner>();
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

    internal class CameraCapturerTextures
    {
        internal RenderTexture camRenderTexture;
        internal List<RenderTexture> webRTCTextures = new List<RenderTexture>();
    }

    public static class CameraExtension
    {
        internal static Dictionary<Camera, CameraCapturerTextures> camCapturerTexturesDict = new Dictionary<Camera, CameraCapturerTextures>();

        public static int GetStreamTextureCount(this Camera cam)
        {
            CameraCapturerTextures textures;
            if (camCapturerTexturesDict.TryGetValue(cam, out textures))
            {
                return textures.webRTCTextures.Count;
            }
            return 0;
        }

        public static RenderTexture GetStreamTexture(this Camera cam, int index)
        {
            CameraCapturerTextures textures;
            if (camCapturerTexturesDict.TryGetValue(cam, out textures))
            {
                if (index >= 0 && index < textures.webRTCTextures.Count)
                {
                    return textures.webRTCTextures[index];
                }
            }
            return null;
        }

        public static void CreateRenderStreamTexture(this Camera cam, int width, int height, int count = 1)
        {
            var cameraCapturerTextures = new CameraCapturerTextures();
            camCapturerTexturesDict.Add(cam, cameraCapturerTextures);

            cameraCapturerTextures.camRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.BGRA32);
            cameraCapturerTextures.camRenderTexture.Create();

            int mipCount = count;
            for (int i = 1, mipLevel = 1; i <= mipCount; ++i, mipLevel *= 4)
            {
                var webRtcTex = new RenderTexture(width / mipLevel, height / mipLevel, 0, RenderTextureFormat.BGRA32);
                webRtcTex.Create();
                cameraCapturerTextures.webRTCTextures.Add(webRtcTex);
            }

            cam.targetTexture = cameraCapturerTextures.camRenderTexture;
            cam.gameObject.AddCleanerCallback(() =>
            {
                cameraCapturerTextures.camRenderTexture.Release();
                UnityEngine.Object.Destroy(cameraCapturerTextures.camRenderTexture);

                foreach (var v in cameraCapturerTextures.webRTCTextures)
                {
                    v.Release();
                    UnityEngine.Object.Destroy(v);
                }
                cameraCapturerTextures.webRTCTextures.Clear();
            });
        }
    }

    public static class Audio
    {
        private static bool started = false;
        private static AudioInput audioInput = new AudioInput();

        public static void Update()
        {
            if (started)
            {
                audioInput.UpdateAudio();
            }
        }

        public static void Start()
        {
            audioInput.BeginRecording();
            started = true;
        }

        public static void Stop()
        {
            if (started)
            {
                AudioRenderer.Stop();
                started = false;
            }
        }
    }
    public class AudioInput
    {
        private ushort channelCount;
        private NativeArray<float> buffer;

        public void BeginRecording()
        {

            switch (AudioSettings.speakerMode)
            {
                case AudioSpeakerMode.Mono:
                    channelCount = 1;
                    break;
                case AudioSpeakerMode.Stereo:
                    channelCount = 2;
                    break;
                case AudioSpeakerMode.Quad:
                    channelCount = 4;
                    break;
                case AudioSpeakerMode.Surround:
                    channelCount = 5;
                    break;
                case AudioSpeakerMode.Mode5point1:
                    channelCount = 6;
                    break;
                case AudioSpeakerMode.Mode7point1:
                    channelCount = 7;
                    break;
                case AudioSpeakerMode.Prologic:
                    channelCount = 2;
                    break;
                default:
                    channelCount = 1;
                    break;
            }
            AudioRenderer.Start();
        }

        public void UpdateAudio()
        {
            var sampleCountFrame = AudioRenderer.GetSampleCountForCaptureFrame();
            //process Stereo mode only(Prologic mode also have 2 channel)
            if (AudioSettings.speakerMode == AudioSpeakerMode.Stereo)
            {
                buffer = new NativeArray<float>((int)sampleCountFrame * (int)channelCount, Allocator.Temp);
                AudioRenderer.Render(buffer);
                float[] audioData = buffer.ToArray();
                NativeMethods.ProcessAudio(audioData, audioData.Length);
                buffer.Dispose();
            }
        }
    }
}
