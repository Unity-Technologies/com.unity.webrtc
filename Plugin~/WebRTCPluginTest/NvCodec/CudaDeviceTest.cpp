#include "pch.h"

#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceTestBase.h"
#include "GraphicsDeviceContainer.h"

namespace unity
{
namespace webrtc
{

    class CudaDeviceTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        CudaDeviceTest()
        {
            container_ = CreateGraphicsDeviceContainer(GetParam());
        }

        void SetUp() override
        {
            if(!container_->device()->IsCudaSupport())
                GTEST_SKIP() << "CUDA is not supported on this device.";
        }
    protected:
        std::unique_ptr<GraphicsDeviceContainer> container_;
    };

    TEST_P(CudaDeviceTest, GetCUcontext) { EXPECT_NE(container_->device()->GetCUcontext(), nullptr); }

    TEST_P(CudaDeviceTest, IsCudaSupport) { EXPECT_TRUE(container_->device()->IsCudaSupport()); }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, CudaDeviceTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
