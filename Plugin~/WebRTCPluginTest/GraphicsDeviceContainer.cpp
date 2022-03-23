#include "pch.h"

#include "GraphicsDeviceContainer.h"
#include "GraphicsDevice/GraphicsDevice.h"

#if SUPPORT_D3D11
#include "GraphicsDevice/D3D12/D3D12GraphicsDevice.h"
#include <d3d11.h>
#include <wrl/client.h>
#endif

#if SUPPORT_METAL
#include "GraphicsDevice/Metal/MetalDevice.h"
#import <Metal/Metal.h>
#endif

#if SUPPORT_OPENGL_CORE
#include <GL/glut.h>
#include "GraphicsDevice/OpenGL/OpenGLContext.h"
#endif

#if SUPPORT_OPENGL_ES
#include "GraphicsDevice/OpenGL/OpenGLContext.h"
#endif

#if SUPPORT_VULKAN

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/CudaContext.h"
#include "NvCodecUtils.h"
#include <cuda.h>
#endif

#if _WIN32
#include <vulkan/vulkan_win32.h>
#endif
#include "GraphicsDevice/Vulkan/VulkanUtility.h"
#endif

namespace unity
{
namespace webrtc
{

#if defined(SUPPORT_D3D11) // D3D11

    Microsoft::WRL::ComPtr<IDXGIFactory1> pFactory;
    Microsoft::WRL::ComPtr<IDXGIAdapter> pAdapter;
    Microsoft::WRL::ComPtr<ID3D11Device> pD3D11Device;
    Microsoft::WRL::ComPtr<ID3D11DeviceContext> pD3D11DeviceContext;

    Microsoft::WRL::ComPtr<IDXGIAdapter1> pAdapter1;
    Microsoft::WRL::ComPtr<IDXGIFactory4> pFactory4;
    Microsoft::WRL::ComPtr<ID3D12Device5> pD3D12Device;
    Microsoft::WRL::ComPtr<ID3D12CommandQueue> pCommandQueue;

    const int kD3D12NodeMask = 0;

    //---------------------------------------------------------------------------------------------------------------------

    void* CreateDeviceD3D11()
    {
        // recycle device
        if (pD3D11Device.Get() != nullptr)
            return pD3D11Device.Get();

        auto hr = CreateDXGIFactory1(IID_PPV_ARGS(&pFactory));
        EXPECT_TRUE(SUCCEEDED(hr));
        EXPECT_NE(nullptr, pFactory.Get());

        hr = pFactory->EnumAdapters(0, pAdapter.GetAddressOf());
        EXPECT_TRUE(SUCCEEDED(hr));
        EXPECT_NE(nullptr, pAdapter.Get());

        hr = D3D11CreateDevice(
            pAdapter.Get(),
            D3D_DRIVER_TYPE_UNKNOWN,
            nullptr,
            0,
            nullptr,
            0,
            D3D11_SDK_VERSION,
            pD3D11Device.GetAddressOf(),
            nullptr,
            pD3D11DeviceContext.GetAddressOf());

        EXPECT_TRUE(SUCCEEDED(hr));
        EXPECT_NE(nullptr, pD3D11Device.Get());
        return pD3D11Device.Get();
    }

    // Helper function for acquiring the first available hardware adapter that supports Direct3D 12.
    // If no such adapter can be found, *ppAdapter will be set to nullptr.
    void GetHardwareAdapter(IDXGIFactory2* pFactory, IDXGIAdapter1** ppAdapter)
    {
        Microsoft::WRL::ComPtr<IDXGIAdapter1> adapter;
        *ppAdapter = nullptr;

        for (UINT adapterIndex = 0; DXGI_ERROR_NOT_FOUND != pFactory->EnumAdapters1(adapterIndex, &adapter);
             ++adapterIndex)
        {
            DXGI_ADAPTER_DESC1 desc;
            adapter->GetDesc1(&desc);

            if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
            {
                // Don't select the Basic Render Driver adapter.
                // If you want a software adapter, pass in "/warp" on the command line.
                continue;
            }

            // Check to see if the adapter supports Direct3D 12, but don't create the
            // actual device yet.
            if (SUCCEEDED(D3D12CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_0, _uuidof(ID3D12Device), nullptr)))
            {
                break;
            }
        }
        *ppAdapter = adapter.Detach();
    }

