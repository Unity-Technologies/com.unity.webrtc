#pragma once

#include <vulkan/vulkan.h>
#include <cuda.h>

namespace WebRTC {

class CudaContext {
public:
    CudaContext();
    ~CudaContext();

    CUresult Init(const VkInstance instance, VkPhysicalDevice physicalDevice);
    void Shutdown();
    inline const CUcontext GetContext() const;
private:
    CUcontext m_context;

};

inline const CUcontext CudaContext::GetContext() const { return m_context; }

} //end namespace;
