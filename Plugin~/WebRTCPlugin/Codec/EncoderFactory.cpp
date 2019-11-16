#include "pch.h"
#include "IEncoder.h"
#include "EncoderFactory.h"
#include "NvCodec/NvEncoder.h"

#if defined(SUPPORT_OPENGL_CORE)
#include "NvCodec/NvEncoderGL.h"
#endif

#if defined(SUPPORT_D3D11)
#include "NvCodec/NvEncoderD3D11.h"
#endif


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
#if defined(SUPPORT_OPENGL_CORE)
        m_encoder = std::make_unique<NvEncoderGL>(width, height, device);
#endif

#if defined(SUPPORT_D3D11)
        m_encoder = std::make_unique<NvEncoderD3D11>(width, height, device);
#endif

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
