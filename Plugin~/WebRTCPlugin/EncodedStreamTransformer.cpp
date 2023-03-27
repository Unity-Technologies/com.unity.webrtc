#include "pch.h"

#include "EncodedStreamTransformer.h"

namespace unity
{
namespace webrtc
{
    DelegateTransformedFrame EncodedStreamTransformer::s_callback = nullptr;

    EncodedStreamTransformer::EncodedStreamTransformer() { }

    void EncodedStreamTransformer::RegisterTransformedFrameSinkCallback(
        rtc::scoped_refptr<webrtc::TransformedFrameCallback> callback, uint32_t ssrc)
    {
        std::lock_guard<std::mutex> lock(mutex_);

        for (auto& callback_ : sink_callbacks_)
        {
            if (callback_.first == ssrc)
            {
                callback_.second = std::move(callback);
                return;
            }
        }
        sink_callbacks_.push_back(std::make_pair(ssrc, callback));
    }

    void
    EncodedStreamTransformer::RegisterTransformedFrameCallback(rtc::scoped_refptr<TransformedFrameCallback> callback)
    {
        RegisterTransformedFrameSinkCallback(callback, 0);
    }

    void EncodedStreamTransformer::UnregisterTransformedFrameCallback() { UnregisterTransformedFrameSinkCallback(0); }

    void EncodedStreamTransformer::UnregisterTransformedFrameSinkCallback(uint32_t ssrc)
    {
        std::lock_guard<std::mutex> lock(mutex_);

        sink_callbacks_.erase(std::remove_if(
            sink_callbacks_.begin(),
            sink_callbacks_.end(),
            [ssrc](std::pair<uint32_t, rtc::scoped_refptr<webrtc::TransformedFrameCallback>> v) {
                return v.first == ssrc;
            }));
    }

    void EncodedStreamTransformer::Transform(std::unique_ptr<::webrtc::TransformableFrameInterface> frame)
    {
        std::lock_guard<std::mutex> lock(mutex_);

        s_callback(this, frame.release());
    }

    void EncodedStreamTransformer::SendFrameToSink(std::unique_ptr<::webrtc::TransformableFrameInterface> frame)
    {
        if (sink_callbacks_.size() == 1 && sink_callbacks_[0].first == 0)
        {
            sink_callbacks_[0].second->OnTransformedFrame(std::move(frame));
            return;
        }

        for (const auto& sink_callback : sink_callbacks_)
        {
            if (sink_callback.first == frame->GetSsrc())
            {
                sink_callback.second->OnTransformedFrame(std::move(frame));
                return;
            }
        }
    }

} // end namespace webrtc
} // end namespace unity
