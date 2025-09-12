using System;
using System.ComponentModel;
using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    /// Provides extension methods for <see cref="Camera"/> objects to facilitate video streaming functionalities.
    /// </summary>
    public static class CameraExtension
    {
        /// <summary>
        /// Creates an instance of <see cref="VideoStreamTrack"/> for streaming video from a <see cref="Camera"/> object.
        /// </summary>
        /// <remarks>
        /// It is recommended to maintain a reference to the <see cref="VideoStreamTrack"/> instance created by this method.
        /// Without a reference, the instance may be collected by the garbage collector automatically.
        /// </remarks>
        /// <param name="cam">The camera from which to capture video frames</param>
        /// <param name="width">The desired width of the video stream, in pixels. Must be greater than zero</param>
        /// <param name="height">The desired height of the video stream, in pixels. Must be greater than zero</param>
        /// <param name="depth">The depth buffer format for the render texture. Default is <see cref="RenderTextureDepth.Depth24"/></param>
        /// <param name="textureCopy">An optional <see cref="CopyTexture"/> to facilitate texture copying. Default is null</param>
        /// <returns>A <see cref="VideoStreamTrack"/> instance that can be used to stream video.</returns>
        /// <example>
        ///     <para>Creates a GameObject with a Camera component and a VideoStreamTrack capturing video from the camera.</para>
        ///     <code lang="cs"><![CDATA[
        ///         private void AddVideoObject()
        ///         {
        ///             Camera newCam = new GameObject("Camera").AddComponent<Camera>();
        ///             RawImage newSource = new GameObject("SourceImage").AddComponent<RawImage>();
        ///             try
        ///             {
        ///                 videoStreamTrackList.Add(newCam.CaptureStreamTrack(WebRTCSettings.StreamSize.x, WebRTCSettings.StreamSize.y));
        ///                 newSource.texture = newCam.targetTexture;
        ///             }
        ///             catch (Exception e)
        ///             {
        ///                 Debug.LogError(e.Message);
        ///             }
        ///         }
        ///     ]]></code>
        /// </example>
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
        /// Creates an instance of <see cref="MediaStream"/> capturing video from a <see cref="Camera"/> object.
        /// </summary>
        /// <remarks>
        /// It is recommended to maintain a reference to the <see cref="MediaStream"/> instance created by this method.
        /// Without a reference, the instance may be collected by the garbage collector automatically.
        /// </remarks>
        /// <param name="cam">The camera from which to capture video frames</param>
        /// <param name="width">The desired width of the video stream, in pixels. Must be greater than zero</param>
        /// <param name="height">The desired height of the video stream, in pixels. Must be greater than zero</param>
        /// <param name="depth">The depth buffer format for the render texture. Default is <see cref="RenderTextureDepth.Depth24"/></param>
        /// <returns>A <see cref="MediaStream"/> containing the video track captured from the camera.</returns>
        /// <example>
        ///     <para>Creates a MediaStream with a VideoStreamTrack capturing video from the camera.</para>
        ///     <code lang="cs"><![CDATA[
        ///         private static MediaStream CreateMediaStream()
        ///         {
        ///             MediaStream videoStream = Camera.main.CaptureStream(1280, 720);
        ///             return videoStream;
        ///         }
        ///     ]]></code>
        /// </example>

        public static MediaStream CaptureStream(this Camera cam, int width, int height, RenderTextureDepth depth = RenderTextureDepth.Depth24)
        {
            var stream = new MediaStream();
            var track = cam.CaptureStreamTrack(width, height, depth);
            stream.AddTrack(track);
            return stream;
        }
    }
}
