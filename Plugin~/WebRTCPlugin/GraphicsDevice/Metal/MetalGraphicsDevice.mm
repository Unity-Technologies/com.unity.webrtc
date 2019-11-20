#include "pch.h"
#include "MetalGraphicsDevice.h"
#include "MetalTexture2D.h"

namespace WebRTC {

    MetalGraphicsDevice::MetalGraphicsDevice() {
    }

//---------------------------------------------------------------------------------------------------------------------
    MetalGraphicsDevice::~MetalGraphicsDevice() {

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
        return nullptr;
    }

//---------------------------------------------------------------------------------------------------------------------
    ITexture2D* MetalGraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
        return nullptr;
    }


//---------------------------------------------------------------------------------------------------------------------
    bool MetalGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
        return true;
    }

//---------------------------------------------------------------------------------------------------------------------
    bool MetalGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    }
}