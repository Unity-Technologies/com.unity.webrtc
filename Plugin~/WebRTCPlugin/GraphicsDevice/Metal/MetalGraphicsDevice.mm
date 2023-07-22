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
    MetalGraphicsDevice::CreateDefaultTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat)
    {
        id<MTLDevice> device = m_device->Device();

        NSDictionary* dict = [NSDictionary dictionaryWithObjectsAndKeys:
                               [NSNumber numberWithInt:width], kIOSurfaceWidth,
                               [NSNumber numberWithInt:height], kIOSurfaceHeight,
                               [NSNumber numberWithInt:4], kIOSurfaceBytesPerElement,
                               [NSNumber numberWithInt:kCVPixelFormatType_32BGRA], kIOSurfacePixelFormat,
                               // [NSNumber numberWithBool:YES], kIOSurfaceIsGlobal,
                               nil];
        
        IOSurfaceRef surface = IOSurfaceCreate((CFDictionaryRef)dict);
                
        MTLTextureDescriptor* textureDescriptor = [[MTLTextureDescriptor alloc] init];
        textureDescriptor.pixelFormat = ConvertFormat(textureFormat);
        textureDescriptor.width = width;
        textureDescriptor.height = height;
        id<MTLTexture> texture = [device newTextureWithDescriptor:textureDescriptor iosurface:surface plane:0];
        return new MetalTexture2D(width, height, texture);
    }

    ITexture2D* MetalGraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr)
    {
        id<MTLTexture> texture = (__bridge id<MTLTexture>)nativeTexturePtr;
        return new MetalTexture2D(w, h, texture);
    }

    bool MetalGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src)
    {
        id<MTLTexture> srcTexture = (__bridge id<MTLTexture>)src->GetNativeTexturePtrV();
        MetalTexture2D* texture2D = static_cast<MetalTexture2D*>(dest);
        return CopyTexture(texture2D, srcTexture);
    }

    static CVPixelBufferRef GetPixelBuffer(rtc::scoped_refptr<VideoFrameBuffer> buffer)
    {
        webrtc::ObjCFrameBuffer* objcBuffer = static_cast<webrtc::ObjCFrameBuffer*>(buffer.get());
        RTC_DCHECK(objcBuffer);
        if ([objcBuffer->wrapped_frame_buffer() isKindOfClass:[RTC_OBJC_TYPE(RTCCVPixelBuffer) class]])
        {
            RTC_OBJC_TYPE(RTCCVPixelBuffer) *bufferWrapper = (RTC_OBJC_TYPE(RTCCVPixelBuffer) *)objcBuffer->wrapped_frame_buffer();
            return bufferWrapper.pixelBuffer;
        }
        return nullptr;
    }

    static MTLPixelFormat ConvertToMTLPixelFormat(OSType pixel_format, size_t plane)
    {
        switch (pixel_format)
        {
        case kCVPixelFormatType_32BGRA:
            NSCAssert(plane == 0, @"Invalid plane number");
            return MTLPixelFormatBGRA8Unorm;
        case kCVPixelFormatType_64RGBAHalf:
            NSCAssert(plane == 0, @"Invalid plane number");
            return MTLPixelFormatRGBA16Float;
        case kCVPixelFormatType_OneComponent8:
            NSCAssert(plane == 0, @"Invalid plane number");
            return MTLPixelFormatR8Uint;
        case kCVPixelFormatType_420YpCbCr8BiPlanarVideoRange:
        case kCVPixelFormatType_420YpCbCr8BiPlanarFullRange:
            if (plane == 0)
                return MTLPixelFormatR8Unorm;
            else if (plane == 1)
                return MTLPixelFormatRG8Unorm;
            else
                NSCAssert(NO, @"Invalid plane number");
            break;
        case kCVPixelFormatType_TwoComponent16Half:
            NSCAssert(plane == 0, @"Invalid plane number");
            return MTLPixelFormatRG16Float;
        case kCVPixelFormatType_OneComponent32Float:
            NSCAssert(plane == 0, @"Invalid plane number");
            return MTLPixelFormatR32Float;
        default:
            NSCAssert(NO, @"Invalid pixel buffer format");
            break;
        }
        return MTLPixelFormatInvalid;
    }

    static OSType ConvertToCVPixelFormatType(MTLPixelFormat pixel_format)
    {
        switch (pixel_format)
        {
        case MTLPixelFormatBGRA8Unorm:
        case MTLPixelFormatBGRA8Unorm_sRGB:
            return kCVPixelFormatType_32BGRA;
        case MTLPixelFormatRGBA16Float:
            return kCVPixelFormatType_64RGBAHalf;
        case MTLPixelFormatR8Uint:
            return kCVPixelFormatType_OneComponent8;
        case MTLPixelFormatRG16Float:
            return kCVPixelFormatType_TwoComponent16Half;
        case MTLPixelFormatR32Float:
            return kCVPixelFormatType_OneComponent32Float;
        default:
            break;
        }
        throw;
    }

    bool MetalGraphicsDevice::CopyResourceFromBuffer(ITexture2D* dest, rtc::scoped_refptr<VideoFrameBuffer> buffer)
    {
        CVPixelBufferRef pixelBuffer = GetPixelBuffer(buffer);
        id<MTLDevice> metalDevice = (__bridge id<MTLDevice>)m_device->Device();
        const OSType pixelFormat = CVPixelBufferGetPixelFormatType(pixelBuffer);
        MTLPixelFormat metalPixelFormat = ConvertToMTLPixelFormat(pixelFormat, 0);
        CVMetalTextureCacheRef metalTextureCache = nullptr;
        CVReturn result = CVMetalTextureCacheCreate(kCFAllocatorDefault, nullptr, metalDevice, nullptr, &metalTextureCache);
        if(result != kCVReturnSuccess)
        {
            RTC_LOG(LS_INFO) << "CVMetalTextureCacheCreate failed. result=" << result;
            return false;
        }
        CVMetalTextureRef cvTexture;
        result = CVMetalTextureCacheCreateTextureFromImage(kCFAllocatorDefault, metalTextureCache, pixelBuffer, nullptr, metalPixelFormat, CVPixelBufferGetWidth(pixelBuffer), CVPixelBufferGetHeight(pixelBuffer), 0, &cvTexture);
        if(result != kCVReturnSuccess)
        {
            RTC_LOG(LS_INFO) << "CVMetalTextureCacheCreateTextureFromImage failed. result=" << result;
            return false;
        }
        id<MTLTexture> dstTexture = (__bridge id<MTLTexture>)dest->GetNativeTexturePtrV();
        id<MTLTexture> srcTexture = CVMetalTextureGetTexture(cvTexture);
        CFRelease(cvTexture);
        return CopyTexture(dstTexture, srcTexture);
    }
    
    bool MetalGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr)
    {
        if (!nativeTexturePtr)
        {
            RTC_LOG(LS_INFO) << "nativeTexturePtr is nullptr.";
            return false;
        }
        id<MTLTexture> srcTexture = (__bridge id<MTLTexture>)nativeTexturePtr;
        MetalTexture2D* texture2D = static_cast<MetalTexture2D*>(dest);
        return CopyTexture(texture2D, srcTexture);
    }

    bool MetalGraphicsDevice::CopyTexture(MetalTexture2D* dest, id<MTLTexture> src)
    {
        id<MTLTexture> mtlTexture = (__bridge id<MTLTexture>)dest->GetNativeTexturePtrV();
        __block dispatch_semaphore_t semaphore = dest->GetSemaphore();
        dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);

        RTC_DCHECK_NE(src, mtlTexture);
        RTC_DCHECK_EQ(src.pixelFormat, mtlTexture.pixelFormat);
        RTC_DCHECK_EQ(src.width, mtlTexture.width);
        RTC_DCHECK_EQ(src.height, mtlTexture.height);

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
                    toTexture:mtlTexture
             destinationSlice:0
             destinationLevel:0
            destinationOrigin:outTxtOrigin];

