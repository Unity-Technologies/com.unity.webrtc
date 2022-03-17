#pragma once

#include "api/test/frame_generator_interface.h"

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

    class VideoFrameGenerator : public FrameGeneratorInterface
    {
    public:
        VideoFrameGenerator(IGraphicsDevice* device, int width, int height, OutputType type, int num_squares);

        void ChangeResolution(size_t width, size_t height) override;
        VideoFrameData NextFrame() override;

    private:
        IGraphicsDevice* device_;
        Mutex mutex_;
        int width_;
        int height_;
        //const OutputType type_;
    };

}
}
