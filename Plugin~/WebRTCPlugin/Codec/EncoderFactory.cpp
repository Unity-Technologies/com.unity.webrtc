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

#if defined(SUPPORT_D3D12)
#include "NvCodec/NvEncoderD3D12.h"
#endif

#include "SoftwareCodec/SoftwareEncoder.h"

#if !defined(UNITY_OSX)
#include "NvCodec/NvEncoderCuda.h"
#endif

#include "GraphicsDevice/IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

    EncoderFactory& EncoderFactory::GetInstance() {
        static EncoderFactory factory;
        return factory;
    }

    bool EncoderFactory::GetHardwareEncoderSupport()
    {
#if defined(SUPPORT_METAL)
        return false;
#else
        return NvEncoder::LoadModule();
#endif
    }

    //Can throw exception. The caller is expected to catch it.
    std::unique_ptr<IEncoder> EncoderFactory::Init(int width, int height, IGraphicsDevice* device, UnityEncoderType encoderType)
    {
        std::unique_ptr<IEncoder> encoder;
        const GraphicsDeviceType deviceType = device->GetDeviceType();
        switch (deviceType) {
#if defined(SUPPORT_D3D11)
            case GRAPHICS_DEVICE_D3D11: {
                if (encoderType == UnityEncoderType::UnityEncoderHardware)
                {
                    encoder = std::make_unique<NvEncoderD3D11>(width, height, device);
                } else {
                    encoder = std::make_unique<SoftwareEncoder>(width, height, device);
                }
                break;
            }
#endif
#if defined(SUPPORT_D3D12)
            case GRAPHICS_DEVICE_D3D12: {
                if (encoderType == UnityEncoderType::UnityEncoderHardware)
                {
                    encoder = std::make_unique<NvEncoderD3D12>(width, height, device);
                } else {
                    encoder = std::make_unique<SoftwareEncoder>(width, height, device);
                }
                break;
            }
#endif
#if defined(SUPPORT_OPENGL_CORE)
            case GRAPHICS_DEVICE_OPENGL: {
                encoder = std::make_unique<NvEncoderGL>(width, height, device);
                break;
            }
#endif
#if defined(SUPPORT_VULKAN)
            case GRAPHICS_DEVICE_VULKAN: {
                if (encoderType == UnityEncoderType::UnityEncoderHardware)
                {
                    encoder = std::make_unique<NvEncoderCuda>(width, height, device);
                }
                else {
                    encoder = std::make_unique<SoftwareEncoder>(width, height, device);
                }
                break;
            }
#endif            
#if defined(SUPPORT_METAL) && defined(SUPPORT_SOFTWARE_ENCODER)
            case GRAPHICS_DEVICE_METAL: {
                encoder = std::make_unique<SoftwareEncoder>(width, height, device);
                break;
            }
#endif            
            default: {
                throw std::invalid_argument("Invalid device to initialize NvEncoder");
                break;
            }           
        }
        encoder->InitV();
        return encoder;
    }
    
} // end namespace webrtc
} // end namespace unity
