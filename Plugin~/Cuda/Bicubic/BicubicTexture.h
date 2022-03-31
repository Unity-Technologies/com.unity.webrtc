
#include <cuda.h>


namespace unity
{
namespace webrtc
{
    enum FilterMode
    {
        MODE_NEAREST,
        MODE_BILINEAR,
        MODE_BICUBIC,
        MODE_FAST_BICUBIC,
        MODE_CATMULL_ROM,
        NUM_MODES
    };

    void Resize(const CUarray& src, CUarray& dst, int width, int height, FilterMode mode);
}
}
