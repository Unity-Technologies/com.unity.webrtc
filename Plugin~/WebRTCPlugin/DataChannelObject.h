#pragma once

namespace unity
{
namespace webrtc
{

    namespace webrtc = ::webrtc;

    class PeerConnectionObject;
    class DataChannelObject;
    using DelegateOnMessage = void(*)(DataChannelObject*, const byte*, int);
    using DelegateOnOpen = void(*)(DataChannelObject*);
    using DelegateOnClose = void(*)(DataChannelObject*);

    class DataChannelObject : public webrtc::DataChannelObserver
    {
    public:
        DataChannelObject(rtc::scoped_refptr<webrtc::DataChannelInterface> channel, PeerConnectionObject& pc);
        ~DataChannelObject();

        std::string GetLabel() const
        {
            return dataChannel->label();
        }
        int GetID() const
        {
            return dataChannel->id();
        }
        void Close()
        {
            dataChannel->Close();
        }
        void Send(const char* data)
        {
            dataChannel->Send(webrtc::DataBuffer(std::string(data)));
        }

        webrtc::DataChannelInterface::DataState GetReadyState() const
        {
            return dataChannel->state();
        }

        void Send(const byte* data, int len)
        {
            rtc::CopyOnWriteBuffer buf(data, len);
            dataChannel->Send(webrtc::DataBuffer(buf, true));
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
    private:
        rtc::scoped_refptr<webrtc::DataChannelInterface> dataChannel;
    };

} // end namespace webrtc
} // end namespace unity
