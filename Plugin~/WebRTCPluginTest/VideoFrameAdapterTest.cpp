#include "pch.h"

#include <api/make_ref_counted.h>
#include <api/video/i420_buffer.h>

#include "GpuMemoryBuffer.h"
#include "VideoFrameAdapter.h"

namespace unity
{
namespace webrtc
{
    class ReadbackTestBuffer : public GpuMemoryBufferInterface
    {
    public:
        bool fail = true;

        Size GetSize() const override { return Size(16, 16); }
        UnityRenderingExtTextureFormat GetFormat() const override { return kUnityRenderingExtFormatR8G8B8A8_SRGB; }
        rtc::scoped_refptr<I420BufferInterface> ToI420() override
        {
            if (fail)
                return nullptr;
            return I420Buffer::Create(16, 16);
        }
        const GpuMemoryBufferHandle* handle() const override { return nullptr; }
    };

    static ::webrtc::VideoFrame CreateReadbackTestFrame(rtc::scoped_refptr<ReadbackTestBuffer> buffer)
    {
        return VideoFrameAdapter::CreateVideoFrame(VideoFrame::WrapExternalGpuMemoryBuffer(
            buffer->GetSize(), buffer, nullptr, TimeDelta::Zero()));
    }

    TEST(VideoFrameAdapterTest, FailedReadbackReturnsNullAndCanRecover)
    {
        auto gpuBuffer = rtc::make_ref_counted<ReadbackTestBuffer>();
        auto frame = CreateReadbackTestFrame(gpuBuffer);
        auto buffer = frame.video_frame_buffer();

        EXPECT_EQ(buffer->GetI420(), nullptr);
        EXPECT_EQ(buffer->ToI420(), nullptr);

        gpuBuffer->fail = false;
        auto recovered = buffer->ToI420();
        ASSERT_NE(recovered, nullptr);
        EXPECT_EQ(recovered->width(), 16);
        EXPECT_EQ(recovered->height(), 16);
        EXPECT_EQ(buffer->GetI420(), recovered.get());
    }

    TEST(VideoFrameAdapterTest, FailedScaledReadbackIsNotCachedAndCanRecover)
    {
        auto gpuBuffer = rtc::make_ref_counted<ReadbackTestBuffer>();
        auto frame = CreateReadbackTestFrame(gpuBuffer);
        auto scaled = frame.video_frame_buffer()->Scale(8, 8);
        ASSERT_NE(scaled, nullptr);
        VideoFrameBuffer::Type types[] = { VideoFrameBuffer::Type::kI420 };

        EXPECT_EQ(scaled->GetI420(), nullptr);
        EXPECT_EQ(scaled->ToI420(), nullptr);
        EXPECT_EQ(scaled->GetMappedFrameBuffer(types), nullptr);

        gpuBuffer->fail = false;
        auto recovered = scaled->ToI420();
        ASSERT_NE(recovered, nullptr);
        EXPECT_EQ(recovered->width(), 8);
        EXPECT_EQ(recovered->height(), 8);
        EXPECT_EQ(scaled->GetI420(), recovered.get());
        EXPECT_NE(scaled->GetMappedFrameBuffer(types), nullptr);
    }

} // namespace webrtc
} // namespace unity
