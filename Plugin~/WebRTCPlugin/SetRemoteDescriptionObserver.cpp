#include "pch.h"

#include "PeerConnectionObject.h"
#include "SetRemoteDescriptionObserver.h"

namespace unity
{
namespace webrtc
{
    DelegateSetRemoteDesc SetRemoteDescriptionObserver::s_setRemoteDescCallback = nullptr;

    rtc::scoped_refptr<SetRemoteDescriptionObserver>
    SetRemoteDescriptionObserver::Create(PeerConnectionObject* connection)
    {
        return rtc::make_ref_counted<SetRemoteDescriptionObserver>(connection);
    }

    SetRemoteDescriptionObserver::SetRemoteDescriptionObserver(PeerConnectionObject* connection)
    {
        m_connection = connection;
    }

    void SetRemoteDescriptionObserver::OnSetRemoteDescriptionComplete(RTCError error)
    {
        s_setRemoteDescCallback(m_connection, this, error.type(), error.message());
    }
} // end namespace webrtc
} // end namespace unity
