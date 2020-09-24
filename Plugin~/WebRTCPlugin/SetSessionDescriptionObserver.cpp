#include "pch.h"
#include "SetSessionDescriptionObserver.h"
#include "PeerConnectionObject.h"

namespace unity
{
namespace webrtc
{

    rtc::scoped_refptr<SetSessionDescriptionObserver> SetSessionDescriptionObserver::Create(PeerConnectionObject* connection)
    {
        return new rtc::RefCountedObject<SetSessionDescriptionObserver>(connection);
    }

    SetSessionDescriptionObserver::SetSessionDescriptionObserver(PeerConnectionObject* connection)
    {
        m_connection = connection;
    }

    void SetSessionDescriptionObserver::RegisterDelegateOnSuccess(DelegateSetSessionDescSuccess onSuccess)
    {
        m_vectorDelegateSetSDSuccess.push_back(onSuccess);
	}

    void SetSessionDescriptionObserver::RegisterDelegateOnFailure(DelegateSetSessionDescFailure onFailure)
    {
        m_vectorDelegateSetSDFailure.push_back(onFailure);
    }

    void SetSessionDescriptionObserver::OnSuccess()
    {
        for (auto delegate: m_vectorDelegateSetSDSuccess)
        {
            delegate(m_connection);
        }
    }

    void SetSessionDescriptionObserver::OnFailure(webrtc::RTCError error)
    {
        for (auto delegate : m_vectorDelegateSetSDFailure)
        {
            delegate(m_connection, error.type(), error.message());
        }
    }
    
} // end namespace webrtc
} // end namespace unity
