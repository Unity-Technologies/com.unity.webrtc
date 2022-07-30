#include "pch.h"

#include <absl/strings/match.h>
#include <api/video/video_codec_constants.h>
#include <api/video/video_codec_type.h>
#include <common_video/h264/h264_common.h>
#include <media/base/media_constants.h>
#include <modules/video_coding/include/video_codec_interface.h>

#include <media/NdkMediaCodec.h>

#include <android/hardware_buffer_jni.h>

#include "MediaCodecEncoderImpl.h"

namespace unity
{
namespace webrtc
{
    const char VIDEO_H264[] = "video/avc";
//    const char VIDEO_VP8[] = "video/x-vnd.on2.vp8";
//    const char VIDEO_VP9[] = "video/x-vnd.on2.vp9";
//    const char VIDEO_AV1[] = "video/av01";

    MediaCodecEncoderImpl::MediaCodecEncoderImpl(
        const cricket::VideoCodec& codec,
        ProfilerMarkerFactory* profiler)
        : codec_(nullptr)
    {
    }

    MediaCodecEncoderImpl::~MediaCodecEncoderImpl()
    {
    }

    // webrtc::VideoEncoder
    // Initialize the encoder with the information from the codecSettings
    int32_t MediaCodecEncoderImpl::InitEncode(const VideoCodec* codec_settings, const VideoEncoder::Settings& settings)
    {
        codec_ = AMediaCodec_createEncoderByType(VIDEO_H264);
        RTC_DCHECK(codec_);

        return WEBRTC_VIDEO_CODEC_OK;
    }

    // Free encoder memory.
    int32_t MediaCodecEncoderImpl::Release()
    {
        if(codec_)
            AMediaCodec_delete(codec_);
        return WEBRTC_VIDEO_CODEC_OK;
    }

    // Register an encode complete m_encodedCompleteCallback object.
    int32_t MediaCodecEncoderImpl::RegisterEncodeCompleteCallback(EncodedImageCallback* callback)
    {
        return WEBRTC_VIDEO_CODEC_OK;
    }

    // Encode an I420 image (as a part of a video stream). The encoded image
    // will be returned to the user through the encode complete m_encodedCompleteCallback.
    int32_t MediaCodecEncoderImpl::Encode(const ::webrtc::VideoFrame& frame, const std::vector<VideoFrameType>* frame_types)
    {
        return WEBRTC_VIDEO_CODEC_OK;
    }

    // Default fallback: Just use the sum of bitrates as the single target rate.
    void MediaCodecEncoderImpl::SetRates(const RateControlParameters& parameters)
    {}

    // Returns meta-data about the encoder, such as implementation name.
    VideoEncoder::EncoderInfo MediaCodecEncoderImpl::GetEncoderInfo() const
    {
        VideoEncoder::EncoderInfo info;
        info.implementation_name = "MediaCodec";
        info.is_hardware_accelerated = true;
        return info;
    }
}
}