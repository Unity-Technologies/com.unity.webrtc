#include "pch.h"
#include "MetalGraphicsDevice.h"
#include "MetalTexture2D.h"
#import <Metal/Metal.h>

namespace WebRTC {

    MetalGraphicsDevice::MetalGraphicsDevice(void* device)
        : m_device((__bridge id<MTLDevice>)device)
    {
        m_commandQueue = [m_device newCommandQueue];
    }

//---------------------------------------------------------------------------------------------------------------------
    MetalGraphicsDevice::~MetalGraphicsDevice() {
        [m_commandQueue release];
    }

//---------------------------------------------------------------------------------------------------------------------
    bool MetalGraphicsDevice::InitV() {
        return true;
    }
//---------------------------------------------------------------------------------------------------------------------

    void MetalGraphicsDevice::ShutdownV() {
    }

//---------------------------------------------------------------------------------------------------------------------
    ITexture2D* MetalGraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h) {
        MTLTextureDescriptor *textureDescriptor = [[MTLTextureDescriptor alloc] init];
        textureDescriptor.pixelFormat = MTLPixelFormatBGRA8Unorm_sRGB;
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

    bool MetalGraphicsDevice::CopyTexture(id<MTLTexture> dest, id<MTLTexture> src)
    {
        if(dest == src)
            return false;

        id<MTLCommandBuffer> commandBuffer = [m_commandQueue commandBuffer];
        id<MTLBlitCommandEncoder> commandEncoder = [commandBuffer blitCommandEncoder];

        NSUInteger width = src.width;
        NSUInteger height = src.height;

        MTLSize inTxtSize = MTLSizeMake(width, height, 1);
        MTLOrigin inTxtOrigin = MTLOriginMake(0, 0, 0);
        MTLOrigin outTxtOrigin = MTLOriginMake(0, 0, 0);
    
        [commandEncoder copyFromTexture:src
                        sourceSlice:0
                        sourceLevel:0
                        sourceOrigin:inTxtOrigin
                        sourceSize:inTxtSize
                        toTexture:dest
                        destinationSlice:0
                        destinationLevel:0
                        destinationOrigin:outTxtOrigin];
        [commandEncoder endEncoding];
        [commandBuffer commit];
        [commandBuffer waitUntilCompleted];

        return true;
    }
}
