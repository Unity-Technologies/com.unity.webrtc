#pragma once

#include "d3d12.h"

namespace WebRTC {

struct D3D12ResourceFootprint {
    D3D12_PLACED_SUBRESOURCE_FOOTPRINT Footprint;
    UINT NumRows;
    UINT64 RowSize;
    UINT64 ResourceSize;

};

} //end namespace


