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

#include <android/hardware_buffer_jni.h>
#include <android/window.h>

#include "MediaCodecEncoderImpl.h"
#include "NativeFrameBuffer.h"

namespace unity
{
namespace webrtc
{
    MediaCodecEncoderImpl::MediaCodecEncoderImpl(
        const cricket::VideoCodec& codec,
        IGraphicsDevice* device,
        ProfilerMarkerFactory* profiler)
        : device_(device)
        , codecImpl_(nullptr)
    {
        RTC_CHECK(absl::EqualsIgnoreCase(codec.name, cricket::kH264CodecName));

        std::string profileLevelIdString;
        RTC_CHECK(codec.GetParam(cricket::kH264FmtpProfileLevelId, &profileLevelIdString));

        // auto profileLevelId = ParseH264ProfileLevelId(profileLevelIdString.c_str());
    }

    MediaCodecEncoderImpl::~MediaCodecEncoderImpl()
    {
        Release();
    }

    // webrtc::VideoEncoder
    // Initialize the encoder with the information from the codecSettings
    int32_t MediaCodecEncoderImpl::InitEncode(const VideoCodec* codec, const VideoEncoder::Settings& settings)
    {
        if (!codec || codec->codecType != kVideoCodecH264)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        if (codec->maxFramerate == 0)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        if (codec->width < 1 || codec->height < 1)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        codec_ = *codec;

        codecImpl_ = AMediaCodec_createEncoderByType(VIDEO_H264);
        RTC_DCHECK(codecImpl_);

        AMediaFormat* format = AMediaFormat_new();
        RTC_DCHECK(format);

        AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_WIDTH, codec_.width);
        AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_HEIGHT, codec_.height);
        AMediaFormat_setString(format,AMEDIAFORMAT_KEY_MIME,VIDEO_H264);
        AMediaFormat_setInt32(format,AMEDIAFORMAT_KEY_BIT_RATE,codec_.startBitrate);
        AMediaFormat_setInt32(format,AMEDIAFORMAT_KEY_FRAME_RATE,codec_.maxFramerate);
        AMediaFormat_setInt32(format,AMEDIAFORMAT_KEY_COLOR_FORMAT,ColorFormatSurface);
        AMediaFormat_setInt32(format, "bitrate-mode", VIDEO_ControlRateConstant);
        AMediaFormat_setInt32(format, AMEDIAFORMAT_KEY_I_FRAME_INTERVAL, keyFrameIntervalSec);
        AMediaFormat_setInt32(format, "profile", VIDEO_AVC_PROFILE_HIGH);
        AMediaFormat_setInt32(format, "level", VIDEO_AVC_LEVEL_3);

        media_status_t result = AMediaCodec_configure(codecImpl_, format, nullptr, nullptr, AMEDIACODEC_CONFIGURE_FLAG_ENCODE);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_configure failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        ANativeWindow* window = nullptr;
        result = AMediaCodec_createInputSurface(codecImpl_, &window);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_createInputSurface failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        surface_ = device_->GetSurface(window);
        RTC_DCHECK(surface_);

