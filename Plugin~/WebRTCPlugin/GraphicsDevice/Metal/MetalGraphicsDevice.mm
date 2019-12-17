#include "pch.h"
#include "MetalGraphicsDevice.h"
#include "MetalTexture2D.h"
#import <Metal/Metal.h>

namespace WebRTC {

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

//---------------------------------------------------------------------------------------------------------------------

    bool MetalGraphicsDevice::CopyTexture(id<MTLTexture> dest, id<MTLTexture> src)
    {
        if(dest == src)
            return false;
        
        m_unityGraphicsMetal->EndCurrentCommandEncoder();

        id<MTLCommandBuffer> commandBuffer = m_unityGraphicsMetal->CurrentCommandBuffer();
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
        commandEncoder = nil;
        m_unityGraphicsMetal->EndCurrentCommandEncoder();
        //[commandBuffer commit];
        //[commandBuffer waitUntilCompleted];

        return true;
    }

//---------------------------------------------------------------------------------------------------------------------
    ITexture2D* MetalGraphicsDevice::CreateCPUReadTextureV(uint32_t width, uint32_t height)
    {
        MTLTextureDescriptor *textureDescriptor = [[MTLTextureDescriptor alloc] init];
        textureDescriptor.pixelFormat = MTLPixelFormatBGRA8Unorm_sRGB;
        textureDescriptor.width = width;
        textureDescriptor.height = height;
        textureDescriptor.allowGPUOptimizedContents = false;
        textureDescriptor.storageMode = MTLStorageMode(MTLStorageModeManaged ) ;
        
        id<MTLTexture> texture = [m_device newTextureWithDescriptor:textureDescriptor];
        return new MetalTexture2D(width, height, texture);

    }

//---------------------------------------------------------------------------------------------------------------------
    rtc::scoped_refptr<webrtc::I420Buffer> MetalGraphicsDevice::ConvertRGBToI420(ITexture2D* tex){
        id<MTLTexture> nativeTex = (__bridge id<MTLTexture>)tex->GetNativeTexturePtrV();
        const uint32_t BYTES_PER_HEIGHT = 4;
        const uint32_t width  = tex->GetWidth();
        const uint32_t height = tex->GetHeight();
        const uint32_t bufferSize = width * tex->GetHeight() * BYTES_PER_HEIGHT;
        

        std::vector<uint8_t> buffer;
        buffer.resize(bufferSize);
        if (nil == nativeTex)
            return nullptr;
        
        [nativeTex getBytes:buffer.data() bytesPerRow:width*4 fromRegion:MTLRegionMake2D(0,0,width,height) mipmapLevel:0];

        rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Create(width, height);

        int yIndex = 0;
        int uIndex = 0;
        int vIndex = 0;

        uint8_t* yuv_y = i420_buffer->MutableDataY();
        uint8_t* yuv_u = i420_buffer->MutableDataU();
        uint8_t* yuv_v = i420_buffer->MutableDataV();

        for (uint32_t i = 0; i < height; i++) {
            for (uint32_t j = 0; j < width; j++) {
                int R, G, B, Y, U, V;
                int startIndex = i * width + j * BYTES_PER_HEIGHT;
                B = buffer[startIndex + 0];
                G = buffer[startIndex + 1];
                R = buffer[startIndex + 2];
                
                //[TODO-sin: 2019-12-16] Turn this into a common function
                Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
                U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
                V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;

                yuv_y[yIndex++] = (uint8_t)((Y < 0) ? 0 : ((Y > 255) ? 255 : Y));
                if (i % 2 == 0 && j % 2 == 0)
                {
                    yuv_u[uIndex++] = (uint8_t)((U < 0) ? 0 : ((U > 255) ? 255 : U));
                    yuv_v[vIndex++] = (uint8_t)((V < 0) ? 0 : ((V > 255) ? 255 : V));
                }
            }
        }

        return i420_buffer;

    }

}
