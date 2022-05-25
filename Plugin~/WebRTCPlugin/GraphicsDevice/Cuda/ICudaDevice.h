#pragma once

#include <cuda.h>

#include "nvEncodeAPI.h"

namespace unity
{
namespace webrtc
{

    class ICudaDevice
    {
    public:
        virtual ~ICudaDevice() = default;
        virtual bool IsCudaSupport() = 0;
        virtual CUcontext GetCUcontext() = 0;
        virtual NV_ENC_BUFFER_FORMAT GetEncodeBufferFormat() = 0;
    };
} // namespace webrtc
} // namespace unity
