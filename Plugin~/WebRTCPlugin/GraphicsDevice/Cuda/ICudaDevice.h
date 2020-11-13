#pragma once
#include <cuda.h>
#include <nvEncodeAPI.h>

namespace unity
{
namespace webrtc
{

class ICudaDevice
{
public:
    virtual bool IsCudaSupport() = 0;
    virtual CUcontext GetCuContext() = 0;
    virtual NV_ENC_BUFFER_FORMAT GetEncodeBufferFormat() = 0;
};
} // namespace webrtc
} // namespace unity
