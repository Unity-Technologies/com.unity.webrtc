#pragma once
#include <vector>
#include <thread>
#include <atomic>
#include "Codec/IEncoder.h"

namespace unity
{
namespace webrtc
{

    class IGraphicsDevice;
    class ITexture2D;
    class VideoToolboxEncoder : public IEncoder
    {
    public:
        VideoToolboxEncoder(int _width, int _height, IGraphicsDevice* device, UnityRenderingExtTextureFormat textureFormat);
        virtual ~VideoToolboxEncoder();
        void InitV() override;
        void SetRates(uint32_t bitRate, int64_t frameRate) override {}
        void UpdateSettings() override {}
        bool CopyBuffer(void* frame) override;
        bool EncodeFrame(int64_t timestamp_us) override;
        bool IsSupported() const override { return true; }
        void SetIdrFrame() override {}
        uint64 GetCurrentFrameCount() const override { return m_frameCount; }

    private:
        IGraphicsDevice* m_device;
        ITexture2D* m_encodeTex[bufferedFrameNum];
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> m_videoFrameBuffer[bufferedFrameNum];
        int m_width;
        int m_height;
        UnityRenderingExtTextureFormat m_textureFormat;
        uint64 m_frameCount;
    };
//---------------------------------------------------------------------------------------------------------------------

} //end namespace webrtc
} //end namespace unity
