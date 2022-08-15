#pragma once

#include <api/video_codecs/video_codec.h>
#include <api/video_codecs/video_encoder.h>
#include <common_video/h264/h264_bitstream_parser.h>
#include <common_video/include/bitrate_adjuster.h>
#include <media/base/codec.h>
#include <system_wrappers/include/clock.h>

#include "MediaCodec.h"

struct AMediaCodec;

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class Surface;
    class MediaCodecEncoderImpl : public MediaCodecEncoder
    {
    public:
        MediaCodecEncoderImpl(
            const cricket::VideoCodec& codec,
            IGraphicsDevice* device,
            ProfilerMarkerFactory* profiler);
        MediaCodecEncoderImpl(const MediaCodecEncoderImpl&) = delete;
        MediaCodecEncoderImpl& operator=(const MediaCodecEncoderImpl&) = delete;
        ~MediaCodecEncoderImpl() override;
        // webrtc::VideoEncoder
        // Initialize the encoder with the information from the codecSettings
        int32_t InitEncode(const VideoCodec* codec_settings, const VideoEncoder::Settings& settings) override;
        // Free encoder memory.
        int32_t Release() override;
        // Register an encode complete m_encodedCompleteCallback object.
        int32_t RegisterEncodeCompleteCallback(EncodedImageCallback* callback) override;
        // Encode an I420 image (as a part of a video stream). The encoded image
        // will be returned to the user through the encode complete m_encodedCompleteCallback.
        int32_t Encode(const ::webrtc::VideoFrame& frame, const std::vector<VideoFrameType>* frame_types) override;
        // Default fallback: Just use the sum of bitrates as the single target rate.
        void SetRates(const RateControlParameters& parameters) override;
        // Returns meta-data about the encoder, such as implementation name.
        EncoderInfo GetEncoderInfo() const override;
    private:
        IGraphicsDevice* device_;
        VideoCodec codec_;
        AMediaCodec* codecImpl_;
        EncodedImageCallback* encodedCompleteCallback_;
        EncodedImage encodedImage_;
        H264BitstreamParser h264BitstreamParser_;

        std::unique_ptr<Surface> surface_;
        std::unique_ptr<BitrateAdjuster> bitrateAdjuster_;
        std::vector<uint8_t> configBuffer_;
    };
}
}