#pragma once

#include <api/peer_connection_interface.h>

#include "DataChannelObject.h"
#include "PeerConnectionStatsCollectorCallback.h"
#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    extern webrtc::SdpType ConvertSdpType(RTCSdpType type);
    extern RTCSdpType ConvertSdpType(webrtc::SdpType type);

    using DelegateCreateSDSuccess = void (*)(PeerConnectionObject*, RTCSdpType, const char*);
    using DelegateCreateSDFailure = void (*)(PeerConnectionObject*, RTCErrorType, const char*);
    using DelegateLocalSdpReady = void (*)(PeerConnectionObject*, const char*, const char*);
    using DelegateIceCandidate = void (*)(PeerConnectionObject*, const char*, const char*, const int);
    using DelegateOnIceConnectionChange = void (*)(PeerConnectionObject*, PeerConnectionInterface::IceConnectionState);
    using DelegateOnIceGatheringChange = void (*)(PeerConnectionObject*, PeerConnectionInterface::IceGatheringState);
    using DelegateOnConnectionStateChange =
        void (*)(PeerConnectionObject*, PeerConnectionInterface::PeerConnectionState);
    using DelegateOnDataChannel = void (*)(PeerConnectionObject*, DataChannelInterface*);
    using DelegateOnRenegotiationNeeded = void (*)(PeerConnectionObject*);
    using DelegateOnTrack = void (*)(PeerConnectionObject*, RtpTransceiverInterface*);
    using DelegateOnRemoveTrack = void (*)(PeerConnectionObject*, RtpReceiverInterface*);

    class PeerConnectionObject : public PeerConnectionObserver
    {
    public:
        PeerConnectionObject(Context& context);
        ~PeerConnectionObject() override;

        void Close();
        RTCErrorType SetLocalDescription(
            const RTCSessionDescription& desc,
            rtc::scoped_refptr<SetLocalDescriptionObserverInterface> observer,
            std::string& error);
        RTCErrorType SetLocalDescriptionWithoutDescription(
            rtc::scoped_refptr<SetLocalDescriptionObserverInterface> observer, std::string& error);
        RTCErrorType SetRemoteDescription(
            const RTCSessionDescription& desc,
            rtc::scoped_refptr<SetRemoteDescriptionObserverInterface>,
            std::string& error);

        bool GetSessionDescription(const SessionDescriptionInterface* sdp, RTCSessionDescription& desc) const;
        RTCErrorType SetConfiguration(const std::string& config);
        std::string GetConfiguration() const;
        void CreateOffer(const RTCOfferAnswerOptions& options, CreateSessionDescriptionObserver* observer);
        void CreateAnswer(const RTCOfferAnswerOptions& options, CreateSessionDescriptionObserver* observer);
        void ReceiveStatsReport(const rtc::scoped_refptr<const RTCStatsReport>& report);

        void RegisterCallbackCreateSD(DelegateCreateSDSuccess onSuccess, DelegateCreateSDFailure onFailure)
        {
            onCreateSDSuccess = onSuccess;
            onCreateSDFailure = onFailure;
        }

        void RegisterLocalSdpReady(DelegateLocalSdpReady callback) { onLocalSdpReady = callback; }
        void RegisterIceCandidate(DelegateIceCandidate callback) { onIceCandidate = callback; }
        void RegisterIceConnectionChange(DelegateOnIceConnectionChange callback) { onIceConnectionChange = callback; }
        void RegisterConnectionStateChange(DelegateOnConnectionStateChange callback)
        {
            onConnectionStateChange = callback;
        }
        void RegisterIceGatheringChange(DelegateOnIceGatheringChange callback) { onIceGatheringChange = callback; }
        void RegisterOnDataChannel(DelegateOnDataChannel callback) { onDataChannel = callback; }
        void RegisterOnRenegotiationNeeded(DelegateOnRenegotiationNeeded callback) { onRenegotiationNeeded = callback; }
        void RegisterOnTrack(DelegateOnTrack callback) { onTrack = callback; }
        void RegisterOnRemoveTrack(DelegateOnRemoveTrack callback) { onRemoveTrack = callback; }

        // webrtc::PeerConnectionObserver
        // Triggered when the SignalingState changed.
        void OnSignalingChange(PeerConnectionInterface::SignalingState new_state) override;
        // Triggered when media is received on a new stream from remote peer.
        void OnAddStream(rtc::scoped_refptr<MediaStreamInterface> stream) override;
        // Triggered when a remote peer closes a stream.
        void OnRemoveStream(rtc::scoped_refptr<MediaStreamInterface> stream) override;
        // Triggered when a remote peer opens a data channel.
        void OnDataChannel(rtc::scoped_refptr<DataChannelInterface> data_channel) override;
        // Triggered when renegotiation is needed. For example, an ICE restart
        // has begun.
        void OnRenegotiationNeeded() override;
        // Called any time the IceConnectionState changes.
        void OnIceConnectionChange(PeerConnectionInterface::IceConnectionState new_state) override;
        // Called any time the PeerConnectionState changes.
        virtual void OnConnectionChange(PeerConnectionInterface::PeerConnectionState new_state) override;
        // Called any time the IceGatheringState changes.
        void OnIceGatheringChange(PeerConnectionInterface::IceGatheringState new_state) override;
        // A new ICE candidate has been gathered.
        void OnIceCandidate(const IceCandidateInterface* candidate) override;
        // Ice candidates have been removed.
        void OnIceCandidatesRemoved(const std::vector<cricket::Candidate>& candidates) override { }
        // Called when the ICE connection receiving status changes.
        void OnIceConnectionReceivingChange(bool Receiving) override { }
        // This is called when signaling indicates a transceiver will be receiving
        // media from the remote endpoint. This is fired during a call to
        // SetRemoteDescription. The receiving track can be accessed by:
        // |transceiver->receiver()->track()| and its associated streams by
        // |transceiver->receiver()->streams()|.
        // Note: This will only be called if Unified Plan semantics are specified.
        // This behavior is specified in section 2.2.8.2.5 of the "Set the
        // RTCSessionDescription" algorithm:
        // https://w3c.github.io/webrtc-pc/#set-description
        void OnTrack(rtc::scoped_refptr<RtpTransceiverInterface> transceiver) override;

        void OnRemoveTrack(rtc::scoped_refptr<RtpReceiverInterface> receiver) override;

        friend class DataChannelObject;

        DelegateCreateSDSuccess onCreateSDSuccess = nullptr;
        DelegateCreateSDFailure onCreateSDFailure = nullptr;
        DelegateIceCandidate onIceCandidate = nullptr;
        DelegateLocalSdpReady onLocalSdpReady = nullptr;
        DelegateOnConnectionStateChange onConnectionStateChange = nullptr;
        DelegateOnIceConnectionChange onIceConnectionChange = nullptr;
        DelegateOnIceGatheringChange onIceGatheringChange = nullptr;
        DelegateOnDataChannel onDataChannel = nullptr;
        DelegateOnRenegotiationNeeded onRenegotiationNeeded = nullptr;
        DelegateOnTrack onTrack = nullptr;
        DelegateOnRemoveTrack onRemoveTrack = nullptr;
        rtc::scoped_refptr<PeerConnectionInterface> connection = nullptr;

    private:
        Context& context;
    };

} // end namespace webrtc
} // end namespace unity