        result = AMediaCodec_start(codecImpl_);
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_start failed. result=" << result;
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }

    // Free encoder memory.
    int32_t MediaCodecEncoderImpl::Release()
    {
        if(codecImpl_)
        {
            AMediaCodec_delete(codecImpl_);
            codecImpl_ = nullptr;
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }

    // Register an encode complete m_encodedCompleteCallback object.
    int32_t MediaCodecEncoderImpl::RegisterEncodeCompleteCallback(EncodedImageCallback* callback)
    {
        encodedCompleteCallback_ = callback;
        return WEBRTC_VIDEO_CODEC_OK;
    }

    // Encode an I420 image (as a part of a video stream). The encoded image
    // will be returned to the user through the encode complete m_encodedCompleteCallback.
    int32_t MediaCodecEncoderImpl::Encode(const ::webrtc::VideoFrame& frame, const std::vector<VideoFrameType>* frameTypes)
    {
        RTC_DCHECK_EQ(frame.width(), codec_.width);
        RTC_DCHECK_EQ(frame.height(), codec_.height);

        if (!codecImpl_)
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        if (!encodedCompleteCallback_)
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;

        auto videoFrameBuffer = static_cast<ScalableBufferInterface*>(frame.video_frame_buffer().get());
        auto nativeBuffer = static_cast<NativeFrameBuffer*>(videoFrameBuffer);
        RTC_DCHECK(nativeBuffer);

        bool requestedKeyFrame = false;
        if (!requestedKeyFrame && frameTypes)
        {
            if ((*frameTypes)[0] == VideoFrameType::kVideoFrameKey)
            {
                requestedKeyFrame = true;
            }
        }

        surface_->DrawFrame(nativeBuffer);

        std::vector<uint8_t> packet;
        for(;;)
        {
            AMediaCodecBufferInfo info = {};
            ssize_t bufferIndex =
                AMediaCodec_dequeueOutputBuffer(codecImpl_, &info, kDequeueOutputBufferTimeoutMicrosecond);
            if (bufferIndex < 0)
            {
                if (bufferIndex == AMEDIACODEC_INFO_OUTPUT_FORMAT_CHANGED)
                {
                    AMediaFormat *format = AMediaCodec_getOutputFormat(codecImpl_);
                    AMediaFormat_delete(format);
                }
                else if (bufferIndex == AMEDIACODEC_INFO_OUTPUT_BUFFERS_CHANGED)
                {
                    // outputBuffersBusyCount.waitForZero();
                }
                rtc::Thread::SleepMs(1);
                continue;
            }
            size_t size = 0;
            const uint8_t* buf = AMediaCodec_getOutputBuffer(codecImpl_, bufferIndex, &size);
            if(!buf)
            {
                RTC_LOG(LS_INFO) << "AMediaCodec_getOutputBuffer returns nullptr.";
                AMediaCodec_releaseOutputBuffer(codecImpl_, bufferIndex, false);
                return WEBRTC_VIDEO_CODEC_NO_OUTPUT;
            }
            if(info.flags & AMEDIACODEC_BUFFER_FLAG_CODEC_CONFIG)
            {
                configBuffer_.resize(info.size);
                std::memcpy(configBuffer_.data(), buf, info.size);
                rtc::Thread::SleepMs(1);
                continue;
            }
            packet.resize(configBuffer_.size() + info.size);
            std::memcpy(packet.data(), configBuffer_.data(), configBuffer_.size());
            std::memcpy(packet.data() + configBuffer_.size(), buf, info.size);
            AMediaCodec_releaseOutputBuffer(codecImpl_, bufferIndex, false);
            break;
        }

        encodedImage_.SetEncodedData(EncodedImageBuffer::Create(packet.data(), packet.size()));
        encodedImage_.set_size(packet.size());

        h264BitstreamParser_.ParseBitstream(encodedImage_);
        encodedImage_.qp_ = h264BitstreamParser_.GetLastSliceQp().value_or(-1);

        encodedImage_._encodedWidth = nativeBuffer->width();
        encodedImage_._encodedHeight = nativeBuffer->height();
        encodedImage_.SetTimestamp(frame.timestamp());
        encodedImage_.SetSpatialIndex(0);
        encodedImage_.ntp_time_ms_ = frame.ntp_time_ms();
        encodedImage_.capture_time_ms_ = frame.render_time_ms();
        encodedImage_.rotation_ = frame.rotation();
        encodedImage_.content_type_ = VideoContentType::UNSPECIFIED;
        encodedImage_.timing_.flags = VideoSendTiming::kInvalid;
        encodedImage_._frameType = VideoFrameType::kVideoFrameDelta;
        encodedImage_.SetColorSpace(frame.color_space());

        CodecSpecificInfo codecInfo;
        codecInfo.codecType = kVideoCodecH264;
        codecInfo.codecSpecific.H264.packetization_mode = H264PacketizationMode::NonInterleaved;

        const auto result = encodedCompleteCallback_->OnEncodedImage(encodedImage_, &codecInfo);
        if (result.error != EncodedImageCallback::Result::OK)
        {
            RTC_LOG(LS_ERROR) << "Encode m_encodedCompleteCallback failed " << result.error;
            return WEBRTC_VIDEO_CODEC_ERROR;
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }

    // Default fallback: Just use the sum of bitrates as the single target rate.
    void MediaCodecEncoderImpl::SetRates(const RateControlParameters& parameters)
    {
        if (!codecImpl_)
        {
            RTC_LOG(LS_WARNING) << "while uninitialized.";
            return;
        }

        if (parameters.framerate_fps < 1.0)
        {
            RTC_LOG(LS_WARNING) << "Invalid frame rate: " << parameters.framerate_fps;
            return;
        }

        if (parameters.bitrate.get_sum_bps() == 0)
        {
            RTC_LOG(LS_WARNING) << "Encoder paused, turn off all encoding";
            // m_configurations[0].SetStreamState(false);
            return;
        }

        bitrateAdjuster_->SetTargetBitrateBps(parameters.bitrate.get_sum_bps());
        const uint32_t bitrate = bitrateAdjuster_->GetAdjustedBitrateBps();

        codec_.maxFramerate = static_cast<uint32_t>(parameters.framerate_fps);
        codec_.maxBitrate = bitrate;

        AMediaFormat* format = AMediaFormat_new();
        RTC_DCHECK(format);

        AMediaFormat_setInt32(format,AMEDIAFORMAT_KEY_BIT_RATE,codec_.maxBitrate);
        AMediaFormat_setInt32(format,AMEDIAFORMAT_KEY_FRAME_RATE,codec_.maxFramerate);

        // This API requires Android API version higher than 26.
        media_status_t result =  AMediaCodec_setParameters(
            codecImpl_, format
        );
        if(result != AMEDIA_OK)
        {
            RTC_LOG(LS_INFO) << "AMediaCodec_setParameters failed. result=" << result;
            return;
        }
    }

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