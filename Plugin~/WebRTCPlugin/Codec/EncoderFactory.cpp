#include "pch.h"
#include "IEncoder.h"
#include "Context.h"
#include "EncoderFactory.h"

#if defined(SUPPORT_OPENGL_CORE)
#include "NvCodec/NvEncoderGL.h"
#endif

#if defined(SUPPORT_D3D11)
#include "NvCodec/NvEncoderD3D11.h"
#endif
#include "SoftwareCodec/SoftwareEncoder.h"

#include "NvCodec/NvEncoderCuda.h"

#include "GraphicsDevice/IGraphicsDevice.h"
#if defined(SUPPORT_METAL)
#include "VideoToolbox/VTEncoderMetal.h"
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

    //Can throw exception. The caller is expected to catch it.
    void EncoderFactory::Init(int width, int height, IGraphicsDevice* device)
    {
        const GraphicsDeviceType deviceType = device->GetDeviceType();
        switch (deviceType) {
#if defined(SUPPORT_D3D11)
            case GRAPHICS_DEVICE_D3D11: {
                if (!ContextManager::s_use_software_encoder)
                {
                    m_encoder = std::make_unique<NvEncoderD3D11>(width, height, device);
                } else {
                    m_encoder = std::make_unique<SoftwareEncoder>(width, height, device);
                }
                break;
            }
#endif
#if defined(SUPPORT_OPENGL_CORE)
            case GRAPHICS_DEVICE_OPENGL: {
                m_encoder = std::make_unique<NvEncoderGL>(width, height, device);
                break;
            }
#endif
#if defined(SUPPORT_VULKAN)
            case GRAPHICS_DEVICE_VULKAN: {
                m_encoder = std::make_unique<NvEncoderCuda>(width, height, device);
                break;
            }
#endif            
#if defined(SUPPORT_METAL) && defined(SUPPORT_SOFTWARE_ENCODER)
            case GRAPHICS_DEVICE_METAL: {
                m_encoder = std::make_unique<SoftwareEncoder>(width, height, device);
                break;
            }
#endif            
            default: {
                throw std::invalid_argument("Invalid device to initialize NvEncoder");
                break;
            }           
        }

        m_encoder->InitV();
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
