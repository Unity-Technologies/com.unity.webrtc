#include "pch.h"

#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"

#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

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
        const uint32_t kWidth = 256;
        const uint32_t kHeight = 256;
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
    };

    TEST_P(CudaDeviceTest, GetCUcontext) { EXPECT_NE(device_->GetCUcontext(), nullptr); }

    TEST_P(CudaDeviceTest, IsCudaSupport) { EXPECT_TRUE(device_->IsCudaSupport()); }

    TEST_P(CudaDeviceTest, Resize) 
    {
        const uint32_t kWidth2 = kWidth * 2;
        const uint32_t kHeight2 = kHeight * 2;

        std::unique_ptr<ITexture2D> texture(device_->CreateDefaultTextureV(kWidth2, kHeight2, kFormat));
        std::unique_ptr<GpuMemoryBufferHandle> handle = device_->Map(texture.get());

        GpuMemoryBufferCudaHandle* cudaHandle = static_cast<GpuMemoryBufferCudaHandle*>(handle.get());

        //std::unique_ptr<GpuMemoryBufferCudaHandle> resized(device_->Resize(*cudaHandle, Size(kWidth, kHeight)));
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, CudaDeviceTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
