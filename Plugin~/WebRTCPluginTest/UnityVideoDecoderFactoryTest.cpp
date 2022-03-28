#include "pch.h"

#include "GraphicsDeviceContainer.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "UnityVideoDecoderFactory.h"

namespace unity
{
namespace webrtc
{
    class UnityVideoDecoderFactoryTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        UnityVideoDecoderFactoryTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            if (!device_->IsCudaSupport())
                GTEST_SKIP() << "CUDA is not supported on this device.";
        }
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(UnityVideoDecoderFactoryTest, GetSupportedFormats)
    {
        auto factory = std::make_unique<UnityVideoDecoderFactory>(container_->device());
        EXPECT_NE(factory, nullptr);
        auto formats = factory->GetSupportedFormats();
        EXPECT_GT(formats.size(), 0);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, UnityVideoDecoderFactoryTest, testing::ValuesIn(supportedGfxDevices));
} // namespace webrtc
} // namespace unity
