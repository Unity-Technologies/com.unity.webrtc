#pragma once

#include "WebRTCConstants.h"
#include "GraphicsDevice/IGraphicsDevice.h"

#if defined(CUDA_PLATFORM)
#include "GraphicsDevice/Cuda/CudaContext.h"
#endif

namespace unity
{
namespace webrtc
{

namespace webrtc = ::webrtc;

class OpenGLGraphicsDevice : public IGraphicsDevice{
public:
    OpenGLGraphicsDevice();
    virtual ~OpenGLGraphicsDevice();

    virtual bool InitV() override;
    virtual void ShutdownV() override;
    inline virtual void* GetEncodeDevicePtrV() override;

    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
    virtual ITexture2D* CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) override;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
    virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
    inline virtual GraphicsDeviceType GetDeviceType() const override;

#if defined(CUDA_PLATFORM)
    virtual bool IsCudaSupport() override { return m_isCudaSupport; }
    virtual CUcontext GetCuContext() override { return m_cudaContext.GetContext(); }
#endif
private:
    bool CopyResource(GLuint dstName, GLuint srcName, uint32 width, uint32 height);

#if defined(CUDA_PLATFORM)
    CudaContext m_cudaContext;
    bool m_isCudaSupport;
#endif
};

void* OpenGLGraphicsDevice::GetEncodeDevicePtrV() { return nullptr; }
GraphicsDeviceType OpenGLGraphicsDevice::GetDeviceType() const { return GRAPHICS_DEVICE_OPENGL; }

//---------------------------------------------------------------------------------------------------------------------
} // end namespace webrtc
} // end namespace unity
