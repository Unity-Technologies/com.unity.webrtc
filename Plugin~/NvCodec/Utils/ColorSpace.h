#pragma once
#include <stdint.h>
#include <cuda_runtime.h>

typedef enum ColorSpaceStandard {
    ColorSpaceStandard_BT709 = 0,
    ColorSpaceStandard_BT601 = 2,
    ColorSpaceStandard_BT2020 = 4
} ColorSpaceStandard;

union BGRA32 {
    uint32_t d;
    uchar4 v;
    struct {
        uint8_t b, g, r, a;
    } c;
};

union RGBA32 {
    uint32_t d;
    uchar4 v;
    struct {
        uint8_t r, g, b, a;
    } c;
};

union BGRA64 {
    uint64_t d;
    ushort4 v;
    struct {
        uint16_t b, g, r, a;
    } c;
};

union RGBA64 {
    uint64_t d;
    ushort4 v;
    struct {
        uint16_t r, g, b, a;
    } c;
};
