#pragma once

#include <mutex>

#include <api/video/video_frame.h>
#include <api/video/video_sink_interface.h>
#include <third_party/libyuv/include/libyuv.h>

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class UnityVideoRenderer : public rtc::VideoSinkInterface<::webrtc::VideoFrame>
    {
    public:
        UnityVideoRenderer(uint32_t id, DelegateVideoFrameResize callback, bool needFlipVertical);
        ~UnityVideoRenderer() override;
        void OnFrame(const ::webrtc::VideoFrame& frame) override;

        uint32_t GetId();
        rtc::scoped_refptr<VideoFrameBuffer> GetFrameBuffer();
        void SetFrameBuffer(rtc::scoped_refptr<VideoFrameBuffer> buffer, int64_t timestamp);

        // used in UnityRenderingExtEventUpdateTexture
        // called on RenderThread
        void* ConvertVideoFrameToTextureAndWriteToBuffer(int width, int height, libyuv::FourCC format);

    private:
        uint32_t m_id;
        std::mutex m_mutex;
        std::vector<uint8_t> tempBuffer;
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> m_frameBuffer;
        int64_t m_last_renderered_timestamp;
        std::atomic<int64_t> m_timestamp;
        DelegateVideoFrameResize m_callback;
        bool m_needFlipVertical;
    };

} // end namespace webrtc
} // end namespace unity
