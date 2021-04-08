#pragma once

#include "IEncoder.h"

namespace unity
{
namespace webrtc
{
    
    class IEncoder;
    class IGraphicsDevice;
    class EncoderFactory
    {
    public:
        static EncoderFactory& GetInstance();
        static bool GetHardwareEncoderSupport();
        std::unique_ptr<IEncoder> Init(
                int width, int height, IGraphicsDevice* device, UnityEncoderType encoderType,
                UnityRenderingExtTextureFormat textureFormat); //Can throw exception.
    private:
        EncoderFactory() = default;
        EncoderFactory(EncoderFactory const&) = delete;
        EncoderFactory& operator=(EncoderFactory const&) = delete;
    };

} // end namespace webrtc
} // end namespace unity
