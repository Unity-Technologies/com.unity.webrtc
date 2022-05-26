#pragma once

#include <d3d12.h>

namespace unity
{
namespace webrtc
{

    const D3D12_HEAP_PROPERTIES D3D12_DEFAULT_HEAP_PROPS = {
        D3D12_HEAP_TYPE_DEFAULT, D3D12_CPU_PAGE_PROPERTY_UNKNOWN, D3D12_MEMORY_POOL_UNKNOWN, 0, 0
    };

    const D3D12_HEAP_PROPERTIES D3D12_READBACK_HEAP_PROPS = {
        D3D12_HEAP_TYPE_READBACK, D3D12_CPU_PAGE_PROPERTY_UNKNOWN, D3D12_MEMORY_POOL_UNKNOWN, 0, 0
    };

} // end namespace webrtc
} // end namespace unity
