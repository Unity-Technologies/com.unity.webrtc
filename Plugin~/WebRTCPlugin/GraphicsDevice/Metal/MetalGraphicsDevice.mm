#include "pch.h"

#include <third_party/libyuv/include/libyuv/convert.h>
#import <sdk/objc/native/src/objc_frame_buffer.h>
#import <components/video_frame_buffer/RTCCVPixelBuffer.h>

#include "GraphicsDevice/GraphicsUtility.h"
#include "MetalDevice.h"
#include "MetalGraphicsDevice.h"
#include "MetalTexture2D.h"

namespace unity
{
namespace webrtc
{

    MetalGraphicsDevice::MetalGraphicsDevice(
        MetalDevice* device, UnityGfxRenderer renderer, ProfilerMarkerFactory* profiler)
        : IGraphicsDevice(renderer, profiler)
        , m_device(device)
    {
    }

    bool MetalGraphicsDevice::InitV()
    {
        m_queue = [m_device->Device() newCommandQueue];
        return true;
    }

    void MetalGraphicsDevice::ShutdownV() { }

    rtc::scoped_refptr<VideoFrameBuffer>
    MetalGraphicsDevice::CreateVideoFrameBuffer(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat format)
    {
        CVImageBufferRef buffer = nullptr;
        NSDictionary* options = [NSDictionary dictionaryWithObjectsAndKeys:[NSNumber numberWithBool:YES],
                                                                           kCVPixelBufferMetalCompatibilityKey,
                                                                           @ {},
                                                                           kCVPixelBufferIOSurfacePropertiesKey,
                                                                           nil];
        CVReturn result = CVPixelBufferCreate(
            kCFAllocatorDefault,
            width,
            height,
            kCVPixelFormatType_32BGRA,
            (CFDictionaryRef)options,
            &buffer);
        if (result != kCVReturnSuccess)
        {
            RTC_LOG(LS_INFO) << "CVPixelBufferCreateWithIOSurface failed. result=" << result;
            return nullptr;
        }
        return rtc::make_ref_counted<ObjCFrameBuffer>(
            [[RTC_OBJC_TYPE(RTCCVPixelBuffer) alloc] initWithPixelBuffer:buffer]);
    }


    ITexture2D*
    MetalGraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        id<MTLDevice> device = m_device->Device();
        MTLTextureDescriptor* textureDescriptor = [[MTLTextureDescriptor alloc] init];
        textureDescriptor.pixelFormat = ConvertFormat(textureFormat);
        textureDescriptor.width = w;
        textureDescriptor.height = h;
        id<MTLTexture> texture = [device newTextureWithDescriptor:textureDescriptor];
        return new MetalTexture2D(w, h, textureFormat, texture);
    }

    bool MetalGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src)
    {
        id<MTLTexture> dstTexture = (__bridge id<MTLTexture>)dest->GetNativeTexturePtrV();
        id<MTLTexture> srcTexture = (__bridge id<MTLTexture>)src->GetNativeTexturePtrV();
        return CopyTexture(dstTexture, srcTexture);
    }

    bool MetalGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr)
    {
        if (nativeTexturePtr == nullptr)
        {
            return false;
        }
        id<MTLTexture> dstTexture = (__bridge id<MTLTexture>)dest->GetNativeTexturePtrV();
        id<MTLTexture> srcTexture = (__bridge id<MTLTexture>)nativeTexturePtr;
        return CopyTexture(dstTexture, srcTexture);
    }

    bool MetalGraphicsDevice::CopyTexture(id<MTLTexture> dest, id<MTLTexture> src)
    {
        RTC_DCHECK_NE(dest, src);
        RTC_DCHECK_EQ(src.pixelFormat, dest.pixelFormat);
        RTC_DCHECK_EQ(src.width, dest.width);
        RTC_DCHECK_EQ(src.height, dest.height);

        m_device->EndCurrentCommandEncoder();

        id<MTLCommandBuffer> commandBuffer = [m_queue commandBuffer];
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

#if TARGET_OS_OSX
        // must be explicitly synchronized if the storageMode is Managed.
        if (dest.storageMode == MTLStorageModeManaged)
            [blit synchronizeResource:dest];
#endif
        [blit endEncoding];

        // Commit the current command buffer and wait until the GPU process is completed.
        [commandBuffer commit];
        [commandBuffer waitUntilCompleted];

        return true;
    }

    static CVPixelBufferRef GetPixelBuffer(rtc::scoped_refptr<VideoFrameBuffer>& buffer)
    {
        webrtc::ObjCFrameBuffer* objcBuffer = static_cast<webrtc::ObjCFrameBuffer*>(buffer.get());
        RTC_DCHECK(objcBuffer);
        if ([objcBuffer->wrapped_frame_buffer() isKindOfClass:[RTC_OBJC_TYPE(RTCCVPixelBuffer) class]])
        {
            RTC_OBJC_TYPE(RTCCVPixelBuffer)* bufferWrapper =
                (RTC_OBJC_TYPE(RTCCVPixelBuffer)*)objcBuffer->wrapped_frame_buffer();
            return bufferWrapper.pixelBuffer;
        }
        return nullptr;
    }

