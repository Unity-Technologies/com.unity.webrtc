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
        ITexture2D(uint32_t w, uint32_t h)
            : m_width(w)
            , m_height(h)
        {
        }
        bool IsSize(uint32_t w, uint32_t h) const { return m_width == w && m_height == h; }

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

    protected:
        uint32_t m_width;
        uint32_t m_height;
    };
} // end namespace webrtc
} // end namespace unity
