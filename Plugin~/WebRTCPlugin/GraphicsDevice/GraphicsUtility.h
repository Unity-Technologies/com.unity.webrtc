#pragma once


namespace unity
{
namespace webrtc
{

class GraphicsUtility {
public:
    static rtc::scoped_refptr<::webrtc::I420Buffer> ConvertRGBToI420Buffer(const uint32_t width, const uint32_t height,
        const uint32_t rowToRowInBytes, const uint8_t* srcData);
};

} // end namespace webrtc
} // end namespace unity
