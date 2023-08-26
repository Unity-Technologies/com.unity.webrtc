#include "pch.h"

#include "GraphicsDevice/GraphicsDevice.h"
#include "GraphicsDeviceContainer.h"

#if SUPPORT_D3D11
#include "GraphicsDevice/D3D12/D3D12GraphicsDevice.h"
#endif

#if SUPPORT_METAL
#include "GraphicsDevice/Metal/MetalDevice.h"
#endif

#if SUPPORT_OPENGL_CORE
#include "GraphicsDevice/OpenGL/OpenGLContext.h"
#include <GLFW/glfw3.h>
#include <sanitizer/lsan_interface.h>
#endif

#if SUPPORT_OPENGL_ES
#include "GraphicsDevice/OpenGL/OpenGLContext.h"
#endif

#if SUPPORT_VULKAN

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/CudaContext.h"
#include "NvCodecUtils.h"
#endif

#include "GraphicsDevice/Vulkan/VulkanUtility.h"
#endif

#if _WIN32
// nonstandard extension used : class rvalue used as lvalue
#pragma clang diagnostic ignored "-Wlanguage-extension-token"
#endif

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    // For symbol compatibility with the plugin
    static IGraphicsDevice* s_gfxDevice = nullptr;
    IGraphicsDevice* Plugin::GraphicsDevice() { return s_gfxDevice; }

