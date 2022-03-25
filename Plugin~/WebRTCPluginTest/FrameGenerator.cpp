#include "pch.h"

#include "FrameGenerator.h"
#include "UnityVideoTrackSource.h"
#include "VideoFrameUtil.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{
    std::unique_ptr<FrameGeneratorInterface> CreateVideoFrameGenerator(
        IGraphicsDevice* device,
        int width,
        int height,
        absl::optional<FrameGeneratorInterface::OutputType> type,
        absl::optional<int> num_squares)
    {
        return std::make_unique<VideoFrameGenerator>(
            device, width, height, type.value_or(FrameGeneratorInterface::OutputType::kI420), num_squares.value_or(10));
    }

    VideoFrameGenerator::VideoFrameGenerator(
        IGraphicsDevice* device, int width, int height, OutputType type, int num_squares)
        : device_(device)
    {

        ChangeResolution(width, height);
        for (int i = 0; i < num_squares; ++i)
        {
            // squares_.emplace_back(new Square(width, height, i + 1));
        }
    }

    void VideoFrameGenerator::ChangeResolution(size_t width, size_t height)
    {
        MutexLock lock(&mutex_);
        width_ = static_cast<int>(width);
        height_ = static_cast<int>(height);
        RTC_CHECK(width_ > 0);
        RTC_CHECK(height_ > 0);
    }

    FrameGeneratorInterface::VideoFrameData VideoFrameGenerator::NextFrame()
    {
        MutexLock lock(&mutex_);

        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;

        ITexture2D* texture = device_->CreateCPUReadTextureV(width_, height_, kFormat);
        queue_.push(std::unique_ptr<ITexture2D>(texture));
        rtc::scoped_refptr<VideoFrame> frame = CreateTestFrame(device_, texture, kFormat);
        
        ::webrtc::VideoFrame videoFrame = VideoFrameAdapter::CreateVideoFrame(frame);
        rtc::scoped_refptr<VideoFrameBuffer> buffer = videoFrame.video_frame_buffer();
        return VideoFrameData(buffer, absl::nullopt);
    }
}
}
