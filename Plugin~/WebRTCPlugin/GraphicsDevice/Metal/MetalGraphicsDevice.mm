#include "pch.h"

#include <third_party/libyuv/include/libyuv/convert.h>

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

    ITexture2D*
    MetalGraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        id<MTLDevice> device = m_device->Device();
        MTLTextureDescriptor* textureDescriptor = [[MTLTextureDescriptor alloc] init];
        textureDescriptor.pixelFormat = ConvertFormat(textureFormat);
        textureDescriptor.width = w;
        textureDescriptor.height = h;
        id<MTLTexture> texture = [device newTextureWithDescriptor:textureDescriptor];
        return new MetalTexture2D(w, h, texture);
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

    bool MetalGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr)
    {
        if (nativeTexturePtr == nullptr)
        {
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
        __block MetalTexture2D* destTexture = dest;
        DebugLog("CopyTexture dispatch_semaphore_wait:%x", destTexture);
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
        [commandBuffer addCompletedHandler:^(id<MTLCommandBuffer> buffer)
        {
            DebugLog("CopyTexture dispatch_semaphore_signal:%x", destTexture);
            dispatch_semaphore_signal(semaphore);
        }];

        // Commit the current command buffer and wait until the GPU process is completed.
        [commandBuffer commit];
        return true;
    }

    bool MetalGraphicsDevice::WaitSync(const ITexture2D* texture, uint64_t nsTimeout)
    {
        const MetalTexture2D* texture2D = static_cast<const MetalTexture2D*>(texture);
        dispatch_semaphore_t semaphore = texture2D->GetSemaphore();

        DebugLog("dispatch_semaphore_wait:%x", texture);
        intptr_t value = dispatch_semaphore_wait(semaphore, dispatch_time(DISPATCH_TIME_NOW, nsTimeout));
        DebugLog("dispatch_semaphore_wait end:%x", texture);
        if(value != 0)
        {
            DebugLog("dispatch_semaphore_wait end. The timeout occurred.:%x", texture);
            RTC_LOG(LS_INFO) << "The timeout occurred.";
            return false;
        }
        return true;
    }

    bool MetalGraphicsDevice::ResetSync(const ITexture2D* texture)
    {
        const MetalTexture2D* texture2D = static_cast<const MetalTexture2D*>(texture);
        dispatch_semaphore_t semaphore = texture2D->GetSemaphore();
        dispatch_semaphore_signal(semaphore);
        return true;
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
        return new MetalTexture2D(width, height, texture);
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

        rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer =
            webrtc::I420Buffer::Create(static_cast<int32_t>(width), static_cast<int32_t>(height));
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
