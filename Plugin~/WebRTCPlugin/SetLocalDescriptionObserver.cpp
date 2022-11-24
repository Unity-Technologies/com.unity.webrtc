#include "pch.h"

#include "PeerConnectionObject.h"
#include "SetLocalDescriptionObserver.h"

namespace unity
{
namespace webrtc
{
    DelegateSetLocalDesc SetLocalDescriptionObserver::s_setLocalDescCallback = nullptr;

    rtc::scoped_refptr<SetLocalDescriptionObserver>
    SetLocalDescriptionObserver::Create(PeerConnectionObject* connection)
    {
        return rtc::make_ref_counted<SetLocalDescriptionObserver>(connection);
    }

    SetLocalDescriptionObserver::SetLocalDescriptionObserver(PeerConnectionObject* connection)
    {
        m_connection = connection;
    }

    void SetLocalDescriptionObserver::OnSetLocalDescriptionComplete(RTCError error)
    {
        s_setLocalDescCallback(m_connection, this, error.type(), error.message());
    }
} // end namespace webrtc
} // end namespace unity
