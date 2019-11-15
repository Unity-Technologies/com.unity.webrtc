#pragma once

namespace WebRTC {

    class IEncoder {
    public:
        virtual ~IEncoder() {};
        virtual void SetRate(uint32 rate) = 0;
        virtual void UpdateSettings() = 0;
        virtual bool CopyFrame(void* frame) = 0;
        virtual void EncodeFrame() = 0;
        virtual bool IsSupported() const = 0;
        virtual void SetIdrFrame() = 0;
        virtual uint64 GetCurrentFrameCount() = 0;
        sigslot::signal1<std::vector<uint8>&> CaptureFrame;
    };
}
