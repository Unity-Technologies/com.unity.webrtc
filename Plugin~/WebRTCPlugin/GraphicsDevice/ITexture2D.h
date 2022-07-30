#pragma once

#include <stdint.h>

namespace unity
{
namespace webrtc
{

    class ITexture2D
    {
    public:
        //[TODO-Sin: 2019-19-11] ITexture2D should not be created directly, but should be called using
        // GraphicsDevice->CreateDefaultTexture
        ITexture2D(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat format)
            : m_width(width)
            , m_height(height)
            , m_format(format)
        {
        }
        bool IsSize(uint32_t width, uint32_t height) const { return m_width == width && m_height == height; }

        virtual ~ITexture2D() = default;

        /// <summary>
        /// Get the pointer taken from Unity
        /// </summary>
        virtual void* GetNativeTexturePtrV() = 0;
        /// <summary>
        /// Get the pointer taken from Unity
        /// </summary>
        virtual const void* GetNativeTexturePtrV() const = 0;

        virtual void* GetEncodeTexturePtrV() = 0;
        virtual const void* GetEncodeTexturePtrV() const = 0;

        uint32_t GetWidth() const { return m_width; }
        uint32_t GetHeight() const { return m_height; }
        UnityRenderingExtTextureFormat GetFormat() const { return m_format; }
    protected:
        uint32_t m_width;
        uint32_t m_height;
        UnityRenderingExtTextureFormat m_format;
    };
} // end namespace webrtc
} // end namespace unity
