#include "pch.h"

#include "GpuMemoryBufferPool.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceContainer.h"

namespace unity
{
namespace webrtc
{

    class GpuMemoryBufferPoolTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        explicit GpuMemoryBufferPoolTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
            , timestamp_(0)
        {
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            std::unique_ptr<ITexture2D> texture(device_->CreateDefaultTextureV(kWidth, kHeight, kFormat));
            if (!texture)
                GTEST_SKIP() << "The graphics driver cannot create a texture resource.";

            bufferPool_ = std::make_unique<GpuMemoryBufferPool>(device_);
            timestamp_ = Clock::GetRealTimeClock()->TimeInMicroseconds();
        }

        std::unique_ptr<ITexture2D> CreateTexture(const Size& size, UnityRenderingExtTextureFormat format)
        {
            ITexture2D* tex = device_->CreateDefaultTextureV(size.width(), size.height(), format);
            return std::unique_ptr<ITexture2D>(tex);
        }

        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
        std::unique_ptr<GpuMemoryBufferPool> bufferPool_;
        int64_t timestamp_;
        const uint32_t kWidth = 256;
        const uint32_t kHeight = 256;
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
    };

    TEST_P(GpuMemoryBufferPoolTest, CreateFrame)
    {
        const Size kSize(kWidth, kHeight);
        auto tex = CreateTexture(kSize, kFormat);
        void* ptr = tex->GetNativeTexturePtrV();

        auto frame = bufferPool_->CreateFrame(ptr, kSize, kFormat, timestamp_);
        EXPECT_EQ(frame->size(), kSize);
        EXPECT_EQ(kFormat, frame->format());
        EXPECT_EQ(1u, bufferPool_->bufferCount());
    }

    TEST_P(GpuMemoryBufferPoolTest, ReuseFirstResource)
    {
        const Size kSize(kWidth, kHeight);
        auto tex = CreateTexture(kSize, kFormat);
        void* ptr = tex->GetNativeTexturePtrV();

        auto frame1 = bufferPool_->CreateFrame(ptr, kSize, kFormat, timestamp_);
        EXPECT_NE(frame1, nullptr);
        EXPECT_EQ(1u, bufferPool_->bufferCount());

        auto frame2 = bufferPool_->CreateFrame(ptr, kSize, kFormat, timestamp_);
        EXPECT_NE(frame2, nullptr);
        EXPECT_EQ(2u, bufferPool_->bufferCount());

        frame1 = nullptr;
        frame2 = nullptr;
        auto frame3 = bufferPool_->CreateFrame(ptr, kSize, kFormat, timestamp_);
        EXPECT_EQ(2u, bufferPool_->bufferCount());
    }

    TEST_P(GpuMemoryBufferPoolTest, DropResourceWhenSizeIsDifferent)
    {
        const Size kSize1(kWidth, kHeight);
        auto tex1 = CreateTexture(kSize1, kFormat);
        void* ptr1 = tex1->GetNativeTexturePtrV();

        auto frame1 = bufferPool_->CreateFrame(ptr1, kSize1, kFormat, timestamp_);
        EXPECT_EQ(1u, bufferPool_->bufferCount());

        frame1 = nullptr;

        const Size kSize2(512, 512);
        auto tex2 = CreateTexture(kSize2, kFormat);
        void* ptr2 = tex2->GetNativeTexturePtrV();

        auto frame2 = bufferPool_->CreateFrame(ptr2, kSize2, kFormat, timestamp_);
        EXPECT_EQ(2u, bufferPool_->bufferCount());
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, GpuMemoryBufferPoolTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
