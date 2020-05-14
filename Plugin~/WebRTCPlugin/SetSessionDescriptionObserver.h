#pragma once
#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{

    class SetSessionDescriptionObserver : public webrtc::SetSessionDescriptionObserver
    {
    public:
        static rtc::scoped_refptr<SetSessionDescriptionObserver> Create(PeerConnectionObject* connection);
        void RegisterDelegateOnSuccess(DelegateSetSessionDescSuccess onSuccess);
        void RegisterDelegateOnFailure(DelegateSetSessionDescFailure onFailure);

        void OnSuccess() override;
        void OnFailure(webrtc::RTCError error) override;
    protected:
        explicit SetSessionDescriptionObserver(PeerConnectionObject * connection);
        ~SetSessionDescriptionObserver() = default;
    private:
        PeerConnectionObject* m_connection;
        std::vector<DelegateSetSessionDescSuccess> m_vectorDelegateSetSDSuccess;
        std::vector<DelegateSetSessionDescFailure> m_vectorDelegateSetSDFailure;
    };

} // end namespace webrtc
} // end namespace unity
