#pragma once

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class PeerConnectionObject;
    class DataChannelObject;
    using DelegateOnMessage = void(*)(DataChannelInterface*, const byte*, int32_t);
    using DelegateOnOpen = void(*)(DataChannelInterface*);
    using DelegateOnClose = void(*)(DataChannelInterface*);

    class DataChannelObject : public DataChannelObserver
    {
    public:
        DataChannelObject(rtc::scoped_refptr<DataChannelInterface> channel, PeerConnectionObject& pc);
        ~DataChannelObject();

        void Close()
        {
            dataChannel->Close();
        }
        void RegisterOnMessage(DelegateOnMessage callback)
        {
            onMessage = callback;
        }
        void RegisterOnOpen(DelegateOnOpen callback)
        {
            onOpen = callback;
        }
        void RegisterOnClose(DelegateOnClose callback)
        {
            onClose = callback;
        }
        //werbrtc::DataChannelObserver
       // The data channel state have changed.
        void OnStateChange() override;
        //  A data buffer was successfully received.
        void OnMessage(const webrtc::DataBuffer& buffer) override;

        DelegateOnMessage onMessage = nullptr;
        DelegateOnOpen onOpen = nullptr;
        DelegateOnClose onClose = nullptr;
        rtc::scoped_refptr<webrtc::DataChannelInterface> dataChannel;
    };

} // end namespace webrtc
} // end namespace unity
