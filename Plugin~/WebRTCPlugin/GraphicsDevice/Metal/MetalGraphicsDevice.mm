#include "pch.h"
#include "MetalGraphicsDevice.h"
#include "MetalTexture2D.h"
#include "GraphicsDevice/GraphicsUtility.h"
#import <Metal/Metal.h>

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
        MTLTextureDescriptor *textureDescriptor = [[MTLTextureDescriptor alloc] init];
        textureDescriptor.pixelFormat = ConvertFormat(textureFormat);
        textureDescriptor.width = width;
        textureDescriptor.height = height;
        textureDescriptor.allowGPUOptimizedContents = false;
#if TARGET_OS_OSX
        textureDescriptor.storageMode = MTLStorageMode(MTLStorageModeManaged);
#else
        textureDescriptor.storageMode = MTLStorageMode(MTLStorageModeShared);
#endif
        id<MTLTexture> texture = [m_device newTextureWithDescriptor:textureDescriptor];
        return new MetalTexture2D(width, height, texture);

    }

//---------------------------------------------------------------------------------------------------------------------
    rtc::scoped_refptr<webrtc::I420Buffer> MetalGraphicsDevice::ConvertRGBToI420(ITexture2D* tex){
        id<MTLTexture> nativeTex = (__bridge id<MTLTexture>)tex->GetNativeTexturePtrV();
        if (nil == nativeTex)
            return nullptr;

        const uint32_t BYTES_PER_PIXEL = 4;        
        const uint32_t width  = tex->GetWidth();
        const uint32_t height = tex->GetHeight();
        const uint32_t bytesPerRow = width * BYTES_PER_PIXEL;
        const uint32_t bufferSize = bytesPerRow * height;

        std::vector<uint8_t> buffer;
        buffer.resize(bufferSize);
        
        [nativeTex getBytes:buffer.data()
            bytesPerRow:bytesPerRow
            fromRegion:MTLRegionMake2D(0,0,width,height)
            mipmapLevel:0];

        rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = GraphicsUtility::ConvertRGBToI420Buffer(
            width, height,
            bytesPerRow, buffer.data()
        );
        return i420_buffer;
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
