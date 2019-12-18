#include "pch.h"
#include "GraphicsUtility.h"

namespace WebRTC {

rtc::scoped_refptr<webrtc::I420Buffer> GraphicsUtility::ConvertRGBToI420Buffer(const uint32_t width, const uint32_t height,
    const uint32_t rowToRowInBytes, const uint8_t* srcData)
{

    rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Create(width, height);

    int StrideY = i420_buffer->StrideY();
    int StrideU = i420_buffer->StrideU();
    int StrideV = i420_buffer->StrideV();

    int yIndex = 0;
    int uIndex = 0;
    int vIndex = 0;

    uint8_t* yuv_y = i420_buffer->MutableDataY();
    uint8_t* yuv_u = i420_buffer->MutableDataU();
    uint8_t* yuv_v = i420_buffer->MutableDataV();

    for (uint32_t i = 0; i < height; i++)
    {
        for (uint32_t j = 0; j < width; j++)
        {
            const uint32_t startIndex = i * rowToRowInBytes + j * 4;
            const int B = srcData[startIndex + 0];
            const int G = srcData[startIndex + 1];
            const int R = srcData[startIndex + 2];

            const int Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
            const int U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
            const int V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;

            yuv_y[yIndex++] = static_cast<uint8_t>((Y < 0) ? 0 : ((Y > 255) ? 255 : Y));
            if (i % 2 == 0 && j % 2 == 0)
            {
                yuv_u[uIndex++] = static_cast<uint8_t>((U < 0) ? 0 : ((U > 255) ? 255 : U));
                yuv_v[vIndex++] = static_cast<uint8_t>((V < 0) ? 0 : ((V > 255) ? 255 : V));
            }
        }
    }

    return i420_buffer;

}

} //end namespace

