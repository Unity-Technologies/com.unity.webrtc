#pragma once

#include <cuda.h>
#include <vulkan/vulkan.h>

#if _WIN32
#include <d3d11.h>
#include <d3d12.h>
#endif

namespace unity
{
namespace webrtc
{
    // The minimum version of CUDA Toolkit
    const int kRequiredDriverVersion = 11000;

    // todo(kazuki):
    // This class manages only the context related on the render thread.
    // Not considered using on the multiple threads.
    class CudaContext
    {
    public:
        CudaContext();
        ~CudaContext() = default;

        CUresult Init(const VkInstance instance, VkPhysicalDevice physicalDevice);
        void Shutdown();

#if defined(UNITY_WIN)
        CUresult Init(ID3D11Device* device);
        CUresult Init(ID3D12Device* device);
#endif
#if defined(UNITY_LINUX)
        CUresult InitGL();
#endif

        // This method returns context for the thread which called the method.
        CUcontext GetContext() const;

        static CUresult FindCudaDevice(const uint8_t* uuid, CUdevice* device);

    private:
        CUcontext m_context;
    };

} // end namespace webrtc
} // end namespace unity
