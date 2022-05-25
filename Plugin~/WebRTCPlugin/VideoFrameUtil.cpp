#include "pch.h"

#include <system_wrappers/include/clock.h>

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/ITexture2D.h"
#include "VideoFrameUtil.h"

namespace unity
{
namespace webrtc
{

    rtc::scoped_refptr<VideoFrame>
    CreateTestFrame(IGraphicsDevice* device, const ITexture2D* texture, UnityRenderingExtTextureFormat format)
    {
        NativeTexPtr ptr = NativeTexPtr(texture->GetNativeTexturePtrV());
        Size size = Size(static_cast<int>(texture->GetWidth()), static_cast<int>(texture->GetHeight()));

        rtc::scoped_refptr<GpuMemoryBufferInterface> gmb =
            new rtc::RefCountedObject<GpuMemoryBufferFromUnity>(device, ptr, size, format);

        const int64_t timestamp_us = webrtc::Clock::GetRealTimeClock()->TimeInMicroseconds();

        return VideoFrame::WrapExternalGpuMemoryBuffer(
            size, std::move(gmb), nullptr, webrtc::TimeDelta::Micros(timestamp_us));
    }

} // end namespace webrtc
} // end namespace unity
