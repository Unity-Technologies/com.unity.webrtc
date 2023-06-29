#include "pch.h"

#include <api/make_ref_counted.h>
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
        if (!buffer)
            return nullptr;
        VideoFrame::ReturnBufferToPoolCallback callback =
            std::bind(&GpuMemoryBufferPool::OnReturnBuffer, this, std::placeholders::_1);

        return VideoFrame::WrapExternalGpuMemoryBuffer(
            size, buffer, callback, webrtc::TimeDelta::Micros(timestamp.us()));
    }

    rtc::scoped_refptr<GpuMemoryBufferInterface> GpuMemoryBufferPool::GetOrCreateFrameResources(
        NativeTexPtr ptr, const Size& size, UnityRenderingExtTextureFormat format)
    {
        std::lock_guard<std::mutex> lock(mutex_);

        for (auto it = resourcesPool_.begin(); it != resourcesPool_.end(); ++it)
        {
            FrameResources* resources = it->get();
            if (!resources->IsUsed() && AreFrameResourcesCompatible(resources, size, format))
            {
                GpuMemoryBufferFromUnity* buffer = static_cast<GpuMemoryBufferFromUnity*>(resources->buffer_.get());
                if (!buffer->ResetSync())
                {
                    RTC_LOG(LS_INFO) << "It has not signaled yet";
                    continue;
                }
                if (!buffer->CopyBuffer(ptr))
                {
                    RTC_LOG(LS_INFO) << "Copy buffer is failed.";
                    continue;
                }
                resources->MarkUsed(clock_->CurrentTime());
                return resources->buffer_;
            }
        }
        rtc::scoped_refptr<GpuMemoryBufferFromUnity> buffer =
            rtc::make_ref_counted<GpuMemoryBufferFromUnity>(device_, size, format);
        if (!buffer->CopyBuffer(ptr))
        {
            RTC_LOG(LS_INFO) << "Copy buffer is failed.";
            return nullptr;
        }
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
        std::lock_guard<std::mutex> lock(mutex_);

        GpuMemoryBufferInterface* ptr = buffer.get();
        if (!ptr)
        {
            RTC_LOG(LS_INFO) << "buffer is nullptr.";
            return;
        }

        auto result =
            std::find_if(resourcesPool_.begin(), resourcesPool_.end(), [ptr](std::unique_ptr<FrameResources>& x) {
                return x->buffer_.get() == ptr;
            });
        RTC_DCHECK(result != resourcesPool_.end());

        (*result)->MarkUnused(clock_->CurrentTime());
    }

    void GpuMemoryBufferPool::ReleaseStaleBuffers(Timestamp now, TimeDelta timeLimit)
    {
        std::lock_guard<std::mutex> lock(mutex_);

        auto it = resourcesPool_.begin();
        for (; it != resourcesPool_.end();)
        {
            FrameResources* resources = (*it).get();

            if (!resources->IsUsed() && now - resources->lastUseTime() > timeLimit)
            {
                it = resourcesPool_.erase(it);
            }
            else
            {
                ++it;
            }
        }
    }
}
}
