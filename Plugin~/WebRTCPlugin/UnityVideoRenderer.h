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
      struct ImageData {
          uint8_t*  RawData;
          uint32_t  Width;
          uint32_t  Height;
      };
    public:
      UnityVideoRenderer(uint32_t id);
      ~UnityVideoRenderer();
      void OnFrame(const webrtc::VideoFrame &frame) override;

      uint32_t GetId();
      rtc::scoped_refptr<webrtc::VideoFrameBuffer> GetFrameBuffer();
      void SetFrameBuffer(rtc::scoped_refptr<webrtc::VideoFrameBuffer> buffer);
      ImageData* GetImageData();
      uint8_t* tempBuffer = nullptr;

    private:
      uint32_t m_id;
      std::mutex m_mutex;
      ImageData m_imageData;
      rtc::scoped_refptr<webrtc::VideoFrameBuffer> m_frameBuffer;
    };

} // end namespace webrtc
} // end namespace unity
