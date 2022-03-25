#include "pch.h"

#include "GraphicsDeviceContainer.h"
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
        {
        }

    protected:
        std::unique_ptr<GraphicsDeviceContainer> container_;
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
