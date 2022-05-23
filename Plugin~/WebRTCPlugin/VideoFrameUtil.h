#pragma once

#include "VideoFrame.h"

namespace unity
{
namespace webrtc
{

    rtc::scoped_refptr<VideoFrame>
    CreateTestFrame(IGraphicsDevice* device, const ITexture2D* texture, UnityRenderingExtTextureFormat format);

} // end namespace webrtc
} // end namespace unity
