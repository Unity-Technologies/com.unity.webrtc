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
        virtual void SetRates(uint32_t bitRate, int64_t frameRate) = 0;
        virtual void UpdateSettings() = 0;
        virtual bool CopyBuffer(void* frame) = 0;
        virtual bool EncodeFrame(int64_t timestamp_us) = 0;
        virtual bool IsSupported() const = 0;
        virtual void SetIdrFrame() = 0;
        virtual uint64 GetCurrentFrameCount() const = 0;
        sigslot::signal1<const webrtc::VideoFrame&> CaptureFrame;

        // todo(kazuki): remove this virtual method after refactoring DummyVideoEncoder
        virtual void SetEncoderId(const uint32_t id) { m_encoderId = id;  }
        virtual uint32_t Id() const { return m_encoderId; }

        CodecInitializationResult GetCodecInitializationResult() const { return m_initializationResult; }
    protected:
        CodecInitializationResult m_initializationResult = CodecInitializationResult::NotInitialized;
        uint32_t m_encoderId;
    };
    
} // end namespace webrtc
} // end namespace unity
