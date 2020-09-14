#include "pch.h"

#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"
#include "GraphicsDeviceTestBase.h"

namespace unity
{
namespace webrtc
{

    //---------------------------------------------------------------------------------------------------------------------

    GraphicsDeviceTestBase::GraphicsDeviceTestBase()
    {
        std::tie(m_unityGfxRenderer, m_textureFormat) = GetParam();
        container_ = std::make_unique<GraphicsDeviceContainer>(m_unityGfxRenderer);
    }

    void GraphicsDeviceTestBase::SetUp()
    {
#if defined(LEAK_SANITIZER)
        __lsan_disable();
        __lsan_enable();
#endif
    }

    void GraphicsDeviceTestBase::TearDown()
    {
#if defined(LEAK_SANITIZER)
        ASSERT_EQ(__lsan_do_recoverable_leak_check(), 0);
#endif
    }

    GraphicsDeviceTestBase::~GraphicsDeviceTestBase() { }

    IGraphicsDevice* GraphicsDeviceTestBase::device() { return container_->device(); }

} // end namespace webrtc
} // end namespace unity
