#pragma once

#include <cuda.h>

namespace unity
{
namespace webrtc
{

class CudaContext {
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
    inline CUcontext GetContext() const;
private:
    CUcontext m_context;

};

inline CUcontext CudaContext::GetContext() const { return m_context; }

} // end namespace webrtc
} // end namespace unity
