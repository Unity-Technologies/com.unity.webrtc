#include "pch.h"

#include <absl/strings/match.h>
#include <api/video_codecs/video_encoder_factory.h>
#include <modules/video_coding/codecs/h264/include/h264.h>

#include "ProfilerMarkerFactory.h"
#include "MediaCodec.h"
#include "MediaCodecDecoderImpl.h"
#include "MediaCodecEncoderImpl.h"


namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    std::unique_ptr<MediaCodecEncoder> MediaCodecEncoder::Create(
        const cricket::VideoCodec& codec,
        ProfilerMarkerFactory* profiler)
    {
        return std::make_unique<MediaCodecEncoderImpl>(codec, profiler);
    }

    std::unique_ptr<MediaCodecDecoder> MediaCodecDecoder::Create(
        const cricket::VideoCodec& codec,
        ProfilerMarkerFactory* profiler)
    {
        return std::make_unique<MediaCodecDecoderImpl>(profiler);
    }
}
}