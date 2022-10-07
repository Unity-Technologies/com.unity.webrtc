#pragma once

#include <api/jsep.h>

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    class SetSessionDescriptionObserver;
    using DelegateSetSessionDesc =
        void (*)(PeerConnectionObject*, SetSessionDescriptionObserver*, RTCErrorType, const char*);

    class SetSessionDescriptionObserver : public ::webrtc::SetSessionDescriptionObserver
    {
    public:
        static rtc::scoped_refptr<SetSessionDescriptionObserver> Create(PeerConnectionObject* connection);
        static void RegisterCallback(DelegateSetSessionDesc callback) { s_setSessionDescCallback = callback; }

        void OnSuccess() override;
        void OnFailure(webrtc::RTCError error) override;

    protected:
        explicit SetSessionDescriptionObserver(PeerConnectionObject* connection);
        ~SetSessionDescriptionObserver() override = default;

    private:
        PeerConnectionObject* m_connection;
        static DelegateSetSessionDesc s_setSessionDescCallback;
    };

} // end namespace webrtc
} // end namespace unity
