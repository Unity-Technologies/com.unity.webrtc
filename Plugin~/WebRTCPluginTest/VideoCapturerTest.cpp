﻿#include "pch.h"
#include "D3D11GraphicsDeviceTestBase.h"
#include "../unity/include/IUnityGraphics.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"
#include "../WebRTCPlugin/NvVideoCapturer.h"

using namespace WebRTC;

class VideoCapturerTest : public D3D11GraphicsDeviceTestBase
{
protected:
    IEncoder* encoder_ = nullptr;
    //IGraphicsDevice* device_ = nullptr;
    const int width = 256;
    const int height = 256;
    std::unique_ptr<NvVideoCapturer> capturer;

    void SetUp() override {
        /*
        UnityGfxRenderer unityGfxRenderer;
        void* pGraphicsDevice;
        std::tie(unityGfxRenderer, pGraphicsDevice) = GetParam();
        GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice);
        m_device_ = GraphicsDevice::GetInstance().GetDevice();
        */
        EXPECT_NE(nullptr, m_device);

        EncoderFactory::GetInstance().Init(width, height, m_device);
        encoder_ = EncoderFactory::GetInstance().GetEncoder();
        EXPECT_NE(nullptr, encoder_);

        capturer = std::make_unique<NvVideoCapturer>();
    }
    void TearDown() override {
        EncoderFactory::GetInstance().Shutdown();
        D3D11GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(VideoCapturerTest, InitializeAndFinalize) {
    capturer->InitializeEncoder(m_device);
    capturer->FinalizeEncoder();
}

TEST_P(VideoCapturerTest, EncodeVideoData) {
    capturer->InitializeEncoder(m_device);
    auto tex = m_device->CreateDefaultTextureV(width, height);
    capturer->SetFrameBuffer(tex->GetEncodeTexturePtrV());
    capturer->EncodeVideoData();
    capturer->FinalizeEncoder();
}

INSTANTIATE_TEST_CASE_P(
        GraphicsDeviceParameters,
        VideoCapturerTest,
        testing::Values(std::make_tuple(kUnityGfxRendererOpenGLCore, nullptr))
);
