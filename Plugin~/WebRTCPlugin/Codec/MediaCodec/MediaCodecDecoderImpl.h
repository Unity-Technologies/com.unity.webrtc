#pragma once

#include <common_video/h264/h264_bitstream_parser.h>
#include <common_video/include/video_frame_buffer_pool.h>

#include "MediaCodec.h"

using namespace webrtc;

namespace unity
{
namespace webrtc
{
    class ProfilerMarkerFactory;
    class MediaCodecDecoderImpl : public MediaCodecDecoder
    {
    public:
        MediaCodecDecoderImpl(const cricket::VideoCodec& codec, IGraphicsDevice* device, ProfilerMarkerFactory* profiler);
        MediaCodecDecoderImpl(const MediaCodecDecoderImpl&) = delete;
        MediaCodecDecoderImpl& operator=(const MediaCodecDecoderImpl&) = delete;
        ~MediaCodecDecoderImpl() override;

        int32_t InitDecode(const VideoCodec* codec_settings, int32_t number_of_cores) override;
        int32_t Decode(const EncodedImage& input_image, bool missing_frames, int64_t render_time_ms) override;
        int32_t RegisterDecodeCompleteCallback(DecodedImageCallback* callback) override;
        int32_t Release() override;
        DecoderInfo GetDecoderInfo() const override;
    };
}
}