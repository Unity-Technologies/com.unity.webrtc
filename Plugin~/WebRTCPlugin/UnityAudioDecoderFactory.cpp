#include "pch.h"
#include "UnityAudioDecoderFactory.h"

#include <api/audio_codecs/opus/audio_decoder_multi_channel_opus.h>
#include <api/audio_codecs/L16/audio_decoder_L16.h>
#include <api/audio_codecs/g711/audio_decoder_g711.h>
#include <api/audio_codecs/g722/audio_decoder_g722.h>
#include <api/audio_codecs/ilbc/audio_decoder_ilbc.h>
#include <api/audio_codecs/isac/audio_decoder_isac.h>
#include <api/audio_codecs/opus/audio_decoder_opus.h>

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

template <typename T>
struct StereoSupportDecoder {
    using Config = typename T::Config;
    static absl::optional<Config> SdpToConfig(
        const SdpAudioFormat& audio_format) {
        return T::SdpToConfig(audio_format);
    }
    static void AppendSupportedDecoders(
        std::vector<AudioCodecSpec>* specs) {
        std::vector<AudioCodecSpec> new_specs;
        T::AppendSupportedDecoders(&new_specs);

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
    static std::unique_ptr<AudioDecoder> MakeAudioDecoder(
        const Config& config,
        absl::optional<AudioCodecPairId> codec_pair_id) {
        return T::MakeAudioDecoder(config, codec_pair_id);
    }
};

rtc::scoped_refptr<AudioDecoderFactory>
    CreateAudioDecoderFactory() {
    return ::webrtc::CreateAudioDecoderFactory<
        StereoSupportDecoder<AudioDecoderOpus>,
        AudioDecoderMultiChannelOpus,
        AudioDecoderIlbc,
        AudioDecoderIsac,
        AudioDecoderG722,
        AudioDecoderG711,
        AudioDecoderL16
    >();
}

} // end namespace webrtc
} // end namespace unity
