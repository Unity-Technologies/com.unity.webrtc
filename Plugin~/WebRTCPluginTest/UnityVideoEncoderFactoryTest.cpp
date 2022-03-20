#include "pch.h"

#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"
#include "UnityVideoEncoderFactory.h"

namespace unity
{
namespace webrtc
{
    class UnityVideoEncoderFactoryTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        UnityVideoEncoderFactoryTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
        }
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(UnityVideoEncoderFactoryTest, GetSupportedFormats)
    {
        auto factory = std::make_unique<UnityVideoEncoderFactory>(container_->device(), nullptr);
        auto formats = factory->GetSupportedFormats();
        EXPECT_GT(formats.size(), 0);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, UnityVideoEncoderFactoryTest, testing::ValuesIn(supportedGfxDevices));
} // namespace webrtc
} // namespace unity
