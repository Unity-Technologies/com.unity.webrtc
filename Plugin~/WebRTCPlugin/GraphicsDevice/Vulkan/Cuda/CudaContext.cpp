﻿#include "pch.h"

#include "CudaContext.h"

#include <array>
#include "GraphicsDevice/Vulkan/VulkanUtility.h"

namespace WebRTC {

CudaContext::CudaContext() : m_context(nullptr) {
}

//---------------------------------------------------------------------------------------------------------------------

CudaContext::~CudaContext() {
}

//---------------------------------------------------------------------------------------------------------------------

CUresult CudaContext::Init(const VkInstance instance, VkPhysicalDevice physicalDevice) {

    CUdevice dev;
    bool foundDevice = true;

    CUresult result = cuInit(0);
    if (result != CUDA_SUCCESS) {
        return result;
    }

    int numDevices = 0;
    result = cuDeviceGetCount(&numDevices);
    if (result != CUDA_SUCCESS) {
        return result;
    }

    CUuuid id = {};
    std::array<uint8_t, VK_UUID_SIZE> deviceUUID;
    if (!VulkanUtility::GetPhysicalDeviceUUIDInto(instance, physicalDevice, &deviceUUID)) {
        return CUDA_ERROR_INVALID_DEVICE;
    }

    //Loop over the available devices and identify the CUdevice  corresponding to the physical device in use by
    //this Vulkan instance. This is required because there is no other way to match GPUs across API boundaries.
    for (int i = 0; i < numDevices; i++) {
        cuDeviceGet(&dev, i);

        cuDeviceGetUuid(&id, dev);

        if (!std::memcmp(static_cast<const void *>(&id),
                static_cast<const void *>(deviceUUID.data()),
                sizeof(CUuuid))) {
            foundDevice = true;
            break;
        }
    }

    if (!foundDevice) {
        return CUDA_ERROR_NO_DEVICE;
 
    }

    result = cuCtxCreate(&m_context, 0, dev);
    if (result != CUDA_SUCCESS) {
        return result;
    }
}

//---------------------------------------------------------------------------------------------------------------------

void CudaContext::Shutdown() {
    if (nullptr != m_context) {
        cuCtxDestroy(m_context);
        m_context = nullptr;
    }
}

} //end namespace
