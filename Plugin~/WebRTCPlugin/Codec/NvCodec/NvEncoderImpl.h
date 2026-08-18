#pragma once

#include <cuda.h>

#include <api/video_codecs/video_codec.h>
#include <api/video_codecs/video_encoder.h>
#include <common_video/h264/h264_bitstream_parser.h>
#include <common_video/h265/h265_bitstream_parser.h>
#include <media/base/codec.h>

#include "NvCodec.h"
#include "NvEncoder/NvEncoderCuda.h"
#include "Size.h"
#include "absl/types/optional.h"

// todo::
// CMake doesn't support building CUDA kernel with Clang compiler on Windows.
// https://gitlab.kitware.com/cmake/cmake/-/issues/20776
#if !(_WIN32 && __clang__)
#define SUPPORT_CUDA_KERNEL 1
#endif
#if SUPPORT_CUDA_KERNEL
#include "ResizeSurf.h"
#endif

class UnityProfilerMarkerDesc;

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;
    using NvEncoderInternal = ::NvEncoder;

    inline bool operator==(const CUDA_ARRAY_DESCRIPTOR& lhs, const CUDA_ARRAY_DESCRIPTOR& rhs)
    {
        return lhs.Width == rhs.Width && lhs.Height == rhs.Height && lhs.NumChannels == rhs.NumChannels &&
            lhs.Format == rhs.Format;
    }

    inline bool operator!=(const CUDA_ARRAY_DESCRIPTOR& lhs, const CUDA_ARRAY_DESCRIPTOR& rhs) { return !(lhs == rhs); }

    inline absl::optional<webrtc::H265Level> NvEncSupportedLevel(std::vector<SdpVideoFormat>& formats, const GUID& guid)
    {
        for (const auto& format : formats)
        {
            const auto profileLevelId = ParseSdpForH265ProfileTierLevel(format.parameters);
            if (!profileLevelId.has_value())
                continue;
            const auto guid2 = H265ProfileToGuid(profileLevelId.value().profile);
            if (guid2.has_value() && guid == guid2.value())
            {
                return profileLevelId.value().level;
            }
        }
        return absl::nullopt;
    }

    inline absl::optional<NV_ENC_LEVEL>
    NvEncRequiredLevel(const VideoCodec& codec, std::vector<SdpVideoFormat>& formats, const GUID& guid)
    {
        int pixelCount = codec.width * codec.height;
        // auto requiredLevel = unity::webrtc::H264SupportedLevel(
        //     pixelCount, static_cast<int>(codec.maxFramerate), static_cast<int>(codec.maxBitrate));
        auto resolution = Resolution();
        resolution.height = codec.height;
        resolution.width = codec.width;
        auto requiredLevel = GetSupportedH265Level(resolution, static_cast<float>(codec.maxFramerate));

        if (!requiredLevel)
        {
            return absl::nullopt;
        }

        // Check NvEnc supported level.
        auto supportedLevel = NvEncSupportedLevel(formats, guid);
        if (!supportedLevel)
        {
            return absl::nullopt;
        }

        // The supported level must be over the required level.
        if (static_cast<int>(requiredLevel.value()) > static_cast<int>(supportedLevel.value()))
        {
            return absl::nullopt;
        }
        return static_cast<NV_ENC_LEVEL>(requiredLevel.value());
    }

#if SUPPORT_CUDA_KERNEL
    inline CUresult Resize(const CUarray& src, CUarray& dst, const Size& size)
    {
        CUDA_ARRAY_DESCRIPTOR srcDesc = {};
        CUresult result = cuArrayGetDescriptor(&srcDesc, src);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuArrayGetDescriptor failed. error:" << result;
            return result;
        }
        CUDA_ARRAY_DESCRIPTOR dstDesc = {};
        dstDesc.Format = srcDesc.Format;
        dstDesc.NumChannels = srcDesc.NumChannels;
        dstDesc.Width = static_cast<size_t>(size.width());
        dstDesc.Height = static_cast<size_t>(size.height());

        bool create = false;
        if (!dst)
        {
            create = true;
        }
        else
        {
            CUDA_ARRAY_DESCRIPTOR desc = {};
            result = cuArrayGetDescriptor(&desc, dst);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "cuArrayGetDescriptor failed. error:" << result;
                return result;
            }
            if (desc != dstDesc)
            {
                result = cuArrayDestroy(dst);
                if (result != CUDA_SUCCESS)
                {
                    RTC_LOG(LS_ERROR) << "cuArrayDestroy failed. error:" << result;
                    return result;
                }
                dst = nullptr;
                create = true;
            }
        }

        if (create)
        {
            CUresult result = cuArrayCreate(&dst, &dstDesc);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "cuArrayCreate failed. error:" << result;
                return result;
            }
        }
        return ResizeSurf(src, dst);
    }
#endif

    class ITexture2D;
    class IGraphicsDevice;
    class ProfilerMarkerFactory;
    struct GpuMemoryBufferHandle;
    class GpuMemoryBufferInterface;
    class NvEncoderImplH264 : public unity::webrtc::NvEncoder
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
        NvEncoderImplH264(
            const cricket::VideoCodec& codec,
            CUcontext context,
            CUmemorytype memoryType,
            NV_ENC_BUFFER_FORMAT format,
            ProfilerMarkerFactory* profiler);
        NvEncoderImplH264(const NvEncoderImplH264&) = delete;
        NvEncoderImplH264& operator=(const NvEncoderImplH264&) = delete;
        ~NvEncoderImplH264() override;

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
        webrtc::H264BitstreamParser m_h264BitstreamParser;
        GUID m_profileGuid;
        NV_ENC_LEVEL m_level;
        ProfilerMarkerFactory* m_profiler;
        const UnityProfilerMarkerDesc* m_marker;

        std::vector<LayerConfig> m_configurations;

        static std::optional<webrtc::H264Level> s_maxSupportedH264Level;
        static std::vector<SdpVideoFormat> s_formats;
    };

    class NvEncoderImplH265 : public unity::webrtc::NvEncoder
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
        NvEncoderImplH265(
            const cricket::VideoCodec& codec,
            CUcontext context,
            CUmemorytype memoryType,
            NV_ENC_BUFFER_FORMAT format,
            ProfilerMarkerFactory* profiler);
        NvEncoderImplH265(const NvEncoderImplH264&) = delete;
        NvEncoderImplH264& operator=(const NvEncoderImplH265&) = delete;
        ~NvEncoderImplH265() override;

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
        webrtc::H265BitstreamParser m_h265BitstreamParser;
        GUID m_profileGuid;
        H265Level m_level;
        H265Tier m_tier;
        ProfilerMarkerFactory* m_profiler;
        const UnityProfilerMarkerDesc* m_marker;

        std::vector<LayerConfig> m_configurations;

        static std::optional<webrtc::H265Level> s_maxSupportedH265Level;
        static std::vector<SdpVideoFormat> s_formats;
    };
} // end namespace webrtc
} // end namespace unity
