#pragma once

namespace unity
{
namespace webrtc
{
    template <typename T>
    struct StereoSupportDecoder {
        using Config = typename T::Config;
        static absl::optional<Config> SdpToConfig(
            const webrtc::SdpAudioFormat& audio_format) {
            return T::SdpToConfig(audio_format);
        }
        static void AppendSupportedDecoders(
            std::vector<webrtc::AudioCodecSpec>* specs) {
            std::vector<webrtc::AudioCodecSpec> new_specs;
            T::AppendSupportedDecoders(&new_specs);

            RTC_DCHECK_EQ(new_specs.size(), 1);

            auto spec = new_specs[0];
            if (spec.format.num_channels == 2 &&
                spec.format.parameters.find("stereo") == spec.format.parameters.end())
            {
                spec.format.parameters.emplace("stereo", "1");
                spec.info.num_channels = 2;
            }
            specs->push_back(spec);
        }
        static std::unique_ptr<webrtc::AudioDecoder> MakeAudioDecoder(
            const Config& config,
            absl::optional<webrtc::AudioCodecPairId> codec_pair_id) {
            return T::MakeAudioDecoder(config, codec_pair_id);
        }
    };

    rtc::scoped_refptr<AudioDecoderFactory> CreateAudioDecoderFactory();
} // end namespace webrtc
} // end namespace unity
