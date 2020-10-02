#pragma once
#include <mutex>
#include "api/video/video_frame.h"
#include "api/video/video_sink_interface.h"
#include "third_party/libyuv/include/libyuv.h"

namespace unity {
namespace webrtc {

    namespace webrtc = ::webrtc;

    class UnityVideoRenderer : public rtc::VideoSinkInterface<webrtc::VideoFrame>
    {
    public:
        UnityVideoRenderer(uint32_t id);
        ~UnityVideoRenderer();
        void OnFrame(const webrtc::VideoFrame &frame) override;

        uint32_t GetId();
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> GetFrameBuffer();
        void SetFrameBuffer(rtc::scoped_refptr<webrtc::VideoFrameBuffer> buffer);
        std::vector<uint8_t> tempBuffer;

        void ConvertVideoFrameToTextureAndWriteToBuffer(int width, int height, webrtc::VideoType format);

    private:
        uint32_t m_id;
        std::mutex m_mutex;
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> m_frameBuffer;
    };

} // end namespace webrtc
} // end namespace unity
