#pragma once
#include "api/video/video_frame.h"
#include "api/video/video_sink_interface.h"

namespace unity {
namespace webrtc {

    namespace webrtc = ::webrtc;

    class UnityVideoRenderer : public rtc::VideoSinkInterface<webrtc::VideoFrame>
    {
    public:
      UnityVideoRenderer();
      ~UnityVideoRenderer();
      void OnFrame(const webrtc::VideoFrame &frame) override;

    private:
      void *frame_;
    };

} // end namespace webrtc
} // end namespace unity
