#pragma once

#include "base/callback.h"

namespace unity
{
namespace webrtc
{
    struct VideoCaptureFeedback;

    using VideoCaptureFeedbackCB =
    base::RepeatingCallback<void(const VideoCaptureFeedback&)>;

    struct VideoCaptureFeedback
    {
        float maxFramerate;
    };
}
}