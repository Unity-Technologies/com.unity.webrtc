#pragma once

#include <list>
#include <system_wrappers/include/clock.h>

#include "GpuMemoryBuffer.h"
#include "Size.h"
#include "VideoFrame.h"

namespace unity
{
namespace webrtc
{
    class GpuMemoryBufferPool
    {
    public:
        GpuMemoryBufferPool(IGraphicsDevice* device, Clock* clock);
        GpuMemoryBufferPool(const GpuMemoryBufferPool&) = delete;
        GpuMemoryBufferPool& operator=(const GpuMemoryBufferPool&) = delete;

        virtual ~GpuMemoryBufferPool();

        rtc::scoped_refptr<VideoFrame>
        CreateFrame(NativeTexPtr ptr, const Size& size, UnityRenderingExtTextureFormat format, Timestamp timestamp);
        void ReleaseStaleBuffers(Timestamp timestamp, TimeDelta timeLimit);

        size_t bufferCount() { return resourcesPool_.size(); }

    private:
        struct FrameResources
        {
            FrameResources(rtc::scoped_refptr<GpuMemoryBufferInterface> buffer)
                : buffer_(std::move(buffer))
                , lastUsetime_(Timestamp::Zero())
            {
            }
            rtc::scoped_refptr<GpuMemoryBufferInterface> buffer_;
            bool IsUsed() { return isUsed_; }
            void MarkUsed(Timestamp timestamp)
            {
                isUsed_ = true;
                lastUsetime_ = timestamp;
            }
            void MarkUnused(Timestamp timestamp)
            {
                isUsed_ = false;
                lastUsetime_ = timestamp;
            }
            Timestamp lastUseTime() { return lastUsetime_; }
            bool isUsed_;
            Timestamp lastUsetime_;
        };
        rtc::scoped_refptr<GpuMemoryBufferInterface>
        GetOrCreateFrameResources(NativeTexPtr ptr, const Size& size, UnityRenderingExtTextureFormat format);
        void OnReturnBuffer(rtc::scoped_refptr<GpuMemoryBufferInterface> buffer);

        static bool AreFrameResourcesCompatible(
            const FrameResources* resources, const Size& size, UnityRenderingExtTextureFormat format);

        IGraphicsDevice* device_;
        std::mutex mutex_;
        std::list<std::unique_ptr<FrameResources>> resourcesPool_;
        Clock* const clock_;
    };
}
}
