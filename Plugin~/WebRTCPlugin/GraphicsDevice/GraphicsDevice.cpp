#include "pch.h"
#include "GraphicsDevice.h"

//Graphics
#if SUPPORT_D3D11 || SUPPORT_D3D12
#include "D3D11/D3D11GraphicsDevice.h" 
#include "D3D12/D3D12GraphicsDevice.h" 
#endif

#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
#include "OpenGL/OpenGLGraphicsDevice.h"
#endif

#if SUPPORT_VULKAN
#include "Vulkan/VulkanGraphicsDevice.h"
#endif

#if SUPPORT_METAL
#include "Metal/MetalGraphicsDevice.h"
#endif

namespace unity
{
namespace webrtc
{

GraphicsDevice& GraphicsDevice::GetInstance() {
    static GraphicsDevice device;
    return device;
}

//---------------------------------------------------------------------------------------------------------------------

IGraphicsDevice* GraphicsDevice::Init(IUnityInterfaces* unityInterface) {
    const UnityGfxRenderer rendererType =
        unityInterface->Get<IUnityGraphics>()->GetRenderer();
    switch (rendererType) {
#if SUPPORT_D3D11
        case kUnityGfxRendererD3D11: {
            IUnityGraphicsD3D11* deviceInterface =
                unityInterface->Get<IUnityGraphicsD3D11>();
            return Init(rendererType, deviceInterface->GetDevice(), deviceInterface);
        }
#endif
#if SUPPORT_D3D12
        case kUnityGfxRendererD3D12: {
            IUnityGraphicsD3D12v5* deviceInterface =
                unityInterface->Get<IUnityGraphicsD3D12v5>();
            return Init(rendererType, deviceInterface->GetDevice(), deviceInterface);
        }
#endif
#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
        case kUnityGfxRendererOpenGLES20:
        case kUnityGfxRendererOpenGLES30:
        case kUnityGfxRendererOpenGLCore: {
            return Init(rendererType, nullptr, nullptr);
        }
#endif
#if SUPPORT_VULKAN
        case kUnityGfxRendererVulkan : {
            IUnityGraphicsVulkan* deviceInterface =
                unityInterface->Get<IUnityGraphicsVulkan>();
            UnityVulkanInstance vulkan = deviceInterface->Instance();
            return Init(rendererType, reinterpret_cast<void*>(&vulkan), deviceInterface);
        }
#endif
#if SUPPORT_METAL
        case kUnityGfxRendererMetal: {
            IUnityGraphicsMetal* deviceInterface =
                unityInterface->Get<IUnityGraphicsMetal>();
            return Init(rendererType, deviceInterface->MetalDevice(), deviceInterface);
            break;
        }
#endif
        default: {
            DebugError("Unsupported Unity Renderer: %d", rendererType);
            return nullptr;
        }
    }
    return nullptr;
}

//---------------------------------------------------------------------------------------------------------------------

IGraphicsDevice* GraphicsDevice::Init(
    const UnityGfxRenderer rendererType, void* device, IUnityInterface* unityInterface)
{
    IGraphicsDevice* pDevice = nullptr;
    switch (rendererType) {
#if SUPPORT_D3D11
    case kUnityGfxRendererD3D11: {
        RTC_DCHECK(device);
        pDevice = new D3D11GraphicsDevice(static_cast<ID3D11Device*>(device));
        break;
    }
#endif
#if SUPPORT_D3D12
    case kUnityGfxRendererD3D12: {
        RTC_DCHECK(device);
        pDevice = new D3D12GraphicsDevice(static_cast<ID3D12Device*>(device),
            reinterpret_cast<IUnityGraphicsD3D12v5*>(unityInterface)
        );
        break;
    }
#endif
#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
    case kUnityGfxRendererOpenGLES20:
    case kUnityGfxRendererOpenGLES30:
    case kUnityGfxRendererOpenGLCore: {
        pDevice = new OpenGLGraphicsDevice();
        break;
    }
#endif
#if SUPPORT_VULKAN
    case kUnityGfxRendererVulkan: {
        RTC_DCHECK(device);
        const UnityVulkanInstance* vulkan =
            static_cast<const UnityVulkanInstance*>(device);
        pDevice = new VulkanGraphicsDevice(
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
#if SUPPORT_METAL
    case kUnityGfxRendererMetal: {
        RTC_DCHECK(device);
        id<MTLDevice> metalDevice = reinterpret_cast<id<MTLDevice>>(device);
        IUnityGraphicsMetal* metalUnityInterface =
            reinterpret_cast<IUnityGraphicsMetal*>(unityInterface);
        pDevice = new MetalGraphicsDevice(metalDevice, metalUnityInterface);
        break;
    }
#endif
    default: {
        DebugError("Unsupported Unity Renderer: %d", rendererType);
        return nullptr;
    }
    }
    return pDevice;
}

//---------------------------------------------------------------------------------------------------------------------

GraphicsDevice::GraphicsDevice()
{
}

} // end namespace webrtc
} // end namespace unity