#if defined(SUPPORT_D3D11) // D3D11

    using namespace Microsoft::WRL;

    ComPtr<IDXGIFactory1> pFactory;
    ComPtr<IDXGIAdapter> pAdapter;
    ComPtr<ID3D11Device> pD3D11Device;
    ComPtr<ID3D11DeviceContext> pD3D11DeviceContext;
    ComPtr<IDXGIAdapter1> pAdapter1;
    ComPtr<IDXGIFactory4> pFactory4;
    ComPtr<ID3D12Device5> pD3D12Device;
    ComPtr<ID3D12CommandQueue> pCommandQueue;

    const int kD3D12NodeMask = 0;

    //---------------------------------------------------------------------------------------------------------------------

    static void* CreateDeviceD3D11()
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
    static void GetHardwareAdapter(IDXGIFactory2* pDXGIFactory2, IDXGIAdapter1** ppAdapter)
    {
        Microsoft::WRL::ComPtr<IDXGIAdapter1> adapter;
        *ppAdapter = nullptr;

        for (UINT adapterIndex = 0; DXGI_ERROR_NOT_FOUND != pDXGIFactory2->EnumAdapters1(adapterIndex, &adapter);
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

    static void* CreateDeviceD3D12()
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

    static LIBRARY_TYPE s_library = nullptr;

    static bool LoadVulkanModule()
    {
        if (!LoadVulkanLibrary(s_library))
            return false;
        if (!LoadExportedVulkanFunction(s_library))
            return false;
        return LoadGlobalVulkanFunction();
    }

    static int32_t
    GetPhysicalDeviceIndex(VkInstance instance, std::vector<VkPhysicalDevice>& list, bool findCudaDevice, bool* found)
    {
        std::array<uint8_t, VK_UUID_SIZE> deviceUUID;
        for (size_t i = 0; i < list.size(); ++i)
        {
            VkPhysicalDevice physicalDevice = list[i];
            if (!VulkanUtility::GetPhysicalDeviceUUID(instance, physicalDevice, &deviceUUID))
            {
                continue;
            }
#if CUDA_PLATFORM
            if (findCudaDevice && CudaContext::FindCudaDevice(deviceUUID.data(), nullptr) != CUDA_SUCCESS)
            {
                continue;
            }
#endif
            *found = true;
            return static_cast<int32_t>(i);
        }
        *found = false;
        return 0;
    }

    static void* CreateDeviceVulkan()
    {
        // Extension
        std::vector<const char*> instanceExtensions = {
            VK_KHR_SURFACE_EXTENSION_NAME,
#ifdef _WIN32
            VK_KHR_WIN32_SURFACE_EXTENSION_NAME,
#endif
#if defined(_DEBUG)
            VK_EXT_DEBUG_REPORT_EXTENSION_NAME,
#endif
            VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME
        };

        std::vector<const char*> deviceExtensions = {

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
        {
            RTC_LOG(LS_INFO) << "failed loading vulkan module";
            return nullptr;
        }

        VkInstanceCreateInfo instanceInfo {};
        instanceInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
        instanceInfo.enabledExtensionCount = static_cast<uint32_t>(instanceExtensions.size());
        instanceInfo.ppEnabledExtensionNames = instanceExtensions.data();
        instanceInfo.pApplicationInfo = &appInfo;
        VkInstance instance = nullptr;
        VkResult result = vkCreateInstance(&instanceInfo, nullptr, &instance);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkCreateInstance failed. error:" << result;
            return nullptr;
        }

        if (!LoadInstanceVulkanFunction(instance))
        {
            RTC_LOG(LS_INFO) << "LoadInstanceVulkanFunction failed";
            return nullptr;
        }

        // create physical device
        uint32_t devCount = 0;
        result = vkEnumeratePhysicalDevices(instance, &devCount, nullptr);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkEnumeratePhysicalDevices failed. error:" << result;
            return nullptr;
        }
        std::vector<VkPhysicalDevice> physicalDeviceList(devCount);
        result = vkEnumeratePhysicalDevices(instance, &devCount, physicalDeviceList.data());
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkEnumeratePhysicalDevices failed. error:" << result;
            return nullptr;
        }

        if (!VulkanUtility::LoadInstanceFunctions(instance))
        {
            return nullptr;
        }

        bool found = false;
        // todo:: Add test pattern for HWA codecs.
        bool findCudaDevice = false;
        int32_t physicalDeviceIndex = GetPhysicalDeviceIndex(instance, physicalDeviceList, findCudaDevice, &found);
        if (!found)
        {
            RTC_LOG(LS_INFO) << "GetPhysicalDeviceIndex device not found.";
            return nullptr;
        }
        const VkPhysicalDevice physicalDevice = physicalDeviceList[static_cast<size_t>(physicalDeviceIndex)];
        VkPhysicalDeviceMemoryProperties deviceMemoryProperties;
        vkGetPhysicalDeviceMemoryProperties(physicalDevice, &deviceMemoryProperties);

        // create logical device
        uint32_t extensionCount = 0;
        result = vkEnumerateDeviceExtensionProperties(physicalDevice, nullptr, &extensionCount, nullptr);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkEnumerateDeviceExtensionProperties failed. error:" << result;
            return nullptr;
        }
        std::vector<VkExtensionProperties> extensionPropertiesList(extensionCount);
        result = vkEnumerateDeviceExtensionProperties(
            physicalDevice, nullptr, &extensionCount, extensionPropertiesList.data());
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkEnumerateDeviceExtensionProperties failed. error:" << result;
            return nullptr;
        }
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
        result = vkCreateDevice(physicalDevice, &deviceCreateInfo, nullptr, &device);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkCreateDevice failed. error:" << result;
            return nullptr;
        }
        if (!LoadDeviceVulkanFunction(device))
        {
            RTC_LOG(LS_INFO) << "Failed loading vulkan module";
            return nullptr;
        }

        VkQueue queue;
        vkGetDeviceQueue(device, queueFamilyIndex, 0, &queue);

        UnityVulkanInstance* pVkInstance = new UnityVulkanInstance;
        pVkInstance->pipelineCache = nullptr;
        pVkInstance->instance = instance;
        pVkInstance->physicalDevice = physicalDevice;
        pVkInstance->device = device;
        pVkInstance->graphicsQueue = queue;
        pVkInstance->getInstanceProcAddr = nullptr;
        pVkInstance->queueFamilyIndex = queueFamilyIndex;
        return pVkInstance;
    }

    static void DestroyDeviceVulkan(void* pGfxDevice)
    {
        UnityVulkanInstance* pVkInstance = static_cast<UnityVulkanInstance*>(pGfxDevice);
        vkDestroyDevice(pVkInstance->device, nullptr);
        delete pVkInstance;
        pVkInstance = nullptr;
    }

