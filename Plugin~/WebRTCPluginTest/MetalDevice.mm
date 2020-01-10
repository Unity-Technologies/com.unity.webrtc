#import <Metal/Metal.h>

class MetalDevice
{
public:
    MetalDevice();

private:
    id <MTLDevice> device;
};

MetalDevice::MetalDevice()
{
    device = MTLCreateSystemDefaultDevice();
}
