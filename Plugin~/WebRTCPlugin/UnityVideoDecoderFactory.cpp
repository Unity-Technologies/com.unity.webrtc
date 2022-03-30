#include "pch.h"

#include "UnityVideoDecoderFactory.h"

#if CUDA_PLATFORM
#include <cuda.h>
#include "Codec/NvCodec/NvCodec.h"
#endif

#include "GraphicsDevice/GraphicsUtility.h"

#if UNITY_OSX || UNITY_IOS
#import "sdk/objc/components/video_codec/RTCDefaultVideoDecoderFactory.h"
#import "sdk/objc/native/api/video_decoder_factory.h"
#elif UNITY_ANDROID
#include "Android/Jni.h"
#include "Android/AndroidCodecFactoryHelper.h"
#endif

namespace unity
{
namespace webrtc
{
    webrtc::VideoDecoderFactory* CreateNativeDecoderFactory(IGraphicsDevice* gfxDevice)
    {
#if UNITY_OSX || UNITY_IOS
        return webrtc::ObjCToNativeVideoDecoderFactory([[RTCDefaultVideoDecoderFactory alloc] init]).release();
#elif UNITY_ANDROID
        if (IsVMInitialized())
            return CreateAndroidDecoderFactory().release();
#elif CUDA_PLATFORM
        if(gfxDevice->IsCudaSupport())
        {
            CUcontext context = gfxDevice->GetCUcontext();
            return new NvDecoderFactory(context);
        }
#endif
        return nullptr;
    }

    UnityVideoDecoderFactory::UnityVideoDecoderFactory(IGraphicsDevice* gfxDevice)
        : internal_decoder_factory_(new webrtc::InternalDecoderFactory())
        , native_decoder_factory_(CreateNativeDecoderFactory(gfxDevice))
    {
    }

    UnityVideoDecoderFactory::~UnityVideoDecoderFactory() = default;

    std::vector<webrtc::SdpVideoFormat> UnityVideoDecoderFactory::GetSupportedFormats() const
    {
        std::vector<SdpVideoFormat> supported_codecs;
        if(native_decoder_factory_)
            for (const webrtc::SdpVideoFormat& format : native_decoder_factory_->GetSupportedFormats())
                supported_codecs.push_back(format);
        for (const webrtc::SdpVideoFormat& format : internal_decoder_factory_->GetSupportedFormats())
            supported_codecs.push_back(format);
        return supported_codecs;
    }

    std::unique_ptr<webrtc::VideoDecoder>
    UnityVideoDecoderFactory::CreateVideoDecoder(const webrtc::SdpVideoFormat& format)
    {
        if (native_decoder_factory_ && format.IsCodecInList(native_decoder_factory_->GetSupportedFormats()))
        {
            return native_decoder_factory_->CreateVideoDecoder(format);
        }
        return internal_decoder_factory_->CreateVideoDecoder(format);
    }

} // namespace webrtc
} // namespace unity
