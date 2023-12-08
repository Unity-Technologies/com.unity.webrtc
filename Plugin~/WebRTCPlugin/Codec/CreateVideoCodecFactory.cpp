#include "pch.h"

#include <media/engine/internal_decoder_factory.h>
#include <media/engine/internal_encoder_factory.h>

#include "CreateVideoCodecFactory.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "ProfilerMarkerFactory.h"

#if CUDA_PLATFORM
#include "Codec/NvCodec/NvCodec.h"
#include "SimulcastEncoderFactory.h"
#endif

#if UNITY_OSX || UNITY_IOS
#import <sdk/objc/components/video_codec/RTCVideoDecoderFactoryH264.h>
#import <sdk/objc/components/video_codec/RTCVideoEncoderFactoryH264.h>
#import <sdk/objc/native/api/video_decoder_factory.h>
#import <sdk/objc/native/api/video_encoder_factory.h>
#elif UNITY_ANDROID
#include "Android/AndroidCodecFactoryHelper.h"
#include "Android/Jni.h"
#endif

namespace unity
{
namespace webrtc
{
    VideoEncoderFactory*
    CreateVideoEncoderFactory(const std::string& impl, IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler)
    {
        if (impl == kInternalImpl)
        {
            return new webrtc::InternalEncoderFactory();
        }

        if (impl == kVideoToolboxImpl)
        {
#if UNITY_OSX || UNITY_IOS
            return webrtc::ObjCToNativeVideoEncoderFactory([[RTCVideoEncoderFactoryH264 alloc] init]).release();
#endif
        }

        if (impl == kAndroidMediaCodecImpl)
        {
#if UNITY_ANDROID
            if (IsVMInitialized())
            {
                return CreateAndroidEncoderFactory().release();
            }
#endif
        }

        if (impl == kNvCodecImpl)
        {
#if CUDA_PLATFORM
            if (gfxDevice && gfxDevice->IsCudaSupport())
            {
                CUcontext context = gfxDevice->GetCUcontext();
                if (NvEncoder::IsSupported(context))
                {
                    NV_ENC_BUFFER_FORMAT format = gfxDevice->GetEncodeBufferFormat();
                    std::unique_ptr<VideoEncoderFactory> factory =
                        std::make_unique<NvEncoderFactory>(context, format, profiler);
                    return CreateSimulcastEncoderFactory(std::move(factory));
                }
            }
#endif
        }
        return nullptr;
    }

    VideoDecoderFactory*
    CreateVideoDecoderFactory(const std::string& impl, IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler)
    {
        if (impl == kInternalImpl)
        {
            return new webrtc::InternalDecoderFactory();
        }

        if (impl == kVideoToolboxImpl)
        {
#if UNITY_OSX || UNITY_IOS
            return webrtc::ObjCToNativeVideoDecoderFactory([[RTCVideoDecoderFactoryH264 alloc] init]).release();
#endif
        }

        if (impl == kAndroidMediaCodecImpl)
        {
#if UNITY_ANDROID
            if (IsVMInitialized())
                return CreateAndroidDecoderFactory().release();
#endif
        }

        if (impl == kNvCodecImpl)
        {
#if CUDA_PLATFORM
            if (gfxDevice && gfxDevice->IsCudaSupport())
            {
                CUcontext context = gfxDevice->GetCUcontext();
                return new NvDecoderFactory(context, profiler);
            }
#endif
        }
        return nullptr;
    }
}
}
