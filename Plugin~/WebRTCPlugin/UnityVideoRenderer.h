#pragma once
#include <mutex>
#include "api/video/video_frame.h"
#include "api/video/video_sink_interface.h"
#include "third_party/libyuv/include/libyuv.h"
#include "WebRTCPlugin.h"

namespace unity {
namespace webrtc {

    namespace webrtc = ::webrtc;

    class UnityVideoRenderer : public rtc::VideoSinkInterface<webrtc::VideoFrame>
    {
    public:
        UnityVideoRenderer(uint32_t id, DelegateVideoFrameResize callback);
        ~UnityVideoRenderer();
        void OnFrame(const webrtc::VideoFrame &frame) override;

        uint32_t GetId();
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> GetFrameBuffer();
        void SetFrameBuffer(
            rtc::scoped_refptr<webrtc::VideoFrameBuffer> buffer,
            int64_t timestamp);

        // used in UnityRenderingExtEventUpdateTexture 
        // called on RenderThread
        void* ConvertVideoFrameToTextureAndWriteToBuffer(
            int width, int height, libyuv::FourCC format);
        std::vector<uint8_t> tempBuffer;

    private:
        uint32_t m_id;
        std::mutex m_mutex;
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> m_frameBuffer;
        int64_t m_last_renderered_timestamp;
        std::atomic<int64_t> m_timestamp;
        DelegateVideoFrameResize m_callback;
    };

} // end namespace webrtc
} // end namespace unity
