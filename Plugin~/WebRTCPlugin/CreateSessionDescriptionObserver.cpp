#include "pch.h"

#include "CreateSessionDescriptionObserver.h"
#include "PeerConnectionObject.h"

namespace unity
{
namespace webrtc
{
    DelegateCreateSessionDesc CreateSessionDescriptionObserver::s_createSessionDescCallback = nullptr;

    rtc::scoped_refptr<CreateSessionDescriptionObserver>
    CreateSessionDescriptionObserver::Create(PeerConnectionObject* connection)
    {
        return rtc::make_ref_counted<CreateSessionDescriptionObserver>(connection);
    }

    CreateSessionDescriptionObserver::CreateSessionDescriptionObserver(PeerConnectionObject* connection)
    {
        m_connection = connection;
    }

    void CreateSessionDescriptionObserver::OnSuccess(SessionDescriptionInterface* desc)
    {
        std::string out;
        desc->ToString(&out);
        const auto sdpType = ConvertSdpType(desc->GetType());
        s_createSessionDescCallback(m_connection, this, sdpType, out.c_str(), RTCErrorType::NONE, nullptr);
    }

    void CreateSessionDescriptionObserver::OnFailure(webrtc::RTCError error)
    {
        s_createSessionDescCallback(m_connection, this, RTCSdpType(), nullptr, error.type(), error.message());
    }

} // end namespace webrtc
} // end namespace unity