    void* CreateDeviceD3D12()
    {
        auto hr = CreateDXGIFactory2(0, IID_PPV_ARGS(&pFactory4));
        EXPECT_TRUE(SUCCEEDED(hr));
        EXPECT_NE(nullptr, pFactory4.Get());

        GetHardwareAdapter(pFactory4.Get(), &pAdapter1);
        EXPECT_NE(nullptr, pAdapter1.Get());

        hr = D3D12CreateDevice(pAdapter1.Get(), D3D_FEATURE_LEVEL_12_0, IID_PPV_ARGS(&pD3D12Device));
        EXPECT_TRUE(SUCCEEDED(hr));
        EXPECT_NE(nullptr, pD3D12Device.Get());

        D3D12_FEATURE_DATA_D3D12_OPTIONS3 options = {};
        hr = pD3D12Device->CheckFeatureSupport(D3D12_FEATURE_D3D12_OPTIONS3, &options, sizeof(options));
        EXPECT_TRUE(SUCCEEDED(hr));
        EXPECT_TRUE(options.WriteBufferImmediateSupportFlags & (1 << D3D12_COMMAND_LIST_TYPE_DIRECT));
        D3D12_COMMAND_QUEUE_DESC queueDesc = {};
        queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_DISABLE_GPU_TIMEOUT;
        queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
        queueDesc.NodeMask = kD3D12NodeMask;

        hr = pD3D12Device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&pCommandQueue));
        EXPECT_TRUE(SUCCEEDED(hr));
        EXPECT_NE(nullptr, pCommandQueue.Get());

        return pD3D12Device.Get();
    }
