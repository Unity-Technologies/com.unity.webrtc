#include "pch.h"
#include "Context.h"
#include "PeerConnectionObject.h"
#include "SetSessionDescriptionObserver.h"

namespace unity
{
namespace webrtc
{

    PeerConnectionObject::PeerConnectionObject(Context& context) : context(context)
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
            connection->RemoveTrack(sender);
        }

        const auto state = connection->peer_connection_state();
        if (state != webrtc::PeerConnectionInterface::PeerConnectionState::kClosed)
        {
            connection->Close();
        }
        connection = nullptr;
    }

    void PeerConnectionObject::OnSuccess(webrtc::SessionDescriptionInterface* desc)
    {
        std::string out;
        desc->ToString(&out);
        const auto type = ConvertSdpType(desc->GetType());
        if (onCreateSDSuccess != nullptr)
        {
            onCreateSDSuccess(this, type, out.c_str());
        }
    }

    void PeerConnectionObject::OnFailure(webrtc::RTCError error)
    {
        //::TODO
        //RTCError _error = { RTCErrorDetailType::IdpTimeout };
        if (onCreateSDFailure != nullptr)
        {
            onCreateSDFailure(this, error.type(), error.message());
        }
    }

    void PeerConnectionObject::OnDataChannel(rtc::scoped_refptr<webrtc::DataChannelInterface> channel) {
        auto obj = std::make_unique<DataChannelObject>(channel, *this);
        const auto ptr = obj.get();
        context.AddDataChannel(std::move(obj));
        if (onDataChannel != nullptr) {
            onDataChannel(this, ptr);
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
        if(onConnectionStateChange != nullptr)
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
        if (connection != nullptr && connection->peer_connection_state() != webrtc::PeerConnectionInterface::PeerConnectionState::kClosed)
        {
            //Cleanup delegates/callbacks
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
        const RTCSessionDescription& desc, webrtc::SetSessionDescriptionObserver* observer, std::string& error)
    {
        SdpParseError error_;
        std::unique_ptr<SessionDescriptionInterface> _desc =
            CreateSessionDescription(ConvertSdpType(desc.type), desc.sdp, &error_);
        if (!_desc.get())
        {
            DebugLog("Can't parse received session description message.");
            DebugLog("SdpParseError:\n%s", error_.description.c_str());

            error = error_.description;
            return RTCErrorType::SYNTAX_ERROR;
        }
        connection->SetLocalDescription(observer, _desc.release());
        return RTCErrorType::NONE;
    }

    RTCErrorType PeerConnectionObject::SetLocalDescriptionWithoutDescription(webrtc::SetSessionDescriptionObserver* observer, std::string& error)
    {
        connection->SetLocalDescription(observer);
        return RTCErrorType::NONE;
    }

    RTCErrorType PeerConnectionObject::SetRemoteDescription(
        const RTCSessionDescription& desc, webrtc::SetSessionDescriptionObserver* observer, std::string& error)
    {
        SdpParseError error_;
        std::unique_ptr<SessionDescriptionInterface> _desc =
            CreateSessionDescription(ConvertSdpType(desc.type), desc.sdp, &error_);
        if (!_desc.get())
        {
            DebugLog("Can't parse received session description message.");
            DebugLog("SdpParseError:\n%s", error_.description.c_str());

            error = error_.description;
            return RTCErrorType::SYNTAX_ERROR;
        }
        connection->SetRemoteDescription(observer, _desc.release());
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
            LogPrint(error.message());
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

        root["enableDtlsSrtp"] = Json::Value(Json::objectValue);
        root["enableDtlsSrtp"]["hasValue"] = _config.enable_dtls_srtp.has_value();
        root["enableDtlsSrtp"]["value"] = _config.enable_dtls_srtp.value_or(false);

        root["iceCandidatePoolSize"] = Json::Value(Json::objectValue);
        root["iceCandidatePoolSize"]["hasValue"] = true;
        root["iceCandidatePoolSize"]["value"] = _config.ice_candidate_pool_size;
            
        root["bundlePolicy"] = Json::Value(Json::objectValue);
        root["bundlePolicy"]["hasValue"] = true;
        root["bundlePolicy"]["value"] = _config.bundle_policy;

        Json::StreamWriterBuilder builder;
        return Json::writeString(builder, root);
    }

    void PeerConnectionObject::CreateOffer(const RTCOfferAnswerOptions & options)
    {
        webrtc::PeerConnectionInterface::RTCOfferAnswerOptions _options;
        _options.ice_restart = options.iceRestart;
        _options.voice_activity_detection = options.voiceActivityDetection;
        connection->CreateOffer(this, _options);
    }

    void PeerConnectionObject::CreateAnswer(const RTCOfferAnswerOptions& options)
    {
        webrtc::PeerConnectionInterface::RTCOfferAnswerOptions _options;
        _options.ice_restart = options.iceRestart;
        _options.voice_activity_detection = options.voiceActivityDetection;
        connection->CreateAnswer(this, _options);
    }

    void PeerConnectionObject::ReceiveStatsReport(const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report)
    {
        context.AddStatsReport(report);
    }

    bool PeerConnectionObject::GetSessionDescription(const webrtc::SessionDescriptionInterface* sdp, RTCSessionDescription& desc) const
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