#if TARGET_OS_OSX
        // must be explicitly synchronized if the storageMode is Managed.
        if (mtlTexture.storageMode == MTLStorageModeManaged)
            [blit synchronizeResource:mtlTexture];
#endif
        [blit endEncoding];
        [commandBuffer addCompletedHandler:^(id<MTLCommandBuffer> buffer) { dispatch_semaphore_signal(semaphore); }];

        // Commit the current command buffer and wait until the GPU process is completed.
        [commandBuffer commit];
        return true;
    }

    bool MetalGraphicsDevice::WaitSync(const ITexture2D* texture, uint64_t nsTimeout)
    {
        const MetalTexture2D* texture2D = static_cast<const MetalTexture2D*>(texture);
        dispatch_semaphore_t semaphore = texture2D->GetSemaphore();

        intptr_t value = dispatch_semaphore_wait(semaphore, dispatch_time(DISPATCH_TIME_NOW, nsTimeout));
        if (value != 0)
        {
            RTC_LOG(LS_INFO) << "The timeout occurred.";
            return false;
        }
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
        id<MTLTexture> dstTexture = CVMetalTextureGetTexture(cvTexture);

        bool ret = CopyTexture(dstTexture, srcTexture);

        CFRelease(cvTexture);
        CVMetalTextureCacheFlush(metalTextureCache, 0);

        return ret;
    }

    bool MetalGraphicsDevice::CopyResourceFromBuffer(void* dest, rtc::scoped_refptr<VideoFrameBuffer> buffer)
    {
        CVPixelBufferRef pixelBuffer = GetPixelBuffer(buffer);
        id<MTLDevice> metalDevice = (__bridge id<MTLDevice>)m_device->Device();
        MTLPixelFormat metalPixelFormat = MTLPixelFormatBGRA8Unorm;
        CVMetalTextureCacheRef metalTextureCache = nullptr;
        CVReturn result = CVMetalTextureCacheCreate(kCFAllocatorDefault, nullptr, metalDevice, nullptr, &metalTextureCache);
        if(result != kCVReturnSuccess)
        {
            RTC_LOG(LS_INFO) << "CVMetalTextureCacheCreate failed. result=" << result;
            return false;
        }
        CVMetalTextureRef cvTexture;
        result = CVMetalTextureCacheCreateTextureFromImage(kCFAllocatorDefault, metalTextureCache, pixelBuffer, nullptr, metalPixelFormat, CVPixelBufferGetWidth(pixelBuffer), CVPixelBufferGetHeight(pixelBuffer), 0, &cvTexture);
        if(result != kCVReturnSuccess)
        {
            RTC_LOG(LS_INFO) << "CVMetalTextureCacheCreateTextureFromImage failed. result=" << result;
            return false;
        }
        id<MTLTexture> dstTexture = (__bridge id<MTLTexture>)dest;
        id<MTLTexture> srcTexture = CVMetalTextureGetTexture(cvTexture);
        CFRelease(cvTexture);
        return CopyTexture(dstTexture, srcTexture);
    }

    bool MetalGraphicsDevice::ResetSync(const ITexture2D* texture) { return true; }
    
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

    rtc::scoped_refptr<webrtc::I420Buffer> MetalGraphicsDevice::ConvertRGBToI420(ITexture2D* texture)
    {
        MetalTexture2D* texture2D = static_cast<MetalTexture2D*>(texture);
        rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = texture2D->ConvertI420Buffer();

        // Notify finishing usage of semaphore.
        dispatch_semaphore_t semaphore = texture2D->GetSemaphore();
        dispatch_semaphore_signal(semaphore);

        return i420_buffer;
    }

    rtc::scoped_refptr<webrtc::VideoFrameBuffer> MetalGraphicsDevice::ConvertToBuffer(void* ptr)
    {
        id<MTLTexture> source = (__bridge id<MTLTexture>)ptr;

        RTC_DCHECK(source);
        RTC_DCHECK(source.iosurface);
        RTC_DCHECK_GT(source.width, 0);
        RTC_DCHECK_GT(source.height, 0);
        
        CVPixelBufferRef buffer = nullptr;
        OSType pixelFormat = ConvertToCVPixelFormatType(source.pixelFormat);
        NSDictionary *pixelBufferAttributes = @{(NSString *)kCVPixelBufferPixelFormatTypeKey : @(pixelFormat)};
        const CVReturn result = CVPixelBufferCreateWithIOSurface(nullptr, source.iosurface, (__bridge CFDictionaryRef _Nullable)(pixelBufferAttributes), &buffer);
        if(result != kCVReturnSuccess)
        {
            RTC_LOG(LS_INFO) << "CVPixelBufferCreateWithIOSurface failed. result=" << result;
            return nullptr;
        }
        return rtc::make_ref_counted<ObjCFrameBuffer>([[RTC_OBJC_TYPE(RTCCVPixelBuffer) alloc]initWithPixelBuffer:buffer]);
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
