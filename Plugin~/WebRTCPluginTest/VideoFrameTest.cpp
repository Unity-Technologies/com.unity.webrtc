#include "pch.h"

#include "VideoFrame.h"
#include "GpuMemoryBuffer.h"
#include "VideoFrameUtil.h"

#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceContainer.h"

//#include "api/utils/timedelta.h"

namespace unity
{
namespace webrtc
{
    class VideoFrameTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        explicit VideoFrameTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
        {
        }
    protected:
        std::unique_ptr<GraphicsDeviceContainer> container_;
    };

    TEST_P(VideoFrameTest, WrapExternalGpuMemoryBuffer)
    {
        const Size kSize(256, 256);
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
        std::unique_ptr<ITexture2D> tex = std::unique_ptr<ITexture2D>(
            container_->device()->CreateDefaultTextureV(kSize.width(), kSize.height(), kFormat));
        void* ptr = tex->GetNativeTexturePtrV();
        rtc::scoped_refptr<GpuMemoryBufferFromUnity> buffer =
            new rtc::RefCountedObject<GpuMemoryBufferFromUnity>(container_->device(), ptr, kSize, kFormat);
        //Timestamp timestamp = Clock::GetRealTimeClock()->CurrentTime();
        VideoFrame::WrapExternalGpuMemoryBuffer(kSize, buffer, nullptr, TimeDelta::PlusInfinity());
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, VideoFrameTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
