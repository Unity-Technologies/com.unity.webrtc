#include "pch.h"

#include <api/video_codecs/video_codec.h>
#include <media/engine/internal_decoder_factory.h>
#include <modules/video_coding/include/video_error_codes.h>

#include "Codec/CreateVideoCodecFactory.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "ProfilerMarkerFactory.h"
#include "ScopedProfiler.h"
#include "UnityVideoDecoderFactory.h"

namespace unity
{
namespace webrtc
{
    class UnityVideoDecoder : public VideoDecoder
    {
    public:
        UnityVideoDecoder(std::unique_ptr<VideoDecoder> decoder, ProfilerMarkerFactory* profiler)
            : decoder_(std::move(decoder))
            , profiler_(profiler)
            , marker_(nullptr)
            , profilerThread_(nullptr)
        {
            if (profiler)
                marker_ = profiler->CreateMarker(
                    "UnityVideoDecoder.Decode", kUnityProfilerCategoryOther, kUnityProfilerMarkerFlagDefault, 0);
        }
        ~UnityVideoDecoder() override { }

        bool Configure(const Settings& settings) override
        {
            bool result = decoder_->Configure(settings);
            if (result && !profilerThread_)
            {
                std::stringstream ss;
                ss << "Decoder ";
                ss
                    << (decoder_->GetDecoderInfo().implementation_name.empty()
                            ? "VideoDecoder"
                            : decoder_->GetDecoderInfo().implementation_name);
                ss << "(" << CodecTypeToPayloadString(settings.codec_type()) << ")";
                profilerThread_ = profiler_->CreateScopedProfilerThread("WebRTC", ss.str().c_str());
            }

            return result;
        }
        int32_t Decode(const EncodedImage& input_image, bool missing_frames, int64_t render_time_ms) override
        {
            int32_t result;
            {
                std::unique_ptr<const ScopedProfiler> profiler;
                if (profiler_)
                    profiler = profiler_->CreateScopedProfiler(*marker_);
                result = decoder_->Decode(input_image, missing_frames, render_time_ms);
            }
            return result;
        }
        int32_t RegisterDecodeCompleteCallback(DecodedImageCallback* callback) override
        {
            return decoder_->RegisterDecodeCompleteCallback(callback);
        }
        int32_t Release() override { return decoder_->Release(); }
        DecoderInfo GetDecoderInfo() const override { return decoder_->GetDecoderInfo(); }

    private:
        std::unique_ptr<VideoDecoder> decoder_;
        ProfilerMarkerFactory* profiler_;
        const UnityProfilerMarkerDesc* marker_;
        std::unique_ptr<const ScopedProfilerThread> profilerThread_;
    };

    UnityVideoDecoderFactory::UnityVideoDecoderFactory(IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler)
        : profiler_(profiler)
        , encoderFactories_()
        , decoderFactories_()
    {
        const std::vector<std::string> arrayImpl = {
            kInternalImpl, kNvCodecImpl, kAndroidMediaCodecImpl, kVideoToolboxImpl
        };

        for (auto impl : arrayImpl)
        {
            auto encoderFactory = CreateVideoEncoderFactory(impl, gfxDevice, profiler);
            if (encoderFactory)
                encoderFactories_.emplace(impl, encoderFactory);

            auto decoderFactory = CreateVideoDecoderFactory(impl, gfxDevice, profiler);
            if (decoderFactory)
                decoderFactories_.emplace(impl, decoderFactory);
        }
    }

    UnityVideoDecoderFactory::~UnityVideoDecoderFactory() = default;

    struct compare
    {
        bool operator()(const webrtc::SdpVideoFormat& lhs, const webrtc::SdpVideoFormat& rhs) const
        {
            return std::string(lhs.ToString()).compare(rhs.ToString()) > 0;
        }
    };

    struct equal
    {
        bool operator()(const webrtc::SdpVideoFormat& lhs, const webrtc::SdpVideoFormat& rhs) const
        {
            return lhs.IsSameCodec(rhs);
        }
    };

    std::vector<webrtc::SdpVideoFormat> UnityVideoDecoderFactory::GetSupportedFormats() const
    {
        // workaround
        auto formats1 = GetSupportedFormatsInFactories(encoderFactories_);
        auto formats2 = GetSupportedFormatsInFactories(decoderFactories_);
        std::vector<webrtc::SdpVideoFormat> result(formats1);

        result.insert(result.end(), formats2.begin(), formats2.end());
        std::sort(result.begin(), result.end(), compare());
        result.erase(std::unique(result.begin(), result.end(), equal()), result.end());
        return result;
    }

    std::unique_ptr<webrtc::VideoDecoder>
    UnityVideoDecoderFactory::CreateVideoDecoder(const webrtc::SdpVideoFormat& format)
    {
        VideoDecoderFactory* factory = FindCodecFactory(decoderFactories_, format);
        auto decoder = factory->CreateVideoDecoder(format);
        if (!profiler_)
            return decoder;

        // Use Unity Profiler for measuring decoding process.
        return std::make_unique<UnityVideoDecoder>(std::move(decoder), profiler_);
    }

    bool UnityVideoDecoderFactory::IsAvailableFormat(const SdpVideoFormat& format)
    {
        std::vector<SdpVideoFormat> formats = GetSupportedFormatsInFactories(decoderFactories_);
        return format.IsCodecInList(formats);
    }

} // namespace webrtc
} // namespace unity
