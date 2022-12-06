#pragma once

#include <api/jsep.h>
#include <api/set_local_description_observer_interface.h>

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    class SetLocalDescriptionObserver;
    using DelegateSetLocalDesc =
        void (*)(PeerConnectionObject*, SetLocalDescriptionObserver*, RTCErrorType, const char*);

    class SetLocalDescriptionObserver : public ::webrtc::SetLocalDescriptionObserverInterface
    {
    public:
        static rtc::scoped_refptr<SetLocalDescriptionObserver> Create(PeerConnectionObject* connection);
        static void RegisterCallback(DelegateSetLocalDesc callback) { s_setLocalDescCallback = callback; }

        void OnSetLocalDescriptionComplete(RTCError error) override;

    protected:
        explicit SetLocalDescriptionObserver(PeerConnectionObject* connection);
        ~SetLocalDescriptionObserver() override = default;

    private:
        PeerConnectionObject* m_connection;
        static DelegateSetLocalDesc s_setLocalDescCallback;
    };

} // end namespace webrtc
} // end namespace unity
