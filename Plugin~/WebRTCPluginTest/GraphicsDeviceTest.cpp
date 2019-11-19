﻿#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"

using namespace WebRTC;

class GraphicsDeviceTest : public GraphicsDeviceTestBase {};



TEST_P(GraphicsDeviceTest, GraphicsDeviceIsNotNull) {
    EXPECT_NE(nullptr, m_device);
}

TEST_P(GraphicsDeviceTest, CreateDefaultTextureV) {
    auto width = 256;
    auto height = 256;
    auto tex = m_device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(tex->IsSize(width, height));
    EXPECT_FALSE(tex->IsSize(0, 0));
}

TEST_P(GraphicsDeviceTest, CopyResourceV) {
    const auto width = 256;
    const auto height = 256;
    const auto src = m_device->CreateDefaultTextureV(width, height);
    const auto dst = m_device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(m_device->CopyResourceV(dst, src));
    EXPECT_FALSE(m_device->CopyResourceV(src, src));
}

TEST_P(GraphicsDeviceTest, CopyResourceNativeV) {
    const auto width = 256;
    const auto height = 256;
    const auto src = m_device->CreateDefaultTextureV(width, height);
    const auto dst = m_device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(m_device->CopyResourceFromNativeV(dst, src->GetEncodeTexturePtrV()));
    EXPECT_FALSE(m_device->CopyResourceFromNativeV(dst, dst->GetEncodeTexturePtrV()));
}

INSTANTIATE_TEST_CASE_P(
    GraphicsDeviceParameters,
    GraphicsDeviceTest,
    testing::Values(GraphicsDeviceTestBase::CreateParameter())
);