//    static CGColorSpaceRef ColorSpace(IOSurfaceRef surface)
//    {
//        CFTypeRef type = IOSurfaceCopyValue(surface, kIOSurfaceColorSpace);
//        RTC_DCHECK(type);
//        CFStringRef str = (RTC_OBJC_TYPE(CFStringRef))type;
//        return CGColorSpaceCreateWithName(str);
//    }


    bool MetalGraphicsDevice::CopyToVideoFrameBuffer(rtc::scoped_refptr<::webrtc::VideoFrameBuffer>& buffer, void* texture)
    {
        id<MTLDevice> metalDevice = (__bridge id<MTLDevice>)m_device->Device();
        
        CVPixelBufferRef pixelBuffer = GetPixelBuffer(buffer);
        const MTLPixelFormat metalPixelFormat = MTLPixelFormatBGRA8Unorm;
        CVMetalTextureCacheRef metalTextureCache = nullptr;
        CVReturn result =
            CVMetalTextureCacheCreate(kCFAllocatorDefault, nullptr, metalDevice, nullptr, &metalTextureCache);
        if (result != kCVReturnSuccess)
        {
            RTC_LOG(LS_INFO) << "CVMetalTextureCacheCreate failed. result=" << result;
            return false;
        }

        CVMetalTextureRef cvTexture;
        result = CVMetalTextureCacheCreateTextureFromImage(
            kCFAllocatorDefault,
            metalTextureCache,
            pixelBuffer,
            nullptr,
            metalPixelFormat,
            CVPixelBufferGetWidth(pixelBuffer),
            CVPixelBufferGetHeight(pixelBuffer),
            0,
            &cvTexture);

        if (result != kCVReturnSuccess)
        {
            RTC_LOG(LS_INFO) << "CVMetalTextureCacheCreateTextureFromImage failed. result=" << result;
            return false;
        }

        id<MTLTexture> srcTexture = (__bridge id<MTLTexture>)texture;
//        RTC_DCHECK(srcTexture.iosurface);
//        RTC_DCHECK(srcTexture.iosurfacePlane);
//        RTC_LOG(LS_INFO) << ColorSpace(srcTexture.iosurface);
        id<MTLTexture> dstTexture = CVMetalTextureGetTexture(cvTexture);

        bool ret = CopyTexture(dstTexture, srcTexture);

        CFRelease(cvTexture);
        CVMetalTextureCacheFlush(metalTextureCache, 0);

        return ret;
    }

    ITexture2D* MetalGraphicsDevice::CreateCPUReadTextureV(
        uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat)
    {
        id<MTLDevice> device = m_device->Device();
        MTLTextureDescriptor* textureDescriptor = [[MTLTextureDescriptor alloc] init];
        textureDescriptor.pixelFormat = ConvertFormat(textureFormat);
        textureDescriptor.width = width;
        textureDescriptor.height = height;
        if (@available(macOS 10.14, iOS 12.0, *))
        {
            // This texture is a managed or shared resource.
            textureDescriptor.allowGPUOptimizedContents = false;
        }
        else
        {
            // Fallback on earlier versions
        }
#if TARGET_OS_OSX
        textureDescriptor.storageMode = MTLStorageMode(MTLStorageModeManaged);
#else
        textureDescriptor.storageMode = MTLStorageMode(MTLStorageModeShared);
#endif
        id<MTLTexture> texture = [device newTextureWithDescriptor:textureDescriptor];
        return new MetalTexture2D(width, height, textureFormat, texture);
    }

    rtc::scoped_refptr<webrtc::I420Buffer> MetalGraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
    {
        id<MTLTexture> source = (__bridge id<MTLTexture>)tex->GetNativeTexturePtrV();
        const uint32_t width = tex->GetWidth();
        const uint32_t height = tex->GetHeight();

        RTC_DCHECK(source);
        RTC_DCHECK_GT(width, 0);
        RTC_DCHECK_GT(height, 0);

        const uint32_t BYTES_PER_PIXEL = 4;
        const uint32_t bytesPerRow = width * BYTES_PER_PIXEL;
        const uint32_t bufferSize = bytesPerRow * height;

        std::vector<uint8_t> buffer;
        buffer.resize(bufferSize);

        [source getBytes:buffer.data()
             bytesPerRow:bytesPerRow
              fromRegion:MTLRegionMake2D(0, 0, width, height)
             mipmapLevel:0];

        rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Create(
            static_cast<int32_t>(width), static_cast<int32_t>(height));
        libyuv::ARGBToI420(
            buffer.data(),
            static_cast<int32_t>(bytesPerRow),
            i420_buffer->MutableDataY(),
            i420_buffer->StrideY(),
            i420_buffer->MutableDataU(),
            i420_buffer->StrideU(),
            i420_buffer->MutableDataV(),
            i420_buffer->StrideV(),
            static_cast<int32_t>(width),
            static_cast<int32_t>(height));
        return i420_buffer;
    }

    MTLPixelFormat MetalGraphicsDevice::ConvertFormat(UnityRenderingExtTextureFormat format)
    {
        switch (format)
        {
        case kUnityRenderingExtFormatB8G8R8A8_SRGB:
            return MTLPixelFormatBGRA8Unorm_sRGB;
        case kUnityRenderingExtFormatB8G8R8A8_UNorm:
            return MTLPixelFormatBGRA8Unorm;
        case kUnityRenderingExtFormatR8G8B8A8_SRGB:
            return MTLPixelFormatRGBA8Unorm_sRGB;
        default:
            return MTLPixelFormatInvalid;
        }
    }

} // end namespace webrtc
} // end namespace unity
