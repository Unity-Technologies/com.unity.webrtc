﻿#pragma once

#include "WebRTCConstants.h"
#include "GraphicsDevice/Cuda/CudaContext.h"
#include "GraphicsDevice/IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

namespace webrtc = ::webrtc;

class OpenGLGraphicsDevice : public IGraphicsDevice{
public:
    OpenGLGraphicsDevice();
    virtual ~OpenGLGraphicsDevice();

    virtual bool InitV();
    virtual void ShutdownV();
    inline virtual void* GetEncodeDevicePtrV();

    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat);
    virtual ITexture2D* CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat);
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src);
    virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex);
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr);
    inline virtual GraphicsDeviceType GetDeviceType() const;

    virtual bool IsCudaSupport() override { return m_isCudaSupport; }
    virtual CUcontext GetCuContext() override { return m_cudaContext.GetContext(); }

private:
    CudaContext m_cudaContext;
    bool m_isCudaSupport;
    bool CopyResource(GLuint dstName, GLuint srcName, uint32 width, uint32 height);
};

void* OpenGLGraphicsDevice::GetEncodeDevicePtrV() { return nullptr; }
GraphicsDeviceType OpenGLGraphicsDevice::GetDeviceType() const { return GRAPHICS_DEVICE_OPENGL; }

//---------------------------------------------------------------------------------------------------------------------
} // end namespace webrtc
} // end namespace unity