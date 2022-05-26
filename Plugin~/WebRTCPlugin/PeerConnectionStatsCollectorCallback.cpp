#include "pch.h"

#include "PeerConnectionObject.h"
#include "PeerConnectionStatsCollectorCallback.h"

namespace unity
{
namespace webrtc
{
    DelegateCollectStats PeerConnectionStatsCollectorCallback::s_collectStatsCallback = nullptr;

    PeerConnectionStatsCollectorCallback* PeerConnectionStatsCollectorCallback::Create(PeerConnectionObject* connection)
    {
        return new rtc::RefCountedObject<PeerConnectionStatsCollectorCallback>(connection);
    }
    void PeerConnectionStatsCollectorCallback::OnStatsDelivered(
        const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report)
    {
        m_owner->ReceiveStatsReport(report);
        s_collectStatsCallback(m_owner, report.get());
    }
} // end namespace webrtc
} // end namespace unity
