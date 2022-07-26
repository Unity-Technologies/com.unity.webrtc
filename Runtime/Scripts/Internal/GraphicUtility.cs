using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    enum VideoFrameBufferState
    {
        Unknown = 0,
        Unused = 1,
        Reserved = 2,
        Used = 3
    }

    class VideoFrameBuffer : SafeHandle
    {
        private VideoFrameBuffer()
        : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid { get { return handle == IntPtr.Zero; } }


        protected override bool ReleaseHandle()
        {
            return NativeMethods.VideoFrameBufferDelete(handle);
        }

        public bool Reserve()
        {
            return NativeMethods.VideoFrameBufferReserve(handle);
        }

        public VideoFrameBufferState State { get { return NativeMethods.VideoFrameBufferGetState(handle); } }

        public Texture texture
        {
            get => m_texture;
            set
            {
                if (m_texture != null)
                    throw new InvalidOperationException("Not allowed to set value.");
                m_texture = value;
            }
        }
        private Texture m_texture;
    }

    class VideoFrameBufferPool
    {
        List<VideoFrameBuffer> list_ = new List<VideoFrameBuffer>();

        public VideoFrameBuffer Create(int width, int height, GraphicsFormat format)
        {
            var buffer = FindUnusedBuffer(width, height, format);
            if (buffer != null)
                return buffer;

            var texture = GraphicUtility.CreateExternalTexture(width, height, format);
            buffer = texture.CreateVideoFrameBuffer();
            list_.Add(buffer);
            return buffer;
        }

        VideoFrameBuffer FindUnusedBuffer(int width, int height, GraphicsFormat format)
        {
            foreach(var buffer in list_)
            {
                if (buffer.texture.width == width &&
                    buffer.texture.height == height &&
                    buffer.texture.graphicsFormat == format &&
                    buffer.State == VideoFrameBufferState.Unused)
                {
                    return buffer;
                }
            }
            return null;
        }
    }

    internal static class GraphicUtility
    {
        static (TextureFormat, bool) ConvertFormat(GraphicsFormat format)
        {
            return (GraphicsFormatUtility.GetTextureFormat(format),
                !GraphicsFormatUtility.IsSRGBFormat(format));
        }

        public static Texture CreateExternalTexture(int width, int height, GraphicsFormat format)
        {
            IntPtr ptr = NativeMethods.CreateExternalTexture(width, height, format);
            (TextureFormat textureFormat, bool linear) = ConvertFormat(format);
            return Texture2D.CreateExternalTexture(width, height, textureFormat, false, linear, ptr);
        }

        public static VideoFrameBuffer CreateVideoFrameBuffer(this Texture texture)
        {
            VideoFrameBuffer buffer = NativeMethods.ExternalTextureVideoFrameBufferCreate(texture.GetNativeTexturePtr());
            buffer.texture = texture;
            return buffer;
        }
    }
}
