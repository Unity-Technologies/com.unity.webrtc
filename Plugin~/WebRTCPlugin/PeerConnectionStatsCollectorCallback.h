#pragma once

#include <api/stats/rtc_stats_collector_callback.h>
#include <api/stats/rtc_stats_report.h>

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    class PeerConnectionObject;
    class PeerConnectionStatsCollectorCallback;
    using DelegateCollectStats =
        void (*)(PeerConnectionObject*, PeerConnectionStatsCollectorCallback*, const RTCStatsReport*);
    class PeerConnectionStatsCollectorCallback : public RTCStatsCollectorCallback
    {
    public:
        PeerConnectionStatsCollectorCallback(const PeerConnectionStatsCollectorCallback&) = delete;
        PeerConnectionStatsCollectorCallback& operator=(const PeerConnectionStatsCollectorCallback&) = delete;
        static rtc::scoped_refptr<PeerConnectionStatsCollectorCallback> Create(PeerConnectionObject* connection);
        void OnStatsDelivered(const rtc::scoped_refptr<const RTCStatsReport>& report) override;

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