#endif
#if defined(SUPPORT_VULKAN) // Vulkan

    inline void VKCHECK(VkResult result)
    {
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << result;
            throw result;
        }
    }

    LIBRARY_TYPE s_library = nullptr;

    bool LoadVulkanModule()
    {
        if (!LoadVulkanLibrary(s_library))
            return false;
        if (!LoadExportedVulkanFunction(s_library))
            return false;
        return LoadGlobalVulkanFunction();
    }

    int32_t GetPhysicalDeviceIndex(VkInstance instance, std::vector<VkPhysicalDevice>& list, bool* found)
    {
        std::array<uint8_t, VK_UUID_SIZE> deviceUUID;
        for (size_t i = 0; i < list.size(); ++i)
        {
            VkPhysicalDevice physicalDevice = list[i];
            if (!VulkanUtility::GetPhysicalDeviceUUIDInto(instance, physicalDevice, &deviceUUID))
            {
                continue;
            }
#if CUDA_PLATFORM
            if (CudaContext::FindCudaDevice(deviceUUID.data(), nullptr) != CUDA_SUCCESS)
            {
                continue;
            }
#endif
            *found = true;
            return i;
        }
        *found = false;
        return 0;
    }

    void* CreateDeviceVulkan()
    {
        // Extension
        std::vector<const char*> instanceExtensions =
        {
            VK_KHR_SURFACE_EXTENSION_NAME,
#ifdef _WIN32
            VK_KHR_WIN32_SURFACE_EXTENSION_NAME,
#endif
#if defined(_DEBUG)
            VK_EXT_DEBUG_REPORT_EXTENSION_NAME,
#endif
            VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME
        };

        std::vector<const char*> deviceExtensions =
        {

#ifndef _WIN32
            VK_KHR_EXTERNAL_MEMORY_FD_EXTENSION_NAME, VK_KHR_EXTERNAL_SEMAPHORE_FD_EXTENSION_NAME
#else
            VK_KHR_EXTERNAL_MEMORY_WIN32_EXTENSION_NAME, // vkGetMemoryWin32HandleKHR()
            VK_KHR_EXTERNAL_SEMAPHORE_WIN32_EXTENSION_NAME
#endif
        };

        VkApplicationInfo appInfo {};
        appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
        appInfo.pApplicationName = "test";
#if UNITY_ANDROID
        // Android platform doesn't support vulkan api version 1.1.
        appInfo.apiVersion = VK_API_VERSION_1_0;
#else
        appInfo.apiVersion = VK_API_VERSION_1_1;
#endif
        appInfo.engineVersion = 1;

        if (!LoadVulkanModule())
            assert("failed loading vulkan module");

        VkInstanceCreateInfo instanceInfo {};
        instanceInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
        instanceInfo.enabledExtensionCount = static_cast<uint32_t>(instanceExtensions.size());
        instanceInfo.ppEnabledExtensionNames = instanceExtensions.data();
        instanceInfo.pApplicationInfo = &appInfo;
        VkInstance instance = nullptr;
        VKCHECK(vkCreateInstance(&instanceInfo, nullptr, &instance));

        if (!LoadInstanceVulkanFunction(instance))
            assert("failed loading vulkan module");

        // create physical device
        uint32_t devCount = 0;
        VKCHECK(vkEnumeratePhysicalDevices(instance, &devCount, nullptr));
        std::vector<VkPhysicalDevice> physicalDeviceList(devCount);
        VKCHECK(vkEnumeratePhysicalDevices(instance, &devCount, physicalDeviceList.data()));
        bool found = false;
        int32_t physicalDeviceIndex = GetPhysicalDeviceIndex(instance, physicalDeviceList, &found);
        if (!found)
            assert("vulkan physical device not found");

        const VkPhysicalDevice physicalDevice = physicalDeviceList[physicalDeviceIndex];
        VkPhysicalDeviceMemoryProperties deviceMemoryProperties;
        vkGetPhysicalDeviceMemoryProperties(physicalDevice, &deviceMemoryProperties);

        // create logical device
        uint32_t extensionCount = 0;
        VKCHECK(vkEnumerateDeviceExtensionProperties(physicalDevice, nullptr, &extensionCount, nullptr));
        std::vector<VkExtensionProperties> extensionPropertiesList(extensionCount);
        VKCHECK(vkEnumerateDeviceExtensionProperties(
            physicalDevice, nullptr, &extensionCount, extensionPropertiesList.data()));
        std::vector<const char*> availableExtensions;
        for (const auto& v : extensionPropertiesList)
        {
            availableExtensions.push_back(v.extensionName);
        }

        // queueFamilyIndex
        uint32_t propertiesCount = 0;
        vkGetPhysicalDeviceQueueFamilyProperties(physicalDevice, &propertiesCount, nullptr);
        std::vector<VkQueueFamilyProperties> properies(propertiesCount);
        vkGetPhysicalDeviceQueueFamilyProperties(physicalDevice, &propertiesCount, properies.data());
        uint32_t queueFamilyIndex = 0;
        for (uint32_t i = 0; i < propertiesCount; i++)
        {
            if (properies[i].queueFlags & VK_QUEUE_GRAPHICS_BIT)
            {
                queueFamilyIndex = i;
                break;
            }
        }

        // create device queue create info
        const float defaultQueuePriority = 1.0f;
        VkDeviceQueueCreateInfo deviceQueueCreateInfo = {};
        deviceQueueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
        deviceQueueCreateInfo.queueFamilyIndex = queueFamilyIndex;
        deviceQueueCreateInfo.pNext = nullptr;
        deviceQueueCreateInfo.queueCount = 1;
        deviceQueueCreateInfo.pQueuePriorities = &defaultQueuePriority;

        // create device create info
        VkDeviceCreateInfo deviceCreateInfo = {};
        deviceCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
        deviceCreateInfo.ppEnabledExtensionNames = deviceExtensions.data();
        deviceCreateInfo.enabledExtensionCount = static_cast<uint32_t>(deviceExtensions.size());
        deviceCreateInfo.pQueueCreateInfos = &deviceQueueCreateInfo;
        deviceCreateInfo.queueCreateInfoCount = 1;
        VkDevice device;
        VKCHECK(vkCreateDevice(physicalDevice, &deviceCreateInfo, nullptr, &device));

        if (!LoadDeviceVulkanFunction(device))
            assert("failed loading vulkan module");

        VkQueue queue;
        vkGetDeviceQueue(device, queueFamilyIndex, 0, &queue);

        UnityVulkanInstance* pVkInstance = new UnityVulkanInstance;
        pVkInstance->instance = instance;
        pVkInstance->physicalDevice = physicalDevice;
        pVkInstance->device = device;
        pVkInstance->queueFamilyIndex = queueFamilyIndex;
        pVkInstance->graphicsQueue = queue;
        return pVkInstance;
    }

    void DestroyDeviceVulkan(void* pGfxDevice)
    {
        UnityVulkanInstance* pVkInstance = static_cast<UnityVulkanInstance*>(pGfxDevice);
        vkDestroyDevice(pVkInstance->device, nullptr);
        delete pVkInstance;
        pVkInstance = nullptr;
    }

#endif

#if SUPPORT_METAL

    void* CreateDeviceMetal() { return MetalDevice::CreateForTest().release(); }

    void DestroyDeviceMetalDevice(void* ptr)
    {
        MetalDevice* device = static_cast<MetalDevice*>(ptr);
        delete device;
    }

