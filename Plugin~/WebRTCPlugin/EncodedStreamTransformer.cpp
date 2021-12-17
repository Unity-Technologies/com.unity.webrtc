#include "pch.h"
#include "EncodedStreamTransformer.h"

namespace unity
{
namespace webrtc
{

EncodedStreamTransformer::EncodedStreamTransformer(DelegateTransformedFrame callback)
    : callback_(callback)
{
}

void EncodedStreamTransformer::RegisterTransformedFrameSinkCallback(
    rtc::scoped_refptr<webrtc::TransformedFrameCallback> callback,
    uint32_t ssrc)
{
    webrtc::MutexLock lock(&mutex_);

    for (auto& callback_ : sink_callbacks_) {
        if (callback_.first == ssrc) {
            callback_.second = std::move(callback);
            return;
        }
    }
    sink_callbacks_.push_back(std::make_pair(ssrc, callback));
}

void EncodedStreamTransformer::UnregisterTransformedFrameSinkCallback(
    uint32_t ssrc)
{
    webrtc::MutexLock lock(&mutex_);
    
    sink_callbacks_.erase(std::remove_if(
        sink_callbacks_.begin(),
        sink_callbacks_.end(),
        [ssrc](std::pair<uint32_t, rtc::scoped_refptr<webrtc::TransformedFrameCallback>> v) {
            return v.first == ssrc;
        }
    ));
}

void EncodedStreamTransformer::Transform(
    std::unique_ptr<::webrtc::TransformableFrameInterface> frame)
{
    webrtc::MutexLock lock(&mutex_);

    if (callback_ != nullptr)
        callback_(this, frame.get());

    for (const auto& sink_callback : sink_callbacks_) {
        if (sink_callback.first == frame->GetSsrc()) {
            sink_callback.second->OnTransformedFrame(std::move(frame));
            return;
        }
    }
}

} // end namespace webrtc
} // end namespace unity
