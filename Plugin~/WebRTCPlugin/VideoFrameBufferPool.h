#pragma once

#include <common_video/include/video_frame_buffer.h>
#include <list>
#include <shared_mutex>
#include <system_wrappers/include/clock.h>

#include "GpuMemoryBuffer.h"
#include "Size.h"
#include "VideoFrame.h"

namespace unity
{
namespace webrtc
{
    enum VideoFrameBufferState
    {
        kUnknown = 0,
        kUnused = 1,
        kReserved = 2,
        kUsed = 3,
    };

    class IGraphicsDevice;
    class VideoFrameBufferPool
    {
    public:
        VideoFrameBufferPool(IGraphicsDevice* device, Clock* clock);
        VideoFrameBufferPool(const VideoFrameBufferPool&) = delete;
        VideoFrameBufferPool& operator=(const VideoFrameBufferPool&) = delete;

        virtual ~VideoFrameBufferPool();

        VideoFrameBuffer* Create(void* texture);
        bool Delete(const VideoFrameBuffer* buffer);
        bool Reserve(const VideoFrameBuffer* buffer);
        rtc::scoped_refptr<VideoFrameBuffer> Retain(const VideoFrameBuffer* buffer);
        VideoFrameBufferState GetState(const VideoFrameBuffer* buffer) const;

    private:
        IGraphicsDevice* device_;
        Clock* clock_;
        std::list<rtc::scoped_refptr<VideoFrameBuffer>> pool_;
        std::list<rtc::scoped_refptr<VideoFrameBuffer>> reservedPool_;
    };
}
}
