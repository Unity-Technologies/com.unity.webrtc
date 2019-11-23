#include "pch.h"
#include "Codec/VideoToolbox/VTEncoderMetal.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace WebRTC {

    void callbackCompressed(void *outputCallbackRefCon,
                            void *sourceFrameRefCon,
                            OSStatus status,
                            VTEncodeInfoFlags infoFlags,
                            CMSampleBufferRef sampleBuffer)
    {
        NSLog(@"didCompressH264 called with status %d infoFlags %d", (int)status, (int)infoFlags);
        if (status != noErr) return;
        
        if (!CMSampleBufferDataIsReady(sampleBuffer))
        {
            NSLog(@"didCompressH264 data is not ready ");
            return;
        }
        auto encoder = reinterpret_cast<VTEncoderMetal*>(sourceFrameRefCon);
        CMBlockBufferRef dataBuffer = CMSampleBufferGetDataBuffer(sampleBuffer);
        std::vector<uint8> encodedFrame;
        size_t length = CMBlockBufferGetDataLength(dataBuffer);
        encodedFrame.resize(length);
        status = CMBlockBufferGetDataPointer(dataBuffer, 0, nil, nil, (char **)(encodedFrame.data()));
        if (status != noErr) return;
        encoder->CaptureFrame(encodedFrame);
    }

    VTEncoderMetal::VTEncoderMetal(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device)
        : m_device(device), m_width(nWidth), m_height(nHeight)
    {
        OSStatus status = VTCompressionSessionCreate(NULL, nWidth, nHeight,
                                                     kCMVideoCodecType_H264,
                                                     NULL, NULL, NULL,
                                                     callbackCompressed, (__bridge void*)(this),
                                                     &encoderSession);
    
        if (status != noErr)
        {
            NSLog(@"VTCompressionSessionCreate failed %d", status);
            // return false;
        }
    
        for(NSInteger i = 0; i < bufferedFrameNum; i++)
        {
            CVPixelBufferPoolRef pixelBufferPool; // Pool to precisely match the format
            pixelBufferPool = VTCompressionSessionGetPixelBufferPool(encoderSession);
        
            CVReturn result = CVPixelBufferPoolCreatePixelBuffer(NULL, pixelBufferPool, &pixelBuffers[i]);
            if(result != kCVReturnSuccess)
            {
                throw;
            }
            id<MTLDevice> device_ = (__bridge id<MTLDevice>)m_device->GetEncodeDevicePtrV();

        
            CVMetalTextureCacheRef textureCache;
            result = CVMetalTextureCacheCreate(kCFAllocatorDefault,
            nil, device_, nil, &textureCache);
            if(result != kCVReturnSuccess)
            {
                throw;
            }

            CVMetalTextureRef imageTexture;
            result = CVMetalTextureCacheCreateTextureFromImage(kCFAllocatorDefault,
                                                               textureCache,
                                                               pixelBuffers[i],
                                                               nil,
                                                               MTLPixelFormatBGRA8Unorm,
                                                               m_width, m_height, 0,
                                                               &imageTexture);
            if(result != kCVReturnSuccess)
            {
                throw;
            }
            id<MTLTexture> tex = CVMetalTextureGetTexture(imageTexture);
            renderTextures[i] = m_device->CreateDefaultTextureFromNativeV(m_width, m_height, tex);
        }
        
    }

    VTEncoderMetal::~VTEncoderMetal()
    {
        OSStatus status = VTCompressionSessionCompleteFrames(encoderSession, kCMTimeInvalid);
        if (status != noErr)
        {
            NSLog(@"VTCompressionSessionCompleteFrames failed %d", status);
        }
    }
    void VTEncoderMetal::SetRate(uint32_t rate)
    {
    }
    void VTEncoderMetal::UpdateSettings()
    {
    }
    bool VTEncoderMetal::CopyBuffer(void* frame)
    {
        const int curFrameNum = GetCurrentFrameCount() % bufferedFrameNum;
        const auto tex = renderTextures[curFrameNum];
        if (tex == nullptr)
            return false;
        m_device->CopyResourceFromNativeV(tex, frame);
        return true;
    }
    bool VTEncoderMetal::EncodeFrame()
    {
        UpdateSettings();
        uint32 bufferIndexToWrite = frameCount % bufferedFrameNum;

        CMTime presentationTimeStamp = CMTimeMake(frameCount, 1000);
        VTEncodeInfoFlags flags;
        OSStatus status = VTCompressionSessionEncodeFrame(encoderSession,
                                                          pixelBuffers[bufferIndexToWrite],
                                                          presentationTimeStamp,
                                                          kCMTimeInvalid,
                                                          NULL, this, &flags);
    
        if (status != noErr)
        {
            return false;
        }
        frameCount++;
        return true;
    }
    bool VTEncoderMetal::IsSupported() const
    {
        return true;
    }
    void VTEncoderMetal::SetIdrFrame()
    {
        // Nothing to do
    }
}
