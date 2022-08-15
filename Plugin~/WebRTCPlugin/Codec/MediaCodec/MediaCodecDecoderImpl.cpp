#include "pch.h"

#include <absl/strings/match.h>
#include <api/video/video_codec_constants.h>
#include <api/video/video_codec_type.h>
#include <common_video/h264/h264_common.h>
#include <media/base/media_constants.h>
#include <modules/video_coding/include/video_codec_interface.h>
#include <rtc_base/thread.h>

#include <media/NdkMediaCodec.h>
#include <media/NdkMediaFormat.h>
#include <media/NdkImageReader.h>

#include "MediaCodecDecoderImpl.h"


namespace unity
{
namespace webrtc
{

    MediaCodecDecoderImpl::MediaCodecDecoderImpl(
        const cricket::VideoCodec& codec, IGraphicsDevice* device, ProfilerMarkerFactory* profiler)
        : device_(device)
        , codecImpl_(nullptr)
    {
        RTC_DCHECK(device_);
    }

    MediaCodecDecoderImpl::~MediaCodecDecoderImpl() { Release(); }

    int32_t MediaCodecDecoderImpl::InitDecode(const VideoCodec* codec_settings, int32_t number_of_cores)
    {
        if (codec_settings == nullptr)
        {
            RTC_LOG(LS_ERROR) << "initialization failed on codec_settings is null ";
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        if (codec_settings->codecType != kVideoCodecH264)
        {
            RTC_LOG(LS_ERROR) << "initialization failed on codectype is not kVideoCodecH264";
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        if (codec_settings->width < 1 || codec_settings->height < 1)
        {
            RTC_LOG(LS_ERROR) << "initialization failed on codec_settings width < 0 or height < 0";
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        codec_ = *codec_settings;

        codecImpl_ = AMediaCodec_createDecoderByType(VIDEO_H264);
        RTC_DCHECK(codecImpl_);

        ANativeWindow *window;
        int maxImages = 2;
        media_status_t result = AImageReader_newWithUsage(codec_.width, codec_.height, ColorFormatPrivate, AHARDWAREBUFFER_USAGE_GPU_SAMPLED_IMAGE, maxImages, &reader_);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AImageReader_newWithUsage failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        result = AImageReader_getWindow(reader_, &window);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AImageReader_getWindow failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        AMediaFormat* format = AMediaFormat_new();
        RTC_DCHECK(format);

        AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_WIDTH, codec_.width);
        AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_HEIGHT, codec_.height);
        AMediaFormat_setString(format,AMEDIAFORMAT_KEY_MIME,VIDEO_H264);
        AMediaFormat_setInt32(format, "profile", VIDEO_AVC_PROFILE_HIGH);
        AMediaFormat_setInt32(format, "level", VIDEO_AVC_LEVEL_3);

        result = AMediaCodec_configure(codecImpl_, format, window, nullptr, 0);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_configure failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        result = AMediaCodec_start(codecImpl_);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_start failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }
    int32_t MediaCodecDecoderImpl::Decode(const EncodedImage& input_image, bool missing_frames, int64_t render_time_ms)
    {
        if (decodedCompleteCallback_ == nullptr)
        {
            RTC_LOG(LS_ERROR) << "decode failed on not set m_decodedCompleteCallback";
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        }

        media_status_t result;
        ssize_t bufferIndex = AMediaCodec_dequeueInputBuffer(codecImpl_, kDequeueOutputBufferTimeoutMicrosecond);
        if (bufferIndex < 0)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_dequeueInputBuffer failed. bufferIndex=" << bufferIndex;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        size_t size;
        uint8_t* buffer = AMediaCodec_getInputBuffer(codecImpl_, bufferIndex, &size);
        if(size < input_image.size())
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_getInputBuffer returns a small buffer.";
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        std::memcpy(buffer, input_image.data(), input_image.size());

        off_t offset = 0;
        uint64_t time = Timestamp::Millis(input_image.capture_time_ms_).us();
        result = AMediaCodec_queueInputBuffer(codecImpl_, bufferIndex, offset, input_image.size(), time, 0);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_queueInputBuffer failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        AMediaCodecBufferInfo info = {};
        for(;;)
        {
            bufferIndex = AMediaCodec_dequeueOutputBuffer(codecImpl_, &info, kDequeueOutputBufferTimeoutMicrosecond);
            if (bufferIndex < 0)
            {
                if (bufferIndex == AMEDIACODEC_INFO_OUTPUT_FORMAT_CHANGED)
                {
                    AMediaFormat* format = AMediaCodec_getOutputFormat(codecImpl_);
                    AMediaFormat_delete(format);
                }
                rtc::Thread::SleepMs(1);
                continue;
            }
            break;
        }

        result = AMediaCodec_releaseOutputBuffer(codecImpl_, bufferIndex, true);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_releaseOutputBuffer failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        AImage* image = nullptr;
        result = AImageReader_acquireLatestImage(reader_, &image);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AImageReader_acquireLatestImage failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        AHardwareBuffer *hardwareBuffer;
        result = AImage_getHardwareBuffer(image, &hardwareBuffer);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AImage_getHardwareBuffer failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        // todo:: copy buffer to unity texture.
//        VideoFrame decoded_frame = VideoFrame::Builder()
//                                       .set_video_frame_buffer(i420_buffer)
//                                       .set_timestamp_rtp(static_cast<uint32_t>(timeStamp))
//                                       .set_color_space(color_space)
//                                       .build();

        AImage_delete(image);

        return WEBRTC_VIDEO_CODEC_OK;
    }
    int32_t MediaCodecDecoderImpl::RegisterDecodeCompleteCallback(DecodedImageCallback* callback)
    {
        this->decodedCompleteCallback_ = callback;
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