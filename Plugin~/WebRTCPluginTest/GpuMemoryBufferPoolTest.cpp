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
            , timestamp_(Timestamp::Zero())
        {
        }

    protected:
        void SetUp() override
        {
            bufferPool_ = std::make_unique<GpuMemoryBufferPool>(container_->device());
            timestamp_ = Clock::GetRealTimeClock()->CurrentTime();
        }

        std::unique_ptr<ITexture2D> CreateTexture(const Size& size, UnityRenderingExtTextureFormat format)
        {
            ITexture2D* tex = container_->device()->CreateDefaultTextureV(size.width(), size.height(), format);
            return std::unique_ptr<ITexture2D>(tex);
        }

        std::unique_ptr<GraphicsDeviceContainer> container_;
        std::unique_ptr<GpuMemoryBufferPool> bufferPool_;
        Timestamp timestamp_;
    };

    TEST_P(GpuMemoryBufferPoolTest, CreateFrame)
    {
        const Size kSize(256, 256);
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
        auto tex = CreateTexture(kSize, kFormat);
        void* ptr = tex->GetNativeTexturePtrV();

        auto frame = bufferPool_->CreateFrame(ptr, kSize, kFormat, timestamp_.ms());
        EXPECT_EQ(frame->size(), kSize);
        EXPECT_EQ(kFormat, frame->format());
        EXPECT_EQ(1u, bufferPool_->bufferCount());
    }

    TEST_P(GpuMemoryBufferPoolTest, ReuseFirstResource)
    {
        const Size kSize(256, 256);
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
        auto tex = CreateTexture(kSize, kFormat);
        void* ptr = tex->GetNativeTexturePtrV();

        auto frame1 = bufferPool_->CreateFrame(ptr, kSize, kFormat, timestamp_.us());
        EXPECT_NE(frame1, nullptr);
        EXPECT_EQ(1u, bufferPool_->bufferCount());

        auto frame2 = bufferPool_->CreateFrame(ptr, kSize, kFormat, timestamp_.us());
        EXPECT_NE(frame2, nullptr);
        EXPECT_EQ(2u, bufferPool_->bufferCount());

        frame1 = nullptr;
        frame2 = nullptr;
        auto frame3 = bufferPool_->CreateFrame(ptr, kSize, kFormat, timestamp_.us());
        EXPECT_EQ(2u, bufferPool_->bufferCount());
    }

    TEST_P(GpuMemoryBufferPoolTest, DropResourceWhenSizeIsDifferent)
    {
        const Size kSize1(256, 256);
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
        auto tex1 = CreateTexture(kSize1, kFormat);
        void* ptr1 = tex1->GetNativeTexturePtrV();

        auto frame1 = bufferPool_->CreateFrame(ptr1, kSize1, kFormat, timestamp_.us());
        EXPECT_EQ(1u, bufferPool_->bufferCount());

        frame1 = nullptr;

        const Size kSize2(512, 512);
        auto tex2 = CreateTexture(kSize2, kFormat);
        void* ptr2 = tex2->GetNativeTexturePtrV();

        auto frame2 = bufferPool_->CreateFrame(ptr2, kSize2, kFormat, timestamp_.us());
        EXPECT_EQ(2u, bufferPool_->bufferCount());
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, GpuMemoryBufferPoolTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
