#include "pch.h"

#include "DataChannelObject.h"

namespace unity
{
namespace webrtc
{

    DataChannelObject::DataChannelObject(
        rtc::scoped_refptr<webrtc::DataChannelInterface> channel, PeerConnectionObject& pc)
        : dataChannel(channel)
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
            if (onOpen != nullptr)
            {
                onOpen(this->dataChannel.get());
            }
            break;
        case webrtc::DataChannelInterface::kClosed:
            if (onClose != nullptr)
            {
                onClose(this->dataChannel.get());
            }
            break;
        case webrtc::DataChannelInterface::kConnecting:
        case webrtc::DataChannelInterface::kClosing:
            break;
        }
    }
    void DataChannelObject::OnMessage(const webrtc::DataBuffer& buffer)
    {
        if (onMessage != nullptr)
        {
            size_t size = buffer.data.size();
            if (onMessage != nullptr)
            {
                onMessage(this->dataChannel.get(), buffer.data.data(), static_cast<int32_t>(size));
            }
        }
    }

} // end namespace webrtc
} // end namespace unity
