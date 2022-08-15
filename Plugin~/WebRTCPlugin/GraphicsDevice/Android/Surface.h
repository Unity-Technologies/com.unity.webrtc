#pragma once

namespace unity
{
namespace webrtc
{
    class NativeFrameBuffer;
    class Surface
    {
    public:
        Surface() { }
        virtual ~Surface() = default;

        virtual void DrawFrame(const NativeFrameBuffer* buffer) {};
        virtual void SwapBuffers() {};
    };

    std::unique_ptr<Surface> CreateVulkanSurface(VkSurfaceKHR surface, VkDevice device, VkPhysicalDevice physicalDevice);
    std::unique_ptr<Surface> CreateEGLSurface();
}
}