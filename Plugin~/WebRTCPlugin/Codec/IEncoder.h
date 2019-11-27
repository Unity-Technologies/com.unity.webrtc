﻿#pragma once

namespace WebRTC {

    enum class CodecInitializationResult
    {
        NotInitialized,
        Success,
        DriverNotInstalled,
        DriverVersionDoesNotSupportAPI,
        APINotFound,
        EncoderInitializationFailed
    };

    class IEncoder {
    public:
        virtual ~IEncoder() {};        
        virtual void InitV() = 0;   //Can throw exception. 
        virtual void SetRate(uint32_t rate) = 0;
        virtual void UpdateSettings() = 0;
        virtual bool CopyBuffer(void* frame) = 0;
        virtual void EncodeFrame() = 0;
        virtual bool IsSupported() const = 0;
        virtual void SetIdrFrame() = 0;
        virtual uint64 GetCurrentFrameCount() = 0;
        sigslot::signal1<webrtc::VideoFrame&> CaptureFrame;
    };
}
