#include "pch.h"

#include "FrameGenerator.h"
#include "GraphicsDevice/ITexture2D.h"
#include "UnityVideoTrackSource.h"
#include "VideoFrameAdapter.h"
#include "VideoFrameUtil.h"

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
        , width_(width)
        , height_(height)
    {
    }

    void VideoFrameGenerator::ChangeResolution(size_t width, size_t height)
    {
        MutexLock lock(&mutex_);
        width_ = static_cast<int>(width);
        height_ = static_cast<int>(height);
        RTC_CHECK(width_ > 0);
        RTC_CHECK(height_ > 0);
    }

    FrameGeneratorInterface::Resolution VideoFrameGenerator::GetResolution() const
    {
        return { static_cast<size_t>(width_), static_cast<size_t>(height_) };
    }

    FrameGeneratorInterface::VideoFrameData VideoFrameGenerator::NextFrame()
    {
        MutexLock lock(&mutex_);

        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;

        ITexture2D* texture =
            device_->CreateDefaultTextureV(static_cast<uint32_t>(width_), static_cast<uint32_t>(height_), kFormat);

        queue_.push(std::unique_ptr<ITexture2D>(texture));
        rtc::scoped_refptr<VideoFrame> frame = CreateTestFrame(device_, texture, kFormat);
        EXPECT_TRUE(device_->WaitIdleForTest());

        ::webrtc::VideoFrame videoFrame = VideoFrameAdapter::CreateVideoFrame(frame);
        rtc::scoped_refptr<VideoFrameBuffer> buffer = videoFrame.video_frame_buffer();
        return VideoFrameData(buffer, absl::nullopt);
    }
}
}
