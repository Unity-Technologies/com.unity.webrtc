#pragma once

#include <api/jsep.h>

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    class CreateSessionDescriptionObserver;
    using DelegateCreateSessionDesc = void (*)(
        PeerConnectionObject*, CreateSessionDescriptionObserver*, RTCSdpType, const char*, RTCErrorType, const char*);

    class CreateSessionDescriptionObserver : public ::webrtc::CreateSessionDescriptionObserver
    {
    public:
        static rtc::scoped_refptr<CreateSessionDescriptionObserver> Create(PeerConnectionObject* connection);
        static void RegisterCallback(DelegateCreateSessionDesc callback) { s_createSessionDescCallback = callback; }

        void OnSuccess(SessionDescriptionInterface* desc) override;
        void OnFailure(webrtc::RTCError error) override;

    protected:
        explicit CreateSessionDescriptionObserver(PeerConnectionObject* connection);
        ~CreateSessionDescriptionObserver() override = default;

    private:
        PeerConnectionObject* m_connection;
        static DelegateCreateSessionDesc s_createSessionDescCallback;
    };

} // end namespace webrtc
} // end namespace unity
