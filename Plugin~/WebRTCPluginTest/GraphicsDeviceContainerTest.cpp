#include "pch.h"

#include "GraphicsDeviceContainer.h"

namespace unity
{
namespace webrtc
{
    class GraphicsDeviceContainerTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    };

    TEST_P(GraphicsDeviceContainerTest, Instantiate)
    {
        auto container = CreateGraphicsDeviceContainer(GetParam());
        EXPECT_NE(container, nullptr);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, GraphicsDeviceContainerTest, testing::ValuesIn(supportedGfxDevices));
}
}
