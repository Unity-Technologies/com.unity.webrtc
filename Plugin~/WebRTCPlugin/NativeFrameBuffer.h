#pragma once

#include <api/video/video_frame_buffer.h>
#include <rtc_base/ref_counted_object.h>

// todo(kazuki)::remove
#include "VideoFrameAdapter.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class IGraphicsDevice;
    class ITexture2D;
    struct GpuMemoryBufferHandle;
    class NativeFrameBuffer : public ScalableBufferInterface
    {
    public:
        class ScaledBuffer : public ScalableBufferInterface
        {
        public:
            ScaledBuffer(rtc::scoped_refptr<VideoFrameAdapter> parent, int width, int height);
            ~ScaledBuffer() override;

            VideoFrameBuffer::Type type() const override;
            int width() const override { return width_; }
            int height() const override { return height_; }
            bool scaled() const final { return true; }

            rtc::scoped_refptr<webrtc::I420BufferInterface> ToI420() override;
            const webrtc::I420BufferInterface* GetI420() const override;

            rtc::scoped_refptr<webrtc::VideoFrameBuffer>
            GetMappedFrameBuffer(rtc::ArrayView<webrtc::VideoFrameBuffer::Type> types) override;

            rtc::scoped_refptr<webrtc::VideoFrameBuffer> CropAndScale(
                int offset_x, int offset_y, int crop_width, int crop_height, int scaled_width, int scaled_height)
                override;

        private:
            const rtc::scoped_refptr<NativeFrameBuffer> parent_;
            const int width_;
            const int height_;
        };

        static rtc::scoped_refptr<NativeFrameBuffer>
        Create(int width, int height, UnityRenderingExtTextureFormat format, IGraphicsDevice* device)
        {
            return new rtc::RefCountedObject<NativeFrameBuffer>(width, height, format, device);
        }
        VideoFrameBuffer::Type type() const override { return Type::kNative; }
        int width() const override { return texture_->GetWidth(); }
        int height() const override { return texture_->GetHeight(); }
        bool scaled() const override { return false; }
        UnityRenderingExtTextureFormat format() { return texture_->GetFormat(); };
        ITexture2D* texture() { return texture_.get(); }
        rtc::scoped_refptr<I420BufferInterface> ToI420() override { return I420Buffer::Create(width(), height()); }
        const webrtc::I420BufferInterface* GetI420() const override { return I420Buffer::Create(width(), height()); }
        const GpuMemoryBufferHandle* handle() { return handle_.get(); }

    protected:
        NativeFrameBuffer(int width, int height, UnityRenderingExtTextureFormat format, IGraphicsDevice* device)
            : texture_(device->CreateDefaultTextureV(width, height, format))
            , handle_(device->Map(texture_.get()))
        {
        }
        ~NativeFrameBuffer() override { }

    private:
        std::unique_ptr<ITexture2D> texture_;
        std::unique_ptr<GpuMemoryBufferHandle> handle_;
    };
}
}
