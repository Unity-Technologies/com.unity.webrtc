#pragma once

#include "IGraphicsDevice.h"

namespace WebRTC {

class ITexture2D;

//Singleton
class GraphicsDevice {

    public:
        static GraphicsDevice& GetInstance();
        bool IsInitialized() const { return m_device != nullptr; }
        bool Init(IUnityInterfaces* unityInterface);
        bool Init(UnityGfxRenderer rendererType, void* device);
        void Shutdown();
        IGraphicsDevice* GetDevice() { return m_device; }

        inline ITexture2D* CreateDefaultTexture(uint32_t w , uint32_t h );

        inline void* GetEncodeDevicePtr();
        inline void CopyResource(ITexture2D* dest, ITexture2D* src);
        inline void CopyResourceFromNative(ITexture2D* dest, void* nativeTexturePtr);

    private:
        GraphicsDevice();
        GraphicsDevice(GraphicsDevice const&) = delete;              
        void operator=(GraphicsDevice const&) = delete;

        UnityGfxRenderer m_rendererType;
        IGraphicsDevice* m_device = nullptr;

};

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* GraphicsDevice::CreateDefaultTexture(uint32_t w, uint32_t h) {
    return m_device->CreateDefaultTextureV(w,h);
}

void* GraphicsDevice::GetEncodeDevicePtr() { return m_device->GetEncodeDevicePtrV(); }
void GraphicsDevice::CopyResource(ITexture2D* dest, ITexture2D* src) { m_device->CopyResourceV(dest, src); }
void GraphicsDevice::CopyResourceFromNative(ITexture2D* dest, void* nativeTexturePtr) {
    m_device->CopyResourceFromNativeV(dest, nativeTexturePtr);
};



}
