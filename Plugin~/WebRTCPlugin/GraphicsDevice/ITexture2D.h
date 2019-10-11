#pragma once

namespace WebRTC {

struct ITexture2D {
    //[TODO-Sin: 2019-19-11] This should be in the child class
    UnityFrameBuffer* texture;

    //[TODO-Sin: 2019-19-11] ITexture2D should not be created directly, but should be called using
    //GraphicsDevice->CreateEncoderInputTexture

    int width;
    int height;
    ITexture2D(int w, int h)
    {
        width = w;
        height = h;
        D3D11_TEXTURE2D_DESC desc = { 0 };
        desc.Width = width;
        desc.Height = height;
        desc.MipLevels = 1;
        desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_DEFAULT;
        desc.BindFlags = D3D11_BIND_RENDER_TARGET;
        desc.CPUAccessFlags = 0;
        HRESULT r = g_D3D11Device->CreateTexture2D(&desc, NULL, &texture);
    }

    ~ITexture2D()
    {
        texture->Release();
        texture = nullptr;
    }
};

}
