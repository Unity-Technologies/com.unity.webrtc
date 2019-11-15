#pragma once

#include "IEncoder.h"

namespace WebRTC {
    class IEncoder;
    class IGraphicsDevice;
    class EncoderFactory {
    public:
        static EncoderFactory& GetInstance();
        bool IsInitialized() const;
        void Init(int width, int height, IGraphicsDevice* device);
        void Shutdown();
        IEncoder *GetEncoder() const;
    private:
        std::unique_ptr<IEncoder> m_encoder = nullptr;
    };
}
