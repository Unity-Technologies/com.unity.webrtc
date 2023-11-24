#pragma once

#if SUPPORT_OPENGL_CORE
#include <glad/gl.h>
#endif

#include "GraphicsDevice/IGraphicsDevice.h"

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/CudaContext.h"
#endif

namespace unity
{
namespace webrtc
{

    namespace webrtc = ::webrtc;

    class OpenGLContext;
    struct OpenGLTexture2D;
    class OpenGLGraphicsDevice : public IGraphicsDevice
    {
    public:
        OpenGLGraphicsDevice(UnityGfxRenderer renderer, ProfilerMarkerFactory* profiler);
        virtual ~OpenGLGraphicsDevice();

        virtual bool InitV() override;
        virtual void ShutdownV() override;
        inline virtual void* GetEncodeDevicePtrV() override;

        ITexture2D*
        CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        ITexture2D*
        CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) override;
        bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
        rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;
        bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
        std::unique_ptr<GpuMemoryBufferHandle> Map(ITexture2D* texture) override;
        bool WaitSync(const ITexture2D* texture) override;
        bool ResetSync(const ITexture2D* texture) override;

#if CUDA_PLATFORM
        bool IsCudaSupport() override { return m_isCudaSupport; }
        CUcontext GetCUcontext() override { return m_cudaContext.GetContext(); }
        NV_ENC_BUFFER_FORMAT GetEncodeBufferFormat() override { return NV_ENC_BUFFER_FORMAT_ABGR; }
#endif
    private:
        bool CopyResource(OpenGLTexture2D* texture, GLuint srcName);
        void ReleaseTexture(OpenGLTexture2D* texture);
#if CUDA_PLATFORM
        CudaContext m_cudaContext;
        bool m_isCudaSupport;
#endif
        std::unique_ptr<OpenGLContext> mainContext_;
        std::vector<std::unique_ptr<OpenGLContext>> contexts_;
    };

    void* OpenGLGraphicsDevice::GetEncodeDevicePtrV() { return nullptr; }

} // end namespace webrtc
} // end namespace unity
