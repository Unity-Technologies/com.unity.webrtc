#include "pch.h"
#include "GraphicsDevice.h"

//Graphics
#if defined(SUPPORT_D3D11) || defined(SUPPORT_D3D12)
#include "D3D11/D3D11GraphicsDevice.h" 
#include "D3D12/D3D12GraphicsDevice.h" 
#endif

#if defined(SUPPORT_OPENGL_CORE)
#include "OpenGL/OpenGLGraphicsDevice.h"
#endif

#if defined(SUPPORT_VULKAN)
#include "Vulkan/VulkanGraphicsDevice.h"
#endif

#if defined(SUPPORT_METAL)
#include "Metal/MetalGraphicsDevice.h"
#endif

namespace WebRTC {

GraphicsDevice& GraphicsDevice::GetInstance() {
    static GraphicsDevice device;
    return device;
}

//---------------------------------------------------------------------------------------------------------------------

bool GraphicsDevice::Init(IUnityInterfaces* unityInterface) {
    const UnityGfxRenderer rendererType = unityInterface->Get<IUnityGraphics>()->GetRenderer();
    switch (rendererType) {
#if defined(SUPPORT_D3D11)
        case kUnityGfxRendererD3D11: {
            IUnityGraphicsD3D11* deviceInterface = unityInterface->Get<IUnityGraphicsD3D11>();
            return Init(rendererType, deviceInterface->GetDevice(), deviceInterface);
        }
#endif
#if defined(SUPPORT_D3D12)
        case kUnityGfxRendererD3D12: {
            IUnityGraphicsD3D12* deviceInterface = unityInterface->Get<IUnityGraphicsD3D12>();
            return Init(rendererType, deviceInterface->GetDevice(), deviceInterface);
        }
#endif
        case kUnityGfxRendererOpenGLCore: {
            return Init(rendererType, nullptr, nullptr);
        }
#if defined(SUPPORT_VULKAN)
        case kUnityGfxRendererVulkan : {
            IUnityGraphicsVulkan* deviceInterface = unityInterface->Get<IUnityGraphicsVulkan>();
            UnityVulkanInstance vulkan = deviceInterface->Instance();
            return Init(rendererType, reinterpret_cast<void*>(&vulkan), deviceInterface);
        }
#endif
        case kUnityGfxRendererMetal: {
#if defined(SUPPORT_METAL)
            IUnityGraphicsMetal* deviceInterface = unityInterface->Get<IUnityGraphicsMetal>();
            return Init(rendererType, nullptr, deviceInterface);
#endif
            break;
        }
        default: {
            DebugError("Unsupported Unity Renderer: %d", m_rendererType);
            return false;
        }
    }
    return false;
}

//---------------------------------------------------------------------------------------------------------------------

bool GraphicsDevice::Init(const UnityGfxRenderer rendererType, void* device, IUnityInterface* unityInterface)
{
    m_rendererType = rendererType;
    switch (rendererType) {
    case kUnityGfxRendererD3D11: {
#if defined(SUPPORT_D3D11)
        m_device = new D3D11GraphicsDevice(static_cast<ID3D11Device*>(device));
#endif
        break;
    }
    case kUnityGfxRendererD3D12: {
#if defined(SUPPORT_D3D12)
        m_device = new D3D12GraphicsDevice(static_cast<ID3D12Device*>(device));
#endif
        break;
    }
    case kUnityGfxRendererOpenGLCore: {
#if defined(SUPPORT_OPENGL_CORE)
        m_device = new OpenGLGraphicsDevice();
#endif
        break;
    }
#if defined(SUPPORT_VULKAN)
    case kUnityGfxRendererVulkan: {
        const UnityVulkanInstance* vulkan = reinterpret_cast<const UnityVulkanInstance*>(device);
        m_device = new VulkanGraphicsDevice(
            reinterpret_cast<IUnityGraphicsVulkan*>(unityInterface),
            vulkan->instance,
            vulkan->physicalDevice,
            vulkan->device,
            vulkan->graphicsQueue,
            vulkan->queueFamilyIndex
        );
        break;
    }
#endif
#if defined(SUPPORT_METAL)
    case kUnityGfxRendererMetal: {
        IUnityGraphicsMetal* metalUnityInterface = reinterpret_cast<IUnityGraphicsMetal*>(unityInterface);
        m_device = new MetalGraphicsDevice(metalUnityInterface->MetalDevice(), metalUnityInterface);
        break;
    }
#endif
    default: {
        DebugError("Unsupported Unity Renderer: %d", rendererType);
        return false;
    }
    }
    if(m_device == nullptr) {
        return false;
    }
    return m_device->InitV();
}

//---------------------------------------------------------------------------------------------------------------------

void GraphicsDevice::Shutdown() {
    if (nullptr != m_device) {
        m_device->ShutdownV();
        delete m_device;
        m_device = nullptr;
    }
}

//---------------------------------------------------------------------------------------------------------------------

GraphicsDevice::GraphicsDevice() : m_rendererType(static_cast<UnityGfxRenderer>(0)) {

}

}

