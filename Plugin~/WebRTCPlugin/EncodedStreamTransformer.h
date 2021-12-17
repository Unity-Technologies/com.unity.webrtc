#pragma once
#include "WebRTCPlugin.h"

namespace webrtc {
class TransformableFrameInterface;
class FrameTransformerInterface;
class TransformedFrameCallback;
}  // namespace webrtc

namespace unity
{
namespace webrtc
{
class EncodedStreamTransformer
    : public webrtc::FrameTransformerInterface
{
public:
    explicit EncodedStreamTransformer(DelegateTransformedFrame callback);
    ~EncodedStreamTransformer() override { callback_ = nullptr; }

    void RegisterTransformedFrameSinkCallback(
        rtc::scoped_refptr<webrtc::TransformedFrameCallback> callback,
        uint32_t ssrc) override;
    void UnregisterTransformedFrameSinkCallback(uint32_t ssrc) override;
    void Transform(
        std::unique_ptr<::webrtc::TransformableFrameInterface> frame) override;
private:
    DelegateTransformedFrame callback_;
    std::vector<
        std::pair<uint32_t, rtc::scoped_refptr<webrtc::TransformedFrameCallback>>>
        sink_callbacks_;
    mutable webrtc::Mutex mutex_;
};

} // end namespace webrtc
} // end namespace unity
