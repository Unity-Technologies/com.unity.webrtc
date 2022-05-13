#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "GraphicsDeviceTestBase.h"
#include "Context.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "UnityVideoTrackSource.h"
#include "VideoFrameUtil.h"

using testing::_;
using testing::Invoke;
using testing::Mock;

namespace unity
{
namespace webrtc
{

class MockVideoSink : public rtc::VideoSinkInterface<webrtc::VideoFrame>
{
public:
    MOCK_METHOD1(OnFrame, void(const webrtc::VideoFrame&));
};

const int width = 1280;
const int height = 720;

class VideoTrackSourceTest : public GraphicsDeviceTestBase
{
public:
    VideoTrackSourceTest()
        : m_texture(nullptr)
    {
        m_trackSource = UnityVideoTrackSource::Create(false, absl::nullopt);
        m_trackSource->AddOrUpdateSink(&mock_sink_, rtc::VideoSinkWants());
    }
    ~VideoTrackSourceTest() override
    {
        m_trackSource->RemoveSink(&mock_sink_);
    }
protected:
    void SetUp() override
    {
        if (!device())
            GTEST_SKIP() << "The graphics driver is not installed on the device.";

        m_texture.reset(device()->CreateDefaultTextureV(width, height, format()));
        context = std::make_unique<Context>(device());
    }
    std::unique_ptr<Context> context;
    std::unique_ptr<ITexture2D> m_texture;

    MockVideoSink mock_sink_;
    rtc::scoped_refptr<UnityVideoTrackSource> m_trackSource;

    webrtc::VideoFrame::Builder CreateBlackFrameBuilder(int width, int height)
    {
        rtc::scoped_refptr<webrtc::I420Buffer> buffer =
            webrtc::I420Buffer::Create(width, height);

        webrtc::I420Buffer::SetBlack(buffer);
        return webrtc::VideoFrame::Builder().set_video_frame_buffer(buffer);
    }

    void SendTestFrame(int width, int height)
    {
        m_trackSource->OnFrameCaptured(0);
    }
};

//TEST_P(VideoTrackSourceTest, CreateVideoFrameAdapter)
//{
//    const Size size = Size(width, height);
//    const UnityRenderingExtTextureFormat format = kUnityRenderingExtFormatR8G8B8A8_SRGB;
//    auto frame = CreateTestFrame(size, format);
//
//    rtc::scoped_refptr<VideoFrameAdapter> frame_adapter(
//        new rtc::RefCountedObject<VideoFrameAdapter>(std::move(frame)));
//
//    EXPECT_EQ(VideoFrameBuffer::Type::kNative, frame_adapter->type());
//
//    absl::InlinedVector<VideoFrameBuffer::Type, kMaxPreferredPixelFormats>
//    supported_formats = { VideoFrameBuffer::Type::kI420,
//                         VideoFrameBuffer::Type::kNV12 };
//    auto mapped_frame = frame_adapter->GetMappedFrameBuffer(supported_formats);
//    EXPECT_EQ(nullptr, mapped_frame);
//}

TEST_P(VideoTrackSourceTest, CreateVideoSourceProxy)
{
    std::unique_ptr<rtc::Thread> workerThread = rtc::Thread::Create();
    workerThread->Start();
    std::unique_ptr<rtc::Thread> signalingThread = rtc::Thread::Create();
    signalingThread->Start();

    rtc::scoped_refptr<webrtc::VideoTrackSourceInterface> videoSourceProxy =
        webrtc::VideoTrackSourceProxy::Create(
            signalingThread.get(),
            workerThread.get(), m_trackSource);
}

// todo::(kazuki) fix MetalGraphicsDevice.mm
#if !defined(SUPPORT_METAL)
//TEST_P(VideoTrackSourceTest, SendTestFrame)
//{
//    EXPECT_CALL(mock_sink_, OnFrame(_))
//        .WillOnce(Invoke([](const webrtc::VideoFrame& frame) {
//            EXPECT_EQ(width, frame.width());
//            EXPECT_EQ(height, frame.height());
//
//            //GpuMemoryBuffer* buffer
//            //    = static_cast<GpuMemoryBuffer*>(frame.video_frame_buffer().get());
//            //EXPECT_NE(buffer, nullptr);
//            //rtc::scoped_refptr<I420BufferInterface> i420Buffer = buffer->ToI420();
//            //EXPECT_NE(i420Buffer, nullptr);
//            //CUarray array = buffer->ToArray();
//            //EXPECT_NE(array, nullptr);
//    }));
//    const Size size = Size(width, height);
//    const UnityRenderingExtTextureFormat format = kUnityRenderingExtFormatR8G8B8A8_SRGB;
//
//    auto frame = CreateTestFrame(size, format);
//    m_trackSource->OnFrameCaptured(std::move(frame));
//}
#endif

INSTANTIATE_TEST_SUITE_P(GfxDeviceAndColorSpece,
    VideoTrackSourceTest,
    testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
