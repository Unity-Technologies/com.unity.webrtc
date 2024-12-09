using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Unity.WebRTC
{
    public static class CameraExtension
    {
        /// <summary>
        /// Create an instance of <see cref="VideoStreamTrack"/> to stream a camera.
        /// </summary>
        /// <remarks>
        /// You should keep a reference of <see cref="VideoStreamTrack"/>, created by this method.
        /// This instance is collected by GC automatically if you don't own a reference.
        /// </remarks>
        /// <param name="cam"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static VideoStreamTrack CaptureStreamTrack(this Camera cam, int width, int height,
            RenderTextureDepth depth = RenderTextureDepth.Depth24, CopyTexture textureCopy = null)
        {
            switch (depth)
            {
                case RenderTextureDepth.Depth16:
                case RenderTextureDepth.Depth24:
                case RenderTextureDepth.Depth32:
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(depth), (int)depth, typeof(RenderTextureDepth));
            }

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("width and height are should be greater than zero.");
            }

            int depthValue = (int)depth;
            var format = WebRTC.GetSupportedRenderTextureFormat(SystemInfo.graphicsDeviceType);
            var rt = new UnityEngine.RenderTexture(width, height, depthValue, format);
            rt.Create();
            cam.targetTexture = rt;
            return new VideoStreamTrack(rt, textureCopy);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static MediaStream CaptureStream(this Camera cam, int width, int height,
            RenderTextureDepth depth = RenderTextureDepth.Depth24)
        {
            var stream = new MediaStream();
            var track = cam.CaptureStreamTrack(width, height, depth);
            stream.AddTrack(track);
            return stream;
        }
    }
}
