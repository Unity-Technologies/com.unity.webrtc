#include "pch.h"

#include "GraphicsDevice/ITexture2D.h"
#include "PlatformBase.h"
#include "VideoFrameBufferPool.h"

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

    VideoFrameBufferPool::VideoFrameBufferPool(IGraphicsDevice* device, Clock* clock)
        : device_(device)
        , clock_(clock)
    {
    }
    VideoFrameBufferPool::~VideoFrameBufferPool() { RTC_DCHECK_EQ(pool_.size(), 0); }

    VideoFrameBuffer* VideoFrameBufferPool::Create(void* texture)
    {
        auto buffer = NativeFrameBuffer::Create(texture, device_);
        auto ptr = buffer.get();
        pool_.emplace_back(std::move(buffer));
        return ptr;
    }
    bool VideoFrameBufferPool::Delete(const VideoFrameBuffer* buffer)
    {
        auto result = std::find_if(
            pool_.begin(),
            pool_.end(),
            [buffer](rtc::scoped_refptr<VideoFrameBuffer>& x) { return x.get() == buffer; });

        // not found.
        if (result == pool_.end())
            return false;

        pool_.erase(result);
        return true;
    }

    bool VideoFrameBufferPool::Reserve(const VideoFrameBuffer* buffer)
    {
        auto result = std::find_if(
            pool_.begin(),
            pool_.end(),
            [buffer](const rtc::scoped_refptr<VideoFrameBuffer>& x) { return x.get() == buffer; });
        if (result == pool_.end())
            return false;

        auto result2 = std::find_if(
            reservedPool_.begin(),
            reservedPool_.end(),
            [buffer](const rtc::scoped_refptr<VideoFrameBuffer>& x) { return x.get() == buffer; });

        // already reserved.
        if (result2 != reservedPool_.end())
            return false;

        reservedPool_.emplace_back(*result);
        return true;
    }

    rtc::scoped_refptr<VideoFrameBuffer> VideoFrameBufferPool::Retain(const VideoFrameBuffer* buffer)
    {
        auto result = std::find_if(
            reservedPool_.begin(),
            reservedPool_.end(),
            [buffer](const rtc::scoped_refptr<VideoFrameBuffer>& x) { return x.get() == buffer; });

        // not found.
        if (result == reservedPool_.end())
            return nullptr;

        auto ret = *result;
        reservedPool_.erase(result);
        return ret;
    }
    VideoFrameBufferState VideoFrameBufferPool::GetState(const VideoFrameBuffer* buffer) const
    {
        auto result = std::find_if(
            pool_.begin(),
            pool_.end(),
            [buffer](const rtc::scoped_refptr<VideoFrameBuffer>& x) { return x.get() == buffer; });
        if (result == pool_.end())
            return kUnknown;
        if (HasOneRef(*result))
            return kUnused;

        result = std::find_if(
            reservedPool_.begin(),
            reservedPool_.end(),
            [buffer](const rtc::scoped_refptr<VideoFrameBuffer>& x) { return x.get() == buffer; });
        if (result == reservedPool_.end())
            return kUsed;
        return kReserved;
    }

}
}
