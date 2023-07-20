#pragma once

#include <cuda.h>

#include <api/video_codecs/video_codec.h>
#include <api/video_codecs/video_encoder.h>
#include <common_video/h264/h264_bitstream_parser.h>
#include <media/base/codec.h>

#include "NvCodec.h"
#include "NvEncoder/NvEncoderCuda.h"
#include "Size.h"

class UnityProfilerMarkerDesc;

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;
    using NvEncoderInternal = ::NvEncoder;

    class ITexture2D;
    class IGraphicsDevice;
    class ProfilerMarkerFactory;
    struct GpuMemoryBufferHandle;
    class GpuMemoryBufferInterface;
    class NvEncoderImpl : public unity::webrtc::NvEncoder
    {
    public:
        struct LayerConfig
        {
            int simulcast_idx = 0;
            int width = -1;
            int height = -1;
            bool sending = true;
            bool key_frame_request = false;
            float max_frame_rate = 0;
            uint32_t target_bps = 0;
            uint32_t max_bps = 0;
            int key_frame_interval = 0;
            int num_temporal_layers = 1;

            void SetStreamState(bool send_stream);
        };
        NvEncoderImpl(
            const cricket::VideoCodec& codec,
            CUcontext context,
            CUmemorytype memoryType,
            NV_ENC_BUFFER_FORMAT format,
            ProfilerMarkerFactory* profiler);
        NvEncoderImpl(const NvEncoderImpl&) = delete;
        NvEncoderImpl& operator=(const NvEncoderImpl&) = delete;
        ~NvEncoderImpl() override;

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

    protected:
        int32_t ProcessEncodedFrame(std::vector<uint8_t>& packet, const ::webrtc::VideoFrame& inputFrame);

    private:
        bool CopyResource(
            const NvEncInputFrame* encoderInputFrame,
            GpuMemoryBufferInterface* buffer,
            Size& size,
            CUcontext context,
            CUmemorytype memoryType);

        CUcontext m_context;
        CUmemorytype m_memoryType;
        CUarray m_scaledArray;
        std::unique_ptr<NvEncoderInternal> m_encoder;

        VideoCodec m_codec;

        NV_ENC_BUFFER_FORMAT m_format;
        NV_ENC_INITIALIZE_PARAMS m_initializeParams;
        NV_ENC_CONFIG m_encodeConfig;

        EncodedImageCallback* m_encodedCompleteCallback;
        EncodedImage m_encodedImage;
        //    RTPFragmentationHeader m_fragHeader;
        H264BitstreamParser m_h264BitstreamParser;
        GUID m_profileGuid;
        NV_ENC_LEVEL m_level;
        ProfilerMarkerFactory* m_profiler;
        const UnityProfilerMarkerDesc* m_marker;

        std::vector<LayerConfig> m_configurations;

        static absl::optional<webrtc::H264Level> s_maxSupportedH264Level;
        static std::vector<SdpVideoFormat> s_formats;
    };
} // end namespace webrtc
} // end namespace unity
