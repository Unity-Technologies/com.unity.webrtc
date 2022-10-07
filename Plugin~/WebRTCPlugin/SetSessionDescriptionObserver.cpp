#include "pch.h"

#include "PeerConnectionObject.h"
#include "SetSessionDescriptionObserver.h"

namespace unity
{
namespace webrtc
{
    DelegateSetSessionDesc SetSessionDescriptionObserver::s_setSessionDescCallback = nullptr;

    rtc::scoped_refptr<SetSessionDescriptionObserver>
    SetSessionDescriptionObserver::Create(PeerConnectionObject* connection)
    {
        return new rtc::RefCountedObject<SetSessionDescriptionObserver>(connection);
    }

    SetSessionDescriptionObserver::SetSessionDescriptionObserver(PeerConnectionObject* connection)
    {
        m_connection = connection;
    }

    void SetSessionDescriptionObserver::OnSuccess()
    {
        s_setSessionDescCallback(m_connection, this, RTCErrorType::NONE, nullptr);
    }

    void SetSessionDescriptionObserver::OnFailure(webrtc::RTCError error)
    {
        s_setSessionDescCallback(m_connection, this, error.type(), error.message());
    }

} // end namespace webrtc
} // end namespace unity
