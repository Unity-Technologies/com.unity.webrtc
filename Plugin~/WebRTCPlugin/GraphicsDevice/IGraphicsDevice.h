#pragma once

#include "GraphicsDevice/GraphicsDeviceType.h"

#if CUDA_PLATFORM
#include "Cuda/ICudaDevice.h"
#endif

namespace unity
{
namespace webrtc
{
using NativeTexPtr = void*;
class ITexture2D;

class IGraphicsDevice
#if CUDA_PLATFORM
    : public ICudaDevice
#endif
{
public:
    IGraphicsDevice(UnityGfxRenderer renderer);
    virtual ~IGraphicsDevice() = 0;
    virtual bool InitV() = 0;
    virtual void ShutdownV() = 0;
    virtual ITexture2D* CreateDefaultTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) = 0;
    virtual void* GetEncodeDevicePtrV() = 0;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) = 0;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, NativeTexPtr nativeTexturePtr) = 0;
    virtual NativeTexPtr ConvertNativeFromUnityPtr(void* tex) { return tex; }
    virtual GraphicsDeviceType GetDeviceType() const = 0;
    virtual UnityGfxRenderer GetGfxRenderer() const { return m_gfxRenderer; }

    //Required for software encoding
    virtual ITexture2D* CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) = 0;
    virtual rtc::scoped_refptr<::webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) = 0;
protected:
    UnityGfxRenderer m_gfxRenderer;
};

} // end namespace webrtc
} // end namespace unity
