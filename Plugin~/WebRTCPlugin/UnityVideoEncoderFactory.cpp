#include "pch.h"

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
        , factories_()
    {
        const std::vector<std::string> arrayImpl = {
            kInternalImpl, kNvCodecImpl, kAndroidMediaCodecImpl, kVideoToolboxImpl
        };

        for (auto impl : arrayImpl)
        {
            auto factory = CreateVideoEncoderFactory(impl, gfxDevice, profiler);
            if (factory)
                factories_.emplace(impl, factory);
        }
    }

    UnityVideoEncoderFactory::~UnityVideoEncoderFactory() = default;

    std::vector<webrtc::SdpVideoFormat> UnityVideoEncoderFactory::GetSupportedFormats() const
    {
        std::vector<SdpVideoFormat> supported_codecs = GetSupportedFormatsInFactories(factories_);

        // Set video codec order: default video codec is VP8
        auto findIndex = [&](webrtc::SdpVideoFormat& format) -> long {
            const std::string sortOrder[4] = { "VP8", "VP9", "H264", "AV1X" };
            auto it = std::find(std::begin(sortOrder), std::end(sortOrder), format.name);
            if (it == std::end(sortOrder))
                return LONG_MAX;
            return static_cast<long>(std::distance(std::begin(sortOrder), it));
        };
        std::sort(
            supported_codecs.begin(),
            supported_codecs.end(),
            [&](webrtc::SdpVideoFormat& x, webrtc::SdpVideoFormat& y) -> int { return (findIndex(x) < findIndex(y)); });
        return supported_codecs;
    }

    webrtc::VideoEncoderFactory::CodecSupport UnityVideoEncoderFactory::QueryCodecSupport(
        const SdpVideoFormat& format, absl::optional<std::string> scalability_mode) const
    {
        VideoEncoderFactory* factory = FindCodecFactory(factories_, format);
        RTC_DCHECK(format.IsCodecInList(factory->GetSupportedFormats()));
        return factory->QueryCodecSupport(format, scalability_mode);
    }

    std::unique_ptr<webrtc::VideoEncoder>
    UnityVideoEncoderFactory::CreateVideoEncoder(const webrtc::SdpVideoFormat& format)
    {
        VideoEncoderFactory* factory = FindCodecFactory(factories_, format);
        auto encoder = factory->CreateVideoEncoder(format);
        if (!profiler_)
            return encoder;

        // Use Unity Profiler for measuring encoding process.
        return std::make_unique<UnityVideoEncoder>(std::move(encoder), profiler_);
    }
}
}
