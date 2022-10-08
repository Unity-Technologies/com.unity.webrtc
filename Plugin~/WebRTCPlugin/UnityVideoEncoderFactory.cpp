#include "pch.h"

#include <algorithm>
#include <api/video_codecs/video_encoder.h>
#include <media/engine/internal_encoder_factory.h>
#include <modules/video_coding/include/video_error_codes.h>
#include <tuple>

#include "Codec/CreateVideoCodecFactory.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "ProfilerMarkerFactory.h"
#include "ScopedProfiler.h"
#include "UnityVideoEncoderFactory.h"

namespace unity
{
namespace webrtc
{
    class UnityVideoEncoder : public VideoEncoder
    {
    public:
        UnityVideoEncoder(std::unique_ptr<VideoEncoder> encoder, ProfilerMarkerFactory* profiler)
            : encoder_(std::move(encoder))
            , profiler_(profiler)
            , marker_(nullptr)
            , profilerThread_(nullptr)
        {
            if (profiler)
                marker_ = profiler->CreateMarker(
                    "UnityVideoEncoder.Encode", kUnityProfilerCategoryOther, kUnityProfilerMarkerFlagDefault, 0);
        }
        ~UnityVideoEncoder() override { }

        void SetFecControllerOverride(FecControllerOverride* fec_controller_override) override
        {
            encoder_->SetFecControllerOverride(fec_controller_override);
        }
        int32_t InitEncode(const VideoCodec* codec_settings, int32_t number_of_cores, size_t max_payload_size) override
        {
            int32_t result = encoder_->InitEncode(codec_settings, number_of_cores, max_payload_size);
            if (result >= WEBRTC_VIDEO_CODEC_OK && !profilerThread_)
            {
                std::stringstream ss;
                ss << "Encoder ";
                ss
                    << (encoder_->GetEncoderInfo().implementation_name.empty()
                            ? "VideoEncoder"
                            : encoder_->GetEncoderInfo().implementation_name);
                ss << "(" << CodecTypeToPayloadString(codec_settings->codecType) << ")";
                profilerThread_ = profiler_->CreateScopedProfilerThread("WebRTC", ss.str().c_str());
            }

            return result;
        }
        int InitEncode(const VideoCodec* codec_settings, const VideoEncoder::Settings& settings) override
        {
            int result = encoder_->InitEncode(codec_settings, settings);
            if (result >= WEBRTC_VIDEO_CODEC_OK && !profilerThread_)
            {
                std::stringstream ss;
                ss << "Encoder ";
                ss
                    << (encoder_->GetEncoderInfo().implementation_name.empty()
                            ? "VideoEncoder"
                            : encoder_->GetEncoderInfo().implementation_name);
                ss << "(" << CodecTypeToPayloadString(codec_settings->codecType) << ")";
                profilerThread_ = profiler_->CreateScopedProfilerThread("WebRTC", ss.str().c_str());
            }

            return result;
        }
        int32_t RegisterEncodeCompleteCallback(EncodedImageCallback* callback) override
        {
            return encoder_->RegisterEncodeCompleteCallback(callback);
        }
        int32_t Release() override { return encoder_->Release(); }
        int32_t Encode(const VideoFrame& frame, const std::vector<VideoFrameType>* frame_types) override
        {
            int32_t result;
            {
                std::unique_ptr<const ScopedProfiler> profiler;
                if (profiler_)
                    profiler = profiler_->CreateScopedProfiler(*marker_);
                result = encoder_->Encode(frame, frame_types);
            }
            return result;
        }
        void SetRates(const RateControlParameters& parameters) override { encoder_->SetRates(parameters); }
        void OnPacketLossRateUpdate(float packet_loss_rate) override
        {
            encoder_->OnPacketLossRateUpdate(packet_loss_rate);
        }
        void OnRttUpdate(int64_t rtt_ms) override { encoder_->OnRttUpdate(rtt_ms); }
        void OnLossNotification(const LossNotification& loss_notification) override
        {
            encoder_->OnLossNotification(loss_notification);
        }
        EncoderInfo GetEncoderInfo() const override { return encoder_->GetEncoderInfo(); }

    private:
        std::unique_ptr<VideoEncoder> encoder_;
        ProfilerMarkerFactory* profiler_;
        const UnityProfilerMarkerDesc* marker_;
        std::unique_ptr<const ScopedProfilerThread> profilerThread_;
    };

    UnityVideoEncoderFactory::UnityVideoEncoderFactory(IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler)
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

    UnityVideoEncoderFactory::~UnityVideoEncoderFactory() = default;


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

    std::vector<webrtc::SdpVideoFormat> UnityVideoEncoderFactory::GetSupportedFormats() const
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

    webrtc::VideoEncoderFactory::CodecSupport UnityVideoEncoderFactory::QueryCodecSupport(
        const SdpVideoFormat& format, absl::optional<std::string> scalability_mode) const
    {
        VideoEncoderFactory* factory = FindCodecFactory(encoderFactories_, format);
        RTC_DCHECK(format.IsCodecInList(factory->GetSupportedFormats()));
        return factory->QueryCodecSupport(format, scalability_mode);
    }

    std::unique_ptr<webrtc::VideoEncoder>
    UnityVideoEncoderFactory::CreateVideoEncoder(const webrtc::SdpVideoFormat& format)
    {
        VideoEncoderFactory* factory = FindCodecFactory(encoderFactories_, format);
        auto encoder = factory->CreateVideoEncoder(format);
        if (!profiler_)
            return encoder;

        // Use Unity Profiler for measuring encoding process.
        return std::make_unique<UnityVideoEncoder>(std::move(encoder), profiler_);
    }

    bool UnityVideoEncoderFactory::IsAvailableFormat(const SdpVideoFormat& format)
    {
        std::vector<SdpVideoFormat> formats = GetSupportedFormatsInFactories(encoderFactories_);
        return format.IsCodecInList(formats);
    }
}
}
