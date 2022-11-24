#include "pch.h"

#include <api/make_ref_counted.h>

#include "GraphicsDevice/GraphicsDevice.h"
#include "VideoFrame.h"

namespace unity
{
namespace webrtc
{

    VideoFrame::VideoFrame(
        const Size& size,
        rtc::scoped_refptr<GpuMemoryBufferInterface> buffer,
        ReturnBufferToPoolCallback returnBufferToPoolCallback,
        TimeDelta timestamp)
        : size_(size)
        , gpu_memory_buffer_(std::move(buffer))
        , returnBufferToPoolCallback_(returnBufferToPoolCallback)
        , timestamp_(timestamp)
    {
    }

    VideoFrame::~VideoFrame()
    {
        if (returnBufferToPoolCallback_)
        {
            returnBufferToPoolCallback_(std::move(gpu_memory_buffer_));
        }
    }

    rtc::scoped_refptr<VideoFrame> VideoFrame::WrapExternalGpuMemoryBuffer(
        const Size& size,
        rtc::scoped_refptr<GpuMemoryBufferInterface> buffer,
        ReturnBufferToPoolCallback returnBufferToPoolCallback,
        TimeDelta timestamp)
    {
        return rtc::make_ref_counted<VideoFrame>(size, std::move(buffer), returnBufferToPoolCallback, timestamp);
    }

    bool VideoFrame::HasGpuMemoryBuffer() const { return gpu_memory_buffer_ != nullptr; }

    GpuMemoryBufferInterface* VideoFrame::GetGpuMemoryBuffer() const { return gpu_memory_buffer_.get(); }

} // end namespace webrtc
} // end namespace unity
