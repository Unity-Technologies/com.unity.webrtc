#include "pch.h"

#include "GpuMemoryBufferPool.h"
#include "media/base/video_common.h"
#include "rtc_base/ref_counted_object.h"

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferPool::GpuMemoryBufferPool(IGraphicsDevice* device)
        : device_(device)
    {
    }

    GpuMemoryBufferPool::~GpuMemoryBufferPool() { }

    rtc::scoped_refptr<VideoFrame> GpuMemoryBufferPool::CreateFrame(
        NativeTexPtr ptr,
        const Size& size,
        UnityRenderingExtTextureFormat format,
        int64_t timestamp)
    {
        auto buffer = GetOrCreateFrameResources(ptr, size, format);
        VideoFrame::ReturnBufferToPoolCallback callback =
            std::bind(&GpuMemoryBufferPool::OnReturnBuffer, this, std::placeholders::_1);

        return VideoFrame::WrapExternalGpuMemoryBuffer(
            size, buffer, callback, webrtc::TimeDelta::Micros(timestamp));
    }

    rtc::scoped_refptr<GpuMemoryBufferInterface> GpuMemoryBufferPool::GetOrCreateFrameResources(
        NativeTexPtr ptr, const Size& size, UnityRenderingExtTextureFormat format)
    {
        auto it = resourcesPool_.begin();
        while (it != resourcesPool_.end())
        {
            FrameReources* resources = it->get();
            if (!resources->IsUsed() && AreFrameResourcesCompatible(resources, size, format))
            {
                resources->MarkUsed();
                // copy texture
                static_cast<GpuMemoryBufferFromUnity*>(resources->buffer_.get())->CopyBuffer(ptr);
                return resources->buffer_;
            }
            else
            {
                it++;
            }
        }

        rtc::scoped_refptr<GpuMemoryBufferInterface> buffer =
            new rtc::RefCountedObject<GpuMemoryBufferFromUnity>(device_, ptr, size, format);
        std::unique_ptr<FrameReources> resources = std::make_unique<FrameReources>(buffer);
        resourcesPool_.push_back(std::move(resources));
        return buffer;
    }

    bool GpuMemoryBufferPool::AreFrameResourcesCompatible(
        const FrameReources* resources, const Size& size, UnityRenderingExtTextureFormat format)
    {
        return resources->buffer_->GetSize() == size && resources->buffer_->GetFormat() == format;
    }

    void GpuMemoryBufferPool::OnReturnBuffer(rtc::scoped_refptr<GpuMemoryBufferInterface> buffer)
    {
        GpuMemoryBufferInterface* ptr = buffer.release();
        auto result = std::find_if(
            resourcesPool_.begin(),
            resourcesPool_.end(),
            [ptr](std::unique_ptr<FrameReources>& x) { return x->buffer_.get() == ptr; });
        RTC_DCHECK(result != resourcesPool_.end());

        (*result)->MarkUnused();
    }

}
}
