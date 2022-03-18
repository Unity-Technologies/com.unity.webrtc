#include "pch.h"

#include "UnityVideoEncoderFactory.h"

#if CUDA_PLATFORM
#include "Codec/NvCodec/NvCodec.h"
#include <cuda.h>
#endif

#include "GraphicsDevice/GraphicsUtility.h"

#if UNITY_OSX || UNITY_IOS
#import "sdk/objc/components/video_codec/RTCDefaultVideoEncoderFactory.h"
#import "sdk/objc/native/api/video_encoder_factory.h"
#elif UNITY_ANDROID
#include "Codec/AndroidCodec/android_codec_factory_helper.h"
#endif

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc::H264;

    webrtc::VideoEncoderFactory* CreateNativeEncoderFactory(IGraphicsDevice* gfxDevice)
    {
#if UNITY_OSX || UNITY_IOS
        return webrtc::ObjCToNativeVideoEncoderFactory([[RTCDefaultVideoEncoderFactory alloc] init]).release();
#elif UNITY_ANDROID
        // todo(kazuki)::workaround
        // return CreateAndroidEncoderFactory().release();
        return nullptr;
#elif CUDA_PLATFORM
        CUcontext context = gfxDevice->GetCUcontext();
        NV_ENC_BUFFER_FORMAT format = gfxDevice->GetEncodeBufferFormat();
        return new NvEncoderFactory(context, format);
#endif
        return nullptr;
    }

    UnityVideoEncoderFactory::UnityVideoEncoderFactory(IGraphicsDevice* gfxDevice)
        : internal_encoder_factory_(new webrtc::InternalEncoderFactory())
        , native_encoder_factory_(CreateNativeEncoderFactory(gfxDevice))
    {
    }

    UnityVideoEncoderFactory::~UnityVideoEncoderFactory() = default;

    std::vector<webrtc::SdpVideoFormat> UnityVideoEncoderFactory::GetSupportedFormats() const
    {
        std::vector<SdpVideoFormat> supported_codecs;

        for (const webrtc::SdpVideoFormat& format : internal_encoder_factory_->GetSupportedFormats())
            supported_codecs.push_back(format);
        if (native_encoder_factory_)
        {
            for (const webrtc::SdpVideoFormat& format : native_encoder_factory_->GetSupportedFormats())
                supported_codecs.push_back(format);
        }

        // Set video codec order: default video codec is VP8
        auto findIndex = [&](webrtc::SdpVideoFormat& format) -> long
        {
            const std::string sortOrder[4] = { "VP8", "VP9", "H264", "AV1X" };
            auto it = std::find(std::begin(sortOrder), std::end(sortOrder), format.name);
            if (it == std::end(sortOrder))
                return LONG_MAX;
            return std::distance(std::begin(sortOrder), it);
        };
        std::sort(
            supported_codecs.begin(),
            supported_codecs.end(),
            [&](webrtc::SdpVideoFormat& x, webrtc::SdpVideoFormat& y) -> int { return (findIndex(x) < findIndex(y)); });
        return supported_codecs;
    }

    webrtc::VideoEncoderFactory::CodecInfo
    UnityVideoEncoderFactory::QueryVideoEncoder(const webrtc::SdpVideoFormat& format) const
    {
        if (native_encoder_factory_ && format.IsCodecInList(native_encoder_factory_->GetSupportedFormats()))
        {
            return native_encoder_factory_->QueryVideoEncoder(format);
        }
        RTC_DCHECK(format.IsCodecInList(internal_encoder_factory_->GetSupportedFormats()));
        return internal_encoder_factory_->QueryVideoEncoder(format);
    }

    std::unique_ptr<webrtc::VideoEncoder>
    UnityVideoEncoderFactory::CreateVideoEncoder(const webrtc::SdpVideoFormat& format)
    {
        if (native_encoder_factory_ && format.IsCodecInList(native_encoder_factory_->GetSupportedFormats()))
        {
            return native_encoder_factory_->CreateVideoEncoder(format);
        }
        return internal_encoder_factory_->CreateVideoEncoder(format);
    }
}
}
