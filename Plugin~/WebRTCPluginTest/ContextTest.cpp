﻿#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"
#include "../WebRTCPlugin/Context.h"

using namespace WebRTC;

class ContextTest : public GraphicsDeviceTestBase
{
protected:
    IEncoder* encoder_ = nullptr;
//    IGraphicsDevice* device_ = nullptr;
    const int width = 256;
    const int height = 256;
    std::unique_ptr<Context> context;

    void SetUp() override {
        GraphicsDeviceTestBase::SetUp();
        EXPECT_NE(nullptr, m_device);

        EncoderFactory::GetInstance().Init(width, height, m_device);
        encoder_ = EncoderFactory::GetInstance().GetEncoder();
        EXPECT_NE(nullptr, encoder_);

        context = std::make_unique<Context>();
    }
    void TearDown() override {
        EncoderFactory::GetInstance().Shutdown();
        GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(ContextTest, InitializeAndFinalizeEncoder) {
    context->InitializeEncoder(m_device);
    context->FinalizeEncoder();
}

TEST_P(ContextTest, CreateAndDeleteVideoStream) {
    context->InitializeEncoder(m_device);
    auto tex = m_device->CreateDefaultTextureV(width, height);
    const auto stream = context->CreateVideoStream(tex->GetEncodeTexturePtrV(), width, height);
    context->DeleteVideoStream(stream);
    context->FinalizeEncoder();
}

TEST_P(ContextTest, CreateAndDeleteAudioStream) {
    const auto stream = context->CreateAudioStream();
    context->DeleteAudioStream(stream);
}

TEST_P(ContextTest, CreateAndDeletePeerConnection) {
    const auto connection = context->CreatePeerConnection();
    context->DeletePeerConnection(connection);
}

TEST_P(ContextTest, CreateAndDeleteDataChannel) {
    const auto connection = context->CreatePeerConnection();
    RTCDataChannelInit init;
    const auto channel = context->CreateDataChannel(connection, "test", init);
    context->DeleteDataChannel(channel);
    context->DeletePeerConnection(connection);
}

INSTANTIATE_TEST_CASE_P(
    GraphicsDeviceParameters,
    ContextTest,
    testing::Values(GraphicsDeviceTestBase::CreateParameter())
);
