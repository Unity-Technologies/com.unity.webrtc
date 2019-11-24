#include "pch.h"
#include "Codec/VideoToolbox/VTEncoderMetal.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

#include <libkern/OSByteOrder.h>

namespace WebRTC {

    class H264Info
    {
        public:
            H264Info() {};
            bool IsKeyFrame() { return isKeyFrame; }
            friend H264Info* CMSampleBufferH264Parser(CMSampleBufferRef sampleBuffer, std::vector<uint8_t>& encodedFrame);
        private:
            bool isKeyFrame;
            int nalUnitHeaderLengthOut = 0;
            size_t parameterSetCountOut = 0;
            size_t naluOffset = 0;
            std::vector<const uint8_t*> params;
            std::vector<size_t> paramSizes;
            CMBlockBufferRef blockBuffer;
            size_t sizeBlockBuffer;
    };

    H264Info* CMSampleBufferH264Parser(CMSampleBufferRef sampleBuffer, std::vector<uint8_t>& encodedFrame)
    {
        const char kAnnexBHeaderBytes[4] = {0, 0, 0, 1};
        auto info = std::make_unique<H264Info>();
        CFArrayRef attachments = CMSampleBufferGetSampleAttachmentsArray(sampleBuffer, false);
        if(attachments != nullptr && CFArrayGetCount(attachments))
        {
            CFDictionaryRef attachment = static_cast<CFDictionaryRef>(CFArrayGetValueAtIndex(attachments, 0));
            info->isKeyFrame = !CFDictionaryContainsKey(attachment, kCMSampleAttachmentKey_NotSync);
        }
    
        CMVideoFormatDescriptionRef description =
            CMSampleBufferGetFormatDescription(sampleBuffer);

        OSStatus status = CMVideoFormatDescriptionGetH264ParameterSetAtIndex(
            description, 0, nullptr, nullptr, &info->parameterSetCountOut, &info->nalUnitHeaderLengthOut);
        if (status != noErr)
        {
            NSLog(@"VTCompressionOutputCallback CMVideoFormatDescriptionGetH264ParameterSetAtIndex returns failed %d", status);
            return nullptr;
        }
    
        if(info->isKeyFrame)
        {
            for(size_t i = 0; i < info->parameterSetCountOut; i++)
            {
                size_t parameterSetSizeOut = 0;
                const uint8_t* parameterSetPointerOut = nullptr;

                status = CMVideoFormatDescriptionGetH264ParameterSetAtIndex(
                    description, i, &parameterSetPointerOut, &parameterSetSizeOut, nullptr, nullptr);
                if (status != noErr)
                {
                    NSLog(@"VTCompressionOutputCallback CMVideoFormatDescriptionGetH264ParameterSetAtIndex returns failed %d", status);
                    return nullptr;
                }
                encodedFrame.insert(encodedFrame.end(), std::begin(kAnnexBHeaderBytes), std::end(kAnnexBHeaderBytes));
                encodedFrame.insert(encodedFrame.end(), parameterSetPointerOut, parameterSetPointerOut + parameterSetSizeOut);
            }
        }
        info->blockBuffer = CMSampleBufferGetDataBuffer(sampleBuffer);
        if(info->blockBuffer == nullptr)
        {
            NSLog(@"VTCompressionOutputCallback CMSampleBufferGetDataBuffer is failed");
            return nullptr;
        }
        if (!CMBlockBufferIsRangeContiguous(info->blockBuffer, 0, 0))
        {
            NSLog(@"VTCompressionOutputCallback block buffer is not contiguous.");
            return nullptr;
        }
        info->sizeBlockBuffer = CMBlockBufferGetDataLength(info->blockBuffer);
    
        //auto sizeHeader = encodedFrame.size();
        //encodedFrame.resize(sizeHeader + info->sizeBlockBuffer);
        uint8_t* dataPointerOut = nullptr;
        status = CMBlockBufferGetDataPointer(info->blockBuffer, 0, nullptr, nullptr, (char **)(&dataPointerOut));
        if (status != noErr)
        {
            NSLog(@"VTCompressionOutputCallback CMBlockBufferGetDataLength is failed");
            return nullptr;
        }
        //auto sizeEncodedFrame = encodedFrame.size();
        auto remaining = info->sizeBlockBuffer;
        while(remaining > 0)
        {
            auto nalUnitSize = *(uint32_t*)(dataPointerOut);
            nalUnitSize = OSSwapBigToHostInt(nalUnitSize);
            auto nalUnitStart = dataPointerOut + info->nalUnitHeaderLengthOut;
            encodedFrame.insert(encodedFrame.end(), std::begin(kAnnexBHeaderBytes), std::end(kAnnexBHeaderBytes));
            encodedFrame.insert(encodedFrame.end(), nalUnitStart, nalUnitStart + nalUnitSize);
        
            auto sizeWritten = nalUnitSize + info->nalUnitHeaderLengthOut;
            remaining -= sizeWritten;
            dataPointerOut += sizeWritten;
        }
        if(remaining != 0)
        {
            NSLog(@"VTCompressionOutputCallback block buffer is broken");
            return nullptr;
        }
        return info.release();
    }

    void VTCompressionOutputCallback(void *outputCallbackRefCon,
                                     void *sourceFrameRefCon,
                                     OSStatus status,
                                     VTEncodeInfoFlags infoFlags,
                                     CMSampleBufferRef sampleBuffer)
    {
        if (status != noErr)
        {
            NSLog(@"VTCompressionOutputCallback returns failed %d", status);
            return;
        }
        if (!CMSampleBufferDataIsReady(sampleBuffer))
        {
            NSLog(@"VTCompressionOutputCallback data is not ready ");
            return;
        }
        VTEncoderMetal* encoder = reinterpret_cast<VTEncoderMetal*>(outputCallbackRefCon);
        std::vector<uint8>* encodedFrame = reinterpret_cast<std::vector<uint8>*>(sourceFrameRefCon);
    
        if(encoder == nullptr || encodedFrame == nullptr) {
            NSLog(@"VTCompressionOutputCallback parameter failed");
            return;
        }
        auto info = CMSampleBufferH264Parser(sampleBuffer, *encodedFrame);
        delete info;
        encoder->CaptureFrame(*encodedFrame);
    }

    VTEncoderMetal::VTEncoderMetal(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device)
        : m_device(device), m_width(nWidth), m_height(nHeight)
    {
        OSStatus status = VTCompressionSessionCreate(NULL, nWidth, nHeight,
                                                     kCMVideoCodecType_H264,
                                                     NULL, NULL, NULL,
                                                     VTCompressionOutputCallback, (__bridge void*)(this),
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
                                                          NULL, (void*)&encodedBuffers[bufferIndexToWrite], &flags);
    
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
