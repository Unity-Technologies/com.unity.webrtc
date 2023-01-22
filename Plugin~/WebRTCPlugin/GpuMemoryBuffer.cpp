#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferHandle::GpuMemoryBufferHandle() { }
    GpuMemoryBufferHandle::GpuMemoryBufferHandle(GpuMemoryBufferHandle&& other) = default;
    GpuMemoryBufferHandle& GpuMemoryBufferHandle::operator=(GpuMemoryBufferHandle&& other) = default;

    GpuMemoryBufferHandle::~GpuMemoryBufferHandle() { }

    GpuMemoryBufferFromUnity::GpuMemoryBufferFromUnity(
        IGraphicsDevice* device, NativeTexPtr ptr, const Size& size, UnityRenderingExtTextureFormat format)
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
            handle_ = device_->Map(texture_.get());
        }
#endif
        CopyBuffer(ptr);
    }

    GpuMemoryBufferFromUnity::~GpuMemoryBufferFromUnity() { }

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

    void GpuMemoryBufferFromUnity::CopyBuffer(NativeTexPtr ptr)
    {
        // One texture cannot map CUDA memory and CPU memory simultaneously.
        // Believe there is still room for improvement.
        device_->CopyResourceFromNativeV(texture_.get(), ptr);
        device_->CopyResourceFromNativeV(textureCpuRead_.get(), ptr);
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
        if (!i420Buffer_)
        {
            int width = static_cast<int>(textureCpuRead_->GetWidth());
            int height = static_cast<int>(textureCpuRead_->GetHeight());
            i420Buffer_ = I420Buffer::Create(width, height);
        }
        if(!device_->ConvertI420Buffer(textureCpuRead_.get(), i420Buffer_))
        {
            RTC_LOG(LS_INFO) << "ConvertI420Buffer failed.";
            return nullptr;
        }
        return i420Buffer_;
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
