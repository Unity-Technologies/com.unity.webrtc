#pragma once
#include <IUnityGraphics.h>
#include <IUnityRenderingExtensions.h>
#include <gmock/gmock.h>
#include <gtest/gtest.h>

#include "PlatformBase.h"

namespace unity
{
namespace webrtc
{
    class IGraphicsDevice;
    class GraphicsDeviceContainer
    {
    public:
        GraphicsDeviceContainer(UnityGfxRenderer renderer);
        virtual ~GraphicsDeviceContainer();
        IGraphicsDevice* device() { return device_.get(); }

    private:
        std::unique_ptr<IGraphicsDevice> device_;
        UnityGfxRenderer renderer_;
        void* nativeGfxDevice_;
    };

    std::unique_ptr<GraphicsDeviceContainer> CreateGraphicsDeviceContainer(UnityGfxRenderer renderer);

    static UnityGfxRenderer supportedGfxDevices[] = {
#if SUPPORT_D3D11
        kUnityGfxRendererD3D11,
#endif // SUPPORT_D3D11
#if SUPPORT_D3D12
        kUnityGfxRendererD3D12,
#endif // SUPPORT_D3D12
#if SUPPORT_METAL
        kUnityGfxRendererMetal,
#endif // SUPPORT_METAL
// todo::(kazuki) windows support
#if SUPPORT_OPENGL_UNIFIED & UNITY_LINUX
        kUnityGfxRendererOpenGLCore,
#endif // SUPPORT_OPENGL_UNIFIED
#if SUPPORT_OPENGL_ES
        kUnityGfxRendererOpenGLES30,
#endif // SUPPORT_OPENGL_ES
#if SUPPORT_VULKAN
        kUnityGfxRendererVulkan,
#endif // SUPPORT_VULKAN
        kUnityGfxRendererNull
    };

} // end namespace webrtc
} // end namespace unity
