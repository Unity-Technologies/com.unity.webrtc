#include "pch.h"
#include "MetalGraphicsDevice.h"
#include "MetalTexture2D.h"
#include "GraphicsDevice/GraphicsUtility.h"

namespace unity
{
namespace webrtc
{

    MetalGraphicsDevice::MetalGraphicsDevice(id<MTLDevice>  device, IUnityGraphicsMetal* unityGraphicsMetal)
        : m_device(device)
        , m_unityGraphicsMetal(unityGraphicsMetal)
    {
    }

//---------------------------------------------------------------------------------------------------------------------
    MetalGraphicsDevice::~MetalGraphicsDevice() {
        m_device = nil;
        m_unityGraphicsMetal = nullptr;
    }

//---------------------------------------------------------------------------------------------------------------------
    bool MetalGraphicsDevice::InitV() {
        return true;
    }
//---------------------------------------------------------------------------------------------------------------------

    void MetalGraphicsDevice::ShutdownV() {
    }

//---------------------------------------------------------------------------------------------------------------------
    ITexture2D* MetalGraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) {
        MTLTextureDescriptor *textureDescriptor = [[MTLTextureDescriptor alloc] init];
        textureDescriptor.pixelFormat = ConvertFormat(textureFormat);
        textureDescriptor.width = w;
        textureDescriptor.height = h;
        id<MTLTexture> texture = [m_device newTextureWithDescriptor:textureDescriptor];
        return new MetalTexture2D(w, h, texture);
    }

//---------------------------------------------------------------------------------------------------------------------
    ITexture2D* MetalGraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
        id<MTLTexture> texture = (__bridge id<MTLTexture>)nativeTexturePtr;
        return new MetalTexture2D(w, h, texture);
    }


//---------------------------------------------------------------------------------------------------------------------
    bool MetalGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
        id<MTLTexture> dstTexture = (__bridge id<MTLTexture>)dest->GetNativeTexturePtrV();
        id<MTLTexture> srcTexture = (__bridge id<MTLTexture>)src->GetNativeTexturePtrV();
        return CopyTexture(dstTexture, srcTexture);
    }

//---------------------------------------------------------------------------------------------------------------------
    bool MetalGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
        if(nativeTexturePtr == nullptr) {
            return false;
        }
        id<MTLTexture> dstTexture = (__bridge id<MTLTexture>)dest->GetNativeTexturePtrV();
        id<MTLTexture> srcTexture = (__bridge id<MTLTexture>)nativeTexturePtr;
        return CopyTexture(dstTexture, srcTexture);
    }

//---------------------------------------------------------------------------------------------------------------------

    bool MetalGraphicsDevice::CopyTexture(id<MTLTexture> dest, id<MTLTexture> src)
    {
        if(dest == src)
            return false;

        if(src.pixelFormat != dest.pixelFormat)
            return false;

        m_unityGraphicsMetal->EndCurrentCommandEncoder();

        id<MTLCommandBuffer> commandBuffer = m_unityGraphicsMetal->CurrentCommandBuffer();
        id<MTLBlitCommandEncoder> blit = [commandBuffer blitCommandEncoder];
        
        NSUInteger width = src.width;
        NSUInteger height = src.height;

        MTLSize inTxtSize = MTLSizeMake(width, height, 1);
        MTLOrigin inTxtOrigin = MTLOriginMake(0, 0, 0);
        MTLOrigin outTxtOrigin = MTLOriginMake(0, 0, 0);

        [blit copyFromTexture:src
                        sourceSlice:0
                        sourceLevel:0
                        sourceOrigin:inTxtOrigin
                        sourceSize:inTxtSize
                        toTexture:dest
                        destinationSlice:0
                        destinationLevel:0
                        destinationOrigin:outTxtOrigin];
        
        //[TODO-sin: 2019-12-18] We don't need this if we are not using software encoding
#if TARGET_OS_OSX
        [blit synchronizeResource:dest];
#endif
        [blit endEncoding];
        blit = nil;
        m_unityGraphicsMetal->EndCurrentCommandEncoder();

        return true;
    }

//---------------------------------------------------------------------------------------------------------------------
    ITexture2D* MetalGraphicsDevice::CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat)
    {
        OSType pixelFormat = kCVPixelFormatType_32BGRA;
        CVPixelBufferRef pixelBuffer = NULL;
        CVMetalTextureCacheRef textureCache = NULL;
        CVMetalTextureRef textureRef = NULL;

        CVReturn err = CVMetalTextureCacheCreate(kCFAllocatorDefault, NULL, m_device, NULL, &textureCache);
        NSCAssert(err == kCVReturnSuccess, @"CVMetalTextureCacheCreate failed: %d", err);

        NSDictionary *attrs = @{
            (id) kCVPixelBufferMetalCompatibilityKey: @YES,
        };

        CFDictionaryRef cfAttrs = (__bridge_retained CFDictionaryRef) attrs;
        err = CVPixelBufferCreate(kCFAllocatorDefault, width, height, pixelFormat, cfAttrs, &pixelBuffer);
        CFRelease(cfAttrs);
        NSCAssert(err == kCVReturnSuccess, @"CVPixelBufferCreate failed: %d", err);

        OSType bufferPixelFormat = CVPixelBufferGetPixelFormatType(pixelBuffer);
        NSCAssert(pixelFormat == bufferPixelFormat, @"wrong pixel format: %u", (unsigned int) bufferPixelFormat);

        err = CVMetalTextureCacheCreateTextureFromImage(kCFAllocatorDefault, textureCache, pixelBuffer, NULL, ConvertFormat(textureFormat), width, height, 0, &textureRef);
        if (!textureRef || err) {
            NSLog(@"CVMetalTextureCacheCreateTextureFromImage failed (error: %d)", err);
        }
        NSCAssert(err == kCVReturnSuccess, @"CVMetalTextureCacheCreateTextureFromImage failed: %d", err);

        return new MetalTexture2D(width, height, CVMetalTextureGetTexture(textureRef), pixelBuffer, textureCache, textureRef);
    }

//---------------------------------------------------------------------------------------------------------------------
    MTLPixelFormat MetalGraphicsDevice::ConvertFormat(UnityRenderingExtTextureFormat format)
    {
        switch(format) {
            case kUnityRenderingExtFormatB8G8R8A8_SRGB:
                return MTLPixelFormatBGRA8Unorm_sRGB;
            case kUnityRenderingExtFormatB8G8R8A8_UNorm:
                return MTLPixelFormatBGRA8Unorm;
            default:
                return MTLPixelFormatInvalid;
        }
    }

} // end namespace webrtc
} // end namespace unity
