#include "pch.h"
#include "IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

IGraphicsDevice::IGraphicsDevice(UnityGfxRenderer renderer) : m_gfxRenderer(renderer)
{
}

//---------------------------------------------------------------------------------------------------------------------

IGraphicsDevice::~IGraphicsDevice() {
}

} // end namespace webrtc
} // end namespace unity
