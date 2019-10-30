#pragma once

#include "IGraphicsDevice.h"

namespace WebRTC {

struct ITexture2D;

//Singleton
class GraphicsDevice {

    public:
        static GraphicsDevice& GetInstance();
        void Init(IUnityInterfaces* unityInterface);
        void Shutdown();
        inline ITexture2D* CreateEncoderInputTexture(uint32_t w , uint32_t h );
        inline ITexture2D* CreateDefaultTextureFromNative(uint32_t width, uint32_t height, void* nativeTexturePtr);

        inline void* GetEncodeDevicePtr();
        inline void CopyNativeResource(void* dest, void* src);

    private:
        GraphicsDevice();
        GraphicsDevice(GraphicsDevice const&);              
        void operator=(GraphicsDevice const&);

        UnityGfxRenderer m_rendererType;
        IGraphicsDevice* m_device = nullptr;

};

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* GraphicsDevice::CreateEncoderInputTexture(uint32_t w, uint32_t h) {
    return m_device->CreateDefaultTextureV(w,h);
}

void* GraphicsDevice::GetEncodeDevicePtr() { return m_device->GetEncodeDevicePtrV(); }
void GraphicsDevice::CopyNativeResource(void* dest, void* src) { m_device->CopyNativeResourceV(dest, src); }

ITexture2D* GraphicsDevice::CreateDefaultTextureFromNative(uint32_t width, uint32_t height, void* nativeTexturePtr) {
    return m_device->CreateDefaultTextureFromNativeV(width, height, nativeTexturePtr);
}



}
