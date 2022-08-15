#include "pch.h"

#include <absl/strings/match.h>
#include <api/video/video_codec_constants.h>
#include <api/video/video_codec_type.h>
#include <common_video/h264/h264_common.h>
#include <media/base/media_constants.h>
#include <modules/video_coding/include/video_codec_interface.h>

#include "MediaCodecDecoderImpl.h"

namespace unity
{
namespace webrtc
{

    MediaCodecDecoderImpl::MediaCodecDecoderImpl(
        const cricket::VideoCodec& codec, IGraphicsDevice* device, ProfilerMarkerFactory* profiler)
    {
    }

    MediaCodecDecoderImpl::~MediaCodecDecoderImpl() { Release(); }

    int32_t MediaCodecDecoderImpl::InitDecode(const VideoCodec* codec_settings, int32_t number_of_cores)
    {
        return WEBRTC_VIDEO_CODEC_OK;
    }
    int32_t MediaCodecDecoderImpl::Decode(const EncodedImage& input_image, bool missing_frames, int64_t render_time_ms)
    {
        return WEBRTC_VIDEO_CODEC_OK;
    }
    int32_t MediaCodecDecoderImpl::RegisterDecodeCompleteCallback(DecodedImageCallback* callback)
    {
        return WEBRTC_VIDEO_CODEC_OK;
    }
    int32_t MediaCodecDecoderImpl::Release()
    {
        return WEBRTC_VIDEO_CODEC_OK;
    }
    VideoDecoder::DecoderInfo MediaCodecDecoderImpl::GetDecoderInfo() const
    {
        VideoDecoder::DecoderInfo info;
        info.implementation_name = "MediaCodec";
        info.is_hardware_accelerated = true;
        return info;
    }


}
}