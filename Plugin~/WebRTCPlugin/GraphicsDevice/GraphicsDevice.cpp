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

#include "Vulkan/VulkanGraphicsDevice.h"

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
            void* device = unityInterface->Get<IUnityGraphicsD3D11>()->GetDevice();
            return Init(rendererType, device);
        }
#endif
#if defined(SUPPORT_D3D12)
        case kUnityGfxRendererD3D12: {
            void* device = unityInterface->Get<IUnityGraphicsD3D12>()->GetDevice();
            return Init(rendererType, device);
        }
#endif
        case kUnityGfxRendererOpenGLCore: {
            return Init(rendererType, nullptr);
        }
        case kUnityGfxRendererVulkan : {
            UnityVulkanInstance vulkan = unityInterface->Get<IUnityGraphicsVulkan>()->Instance();
            return Init(m_rendererType, reinterpret_cast<void*>(&vulkan));
        }
        default: {
            DebugError("Unsupported Unity Renderer: %d", m_rendererType);
            return false;
        }
    }
    return false;
}

//---------------------------------------------------------------------------------------------------------------------

bool GraphicsDevice::Init(const UnityGfxRenderer rendererType, void* device)
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
    case kUnityGfxRendererVulkan: {
        const UnityVulkanInstance* vulkan = reinterpret_cast<const UnityVulkanInstance*>(device);
        m_device = new VulkanGraphicsDevice(
            vulkan->instance,
            vulkan->physicalDevice,
            vulkan->device,
            vulkan->graphicsQueue
        );
        break;
    }
    default: {
        DebugError("Unsupported Unity Renderer: %d", rendererType);
        return false;
    }
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

