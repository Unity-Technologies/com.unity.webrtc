#include "pch.h"

#include <api/audio_codecs/L16/audio_encoder_L16.h>
#include <api/audio_codecs/audio_encoder_factory_template.h>
#include <api/audio_codecs/g711/audio_encoder_g711.h>
#include <api/audio_codecs/g722/audio_encoder_g722.h>
#include <api/audio_codecs/ilbc/audio_encoder_ilbc.h>
#include <api/audio_codecs/opus/audio_encoder_multi_channel_opus.h>
#include <api/audio_codecs/opus/audio_encoder_opus.h>

#include "UnityAudioEncoderFactory.h"

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

    template<typename T>
    struct StereoSupportEncoder
    {
        using Config = typename T::Config;
        static absl::optional<Config> SdpToConfig(const SdpAudioFormat& audio_format)
        {
            return T::SdpToConfig(audio_format);
        }
        static void AppendSupportedEncoders(std::vector<AudioCodecSpec>* specs)
        {
            std::vector<AudioCodecSpec> new_specs;
            T::AppendSupportedEncoders(&new_specs);

            RTC_DCHECK_EQ(new_specs.size(), 1);

            auto spec = new_specs[0];
            if (spec.format.num_channels == 2)
            {
                if (spec.format.parameters.find("stereo") == spec.format.parameters.end())
                    spec.format.parameters.emplace("stereo", "1");
                if (spec.format.parameters.find("sprop-stereo") == spec.format.parameters.end())
                    spec.format.parameters.emplace("sprop-stereo", "1");
                spec.info.num_channels = 2;
            }
            specs->push_back(spec);
        }
        static AudioCodecInfo QueryAudioEncoder(const Config& config) { return T::QueryAudioEncoder(config); }
        static std::unique_ptr<AudioEncoder> MakeAudioEncoder(
            const Config& config, int payload_type, absl::optional<AudioCodecPairId> codec_pair_id = absl::nullopt)
        {
            return T::MakeAudioEncoder(config, payload_type, codec_pair_id);
        }
    };

    rtc::scoped_refptr<AudioEncoderFactory> CreateAudioEncoderFactory()
    {
        return ::webrtc::CreateAudioEncoderFactory<
            StereoSupportEncoder<AudioEncoderOpus>,
            AudioEncoderMultiChannelOpus,
            AudioEncoderIlbc,
            AudioEncoderG722,
            AudioEncoderG711,
            AudioEncoderL16>();
    }

} // end namespace webrtc
} // end namespace unity
