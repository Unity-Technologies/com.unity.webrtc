#pragma once
#include <rtc_base/synchronization/mutex.h>

#include "WebRTCPlugin.h"

namespace webrtc
{
class TransformableFrameInterface;
class FrameTransformerInterface;
class TransformedFrameCallback;
} // namespace webrtc

namespace unity
{
namespace webrtc
{
    class EncodedStreamTransformer : public webrtc::FrameTransformerInterface
    {
    public:
        static void RegisterCallback(DelegateTransformedFrame callback) { s_callback = callback; }

        explicit EncodedStreamTransformer();
        ~EncodedStreamTransformer() override { }

        void RegisterTransformedFrameSinkCallback(
            rtc::scoped_refptr<webrtc::TransformedFrameCallback> callback, uint32_t ssrc) override;
        void RegisterTransformedFrameCallback(rtc::scoped_refptr<TransformedFrameCallback> callback) override;

        void UnregisterTransformedFrameCallback() override;
        void UnregisterTransformedFrameSinkCallback(uint32_t ssrc) override;
        void Transform(std::unique_ptr<::webrtc::TransformableFrameInterface> frame) override;
        void SendFrameToSink(std::unique_ptr<::webrtc::TransformableFrameInterface> frame);

    private:
        std::vector<std::pair<uint32_t, rtc::scoped_refptr<webrtc::TransformedFrameCallback>>> sink_callbacks_;
        mutable std::mutex mutex_;
        static DelegateTransformedFrame s_callback;
    };

} // end namespace webrtc
} // end namespace unity
