#include "pch.h"
#include "UnityVideoRenderer.h"

namespace unity {
namespace webrtc {

UnityVideoRenderer::UnityVideoRenderer()
{
    //Do nothing
}

UnityVideoRenderer::~UnityVideoRenderer() = default;

void UnityVideoRenderer::OnFrame(const webrtc::VideoFrame &frame)
{
    //ToDo Implement
}

} // end namespace webrtc
} // end namespace unity
