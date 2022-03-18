#pragma once

namespace unity
{
namespace webrtc
{
    class OpenGLContext
    {
    public:
        // Call the function on the render thread in Unity.
        static void InitGLContext();

        // Create the context related the thread.
        // Throw exception when the context is already created on the thread.
        static std::unique_ptr<OpenGLContext> CreateGLContext();

        // Whether the context has been created on the thread.
        static bool CurrentContext();
    };
}
}