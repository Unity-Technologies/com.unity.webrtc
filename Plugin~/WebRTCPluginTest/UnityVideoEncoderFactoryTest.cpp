#include "pch.h"

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
        {
        }

    protected:
        std::unique_ptr<GraphicsDeviceContainer> container_;
    };

    TEST_P(UnityVideoEncoderFactoryTest, GetSupportedFormats)
    {
        auto factory = std::make_unique<UnityVideoEncoderFactory>(container_->device());
        auto formats = factory->GetSupportedFormats();
        EXPECT_GT(formats.size(), 0);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, UnityVideoEncoderFactoryTest, testing::ValuesIn(supportedGfxDevices));
} // namespace webrtc
} // namespace unity
