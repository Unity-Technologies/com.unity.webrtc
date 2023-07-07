#include "pch.h"

#include <rtc_base/strings/json.h>

#include "Context.h"
#include "PeerConnectionObject.h"

namespace unity
{
namespace webrtc
{
    webrtc::SdpType ConvertSdpType(RTCSdpType type)
    {
        switch (type)
        {
        case RTCSdpType::Offer:
            return webrtc::SdpType::kOffer;
        case RTCSdpType::PrAnswer:
            return webrtc::SdpType::kPrAnswer;
        case RTCSdpType::Answer:
            return webrtc::SdpType::kAnswer;
        case RTCSdpType::Rollback:
            return webrtc::SdpType::kRollback;
        }
        throw std::invalid_argument("Unknown RTCSdpType");
    }

    RTCSdpType ConvertSdpType(webrtc::SdpType type)
    {
        switch (type)
        {
        case webrtc::SdpType::kOffer:
            return RTCSdpType::Offer;
        case webrtc::SdpType::kPrAnswer:
            return RTCSdpType::PrAnswer;
        case webrtc::SdpType::kAnswer:
            return RTCSdpType::Answer;
        case webrtc::SdpType::kRollback:
            return RTCSdpType::Rollback;
        }
        throw std::invalid_argument("Unknown SdpType");
    }

    PeerConnectionObject::PeerConnectionObject(Context& context)
        : context(context)
    {
    }

    PeerConnectionObject::~PeerConnectionObject()
    {
        if (connection == nullptr)
        {
            return;
        }
        auto senders = connection->GetSenders();
        for (const auto& sender : senders)
        {
            // ignore error.
            connection->RemoveTrackOrError(sender);
        }

        const auto state = connection->peer_connection_state();
        if (state != webrtc::PeerConnectionInterface::PeerConnectionState::kClosed)
        {
            connection->Close();
        }
        connection = nullptr;
    }

    void PeerConnectionObject::OnDataChannel(rtc::scoped_refptr<webrtc::DataChannelInterface> channel)
    {
        context.AddDataChannel(channel, *this);
        if (onDataChannel != nullptr)
        {
            onDataChannel(this, channel.get());
        }
    }

    void PeerConnectionObject::OnIceCandidate(const webrtc::IceCandidateInterface* candidate)
    {
        std::string out;

        if (!candidate->ToString(&out))
        {
            DebugError("Can't make string form of sdp.");
        }
        if (onIceCandidate != nullptr)
        {
            onIceCandidate(this, out.c_str(), candidate->sdp_mid().c_str(), candidate->sdp_mline_index());
        }
    }

    void PeerConnectionObject::OnRenegotiationNeeded()
    {
        if (onRenegotiationNeeded != nullptr)
        {
            onRenegotiationNeeded(this);
        }
    }

    void PeerConnectionObject::OnTrack(rtc::scoped_refptr<webrtc::RtpTransceiverInterface> transceiver)
    {
        context.AddRefPtr(transceiver);
        context.AddRefPtr(transceiver->receiver());
        context.AddRefPtr(transceiver->receiver()->track());

        if (onTrack != nullptr)
        {
            onTrack(this, transceiver.get());
        }
    }

    void PeerConnectionObject::OnRemoveTrack(rtc::scoped_refptr<RtpReceiverInterface> receiver)
    {
        if (onRemoveTrack != nullptr)
        {
            onRemoveTrack(this, receiver.get());
        }
    }

    // Called any time the IceConnectionState changes.
    void PeerConnectionObject::OnIceConnectionChange(webrtc::PeerConnectionInterface::IceConnectionState new_state)
    {
        DebugLog("OnIceConnectionChange %d", new_state);
        if (onIceConnectionChange != nullptr)
        {
            onIceConnectionChange(this, new_state);
        }
    }

    void PeerConnectionObject::OnConnectionChange(PeerConnectionInterface::PeerConnectionState new_state)
    {
        DebugLog("OnConnectionChange %d", new_state);
        if (onConnectionStateChange != nullptr)
        {
            onConnectionStateChange(this, new_state);
        }
    }

    // Called any time the IceGatheringState changes.
    void PeerConnectionObject::OnIceGatheringChange(webrtc::PeerConnectionInterface::IceGatheringState new_state)
    {
        DebugLog("OnIceGatheringChange %d", new_state);
        if (onIceGatheringChange != nullptr)
        {
            onIceGatheringChange(this, new_state);
        }
    }

    void PeerConnectionObject::OnSignalingChange(webrtc::PeerConnectionInterface::SignalingState new_state)
    {
        DebugLog("OnSignalingChange %d", new_state);
    }

    void PeerConnectionObject::OnAddStream(rtc::scoped_refptr<webrtc::MediaStreamInterface> stream)
    {
        DebugLog("OnAddStream");
    }

    void PeerConnectionObject::OnRemoveStream(rtc::scoped_refptr<webrtc::MediaStreamInterface> stream)
    {
        DebugLog("OnRemoveStream");
    }

    void PeerConnectionObject::Close()
    {
        if (connection != nullptr &&
            connection->peer_connection_state() != webrtc::PeerConnectionInterface::PeerConnectionState::kClosed)
        {
            // Cleanup delegates/callbacks
            onCreateSDSuccess = nullptr;
            onCreateSDFailure = nullptr;
            onLocalSdpReady = nullptr;
            onIceCandidate = nullptr;
            onIceConnectionChange = nullptr;
            onDataChannel = nullptr;
            onRenegotiationNeeded = nullptr;
            onTrack = nullptr;

            connection->Close();
        }
    }

