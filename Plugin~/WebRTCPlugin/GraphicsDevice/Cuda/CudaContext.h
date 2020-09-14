#pragma once

#include <cuda.h>

namespace unity
{
namespace webrtc
{

// todo(kazuki):
// This class manages only the context related on the render thread.
// Not considered using on the multiple threads.

class CudaContext
{
public:
    CudaContext();
    ~CudaContext() = default;

    CUresult Init(const VkInstance instance, VkPhysicalDevice physicalDevice);
    static CUresult FindCudaDevice(const uint8_t* uuid, CUdevice* cuDevice);

#if defined(UNITY_WIN)
    CUresult Init(ID3D11Device* device);
    CUresult Init(ID3D12Device* device);
#endif
#if defined(UNITY_LINUX)
    CUresult InitGL();
#endif

    void Shutdown();

    // This method returns context for the thread which called the method.
    CUcontext GetContext() const;
private:
    CUcontext m_context;
};

} // end namespace webrtc
} // end namespace unity
