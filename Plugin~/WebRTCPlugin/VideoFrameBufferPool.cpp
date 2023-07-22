#include "pch.h"

#if UNITY_OSX
#import <sdk/objc/native/src/objc_frame_buffer.h>
#endif

#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "PlatformBase.h"
#include "VideoFrameBufferPool.h"
#include "NativeFrameBuffer.h"


namespace unity
{
namespace webrtc
{
    bool HasOneRef(const rtc::scoped_refptr<VideoFrameBuffer>& buffer)
    {
        // Cast to rtc::RefCountedObject is safe because this function is only called
        // on locally created VideoFrameBuffers, which are either
        // `rtc::RefCountedObject<I420Buffer>`, `rtc::RefCountedObject<I444Buffer>` or
        // `rtc::RefCountedObject<NV12Buffer>`.
        switch (buffer->type())
        {
        case VideoFrameBuffer::Type::kNative:
        {
#if UNITY_OSX
            return static_cast<rtc::RefCountedObject<ObjCFrameBuffer>*>(buffer.get())->HasOneRef();
#else
            return static_cast<rtc::RefCountedObject<NativeFrameBuffer>*>(buffer.get())->HasOneRef();
#endif
        }
        default:
            RTC_CHECK_NOTREACHED();
        }
        return false;
    }

    VideoFrameBufferPool::VideoFrameBufferPool(IGraphicsDevice* device, size_t maxNumberOfBuffers)
        : device_(device)
        , maxNumberOfBuffers_(maxNumberOfBuffers)
    {
    }

    VideoFrameBufferPool::~VideoFrameBufferPool() {}
    
    rtc::scoped_refptr<VideoFrameBuffer>
    VideoFrameBufferPool::Create(int width, int height, UnityRenderingExtTextureFormat format)
    {
        // Release buffers with wrong resolution or different type.
        for (auto it = pool_.begin(); it != pool_.end();)
        {
            const auto& buffer = *it;
            if (buffer->width() != width || buffer->height() != height ||
                buffer->type() != VideoFrameBuffer::Type::kNative)
            {
                it = pool_.erase(it);
            }
            else
            {
                ++it;
            }
        }

        for (const rtc::scoped_refptr<VideoFrameBuffer>& buffer : pool_)
        {
            if(HasOneRef(buffer))
            {
                return buffer;
            }

        }
        if (pool_.size() >= maxNumberOfBuffers_)
            return nullptr;

        auto buffer = device_->CreateVideoFrameBuffer(width, height, format);
        pool_.emplace_back(buffer);
        return buffer;
    }
}
}
