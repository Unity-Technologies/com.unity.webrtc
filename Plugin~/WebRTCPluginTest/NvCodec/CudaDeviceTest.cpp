#include "pch.h"

#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceTestBase.h"

namespace unity
{
namespace webrtc
{

    class CudaDeviceTest : public GraphicsDeviceTestBase
    {
    };

    TEST_P(CudaDeviceTest, GetCUcontext) { EXPECT_NE(device()->GetCUcontext(), nullptr); }

    TEST_P(CudaDeviceTest, IsNvSupported) { EXPECT_TRUE(device()->IsCudaSupport()); }

    INSTANTIATE_TEST_SUITE_P(GfxDeviceAndColorSpece, CudaDeviceTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
