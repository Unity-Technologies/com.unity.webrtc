#pragma once

#include <api/units/time_delta.h>
#include <rtc_base/ref_counted_object.h>
#include <rtc_base/timestamp_aligner.h>

#include "GpuMemoryBuffer.h"
#include "Size.h"

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class VideoFrame : public rtc::RefCountInterface
    {
    public:
        using ReturnBufferToPoolCallback = std::function<void(rtc::scoped_refptr<GpuMemoryBufferInterface>)>;

        VideoFrame() = delete;
        VideoFrame(const VideoFrame&) = delete;
        VideoFrame& operator=(const VideoFrame&) = delete;

        Size size() const { return size_; }
        UnityRenderingExtTextureFormat format() const { return gpu_memory_buffer_->GetFormat(); }
        TimeDelta timestamp() const { return timestamp_; }
        void set_timestamp(TimeDelta timestamp) { timestamp_ = timestamp; }

        GpuMemoryBufferInterface* GetGpuMemoryBuffer() const;
        bool HasGpuMemoryBuffer() const;

        static rtc::scoped_refptr<VideoFrame> WrapExternalGpuMemoryBuffer(
            const Size& size,
            rtc::scoped_refptr<GpuMemoryBufferInterface> gpu_memory_buffer,
            ReturnBufferToPoolCallback returnBufferToPoolCallback,
            TimeDelta timestamp);

    protected:
        VideoFrame(
            const Size& size,
            rtc::scoped_refptr<GpuMemoryBufferInterface> buffer,
            ReturnBufferToPoolCallback returnBufferToPoolCallback,
            TimeDelta timestamp);
        virtual ~VideoFrame() override;

    private:
        Size size_;
        rtc::scoped_refptr<GpuMemoryBufferInterface> gpu_memory_buffer_;
        ReturnBufferToPoolCallback returnBufferToPoolCallback_;
        TimeDelta timestamp_;
    };

} // end namespace webrtc
} // end namespace unity
