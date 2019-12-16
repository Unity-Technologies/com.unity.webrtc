#pragma once

namespace WebRTC {

enum GraphicsDeviceType {
    GRAPHICS_DEVICE_D3D11   = 0,
    GRAPHICS_DEVICE_D3D12   = 1,
    GRAPHICS_DEVICE_OPENGL  = 10,
    GRAPHICS_DEVICE_METAL   = 20,
    GRAPHICS_DEVICE_VULKAN  = 30,
};

} //end namespace
