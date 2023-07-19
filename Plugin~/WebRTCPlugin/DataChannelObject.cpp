#include "pch.h"

#include "DataChannelObject.h"

namespace unity
{
namespace webrtc
{

    DataChannelObject::DataChannelObject(
        rtc::scoped_refptr<webrtc::DataChannelInterface> channel, PeerConnectionObject& pc)
        : dataChannel(channel)
        , onMessage(nullptr)
        , onOpen(nullptr)
        , onClose(nullptr)
        , onError(nullptr)
    {
        dataChannel->RegisterObserver(this);
    }
    DataChannelObject::~DataChannelObject()
    {
        dataChannel->UnregisterObserver();

        auto state = dataChannel->state();
        if (state == webrtc::DataChannelInterface::kOpen)
        {
            dataChannel->Close();
        }
        dataChannel = nullptr;
        onClose = nullptr;
        onOpen = nullptr;
        onMessage = nullptr;
    }

    void DataChannelObject::OnStateChange()
    {
        auto state = dataChannel->state();
        switch (state)
        {
        case webrtc::DataChannelInterface::kOpen:
            if (onOpen)
                onOpen(dataChannel.get());
            break;
        case webrtc::DataChannelInterface::kClosed:
        {
            RTCError error = dataChannel->error();
            if (error.type() == RTCErrorType::NONE)
            {
                if (onClose)
                    onClose(dataChannel.get());
            }
            else
            {
                if (onError)
                    onError(
                        dataChannel.get(),
                        error.type(),
                        error.message(),
                        static_cast<int32_t>(std::strlen(error.message())));
            }
            break;
        }
        case webrtc::DataChannelInterface::kConnecting:
        case webrtc::DataChannelInterface::kClosing:
            break;
        }
    }
    void DataChannelObject::OnMessage(const webrtc::DataBuffer& buffer)
    {
        if (onMessage)
        {
            size_t size = buffer.data.size();
            if (onMessage != nullptr)
            {
                onMessage(dataChannel.get(), buffer.data.data(), static_cast<int32_t>(size));
            }
        }
    }

} // end namespace webrtc
} // end namespace unity
