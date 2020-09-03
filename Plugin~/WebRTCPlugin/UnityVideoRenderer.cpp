#include "pch.h"
#include "UnityVideoRenderer.h"

namespace unity {
namespace webrtc {

UnityVideoRenderer::UnityVideoRenderer()
{
    //Do nothing
    RTC_LOG(LS_INFO) << "Create UnityVideoRenderer";
}

UnityVideoRenderer::~UnityVideoRenderer() = default;

void UnityVideoRenderer::OnFrame(const webrtc::VideoFrame &frame)
{
    //ToDo Implement
    RTC_LOG(LS_INFO) << "Invoked OnFrame on UnityVideoRenderer";
}

} // end namespace webrtc
} // end namespace unity