    RTCErrorType PeerConnectionObject::SetLocalDescription(
        const RTCSessionDescription& desc,
        rtc::scoped_refptr<SetLocalDescriptionObserverInterface> observer,
        std::string& error)
    {
        SdpParseError error_;
        std::unique_ptr<SessionDescriptionInterface> _desc =
            CreateSessionDescription(ConvertSdpType(desc.type), desc.sdp, &error_);
        if (!_desc)
        {
            error = error_.description;
            return RTCErrorType::SYNTAX_ERROR;
        }
        connection->SetLocalDescription(std::move(_desc), observer);
        return RTCErrorType::NONE;
    }

    RTCErrorType PeerConnectionObject::SetLocalDescriptionWithoutDescription(
        rtc::scoped_refptr<SetLocalDescriptionObserverInterface> observer, std::string& error)
    {
        connection->SetLocalDescription(observer);
        return RTCErrorType::NONE;
    }

    RTCErrorType PeerConnectionObject::SetRemoteDescription(
        const RTCSessionDescription& desc,
        rtc::scoped_refptr<SetRemoteDescriptionObserverInterface> observer,
        std::string& error)
    {
        SdpParseError error_;
        std::unique_ptr<SessionDescriptionInterface> _desc =
            CreateSessionDescription(ConvertSdpType(desc.type), desc.sdp, &error_);
        if (!_desc)
        {
            error = error_.description;
            return RTCErrorType::SYNTAX_ERROR;
        }
        connection->SetRemoteDescription(std::move(_desc), observer);
        return RTCErrorType::NONE;
    }

    webrtc::RTCErrorType PeerConnectionObject::SetConfiguration(const std::string& config)
    {
        webrtc::PeerConnectionInterface::RTCConfiguration _config;
        if (!Convert(config, _config))
            return webrtc::RTCErrorType::INVALID_PARAMETER;

        const auto error = connection->SetConfiguration(_config);
        if (!error.ok())
        {
            LogPrint(rtc::LoggingSeverity::LS_ERROR, error.message());
        }
        return error.type();
    }

    std::string PeerConnectionObject::GetConfiguration() const
    {
        auto _config = connection->GetConfiguration();

        Json::Value root;
        root["iceServers"] = Json::Value(Json::arrayValue);
        for (webrtc::PeerConnectionInterface::IceServer iceServer : _config.servers)
        {
            Json::Value jsonIceServer = Json::Value(Json::objectValue);
            jsonIceServer["username"] = iceServer.username;
            jsonIceServer["credential"] = iceServer.password;
            jsonIceServer["credentialType"] = static_cast<int>(RTCIceCredentialType::Password);
            jsonIceServer["urls"] = Json::Value(Json::arrayValue);
            for (auto url : iceServer.urls)
            {
                jsonIceServer["urls"].append(url);
            }
            root["iceServers"].append(jsonIceServer);
        }
        root["iceTransportPolicy"] = Json::Value(Json::objectValue);
        root["iceTransportPolicy"]["hasValue"] = true;
        root["iceTransportPolicy"]["value"] = _config.type;

        root["iceCandidatePoolSize"] = Json::Value(Json::objectValue);
        root["iceCandidatePoolSize"]["hasValue"] = true;
        root["iceCandidatePoolSize"]["value"] = _config.ice_candidate_pool_size;

        root["bundlePolicy"] = Json::Value(Json::objectValue);
        root["bundlePolicy"]["hasValue"] = true;
        root["bundlePolicy"]["value"] = _config.bundle_policy;

        Json::StreamWriterBuilder builder;
        return Json::writeString(builder, root);
    }

    void
    PeerConnectionObject::CreateOffer(const RTCOfferAnswerOptions& options, CreateSessionDescriptionObserver* observer)
    {
        webrtc::PeerConnectionInterface::RTCOfferAnswerOptions _options;
        _options.ice_restart = options.iceRestart;
        _options.voice_activity_detection = options.voiceActivityDetection;
        connection->CreateOffer(observer, _options);
    }

    void
    PeerConnectionObject::CreateAnswer(const RTCOfferAnswerOptions& options, CreateSessionDescriptionObserver* observer)
    {
        webrtc::PeerConnectionInterface::RTCOfferAnswerOptions _options;
        _options.ice_restart = options.iceRestart;
        _options.voice_activity_detection = options.voiceActivityDetection;
        connection->CreateAnswer(observer, _options);
    }

    void PeerConnectionObject::ReceiveStatsReport(const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report)
    {
        context.AddStatsReport(report);
    }

    bool PeerConnectionObject::GetSessionDescription(
        const webrtc::SessionDescriptionInterface* sdp, RTCSessionDescription& desc) const
    {
        if (sdp == nullptr)
        {
            return false;
        }

        std::string out;
        sdp->ToString(&out);

        desc.type = ConvertSdpType(sdp->GetType());
        desc.sdp = static_cast<char*>(CoTaskMemAlloc(out.size() + 1));
        out.copy(desc.sdp, out.size());
        desc.sdp[out.size()] = '\0';
        return true;
    }
} // end namespace webrtc
} // end namespace unity
