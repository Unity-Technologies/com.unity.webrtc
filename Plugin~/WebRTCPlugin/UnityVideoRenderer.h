#pragma once
#include <mutex>
#include "api/video/video_frame.h"
#include "api/video/video_sink_interface.h"

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
      UnityVideoRenderer();
      ~UnityVideoRenderer();
      void OnFrame(const webrtc::VideoFrame &frame) override;

      std::mutex m_mutex;
      ImageData* GetFrameBuffer();

    private:
      ImageData m_imageData;
    };

} // end namespace webrtc
} // end namespace unity
