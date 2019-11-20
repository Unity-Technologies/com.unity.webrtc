#include "pch.h"
#include "IEncoder.h"
#include "EncoderFactory.h"

#include "NvCodec/NvEncoder.h"


namespace WebRTC {

    EncoderFactory& EncoderFactory::GetInstance() {
        static EncoderFactory factory;
        return factory;
    }
    bool EncoderFactory::IsInitialized() const
    {
        return m_encoder.get() != nullptr;
    }
    void EncoderFactory::Init(int width, int height, IGraphicsDevice* device)
    {
        m_encoder = std::make_unique<NvEncoder>(width, height, device);
    }
    void EncoderFactory::Shutdown()
    {
        m_encoder.reset();
    }
    IEncoder* EncoderFactory::GetEncoder() const
    {
        return m_encoder.get();
    }
}
