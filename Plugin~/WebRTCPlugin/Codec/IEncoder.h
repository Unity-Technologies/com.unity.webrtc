#pragma once

namespace unity
{
namespace webrtc
{

    namespace webrtc = ::webrtc;
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
        virtual bool EncodeFrame() = 0;
        virtual bool IsSupported() const = 0;
        virtual void SetIdrFrame() = 0;
        virtual uint64 GetCurrentFrameCount() const = 0;
        sigslot::signal1<webrtc::VideoFrame&> CaptureFrame;

        CodecInitializationResult GetCodecInitializationResult() const { return m_initializationResult; }
    protected:
        CodecInitializationResult m_initializationResult = CodecInitializationResult::NotInitialized;
    };
    
} // end namespace webrtc
} // end namespace unity
