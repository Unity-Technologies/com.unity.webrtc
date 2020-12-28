#include "pch.h"
#include "UnityVideoEncoderFactory.h"
#include "DummyVideoEncoder.h"

#if UNITY_OSX || UNITY_IOS
#import "sdk/objc/components/video_codec/RTCDefaultVideoEncoderFactory.h"
#import "sdk/objc/native/api/video_encoder_factory.h"
#elif UNITY_ANDROID
#include "Codec/AndroidCodec/android_codec_factory_helper.h"
#endif

using namespace ::webrtc::H264;

namespace unity
{    
namespace webrtc
{

    bool IsFormatSupported(
        const std::vector<webrtc::SdpVideoFormat>& supported_formats,
        const webrtc::SdpVideoFormat& format)
    {
        for (const webrtc::SdpVideoFormat& supported_format : supported_formats)
        {
            if (cricket::IsSameCodec(format.name, format.parameters,
                supported_format.name,
                supported_format.parameters))
            {
                return true;
            }
        }
        return false;
    }

    webrtc::VideoEncoderFactory* CreateEncoderFactory()
    {
#if UNITY_OSX || UNITY_IOS
        return webrtc::ObjCToNativeVideoEncoderFactory(
            [[RTCDefaultVideoEncoderFactory alloc] init]).release();
#elif UNITY_ANDROID
        return CreateAndroidEncoderFactory().release();
#else
        return new webrtc::InternalEncoderFactory();
#endif
    }

    UnityVideoEncoderFactory::UnityVideoEncoderFactory(IVideoEncoderObserver* observer)
    : internal_encoder_factory_(CreateEncoderFactory())

    {
        m_observer = observer;
    }
    
    UnityVideoEncoderFactory::~UnityVideoEncoderFactory() = default;

    std::vector<webrtc::SdpVideoFormat> UnityVideoEncoderFactory::GetHardwareEncoderFormats() const
    {
#if CUDA_PLATFORM
        return { webrtc::CreateH264Format(
            webrtc::H264::kProfileConstrainedBaseline,
            webrtc::H264::kLevel5_1, "1") };
#else
        bool h264EncoderFound = false;
        auto formats = internal_encoder_factory_->GetSupportedFormats();
        std::vector<webrtc::SdpVideoFormat> filtered;
        std::copy_if(formats.begin(), formats.end(), std::back_inserter(filtered),
            [&h264EncoderFound](webrtc::SdpVideoFormat format) {
                if(format.name.find("H264") != std::string::npos)
                {
                    h264EncoderFound = true;
                    // On iOS, WebRTC's Apple H264 encoder backend tries to keep up maintaining a list of device
                    // specific max profiles, but on a Mac it's defaulting to baseline/high profile at @ Level 3.1,
                    // which is not enough for meaningful streaming situations
                    //
                    // Regardless of the internal encoder factory, just filter them out and introduce your own
                    //
                    // Codec name: H264, parameters: { level-asymmetry-allowed=1 packetization-mode=1 profile-level-id=640c1f }
                    // Codec name: H264, parameters: { level-asymmetry-allowed=1 packetization-mode=1 profile-level-id=42e01f }
                    return false;
                }
                return true;
            });

        if (h264EncoderFound == true)
        {
            // While most decoders should be just fine with High profile these days, we're advertising kProfileConstrainedBaseline
            // in the SDP for maximum browser compatibility, some browsers have issues picking up other profiles before decoding work
            webrtc::SdpVideoFormat h264 = webrtc::CreateH264Format(webrtc::H264::kProfileConstrainedBaseline, webrtc::H264::kLevel5_1, "1");
            filtered.insert(filtered.begin(), h264);
        }

        return filtered;
#endif
    }


    std::vector<webrtc::SdpVideoFormat> UnityVideoEncoderFactory::GetSupportedFormats() const
    {
        // todo(kazuki): should support codec other than h264 like vp8, vp9 and av1.
        //
        // std::vector <webrtc::SdpVideoFormat> formats2 = internal_encoder_factory_->GetSupportedFormats();
        // formats.insert(formats.end(), formats2.begin(), formats2.end());
        return GetHardwareEncoderFormats();
    }

    webrtc::VideoEncoderFactory::CodecInfo UnityVideoEncoderFactory::QueryVideoEncoder(
        const webrtc::SdpVideoFormat& format) const
    {
        if (IsFormatSupported(GetHardwareEncoderFormats(), format))
        {
            return CodecInfo{ false };
        }
        RTC_DCHECK(IsFormatSupported(GetSupportedFormats(), format));
        return internal_encoder_factory_->QueryVideoEncoder(format);
    }

    std::unique_ptr<webrtc::VideoEncoder> UnityVideoEncoderFactory::CreateVideoEncoder(const webrtc::SdpVideoFormat& format)
    {
#if CUDA_PLATFORM
        if (IsFormatSupported(GetHardwareEncoderFormats(), format))
        {
            return std::make_unique<DummyVideoEncoder>(m_observer);
        }
#endif
        return internal_encoder_factory_->CreateVideoEncoder(format);
    }
}
}
