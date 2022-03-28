#include "pch.h"

#include "VulkanGraphicsDevice.h"
#include "VulkanTexture2D.h"

#include "VulkanUtility.h"
#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/GraphicsUtility.h"

namespace unity
{
namespace webrtc
{

VulkanGraphicsDevice::VulkanGraphicsDevice( IUnityGraphicsVulkan* unityVulkan, const VkInstance instance,
    const VkPhysicalDevice physicalDevice, const VkDevice device, const VkQueue graphicsQueue, const uint32_t queueFamilyIndex, UnityGfxRenderer renderer)
        : IGraphicsDevice(renderer)
    , m_unityVulkan(unityVulkan)
    , m_physicalDevice(physicalDevice)
    , m_device(device)
    , m_graphicsQueue(graphicsQueue)
    , m_commandPool(VK_NULL_HANDLE)
    , m_queueFamilyIndex(queueFamilyIndex)
    , m_allocator(nullptr)
#if CUDA_PLATFORM
    , m_instance(instance)
#endif
{
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::InitV()
{
#if CUDA_PLATFORM
    CUresult result = m_cudaContext.Init(m_instance, m_physicalDevice);
    m_isCudaSupport = CUDA_SUCCESS == result;
#endif
    return VK_SUCCESS == CreateCommandPool();
}

//---------------------------------------------------------------------------------------------------------------------

void VulkanGraphicsDevice::ShutdownV() {
#if CUDA_PLATFORM
    m_cudaContext.Shutdown();
#endif
    VULKAN_SAFE_DESTROY_COMMAND_POOL(m_device, m_commandPool, m_allocator);
}

//---------------------------------------------------------------------------------------------------------------------

std::unique_ptr<UnityVulkanImage> VulkanGraphicsDevice::AccessTexture(void* ptr) const
{
    std::unique_ptr<UnityVulkanImage> unityVulkanImage =
        std::make_unique<UnityVulkanImage>();

    VkImageSubresource subResource{ VK_IMAGE_ASPECT_COLOR_BIT, 0, 0 };
    if (!m_unityVulkan->AccessTexture(
        ptr, &subResource, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
        VK_PIPELINE_STAGE_TRANSFER_BIT, VK_ACCESS_TRANSFER_READ_BIT,
        kUnityVulkanResourceAccess_PipelineBarrier,
        unityVulkanImage.get()))
    {
        return nullptr;
    }
    return unityVulkanImage;
}

//Returns null if failed
ITexture2D* VulkanGraphicsDevice::CreateDefaultTextureV(
    const uint32_t w, const uint32_t h, UnityRenderingExtTextureFormat textureFormat) {

    VulkanTexture2D* vulkanTexture = new VulkanTexture2D(w, h);
    if (!vulkanTexture->Init(m_physicalDevice, m_device)) {
        vulkanTexture->Shutdown();
        delete (vulkanTexture);
        return nullptr;
    }

    //Transition to dest
    if (VK_SUCCESS!= VulkanUtility::DoImageLayoutTransition(m_device, m_commandPool, m_graphicsQueue, 
            vulkanTexture->GetImage(), vulkanTexture->GetTextureFormat(), 
            VK_IMAGE_LAYOUT_UNDEFINED, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT))
    {
        vulkanTexture->Shutdown();
        delete (vulkanTexture);
        return nullptr;       
    }

    return vulkanTexture;
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* VulkanGraphicsDevice::CreateCPUReadTextureV(
    uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) {
    VulkanTexture2D* vulkanTexture = new VulkanTexture2D(w, h);
    if (!vulkanTexture->InitCpuRead(m_physicalDevice, m_device)) {
        delete (vulkanTexture);
        return nullptr;
    }

    //Transition to dest
    if (VK_SUCCESS != VulkanUtility::DoImageLayoutTransition(m_device, m_commandPool, m_graphicsQueue,
        vulkanTexture->GetImage(), vulkanTexture->GetTextureFormat(),
        VK_IMAGE_LAYOUT_UNDEFINED, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
        VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT))
    {
        delete (vulkanTexture);
        return nullptr;
    }
    return vulkanTexture;
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {

    VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
    VulkanTexture2D* srcTexture = reinterpret_cast<VulkanTexture2D*>(src);
    if (destTexture == srcTexture)
        return false;
    if (destTexture == nullptr || srcTexture == nullptr)
        return false;

    //Transition the src texture layout. 
    VULKAN_CHECK_FAILVALUE(
        VulkanUtility::DoImageLayoutTransition(m_device, m_commandPool, m_graphicsQueue, 
            srcTexture->GetImage(), srcTexture->GetTextureFormat(), 
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT
        ),
        false
    );

    //[TODO-sin: 2019-11-21] Optimize so that we don't do vkQueueWaitIdle multiple times here
    //The layouts of All VulkanTexture2D should be VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, so no transition for destTex
    VULKAN_CHECK_FAILVALUE(
        VulkanUtility::CopyImage(m_device, m_commandPool, m_graphicsQueue, srcTexture->GetImage(),
            destTexture->GetImage(), destTexture->GetWidth(), destTexture->GetHeight()
        ),
        false
    );

    //transition the src texture layout back to VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL
    VULKAN_CHECK_FAILVALUE(
        VulkanUtility::DoImageLayoutTransition(m_device, m_commandPool, m_graphicsQueue, 
            srcTexture->GetImage(), srcTexture->GetTextureFormat(), 
            VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT
        ),
        false
    );

    return true;
}

//---------------------------------------------------------------------------------------------------------------------
NativeTexPtr VulkanGraphicsDevice::ConvertNativeFromUnityPtr(void* tex)
{
    std::unique_ptr<UnityVulkanImage> unityVulkanImage = std::make_unique<UnityVulkanImage>();
    VkImageSubresource subResource{ VK_IMAGE_ASPECT_COLOR_BIT, 0, 0 };
    if (!m_unityVulkan->AccessTexture(tex, &subResource, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
        VK_PIPELINE_STAGE_TRANSFER_BIT, VK_ACCESS_TRANSFER_READ_BIT, kUnityVulkanResourceAccess_PipelineBarrier,
        unityVulkanImage.get()))
    {
        return nullptr;
    }
    return unityVulkanImage.release();
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::CopyResourceFromNativeV(
    ITexture2D* dest, void* nativeTexturePtr) {
    if (nullptr == dest || nullptr == nativeTexturePtr)
        return false;

    VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
    UnityVulkanImage* unityVulkanImage = static_cast<UnityVulkanImage*>(nativeTexturePtr);

    //Transition the src texture layout. 
    VkResult result = VulkanUtility::DoImageLayoutTransition(
        m_device, m_commandPool, m_graphicsQueue,
        unityVulkanImage->image, unityVulkanImage->format,
        VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT,
        VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT
    );
    if(result != VK_SUCCESS)
    {
        return false;
    }
    VkImage image = unityVulkanImage->image;

    if (destTexture->GetImage() == image)
        return false;

    // The layouts of All VulkanTexture2D should be VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
    // so no transition for destTex
    VULKAN_CHECK_FAILVALUE(
        VulkanUtility::CopyImage(m_device, m_commandPool, m_graphicsQueue,
            image, destTexture->GetImage(), destTexture->GetWidth(), destTexture->GetHeight()),
        false
    );

    return true;
}

//---------------------------------------------------------------------------------------------------------------------
VkResult VulkanGraphicsDevice::CreateCommandPool()
{
    VkCommandPoolCreateInfo poolInfo = {};
    poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
    poolInfo.queueFamilyIndex = m_queueFamilyIndex;
    poolInfo.flags = 0; 

    return vkCreateCommandPool(m_device, &poolInfo, m_allocator, &m_commandPool);
}

//---------------------------------------------------------------------------------------------------------------------
rtc::scoped_refptr<webrtc::I420Buffer> VulkanGraphicsDevice::ConvertRGBToI420(
    ITexture2D* tex)
{
    VulkanTexture2D* vulkanTexture = static_cast<VulkanTexture2D*>(tex);
    const uint32_t width = tex->GetWidth();
    const uint32_t height = tex->GetHeight();
    const VkDeviceMemory dstImageMemory = vulkanTexture->GetTextureImageMemory();
    VkImageSubresource subresource{ VK_IMAGE_ASPECT_COLOR_BIT, 0, 0 };
    VkSubresourceLayout subresourceLayout;
    vkGetImageSubresourceLayout(m_device, vulkanTexture->GetImage(), &subresource,
        &subresourceLayout);
    const uint32_t rowPitch = static_cast<uint32_t>(subresourceLayout.rowPitch);

    void* data;
    std::vector<uint8_t> dst;
    dst.resize(vulkanTexture->GetTextureImageMemorySize());
    const VkResult result = vkMapMemory(
        m_device, dstImageMemory, 0, VK_WHOLE_SIZE, 0, &data);
    if(result != VK_SUCCESS) {
        return nullptr;
    }
    std::memcpy(static_cast<void*>(dst.data()), data, dst.size());

    vkUnmapMemory(m_device, dstImageMemory);

    // convert format to i420
    rtc::scoped_refptr<webrtc::I420Buffer> i420Buffer = GraphicsUtility::ConvertRGBToI420Buffer(
        width, height, rowPitch, dst.data()
    );

    return i420Buffer;
}

    std::unique_ptr<GpuMemoryBufferHandle> VulkanGraphicsDevice::Map(ITexture2D* texture)
    {
#if CUDA_PLATFORM
        if(!IsCudaSupport())
            return nullptr;

        // set context on the thread.
        cuCtxPushCurrent(GetCUcontext());

        VulkanTexture2D* vulkanTexture = static_cast<VulkanTexture2D*>(texture);

        void* exportHandle = VulkanUtility::GetExportHandle(m_device, vulkanTexture->GetTextureImageMemory());

        if (!exportHandle)
        {
            RTC_LOG(LS_ERROR) << "cannot get export handle";
            throw;
        }

        CUDA_EXTERNAL_MEMORY_HANDLE_DESC memDesc = {};
#ifndef _WIN32
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD;
#else
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32;
#endif
        memDesc.handle.fd = static_cast<int>(reinterpret_cast<uintptr_t>(exportHandle));
        memDesc.size = vulkanTexture->GetTextureImageMemorySize();

        CUresult result;
        CUexternalMemory externalMemory;
        result = cuImportExternalMemory(&externalMemory, &memDesc);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuImportExternalMemory error:" << result;
            throw;
        }

        const VkExtent2D extent = { texture->GetWidth(), texture->GetHeight() };
        CUDA_ARRAY3D_DESCRIPTOR arrayDesc = {};
        arrayDesc.Width = extent.width;
        arrayDesc.Height = extent.height;
        arrayDesc.Depth = 0; /* CUDA 2D arrays are defined to have depth 0 */
        arrayDesc.Format = CU_AD_FORMAT_UNSIGNED_INT32;
        arrayDesc.NumChannels = 1;
        arrayDesc.Flags = CUDA_ARRAY3D_SURFACE_LDST | CUDA_ARRAY3D_COLOR_ATTACHMENT;

        CUDA_EXTERNAL_MEMORY_MIPMAPPED_ARRAY_DESC mipmapArrayDesc = {};
        mipmapArrayDesc.arrayDesc = arrayDesc;
        mipmapArrayDesc.numLevels = 1;

        CUmipmappedArray mipmappedArray;
        result = cuExternalMemoryGetMappedMipmappedArray(&mipmappedArray, externalMemory, &mipmapArrayDesc);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuExternalMemoryGetMappedMipmappedArray error:" << result;
            throw;
        }

        CUarray array;
        result = cuMipmappedArrayGetLevel(&array, mipmappedArray, 0);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuMipmappedArrayGetLevel error:" << result;
            throw;
        }

        cuCtxPopCurrent(NULL);

        std::unique_ptr<GpuMemoryBufferHandle> handle = std::make_unique<GpuMemoryBufferHandle>();
        handle->array = array;
        handle->externalMemory = externalMemory;
        return handle;
#else
        return nullptr;
#endif
    }


} // end namespace webrtc
} // end namespace unity
