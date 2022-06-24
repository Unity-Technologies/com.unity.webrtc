#include "pch.h"

#include <rtc_base/ref_counted_object.h>
#include <system_wrappers/include/clock.h>

#include "GpuMemoryBufferPool.h"

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferPool::GpuMemoryBufferPool(IGraphicsDevice* device, Clock* clock)
        : device_(device)
        , clock_(clock)
    {
    }

    GpuMemoryBufferPool::~GpuMemoryBufferPool() { }

    rtc::scoped_refptr<VideoFrame> GpuMemoryBufferPool::CreateFrame(
        NativeTexPtr ptr, const Size& size, UnityRenderingExtTextureFormat format, Timestamp timestamp)
    {
        auto buffer = GetOrCreateFrameResources(ptr, size, format);
        VideoFrame::ReturnBufferToPoolCallback callback =
            std::bind(&GpuMemoryBufferPool::OnReturnBuffer, this, std::placeholders::_1);

        return VideoFrame::WrapExternalGpuMemoryBuffer(
            size, buffer, callback, webrtc::TimeDelta::Micros(timestamp.us()));
    }

    rtc::scoped_refptr<GpuMemoryBufferInterface> GpuMemoryBufferPool::GetOrCreateFrameResources(
        NativeTexPtr ptr, const Size& size, UnityRenderingExtTextureFormat format)
    {
        auto it = resourcesPool_.begin();
        while (it != resourcesPool_.end())
        {
            FrameResources* resources = it->get();
            if (!resources->IsUsed() && AreFrameResourcesCompatible(resources, size, format))
            {
                resources->MarkUsed(clock_->CurrentTime());
                GpuMemoryBufferFromUnity* buffer = static_cast<GpuMemoryBufferFromUnity*>(resources->buffer_.get());
                buffer->ResetSync();
                buffer->CopyBuffer(ptr);
                return resources->buffer_;
            }
            it++;
        }
        rtc::scoped_refptr<GpuMemoryBufferFromUnity> buffer =
            new rtc::RefCountedObject<GpuMemoryBufferFromUnity>(device_, ptr, size, format);
        std::unique_ptr<FrameResources> resources = std::make_unique<FrameResources>(buffer);
        resources->MarkUsed(clock_->CurrentTime());
        resourcesPool_.push_back(std::move(resources));
        return std::move(buffer);
    }

    bool GpuMemoryBufferPool::AreFrameResourcesCompatible(
        const FrameResources* resources, const Size& size, UnityRenderingExtTextureFormat format)
    {
        return resources->buffer_->GetSize() == size && resources->buffer_->GetFormat() == format;
    }

    void GpuMemoryBufferPool::OnReturnBuffer(rtc::scoped_refptr<GpuMemoryBufferInterface> buffer)
    {
        GpuMemoryBufferInterface* ptr = buffer.get();
        auto result = std::find_if(
            resourcesPool_.begin(),
            resourcesPool_.end(),
            [ptr](std::unique_ptr<FrameResources>& x) { return x->buffer_.get() == ptr; });
        RTC_DCHECK(result != resourcesPool_.end());

        (*result)->MarkUnused(clock_->CurrentTime());
    }

    void GpuMemoryBufferPool::ReleaseStaleBuffers(Timestamp now)
    {
        auto it = resourcesPool_.begin();
        while (it != resourcesPool_.end())
        {
            FrameResources* resources = (*it).get();

            constexpr TimeDelta kStaleFrameLimit = TimeDelta::Seconds(10);
            if (!resources->IsUsed() && now - resources->lastUseTime() > kStaleFrameLimit)
            {
                resourcesPool_.erase(it++);
            }
            else
            {
                it++;
            }
        }
    }
}
}
