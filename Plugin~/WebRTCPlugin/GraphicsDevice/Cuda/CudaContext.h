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

    static CUresult FindCudaDevice(const uint8_t* uuid, CUdevice* cuDevice);
private:
    CUcontext m_context;
};


} // end namespace webrtc
} // end namespace unity
