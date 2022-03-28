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
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            if(!device_->IsCudaSupport())
                GTEST_SKIP() << "CUDA is not supported on this device.";
        }

        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(CudaDeviceTest, GetCUcontext) { EXPECT_NE(device_->GetCUcontext(), nullptr); }

    TEST_P(CudaDeviceTest, IsCudaSupport) { EXPECT_TRUE(device_->IsCudaSupport()); }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, CudaDeviceTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