#endif

#if SUPPORT_OPENGL_CORE

    static bool s_glutInitialized;

    void* CreateDeviceGLCore()
    {
        if (!s_glutInitialized)
        {
            int argc = 0;
            glutInit(&argc, nullptr);
            s_glutInitialized = true;
            glutCreateWindow("test");
        }
        OpenGLContext::Init();
        std::unique_ptr<OpenGLContext> context = OpenGLContext::CreateGLContext();
        return context.release();
    }

    void DestroyDeviceGLCore(void* pGfxDevice)
    {
        OpenGLContext* context = static_cast<OpenGLContext*>(pGfxDevice);
        delete context;
    }
#endif

#if SUPPORT_OPENGL_ES
    void* CreateDeviceGLES()
    {
        OpenGLContext::Init();
        std::unique_ptr<OpenGLContext> context = OpenGLContext::CreateGLContext();
        return context.release();
    }
    void DestroyDeviceGLES(void* pGfxDevice)
    {
        OpenGLContext* context = static_cast<OpenGLContext*>(pGfxDevice);
        delete context;
    }

#endif

    //---------------------------------------------------------------------------------------------------------------------

    void* CreateNativeGfxDevice(UnityGfxRenderer renderer)
    {
        switch (renderer)
        {
#if SUPPORT_D3D11
        case kUnityGfxRendererD3D11:
            return CreateDeviceD3D11();
#endif
#if SUPPORT_D3D12
        case kUnityGfxRendererD3D12:
            return CreateDeviceD3D12();
#endif
#if SUPPORT_OPENGL_CORE
        case kUnityGfxRendererOpenGLCore:
            return CreateDeviceGLCore();
#endif
#if SUPPORT_OPENGL_ES
            case kUnityGfxRendererOpenGLES20:
            case kUnityGfxRendererOpenGLES30:
                return CreateDeviceGLES();
#endif
#if SUPPORT_VULKAN
        case kUnityGfxRendererVulkan:
            return CreateDeviceVulkan();
#endif
#if SUPPORT_METAL
        case kUnityGfxRendererMetal:
            return CreateDeviceMetal();
#endif
        default:
            return nullptr;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------

    void DestroyNativeGfxDevice(void* pGfxDevice, UnityGfxRenderer renderer)
    {
        switch (renderer)
        {
#if SUPPORT_D3D11
        case kUnityGfxRendererD3D11:
            return;
#endif
#if SUPPORT_D3D12
        case kUnityGfxRendererD3D12:
            return;
#endif
#if SUPPORT_OPENGL_CORE
        case kUnityGfxRendererOpenGLCore:
            DestroyDeviceGLCore(pGfxDevice);
            return;
#endif
#if SUPPORT_OPENGL_ES
        case kUnityGfxRendererOpenGLES20:
        case kUnityGfxRendererOpenGLES30:
            DestroyDeviceGLES(pGfxDevice);
            return;
#endif
#if SUPPORT_VULKAN
        case kUnityGfxRendererVulkan:
            DestroyDeviceVulkan(pGfxDevice);
            return;
#endif
#if SUPPORT_METAL
        case kUnityGfxRendererMetal:
            DestroyDeviceMetalDevice(pGfxDevice);
            return;
#endif
        default:
            return;
        }
    }

    GraphicsDeviceContainer::GraphicsDeviceContainer(UnityGfxRenderer renderer)
    {
        nativeGfxDevice_ = CreateNativeGfxDevice(renderer);
        renderer_ = renderer;
        IGraphicsDevice* device = nullptr;
        if (renderer == kUnityGfxRendererD3D12)
        {
#if defined(SUPPORT_D3D12)
            device =
                new D3D12GraphicsDevice(static_cast<ID3D12Device*>(nativeGfxDevice_), pCommandQueue.Get(), renderer);
#endif
        }
        else
        {
            device = GraphicsDevice::GetInstance().Init(renderer, nativeGfxDevice_, nullptr);
        }
        device_ = std::unique_ptr<IGraphicsDevice>(device);
        device_->InitV();
    }
    GraphicsDeviceContainer::~GraphicsDeviceContainer() {
        device_->ShutdownV();
        DestroyNativeGfxDevice(nativeGfxDevice_, renderer_); }

    std::unique_ptr<GraphicsDeviceContainer> CreateGraphicsDeviceContainer(UnityGfxRenderer renderer)
    {
        return std::make_unique<GraphicsDeviceContainer>(renderer);
    }

}
}
