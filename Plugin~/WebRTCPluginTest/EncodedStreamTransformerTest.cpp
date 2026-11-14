#include "pch.h"

#include <api/make_ref_counted.h>

#include "EncodedStreamTransformer.h"

namespace unity
{
namespace webrtc
{
    class CountingTransformCallback : public TransformedFrameCallback
    {
    public:
        int count = 0;
        void OnTransformedFrame(std::unique_ptr<TransformableFrameInterface>) override { ++count; }
    };

    TEST(EncodedStreamTransformerTest, RemovingMissingOrRepeatedSsrcPreservesOtherSinks)
    {
        auto transformer = rtc::make_ref_counted<EncodedStreamTransformer>();
        auto callback = rtc::make_ref_counted<CountingTransformCallback>();
        transformer->RegisterTransformedFrameSinkCallback(callback, 0);
        transformer->UnregisterTransformedFrameSinkCallback(42);
        // A null frame is sufficient: the single SSRC-0 sink forwards it unchanged.
        transformer->SendFrameToSink(nullptr);
        EXPECT_EQ(callback->count, 1);

        transformer->UnregisterTransformedFrameSinkCallback(0);
        transformer->UnregisterTransformedFrameSinkCallback(0);
        transformer->UnregisterTransformedFrameSinkCallback(42);
    }
} // namespace webrtc
} // namespace unity
