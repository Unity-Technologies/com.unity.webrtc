#pragma once

namespace WebRTC {

struct ITexture2D;

//Singleton
class GraphicsDevice {

    public:
        static GraphicsDevice& GetInstance();
        void Init(IUnityInterfaces* unityInterface);
        void Shutdown();
        ITexture2D* CreateEncoderInputTexture(uint32_t w , uint32_t h );

    private:
        GraphicsDevice();
        GraphicsDevice(GraphicsDevice const&);              
        void operator=(GraphicsDevice const&);

        UnityGfxRenderer m_rendererType;

};

}
