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

    GraphicsDeviceTestBase::~GraphicsDeviceTestBase() { }

    IGraphicsDevice* GraphicsDeviceTestBase::device() { return container_->device(); }

} // end namespace webrtc
} // end namespace unity
