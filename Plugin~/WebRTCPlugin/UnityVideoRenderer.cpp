#include "pch.h"
#include "UnityVideoRenderer.h"

namespace unity {
namespace webrtc {

UnityVideoRenderer::UnityVideoRenderer()
{
    //Do nothing
    DebugLog("Create UnityVideoRenderer");
}

UnityVideoRenderer::~UnityVideoRenderer() = default;

void UnityVideoRenderer::OnFrame(const webrtc::VideoFrame &frame)
{
    //ToDo Implement
    DebugLog("Invoked OnFrame on UnityVideoRenderer");
}

} // end namespace webrtc
} // end namespace unity
