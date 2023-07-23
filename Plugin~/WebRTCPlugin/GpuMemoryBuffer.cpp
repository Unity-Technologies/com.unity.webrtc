#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDevice/IGraphicsDevice.h"

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"
#endif

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferHandle::GpuMemoryBufferHandle() { }
    GpuMemoryBufferHandle::GpuMemoryBufferHandle(GpuMemoryBufferHandle&& other) = default;
    GpuMemoryBufferHandle& GpuMemoryBufferHandle::operator=(GpuMemoryBufferHandle&& other) = default;
    GpuMemoryBufferHandle::~GpuMemoryBufferHandle() = default;

#if CUDA_PLATFORM
    GpuMemoryBufferFromCuda::GpuMemoryBufferFromCuda(
        CUcontext context,
        CUdeviceptr ptr,
        const Size& size,
        UnityRenderingExtTextureFormat format,
        GpuMemoryBufferHandle::AccessMode mode)
        : size_(size)
        , handle_(GpuMemoryBufferCudaHandle::CreateHandle(context, ptr, mode))
        , format_(format)
    {
    }

    GpuMemoryBufferFromCuda::~GpuMemoryBufferFromCuda() = default;

    UnityRenderingExtTextureFormat GpuMemoryBufferFromCuda::GetFormat() const { return format_; }
    Size GpuMemoryBufferFromCuda::GetSize() const { return size_; }
    rtc::scoped_refptr<I420BufferInterface> GpuMemoryBufferFromCuda::ToI420()
    {
        assert("This is not supported");
        throw;
    }
    const GpuMemoryBufferHandle* GpuMemoryBufferFromCuda::handle() const { return handle_.get(); }
#endif

#if __ANDROID__
    AHardwareBufferHandle::AHardwareBufferHandle() : buffer(nullptr) { }
    AHardwareBufferHandle::AHardwareBufferHandle(AHardwareBufferHandle&& other) = default;
    AHardwareBufferHandle& AHardwareBufferHandle::operator=(AHardwareBufferHandle&& other) = default;

    AHardwareBufferHandle::~AHardwareBufferHandle() { }
#endif

    GpuMemoryBufferFromUnity::GpuMemoryBufferFromUnity(
        IGraphicsDevice* device, void* ptr, const Size& size, UnityRenderingExtTextureFormat format, GpuMemoryBufferHandle::AccessMode mode)
        : device_(device)
        , format_(format)
        , size_(size)
        , texture_(nullptr)
        , textureCpuRead_(nullptr)
        , handle_(nullptr)
    {
        uint32_t width = static_cast<uint32_t>(size.width());
        uint32_t height = static_cast<uint32_t>(size.height());
        texture_.reset(device_->CreateDefaultTextureV(width, height, format));
        textureCpuRead_.reset(device_->CreateCPUReadTextureV(width, height, format));

// todo(kazuki): need to refactor
#if CUDA_PLATFORM
        if (device_->IsCudaSupport())
        {
            // IGraphicsDevice::Map method is too heavy and stop the graphics process,
            // so must not call this method on the worker thread instead of the render thread.
            handle_ = device_->Map(texture_.get(), mode);
        }
#endif
    }

    GpuMemoryBufferFromUnity::~GpuMemoryBufferFromUnity()
    {
        // Make sure handle_ is released first
        handle_ = nullptr;
        texture_ = nullptr;
        textureCpuRead_ = nullptr;
    }

    bool GpuMemoryBufferFromUnity::ResetSync()
    {
        if (!device_->ResetSync(texture_.get()))
        {
            RTC_LOG(LS_INFO) << "ResetSync failed.";
            return false;
        }
        if (!device_->ResetSync(textureCpuRead_.get()))
        {
            RTC_LOG(LS_INFO) << "ResetSync failed.";
            return false;
        }
        return true;
    }

    bool GpuMemoryBufferFromUnity::CopyBuffer(void* ptr)
    {
        // One texture cannot map CUDA memory and CPU memory simultaneously.
        // Believe there is still room for improvement.
        if (!device_->CopyResourceFromNativeV(texture_.get(), ptr))
            return false;
        if (!device_->CopyResourceFromNativeV(textureCpuRead_.get(), ptr))
            return false;
        return true;
    }

    UnityRenderingExtTextureFormat GpuMemoryBufferFromUnity::GetFormat() const { return format_; }

    Size GpuMemoryBufferFromUnity::GetSize() const { return size_; }

    rtc::scoped_refptr<I420BufferInterface> GpuMemoryBufferFromUnity::ToI420()
    {
        using namespace std::chrono_literals;
        const std::chrono::nanoseconds timeout(30ms); // 30ms
        if (!device_->WaitSync(textureCpuRead_.get(), timeout.count()))
        {
            RTC_LOG(LS_INFO) << "WaitSync failed.";
            return nullptr;
        }
        return device_->ConvertRGBToI420(textureCpuRead_.get());
    }

    const GpuMemoryBufferHandle* GpuMemoryBufferFromUnity::handle() const
    {
        using namespace std::chrono_literals;
        const std::chrono::nanoseconds timeout(30ms); // 30ms
        if (!device_->WaitSync(texture_.get(), timeout.count()))
        {
            RTC_LOG(LS_INFO) << "WaitSync failed.";
            return nullptr;
        }
        return handle_.get();
    }
}
}
