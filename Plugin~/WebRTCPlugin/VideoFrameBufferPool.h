#pragma once

#include <common_video/include/video_frame_buffer.h>
#include <list>
#include <shared_mutex>
#include <system_wrappers/include/clock.h>

#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class IGraphicsDevice;
    class VideoFrameBufferPool
    {
    public:
        VideoFrameBufferPool(
            IGraphicsDevice* device,
            size_t maxNumberOfBuffers = std::numeric_limits<size_t>::max());
        VideoFrameBufferPool(const VideoFrameBufferPool&) = delete;
        VideoFrameBufferPool& operator=(const VideoFrameBufferPool&) = delete;

        virtual ~VideoFrameBufferPool();

        rtc::scoped_refptr<VideoFrameBuffer> Create(int width, int height, UnityRenderingExtTextureFormat format);

    private:
        IGraphicsDevice* device_;
        size_t maxNumberOfBuffers_;
        std::list<rtc::scoped_refptr<VideoFrameBuffer>> pool_;
    };
}
}
