#pragma once

#include <functional>

namespace unity
{
namespace webrtc
{
    struct VideoCaptureFeedback
    {
        float maxFramerate;
    };

    using VideoCaptureFeedbackCB = std::function<void(const VideoCaptureFeedback&)>;

}
}
