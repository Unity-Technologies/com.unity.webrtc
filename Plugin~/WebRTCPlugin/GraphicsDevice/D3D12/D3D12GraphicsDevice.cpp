#include "pch.h"
#include "D3D12GraphicsDevice.h"
#include "D3D12Texture2D.h"
#include "WebRTCPlugin.h"
#include "Logger.h"

namespace WebRTC {

const D3D12_HEAP_PROPERTIES DEFAULT_HEAP_PROPS = {
    D3D12_HEAP_TYPE_DEFAULT,
    D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
    D3D12_MEMORY_POOL_UNKNOWN,
    0,
    0
};

//---------------------------------------------------------------------------------------------------------------------

D3D12GraphicsDevice::D3D12GraphicsDevice(ID3D12Device* nativeDevice, IUnityGraphicsD3D12v5* unityInterface)
    : m_d3d12Device(nativeDevice)
    , m_d3d11Device(nullptr), m_d3d11Context(nullptr)
    , m_unityInterface(unityInterface)
    , m_copyResourceFence(nullptr)
    , m_copyResourceEventHandle(nullptr)
{
}


//---------------------------------------------------------------------------------------------------------------------
D3D12GraphicsDevice::~D3D12GraphicsDevice() {

}

//---------------------------------------------------------------------------------------------------------------------

bool D3D12GraphicsDevice::InitV() {

    ID3D11Device* legacyDevice;
    ID3D11DeviceContext* legacyContext;

    D3D11CreateDevice(
        nullptr,
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        0,
        nullptr,
        0,
        D3D11_SDK_VERSION,
        &legacyDevice,
        nullptr,
        &legacyContext);

    legacyDevice->QueryInterface(IID_PPV_ARGS(&m_d3d11Device));

    legacyDevice->GetImmediateContext(&legacyContext);
    legacyContext->QueryInterface(IID_PPV_ARGS(&m_d3d11Context));

    m_d3d12Device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocator));
    m_d3d12Device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, m_commandAllocator, nullptr, IID_PPV_ARGS(&m_commandList));
    m_d3d12Device->CreateFence( 0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_copyResourceFence));

	m_copyResourceEventHandle = CreateEvent(NULL, FALSE, FALSE, NULL);

    return true;
}

//---------------------------------------------------------------------------------------------------------------------
void D3D12GraphicsDevice::ShutdownV() {
    m_unityInterface = nullptr;
    m_commandList->Release();
    m_commandAllocator->Release();
    SAFE_RELEASE(m_d3d11Device);
    SAFE_RELEASE(m_d3d11Context);
    SAFE_RELEASE(m_copyResourceFence);
    SAFE_CLOSE_HANDLE(m_copyResourceEventHandle);

}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D12GraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h) {

    return CreateSharedD3D12Texture(w,h);
}

//---------------------------------------------------------------------------------------------------------------------

    bool D3D12GraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    //[TODO-sin: 2019-10-15] Implement copying native resource
    return true;
}

//---------------------------------------------------------------------------------------------------------------------
bool D3D12GraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    ID3D12Resource* nativeDest = reinterpret_cast<ID3D12Resource*>(dest->GetNativeTexturePtrV());
    ID3D12Resource* nativeSrc = reinterpret_cast<ID3D12Resource*>(nativeTexturePtr);
    if (nativeSrc == nativeDest)
        return false;
    if (nativeSrc == nullptr || nativeDest == nullptr)
        return false;
    
    m_commandList->Reset(m_commandAllocator, nullptr); 
    m_commandList->CopyResource(nativeDest, nativeSrc);
    m_commandList->Close();

    ID3D12CommandList* cmdList[] = { m_commandList };
    m_unityInterface->GetCommandQueue()->ExecuteCommandLists(1, cmdList);

    m_unityInterface->GetCommandQueue()->Signal(m_copyResourceFence, m_copyResourceFenceValue);
    m_copyResourceFence->SetEventOnCompletion(m_copyResourceFenceValue, m_copyResourceEventHandle);
	WaitForSingleObject(m_copyResourceEventHandle, INFINITE);
    ++m_copyResourceFenceValue;

    return true;
}

//---------------------------------------------------------------------------------------------------------------------

ITexture2D* D3D12GraphicsDevice::CreateSharedD3D12Texture(uint32_t w, uint32_t h) {
    //[Note-sin: 2019-10-30] Taken from RaytracedHardShadow
    // note: sharing textures with d3d11 requires some flags and restrictions:
    // - MipLevels must be 1
    // - D3D12_HEAP_FLAG_SHARED for heap flags
    // - D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET and D3D12_RESOURCE_FLAG_ALLOW_SIMULTANEOUS_ACCESS for resource flags

    D3D12_RESOURCE_DESC desc{};
    desc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
    desc.Alignment = 0;
    desc.Width = w;
    desc.Height = h;
    desc.DepthOrArraySize = 1;
    desc.MipLevels = 1;
    desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    desc.SampleDesc.Count = 1;
    desc.SampleDesc.Quality = 0;
    desc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    desc.Flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    desc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET | D3D12_RESOURCE_FLAG_ALLOW_SIMULTANEOUS_ACCESS;

    const D3D12_HEAP_FLAGS flags = D3D12_HEAP_FLAG_SHARED;
    const D3D12_RESOURCE_STATES initialState = D3D12_RESOURCE_STATE_COPY_DEST;


    ID3D12Resource* nativeTex = nullptr;
    HRESULT hr = m_d3d12Device->CreateCommittedResource(&DEFAULT_HEAP_PROPS, flags, &desc, initialState, nullptr, IID_PPV_ARGS(&nativeTex));
    if (!SUCCEEDED(hr)) {
        return nullptr;
    }

    ID3D11Texture2D* sharedTex = nullptr;
    HANDLE handle = nullptr;   
    hr = m_d3d12Device->CreateSharedHandle(nativeTex, nullptr, GENERIC_ALL, nullptr, &handle);
    if (SUCCEEDED(hr)) {
        //ID3D11Device::OpenSharedHandle() doesn't accept handles created by d3d12. OpenSharedHandle1() is needed.
        hr = m_d3d11Device->OpenSharedResource1(handle, IID_PPV_ARGS(&sharedTex));
    }

    return new D3D12Texture2D(w,h,nativeTex, handle, sharedTex);
}

//----------------------------------------------------------------------------------------------------------------------

ITexture2D* D3D12GraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h) {
    assert(false && "CreateCPUReadTextureV need to implement on D3D12");
    return nullptr;
}

//----------------------------------------------------------------------------------------------------------------------
rtc::scoped_refptr<webrtc::I420Buffer> D3D12GraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
{
    assert(false && "ConvertRGBToI420 need to implement on D3D12");
    return nullptr;
}



} //end namespace