#endif

#if SUPPORT_METAL

    static void* CreateDeviceMetal() { return MetalDevice::CreateForTest().release(); }

    static void DestroyDeviceMetalDevice(void* ptr)
    {
        MetalDevice* device = static_cast<MetalDevice*>(ptr);
        delete device;
    }

#endif

#if SUPPORT_OPENGL_CORE

    static bool s_glfwInitialized;
    static GLFWwindow* s_window;

    static void* CreateDeviceGLCore()
    {
        if (!s_glfwInitialized)
        {
            glfwInit();
            glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
            glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 2);
            glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);

            const GLuint kWidth = 320;
            const GLuint kHeight = 240;
            __lsan_disable();
            s_window = glfwCreateWindow(kWidth, kHeight, "test", nullptr, nullptr);
            __lsan_enable();
            glfwMakeContextCurrent(s_window);
            s_glfwInitialized = true;
        }
        OpenGLContext::Init();
        std::unique_ptr<OpenGLContext> context = OpenGLContext::CreateGLContext();
        return context.release();
    }

    static void DestroyDeviceGLCore(void* pGfxDevice)
    {
        OpenGLContext* context = static_cast<OpenGLContext*>(pGfxDevice);
        delete context;

        glfwSetWindowShouldClose(s_window, GL_TRUE);
        glfwTerminate();
        s_glfwInitialized = false;
    }
#endif

#if SUPPORT_OPENGL_ES
    static void* CreateDeviceGLES()
    {
        OpenGLContext::Init();
        std::unique_ptr<OpenGLContext> context = OpenGLContext::CreateGLContext();
        return context.release();
    }

    static void DestroyDeviceGLES(void* pGfxDevice)
    {
        OpenGLContext* context = static_cast<OpenGLContext*>(pGfxDevice);
        delete context;
    }

#endif

    //---------------------------------------------------------------------------------------------------------------------

    static void* CreateNativeGfxDevice(UnityGfxRenderer renderer)
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

    static void DestroyNativeGfxDevice(void* pGfxDevice, UnityGfxRenderer renderer)
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
        : device_(nullptr)
        , nativeGfxDevice_(nullptr)
    {
        nativeGfxDevice_ = CreateNativeGfxDevice(renderer);
        renderer_ = renderer;

        // native graphics device is not initialized.
        if (!nativeGfxDevice_)
            return;

        IGraphicsDevice* device = nullptr;
        if (renderer == kUnityGfxRendererD3D12)
        {
#if defined(SUPPORT_D3D12)
            device = new D3D12GraphicsDevice(
                static_cast<ID3D12Device*>(nativeGfxDevice_), pCommandQueue.Get(), renderer, nullptr);
#endif
        }
        else
        {
            device = GraphicsDevice::GetInstance().Init(renderer, nativeGfxDevice_, nullptr, nullptr);
        }
        device_ = std::unique_ptr<IGraphicsDevice>(device);
        s_gfxDevice = device_.get();
        EXPECT_TRUE(device_->InitV());
    }

    GraphicsDeviceContainer::~GraphicsDeviceContainer()
    {
        if (device_)
            device_->ShutdownV();
        s_gfxDevice = nullptr;
        if (nativeGfxDevice_)
            DestroyNativeGfxDevice(nativeGfxDevice_, renderer_);
    }

    std::unique_ptr<GraphicsDeviceContainer> CreateGraphicsDeviceContainer(UnityGfxRenderer renderer)
    {
        return std::make_unique<GraphicsDeviceContainer>(renderer);
    }

}
}
