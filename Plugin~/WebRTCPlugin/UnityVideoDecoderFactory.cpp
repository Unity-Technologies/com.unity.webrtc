#include "pch.h"

#include <media/engine/internal_decoder_factory.h>

#include "GraphicsDevice/GraphicsUtility.h"
#include "ProfilerMarkerFactory.h"
#include "ScopedProfiler.h"
#include "UnityVideoDecoderFactory.h"

#if CUDA_PLATFORM
#include "Codec/NvCodec/NvCodec.h"
#endif

#if UNITY_OSX || UNITY_IOS
#import <sdk/objc/components/video_codec/RTCDefaultVideoDecoderFactory.h>
#import <sdk/objc/native/api/video_decoder_factory.h>
#elif UNITY_ANDROID
#include "Android/AndroidCodecFactoryHelper.h"
#include "Android/Jni.h"
#endif

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

        int32_t InitDecode(const VideoCodec* codec_settings, int32_t number_of_cores) override
        {
            return decoder_->InitDecode(codec_settings, number_of_cores);
        }
        int32_t Decode(const EncodedImage& input_image, bool missing_frames, int64_t render_time_ms) override
        {
            if (!profilerThread_)
                profilerThread_ = profiler_->CreateScopedProfilerThread("WebRTC", "VideoDecoder");

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

    static webrtc::VideoDecoderFactory* CreateNativeDecoderFactory(IGraphicsDevice* gfxDevice)
    {
#if UNITY_OSX || UNITY_IOS
        return webrtc::ObjCToNativeVideoDecoderFactory([[RTCDefaultVideoDecoderFactory alloc] init]).release();
#elif UNITY_ANDROID
        if (IsVMInitialized())
            return CreateAndroidDecoderFactory().release();
#elif CUDA_PLATFORM
        if (gfxDevice->IsCudaSupport())
        {
            CUcontext context = gfxDevice->GetCUcontext();
            return new NvDecoderFactory(context);
        }
#endif
        return nullptr;
    }

    UnityVideoDecoderFactory::UnityVideoDecoderFactory(IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler)
        : profiler_(profiler)
        , internal_decoder_factory_(new webrtc::InternalDecoderFactory())
        , native_decoder_factory_(CreateNativeDecoderFactory(gfxDevice))
    {
    }

    UnityVideoDecoderFactory::~UnityVideoDecoderFactory() = default;

    std::vector<webrtc::SdpVideoFormat> UnityVideoDecoderFactory::GetSupportedFormats() const
    {
        std::vector<SdpVideoFormat> supported_codecs;
        if (native_decoder_factory_)
            for (const webrtc::SdpVideoFormat& format : native_decoder_factory_->GetSupportedFormats())
                supported_codecs.push_back(format);
        for (const webrtc::SdpVideoFormat& format : internal_decoder_factory_->GetSupportedFormats())
            supported_codecs.push_back(format);
        return supported_codecs;
    }

    std::unique_ptr<webrtc::VideoDecoder>
    UnityVideoDecoderFactory::CreateVideoDecoder(const webrtc::SdpVideoFormat& format)
    {
        std::unique_ptr<webrtc::VideoDecoder> decoder;
        if (native_decoder_factory_ && format.IsCodecInList(native_decoder_factory_->GetSupportedFormats()))
        {
            decoder = native_decoder_factory_->CreateVideoDecoder(format);
        }
        else
        {
            decoder = internal_decoder_factory_->CreateVideoDecoder(format);
        }
        if (!profiler_)
            return decoder;

        // Use Unity Profiler for measuring decoding process.
        return std::make_unique<UnityVideoDecoder>(std::move(decoder), profiler_);
    }

} // namespace webrtc
} // namespace unity
