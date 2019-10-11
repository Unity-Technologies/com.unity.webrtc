#pragma once

namespace WebRTC {

//Singleton
class GraphicsDevice {

    public:
        static GraphicsDevice& GetInstance();
        void Init(IUnityInterfaces* unityInterface);
        void Shutdown();
        //void* CreateEncoderInputTexture(uint32_t w , uint32_t h );
        //void ReleaseEncoderInputTexture(void *texture);

    private:
        GraphicsDevice();
        GraphicsDevice(GraphicsDevice const&);              
        void operator=(GraphicsDevice const&);

        UnityGfxRenderer m_rendererType;

};

}
