#include "pch.h"
#include "VideoFrame.h"
#include "GpuMemoryBuffer.h"
#include "VideoFrameUtil.h"

namespace unity {
namespace webrtc {

TEST(VideoFrame, WrapExternalGpuMemoryBuffer) {

    //auto timestamp = webrtc::TimeDelta::Micros(
    //    webrtc::Clock::GetRealTimeClock()->TimeInMicroseconds());

    //const Size kSize(1280, 720);
    //const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
    //auto frame = CreateTestFrame(kSize, kFormat);

    //EXPECT_EQ(frame->layout().format(), PIXEL_FORMAT_NV12);
    //EXPECT_EQ(frame->layout().coded_size(), coded_size);
    //EXPECT_EQ(frame->layout().num_planes(), 2u);
    //EXPECT_EQ(frame->layout().is_multi_planar(), false);
    //for (size_t i = 0; i < 2; ++i) {
    //    EXPECT_EQ(frame->layout().planes()[i].stride, coded_size.width());
    //}
    //EXPECT_EQ(frame->layout().modifier(), modifier);
    //EXPECT_EQ(frame->storage_type(), VideoFrame::STORAGE_GPU_MEMORY_BUFFER);
    //EXPECT_TRUE(frame->HasGpuMemoryBuffer());
    //EXPECT_EQ(frame->coded_size(), coded_size);
    //EXPECT_EQ(frame->visible_rect(), visible_rect);
    //EXPECT_EQ(frame->timestamp(), timestamp);
    //EXPECT_EQ(frame->HasTextures(), true);
    //EXPECT_EQ(frame->HasReleaseMailboxCB(), true);
    //EXPECT_EQ(frame->mailbox_holder(0).mailbox, mailbox_holders[0].mailbox);
    //EXPECT_EQ(frame->mailbox_holder(1).mailbox, mailbox_holders[1].mailbox);
}

} // end namespace webrtc
} // end namespace unity

