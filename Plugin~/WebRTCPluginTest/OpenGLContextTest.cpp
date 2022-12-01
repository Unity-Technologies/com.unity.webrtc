#include "pch.h"

#include "GraphicsDevice/OpenGL/OpenGLContext.h"
#include "GraphicsDeviceContainer.h"
#include "rtc_base/thread.h"

namespace unity
{
namespace webrtc
{
    class OpenGLContextTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    };

    TEST_P(OpenGLContextTest, CurrentContext)
    {
        std::unique_ptr<GraphicsDeviceContainer> container = CreateGraphicsDeviceContainer(GetParam());
        std::unique_ptr<OpenGLContext> context = OpenGLContext::CurrentContext();
        ASSERT_NE(context, nullptr);
    }

    TEST_P(OpenGLContextTest, CurrentContextOnOtherThread)
    {
        std::unique_ptr<GraphicsDeviceContainer> container = CreateGraphicsDeviceContainer(GetParam());
        std::unique_ptr<rtc::Thread> thread = rtc::Thread::CreateWithSocketServer();
        thread->Start();

        std::unique_ptr<OpenGLContext> context =
            thread->BlockingCall([&]() { return OpenGLContext::CurrentContext(); });
        ASSERT_EQ(context, nullptr);
    }

    TEST_P(OpenGLContextTest, CreateContextOnOtherThread)
    {
        std::unique_ptr<GraphicsDeviceContainer> container = CreateGraphicsDeviceContainer(GetParam());
        std::unique_ptr<rtc::Thread> thread = rtc::Thread::CreateWithSocketServer();
        thread->Start();

        std::unique_ptr<OpenGLContext> context = OpenGLContext::CurrentContext();
        std::unique_ptr<OpenGLContext> context2 =
            thread->BlockingCall([&]() { return OpenGLContext::CreateGLContext(context.get()); });
        ASSERT_NE(context2, nullptr);
    }

    static UnityGfxRenderer supportedOpenGL[] = {
#if SUPPORT_OPENGL_CORE & UNITY_LINUX
        kUnityGfxRendererOpenGLCore,
#endif // SUPPORT_OPENGL_UNIFIED
#if SUPPORT_OPENGL_ES
        kUnityGfxRendererOpenGLES30,
#endif // SUPPORT_OPENGL_ES
    };
    INSTANTIATE_TEST_SUITE_P(OpenGLDevice, OpenGLContextTest, testing::ValuesIn(supportedOpenGL));

} // end namespace webrtc
} // end namespace unity
