#pragma once

#include <api/data_channel_interface.h>

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class PeerConnectionObject;
    class DataChannelObject;
    using DelegateOnMessage = void (*)(DataChannelInterface*, const uint8_t*, int32_t);
    using DelegateOnOpen = void (*)(DataChannelInterface*);
    using DelegateOnClose = void (*)(DataChannelInterface*);
    using DelegateOnError = void (*)(DataChannelInterface*, RTCErrorType, const char*, int32_t);

    class DataChannelObject : public DataChannelObserver
    {
    public:
        DataChannelObject(rtc::scoped_refptr<DataChannelInterface> channel, PeerConnectionObject& pc);
        ~DataChannelObject() override;

        void Close() { dataChannel->Close(); }
        void RegisterOnMessage(DelegateOnMessage callback) { onMessage = callback; }
        void RegisterOnOpen(DelegateOnOpen callback) { onOpen = callback; }
        void RegisterOnClose(DelegateOnClose callback) { onClose = callback; }
        void RegisterOnError(DelegateOnError callback) { onError = callback; }
        // werbrtc::DataChannelObserver
        // The data channel state have changed.
        void OnStateChange() override;
        //  A data buffer was successfully received.
        void OnMessage(const webrtc::DataBuffer& buffer) override;

        rtc::scoped_refptr<webrtc::DataChannelInterface> dataChannel;
        DelegateOnMessage onMessage;
        DelegateOnOpen onOpen;
        DelegateOnClose onClose;
        DelegateOnError onError;
    };

} // end namespace webrtc
} // end namespace unity
