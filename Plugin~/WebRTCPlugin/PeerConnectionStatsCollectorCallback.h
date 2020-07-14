#pragma once
#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    class PeerConnectionObject;
    using DelegateCollectStats = void(*)(PeerConnectionObject*, const webrtc::RTCStatsReport*);
    class PeerConnectionStatsCollectorCallback : public webrtc::RTCStatsCollectorCallback
    {
    public:
        PeerConnectionStatsCollectorCallback(const PeerConnectionStatsCollectorCallback&) = delete;
        PeerConnectionStatsCollectorCallback& operator=(const PeerConnectionStatsCollectorCallback&) = delete;
        static PeerConnectionStatsCollectorCallback* Create(PeerConnectionObject* connection);
        void OnStatsDelivered(const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report) override;

        static void RegisterOnGetStats(DelegateCollectStats callback) { s_collectStatsCallback = callback; }
    protected:
        explicit PeerConnectionStatsCollectorCallback(PeerConnectionObject* owner) { m_owner = owner; }
        ~PeerConnectionStatsCollectorCallback() override = default;
    private:
        PeerConnectionObject* m_owner = nullptr;

        static DelegateCollectStats s_collectStatsCallback;
    };
} // end namespace webrtc
} // end namespace unity
