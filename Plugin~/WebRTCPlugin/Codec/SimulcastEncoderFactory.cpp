#include "pch.h"

#include <memory>
#include <vector>

#include <absl/strings/match.h>
#include <api/video_codecs/sdp_video_format.h>
#include <api/video_codecs/video_encoder.h>
#include <media/base/codec.h>
#include <media/base/media_constants.h>
#include <media/engine/internal_encoder_factory.h>
#include <media/engine/simulcast_encoder_adapter.h>
#include <rtc_base/checks.h>

#include "SimulcastEncoderFactory.h"

namespace unity
{
namespace webrtc
{

    namespace
    {

        using namespace ::webrtc;

        // This class wraps the internal factory and adds simulcast.
        class SimulcastEncoderFactory : public VideoEncoderFactory
        {
        public:
            SimulcastEncoderFactory(std::unique_ptr<VideoEncoderFactory> factory)
                : internal_encoder_factory_(std::move(factory))
            {
            }

            VideoEncoderFactory::CodecSupport
            QueryCodecSupport(const SdpVideoFormat& format, absl::optional<std::string> scalability_mode) const override
            {
                // Format must be one of the internal formats.
                RTC_DCHECK(format.IsCodecInList(internal_encoder_factory_->GetSupportedFormats()));
                return internal_encoder_factory_->QueryCodecSupport(format, scalability_mode);
            }

            std::unique_ptr<VideoEncoder> CreateVideoEncoder(const SdpVideoFormat& format) override
            {
                // Try creating internal encoder.
                std::unique_ptr<VideoEncoder> internal_encoder;
                if (format.IsCodecInList(internal_encoder_factory_->GetSupportedFormats()))
                {
                    internal_encoder =
                        std::make_unique<SimulcastEncoderAdapter>(internal_encoder_factory_.get(), format);
                }

                return internal_encoder;
            }

            std::vector<SdpVideoFormat> GetSupportedFormats() const override
            {
                return internal_encoder_factory_->GetSupportedFormats();
            }

        private:
            const std::unique_ptr<VideoEncoderFactory> internal_encoder_factory_;
        };

    } // namespace

    VideoEncoderFactory* CreateSimulcastEncoderFactory(std::unique_ptr<VideoEncoderFactory> factory)
    {
        return new SimulcastEncoderFactory(std::move(factory));
    }

} // namespace webrtc
} // namespace unity
