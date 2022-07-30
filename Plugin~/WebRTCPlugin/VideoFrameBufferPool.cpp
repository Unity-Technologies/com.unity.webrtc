#include "pch.h"

#include "GraphicsDevice/ITexture2D.h"
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

    VideoFrameBufferPool::VideoFrameBufferPool(IGraphicsDevice* device, Clock* clock)
        : device_(device)
        , clock_(clock)
    {
    }
    VideoFrameBufferPool::~VideoFrameBufferPool() { RTC_DCHECK_EQ(pool_.size(), 0); }
    rtc::scoped_refptr<VideoFrameBuffer>
    VideoFrameBufferPool::Create(int width, int height, UnityRenderingExtTextureFormat format)
    {
        auto result = std::find_if(
            pool_.begin(),
            pool_.end(),
            [width, height, format](const rtc::scoped_refptr<VideoFrameBuffer>& x)
            {
                return x->width() == width && x->height() == height && x->type() == VideoFrameBuffer::Type::kNative;
                // todo(kazuki):: check format && x->format() == format;
            });
        if (result != pool_.end())
            return *result;

        auto buffer = NativeFrameBuffer::Create(width, height, format, device_);
        auto ptr = buffer.get();
        pool_.emplace_back(std::move(buffer));
        return ptr;
    }
}
}
