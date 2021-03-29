#include "pch.h"
#include "IEncoder.h"
#include "Context.h"
#include "EncoderFactory.h"

#if SUPPORT_OPENGL_CORE && CUDA_PLATFORM
#include "NvCodec/NvEncoderGL.h"
#endif

#if SUPPORT_D3D11
#include "NvCodec/NvEncoderD3D11.h"
#endif

#if SUPPORT_D3D12
#include "NvCodec/NvEncoderD3D12.h"
#endif

#include "SoftwareCodec/SoftwareEncoder.h"

#if SUPPORT_VULKAN && CUDA_PLATFORM
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
#if UNITY_OSX || UNITY_IOS
        // todo(kazuki): check VideoToolbox compatibility
        return true;
#elif UNITY_ANDROID
        // todo(kazuki): check Android hwcodec compatibility
        return true;
#elif CUDA_PLATFORM
        if(!NvEncoder::LoadModule())
        {
            return false;
        }
        return NvEncoder::CheckDriverVersion();
#endif
        return false;
    }

    //Can throw exception. The caller is expected to catch it.
    std::unique_ptr<IEncoder> EncoderFactory::Init(
        int width,
        int height,
        IGraphicsDevice* device,
        UnityEncoderType encoderType,
        UnityRenderingExtTextureFormat textureFormat)
    {
        std::unique_ptr<IEncoder> encoder;
        const GraphicsDeviceType deviceType = device->GetDeviceType();
        switch (deviceType) {
#if SUPPORT_D3D11
            case GRAPHICS_DEVICE_D3D11: {
                if (encoderType == UnityEncoderType::UnityEncoderHardware)
                {
                    encoder = std::make_unique<NvEncoderD3D11>(width, height, device, textureFormat);
                } else {
                    encoder = std::make_unique<SoftwareEncoder>(width, height, device, textureFormat);
                }
                break;
            }
#endif
#if SUPPORT_D3D12
            case GRAPHICS_DEVICE_D3D12: {
                if (encoderType == UnityEncoderType::UnityEncoderHardware)
                {
                    encoder = std::make_unique<NvEncoderD3D12>(width, height, device, textureFormat);
                } else {
                    encoder = std::make_unique<SoftwareEncoder>(width, height, device, textureFormat);
                }
                break;
            }
#endif
#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
            case GRAPHICS_DEVICE_OPENGL: {
#if CUDA_PLATFORM
                if (encoderType == UnityEncoderType::UnityEncoderHardware)
                {
                    encoder = std::make_unique<NvEncoderGL>(width, height, device, textureFormat);
                    break;
                }
#endif
                encoder = std::make_unique<SoftwareEncoder>(width, height, device, textureFormat);
            }
#endif
#if SUPPORT_VULKAN
            case GRAPHICS_DEVICE_VULKAN: {
#if CUDA_PLATFORM
                if (encoderType == UnityEncoderType::UnityEncoderHardware)
                {
                    encoder = std::make_unique<NvEncoderCuda>(width, height, device, textureFormat);
                    break;
                }
#endif
                encoder = std::make_unique<SoftwareEncoder>(width, height, device, textureFormat);
                break;
            }
#endif            
#if SUPPORT_METAL
            case GRAPHICS_DEVICE_METAL: {
                encoder = std::make_unique<SoftwareEncoder>(width, height, device, textureFormat);
                break;
            }
#endif            
            default: {
                throw std::invalid_argument("Invalid device to initialize NvEncoder");
                break;
            }           
        }
        encoder->InitV();
        return std::move(encoder);
    }
    
} // end namespace webrtc
} // end namespace unity
