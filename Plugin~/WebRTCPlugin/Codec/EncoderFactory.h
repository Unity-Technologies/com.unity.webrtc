#pragma once

#include "IEncoder.h"

namespace WebRTC {
    class IEncoder;
    class IGraphicsDevice;
    class EncoderFactory {
    public:
        static EncoderFactory& GetInstance();
        static bool GetHardwareEncoderSupport();
        //bool IsInitialized() const;
        std::unique_ptr<IEncoder> Init(int width, int height, IGraphicsDevice* device, UnityEncoderType encoderType); //Can throw exception.
        //void Shutdown();
        //IEncoder *GetEncoder() const;
    private:
        EncoderFactory() = default;
        EncoderFactory(EncoderFactory const&) = delete;
        EncoderFactory& operator=(EncoderFactory const&) = delete;
        //std::unique_ptr<IEncoder> m_encoder = nullptr;
    };
}
