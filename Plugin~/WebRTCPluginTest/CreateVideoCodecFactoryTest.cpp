#include "pch.h"

#include "Codec/CreateVideoCodecFactory.h"
#include "GraphicsDeviceContainer.h"

namespace unity
{
namespace webrtc
{
    class CreateVideoCodecFactoryTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        CreateVideoCodecFactoryTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
            arrayImpl_ = { kInternalImpl, kNvCodecImpl, kAndroidMediaCodecImpl, kVideoToolboxImpl };
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
        }
        std::unique_ptr<GraphicsDeviceContainer> container_;
        std::vector<std::string> arrayImpl_;
        IGraphicsDevice* device_;
    };

    TEST_P(CreateVideoCodecFactoryTest, CreateVideoEncoderFactory)
    {
        std::map<std::string, std::unique_ptr<VideoEncoderFactory>> factories;
        for (auto impl : arrayImpl_)
        {
            auto factory = CreateVideoEncoderFactory(impl, device_, nullptr);
            if (factory)
                factories.emplace(impl, factory);
        }
        EXPECT_GT(factories.size(), 0);
    }

    TEST_P(CreateVideoCodecFactoryTest, CreateVideoDecoderFactory)
    {
        std::map<std::string, std::unique_ptr<VideoDecoderFactory>> factories;

        for (auto impl : arrayImpl_)
        {
            auto factory = CreateVideoDecoderFactory(impl, device_, nullptr);
            if (factory)
                factories.emplace(impl, factory);
        }
        EXPECT_GT(factories.size(), 0);
    }

    TEST_P(CreateVideoCodecFactoryTest, FindCodecFactory)
    {
        std::map<std::string, std::unique_ptr<VideoEncoderFactory>> factories;
        for (auto impl : arrayImpl_)
        {
            auto factory = CreateVideoEncoderFactory(impl, device_, nullptr);
            if (factory)
                factories.emplace(impl, factory);
        }

        // return nullptr when unknown mimetype
        SdpVideoFormat format("test");
        EXPECT_TRUE(!FindCodecFactory(factories, format));

        // return value when unknown implementation_name
        SdpVideoFormat format2("VP8");
        format.parameters.emplace(kSdpKeyNameCodecImpl, "unknown");
        EXPECT_TRUE(FindCodecFactory(factories, format2));
    }

    TEST_P(CreateVideoCodecFactoryTest, GetSupportedFormatsInFactories)
    {
        std::map<std::string, std::unique_ptr<VideoEncoderFactory>> factories;
        for (auto impl : arrayImpl_)
        {
            auto factory = CreateVideoEncoderFactory(impl, device_, nullptr);
            if (factory)
                factories.emplace(impl, factory);
        }

        std::vector<webrtc::SdpVideoFormat> formats = GetSupportedFormatsInFactories(factories);
        EXPECT_GT(formats.size(), 0);
    }
    INSTANTIATE_TEST_SUITE_P(GfxDevice, CreateVideoCodecFactoryTest, testing::ValuesIn(supportedGfxDevices));
} // namespace webrtc
} // namespace unity
