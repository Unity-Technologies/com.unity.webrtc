/*
* Copyright 2017-2022 NVIDIA Corporation.  All rights reserved.
*
* Please refer to the NVIDIA end user license agreement (EULA) associated
* with this source code for terms and conditions that govern your use of
* this software. Any use, reproduction, disclosure, or distribution of
* this software and related documentation outside the terms of the EULA
* is strictly prohibited.
*
*/
#pragma once

#include <thread>
#include <mutex>
extern "C" {
#include <libavformat/avformat.h>
#include <libavutil/opt.h>
#include <libswresample/swresample.h>
};
#include "Logger.h"

using namespace std;

extern simplelogger::Logger *logger;

static string AvErrorToString(int av_error_code) {
    const auto buf_size = 1024U;
    char *err_string = (char *)calloc(buf_size, sizeof(*err_string));
    if (!err_string) {
        return string();
    }

    if (0 != av_strerror(av_error_code, err_string, buf_size - 1)) {
        free(err_string);
        stringstream ss;
        ss << "Unknown error with code " << av_error_code;
        return ss.str();
    }

    string str(err_string);
    free(err_string);
    return str;
}

class FFmpegStreamer {
private:
    AVFormatContext *oc = NULL;
    AVStream *vs = NULL;
    int nFps = 0;

public:
    FFmpegStreamer(AVCodecID eCodecId, int nWidth, int nHeight, int nFps, const char *szInFilePath) : nFps(nFps) {
        avformat_network_init();

        int ret = 0;

        if ((eCodecId == AV_CODEC_ID_H264) || (eCodecId == AV_CODEC_ID_HEVC))
            ret = avformat_alloc_output_context2(&oc, NULL, "mpegts", NULL);
        else if (eCodecId == AV_CODEC_ID_AV1)
            ret = avformat_alloc_output_context2(&oc, NULL, "ivf", NULL);

        if (ret < 0) {
            LOG(ERROR) << "FFmpeg: failed to allocate an AVFormatContext. Error message: "
                       << AvErrorToString(ret);
            return;
        }

        oc->url = av_strdup(szInFilePath);
        LOG(INFO) << "Streaming destination: " << oc->url;

        // Add video stream to oc
        vs = avformat_new_stream(oc, NULL);
        if (!vs) {
            LOG(ERROR) << "FFMPEG: Could not alloc video stream";
            return;
        }
        vs->id = 0;

        // Set video parameters
        AVCodecParameters *vpar = vs->codecpar;
        vpar->codec_id = eCodecId;
        vpar->codec_type = AVMEDIA_TYPE_VIDEO;
        vpar->width = nWidth;
        vpar->height = nHeight;

        // Everything is ready. Now open the output stream.
        if (avio_open(&oc->pb, oc->url, AVIO_FLAG_WRITE) < 0) {
            LOG(ERROR) << "FFMPEG: Could not open " << oc->url;
            return ;
        }

        // Write the container header
        if (avformat_write_header(oc, NULL)) {
            LOG(ERROR) << "FFMPEG: avformat_write_header error!";
            return;
        }
    }
    ~FFmpegStreamer() {
        if (oc) {
            av_write_trailer(oc);
            avio_close(oc->pb);
            avformat_free_context(oc);
        }
    }

    bool Stream(uint8_t *pData, int nBytes, int nPts) {
        AVPacket *pkt = av_packet_alloc();
        if (!pkt) {
            LOG(ERROR) << "AVPacket allocation failed !";
            return false;
        }
        pkt->pts = av_rescale_q(nPts++, AVRational {1, nFps}, vs->time_base);
        // No B-frames
        pkt->dts = pkt->pts;
        pkt->stream_index = vs->index;
        pkt->data = pData;
        pkt->size = nBytes;

        if(!memcmp(pData, "\x00\x00\x00\x01\x67", 5)) {
            pkt->flags |= AV_PKT_FLAG_KEY;
        }

        // Write the compressed frame into the output
        int ret = av_write_frame(oc, pkt);
        av_write_frame(oc, NULL);
        if (ret < 0) {
            LOG(ERROR) << "FFMPEG: Error while writing video frame";
        }

        av_packet_free(&pkt);
        return true;
    }
};
