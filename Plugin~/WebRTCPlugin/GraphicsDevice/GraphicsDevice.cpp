#include "pch.h"

#include "GraphicsDevice.h"

#if SUPPORT_D3D11 && SUPPORT_D3D12
#include "D3D11/D3D11GraphicsDevice.h"
#include "D3D12/D3D12GraphicsDevice.h"
#endif

#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
#include "OpenGL/OpenGLGraphicsDevice.h"
#endif

#if SUPPORT_VULKAN
#include "UnityVulkanInterfaceFunctions.h"
#include "Vulkan/VulkanGraphicsDevice.h"
#endif

#if SUPPORT_METAL
#include "Metal/MetalDevice.h"
#include "Metal/MetalGraphicsDevice.h"
#endif

namespace unity
{
namespace webrtc
{

    GraphicsDevice& GraphicsDevice::GetInstance()
    {
        static GraphicsDevice device;
        return device;
    }

    IGraphicsDevice* GraphicsDevice::Init(IUnityInterfaces* unityInterfaces, ProfilerMarkerFactory* profiler)
    {
        const UnityGfxRenderer rendererType = unityInterfaces->Get<IUnityGraphics>()->GetRenderer();
        switch (rendererType)
        {
#if SUPPORT_D3D11
        case kUnityGfxRendererD3D11:
        {
            IUnityGraphicsD3D11* deviceInterface = unityInterfaces->Get<IUnityGraphicsD3D11>();
            return Init(rendererType, deviceInterface->GetDevice(), deviceInterface, profiler);
        }
#endif
#if SUPPORT_D3D12
        case kUnityGfxRendererD3D12:
        {
            IUnityGraphicsD3D12v5* deviceInterface = unityInterfaces->Get<IUnityGraphicsD3D12v5>();
            return Init(rendererType, deviceInterface->GetDevice(), deviceInterface, profiler);
        }
#endif
#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
        case kUnityGfxRendererOpenGLES20:
        case kUnityGfxRendererOpenGLES30:
        case kUnityGfxRendererOpenGLCore:
        {
            return Init(rendererType, nullptr, nullptr, profiler);
        }
#endif
#if SUPPORT_VULKAN
        case kUnityGfxRendererVulkan:
        {
            UnityGraphicsVulkan* deviceInterface = UnityGraphicsVulkan::Get(unityInterfaces).release();
            UnityVulkanInstance vulkan = deviceInterface->Instance();
            return Init(
                rendererType, reinterpret_cast<void*>(&vulkan), reinterpret_cast<void*>(deviceInterface), profiler);
        }
#endif
#if SUPPORT_METAL
        case kUnityGfxRendererMetal:
        {
            std::unique_ptr<MetalDevice> device = MetalDevice::Create(unityInterfaces->Get<IUnityGraphicsMetal>());
            return Init(rendererType, device.release(), nullptr, profiler);
            break;
        }
#endif
        default:
        {
            return nullptr;
        }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------

    IGraphicsDevice* GraphicsDevice::Init(
        const UnityGfxRenderer renderer, void* device, void* unityInterface, ProfilerMarkerFactory* profiler)
    {
        IGraphicsDevice* pDevice = nullptr;
        switch (renderer)
        {
#if SUPPORT_D3D11
        case kUnityGfxRendererD3D11:
        {
            RTC_DCHECK(device);
            pDevice = new D3D11GraphicsDevice(static_cast<ID3D11Device*>(device), renderer, profiler);
            break;
        }
#endif
#if SUPPORT_D3D12
        case kUnityGfxRendererD3D12:
        {
            RTC_DCHECK(device);
            pDevice = new D3D12GraphicsDevice(
                static_cast<ID3D12Device*>(device),
                static_cast<IUnityGraphicsD3D12v5*>(unityInterface),
                renderer,
                profiler);
            break;
        }
#endif
#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
        case kUnityGfxRendererOpenGLES20:
        case kUnityGfxRendererOpenGLES30:
        case kUnityGfxRendererOpenGLCore:
        {
            pDevice = new OpenGLGraphicsDevice(renderer, profiler);
            break;
        }
#endif
#if SUPPORT_VULKAN
        case kUnityGfxRendererVulkan:
        {
            RTC_DCHECK(device);
            const UnityVulkanInstance* vulkan = static_cast<const UnityVulkanInstance*>(device);
            pDevice = new VulkanGraphicsDevice(
                static_cast<UnityGraphicsVulkan*>(unityInterface),
                vulkan->instance,
                vulkan->physicalDevice,
                vulkan->device,
                vulkan->graphicsQueue,
                vulkan->queueFamilyIndex,
                renderer,
                profiler);
            break;
        }
#endif
#if SUPPORT_METAL
        case kUnityGfxRendererMetal:
        {
            RTC_DCHECK(device);
            MetalDevice* metalDevice = static_cast<MetalDevice*>(device);
            pDevice = new MetalGraphicsDevice(metalDevice, renderer, profiler);
            break;
        }
#endif
        default:
        {
            DebugError("Unsupported Unity Renderer: %d", renderer);
            return nullptr;
        }
        }
        return pDevice;
    }

    //---------------------------------------------------------------------------------------------------------------------

    GraphicsDevice::GraphicsDevice() { }

} // end namespace webrtc
} // end namespace unity
