#pragma once

#include <queue>

#include "api/test/frame_generator_interface.h"
#include "rtc_base/synchronization/mutex.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;
    using namespace ::webrtc::test;

    class IGraphicsDevice;

    std::unique_ptr<FrameGeneratorInterface> CreateVideoFrameGenerator(
        IGraphicsDevice* device,
        int width,
        int height,
        absl::optional<FrameGeneratorInterface::OutputType> type,
        absl::optional<int> numFrames);

    class ITexture2D;
    class VideoFrameGenerator : public FrameGeneratorInterface
    {
    public:
        VideoFrameGenerator(IGraphicsDevice* device, int width, int height, OutputType type, int num_squares);

        void ChangeResolution(size_t width, size_t height) override;
        FrameGeneratorInterface::Resolution GetResolution() const override;
        VideoFrameData NextFrame() override;

    private:
        IGraphicsDevice* device_;
        Mutex mutex_;
        int width_;
        int height_;
        std::queue<std::unique_ptr<ITexture2D>> queue_;
    };

}
}
