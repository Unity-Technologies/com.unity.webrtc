#include "pch.h"
#include "Context.h"
#include "PeerConnectionObject.h"

namespace WebRTC
{
    PeerConnectionObject::PeerConnectionObject(Context& context) : context(context) {}

    PeerConnectionObject::~PeerConnectionObject()
    {
        if (connection == nullptr)
        {
            return;
        }
        auto senders = connection->GetSenders();
        for (auto sender : senders)
        {
            connection->RemoveTrack(sender);
        }

        auto state = connection->peer_connection_state();
        if (state != webrtc::PeerConnectionInterface::PeerConnectionState::kClosed)
        {
            connection->Close();
        }
        connection.release();
    }

    PeerConnectionObject* Context::CreatePeerConnection()
    {
        rtc::scoped_refptr<PeerConnectionObject> obj = new rtc::RefCountedObject<PeerConnectionObject>(*this);
        webrtc::PeerConnectionInterface::RTCConfiguration _config;
        _config.sdp_semantics = webrtc::SdpSemantics::kUnifiedPlan;
        obj->connection = peerConnectionFactory->CreatePeerConnection(_config, nullptr, nullptr, obj);

        if (obj->connection == nullptr)
        {
            return nullptr;
        }
        auto ptr = obj.get();
        clients[ptr] = std::move(obj);
        return clients[ptr].get();
    }

    PeerConnectionObject* Context::CreatePeerConnection(const std::string& conf)
    {
        rtc::scoped_refptr<PeerConnectionObject> obj = new rtc::RefCountedObject<PeerConnectionObject>(*this);
        webrtc::PeerConnectionInterface::RTCConfiguration _config;
        Convert(conf, _config);
        obj->connection = peerConnectionFactory->CreatePeerConnection(_config, nullptr, nullptr, obj);
        if (obj->connection == nullptr)
        {
            return nullptr;
        }
        auto ptr = obj.get();
        clients[ptr] = std::move(obj);
        return clients[ptr].get();
    }

    void PeerConnectionObject::OnSuccess(webrtc::SessionDescriptionInterface* desc)
    {
        std::string out;
        desc->ToString(&out);
        auto type = ConvertSdpType(desc->GetType());
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
            onCreateSDFailure(this);
        }
    }

    void PeerConnectionObject::OnDataChannel(rtc::scoped_refptr<webrtc::DataChannelInterface> channel)
    {
        auto obj = std::make_unique<DataChannelObject>(channel, *this);
        auto ptr = obj.get();
        context.dataChannels[ptr] = std::move(obj);
        if (onDataChannel != nullptr)
        {
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
        if (onTrack != nullptr)
        {
            onTrack(this, transceiver.get());
        }
    }
    // Called any time the IceConnectionState changes.
    void PeerConnectionObject::OnIceConnectionChange(webrtc::PeerConnectionInterface::IceConnectionState new_state)
    {
        if (onIceConnectionChange != nullptr)
        {
            onIceConnectionChange(this, new_state);
        }
    }
    // Called any time the IceGatheringState changes.
    void PeerConnectionObject::OnIceGatheringChange(webrtc::PeerConnectionInterface::IceGatheringState new_state)
    {
        DebugLog("OnIceGatheringChange");
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
        if (connection != nullptr)
        {
            connection->Close();

            //Cleanup delegates/callbacks
            onCreateSDSuccess = nullptr;
            onCreateSDFailure = nullptr;
            onSetSDSuccess = nullptr;
            onSetSDFailure = nullptr;
            onLocalSdpReady = nullptr;
            onIceCandidate = nullptr;
            onIceConnectionChange = nullptr;
            onDataChannel = nullptr;
            onRenegotiationNeeded = nullptr;
            onTrack = nullptr;
        }
    }

    void PeerConnectionObject::SetLocalDescription(const RTCSessionDescription& desc)
    {
        webrtc::SdpParseError error;
        auto _desc = webrtc::CreateSessionDescription(ConvertSdpType(desc.type), desc.sdp, &error);
        if (!_desc.get())
        {
            DebugLog("Can't parse received session description message.");
            DebugLog("SdpParseError:\n%s", error.description);
            return;
        }
        auto observer = PeerSDPObserver::Create(this);
        connection->SetLocalDescription(observer, _desc.release());
    }

    void PeerConnectionObject::SetRemoteDescription(const RTCSessionDescription& desc)
    {
        webrtc::SdpParseError error;
        auto _desc = webrtc::CreateSessionDescription(ConvertSdpType(desc.type), desc.sdp, &error);
        if (!_desc.get())
        {
            DebugLog("Can't parse received session description message.");
            DebugLog("SdpParseError:\n%s", error.description);
            return;
        }
        auto observer = PeerSDPObserver::Create(this);
        connection->SetRemoteDescription(observer, _desc.release());
    }

    webrtc::RTCErrorType PeerConnectionObject::SetConfiguration(const std::string& config)
    {
        webrtc::PeerConnectionInterface::RTCConfiguration _config;
        Convert(config, _config);

        webrtc::RTCError error;
        if (!connection->SetConfiguration(_config, &error))
        {
            LogPrint(error.message());
        }
        return error.type();
    }

    void PeerConnectionObject::GetConfiguration(std::string& config) const
    {
        auto _config = connection->GetConfiguration();

        Json::Value root;
        root["iceServers"] = Json::Value(Json::arrayValue);
        for (auto iceServer : _config.servers)
        {
            Json::Value jsonIceServer = Json::Value(Json::objectValue);
            jsonIceServer["username"] = iceServer.username;
            jsonIceServer["credential"] = iceServer.password;
            jsonIceServer["credentialType"] = (int)RTCIceCredentialType::Password;
            jsonIceServer["urls"] = Json::Value(Json::arrayValue);
            for (auto url : iceServer.urls)
            {
                jsonIceServer["urls"].append(url);
            }
            root["iceServers"].append(jsonIceServer);
        }
        Json::FastWriter writer;
        config = writer.write(root);
    }

    void PeerConnectionObject::CreateOffer(const RTCOfferOptions & options)
    {
        webrtc::PeerConnectionInterface::RTCOfferAnswerOptions _options;
        _options.ice_restart = options.iceRestart;
        _options.offer_to_receive_audio = options.offerToReceiveAudio;
        _options.offer_to_receive_video = options.offerToReceiveVideo;
        connection->CreateOffer(this, _options);
    }

    void PeerConnectionObject::CreateAnswer(const RTCAnswerOptions& options)
    {
        webrtc::PeerConnectionInterface::RTCOfferAnswerOptions _options;
        _options.ice_restart = options.iceRestart;
        connection->CreateAnswer(this, _options);
    }

    void PeerConnectionObject::AddIceCandidate(const RTCIceCandidate& candidate)
    {
        webrtc::SdpParseError error;
        std::unique_ptr<webrtc::IceCandidateInterface> _candidate(
            webrtc::CreateIceCandidate(candidate.sdpMid, candidate.sdpMLineIndex, candidate.candidate, &error));
        connection->AddIceCandidate(_candidate.get());
    }

    void PeerConnectionObject::GetLocalDescription(RTCSessionDescription& desc) const
    {
        std::string out;
        auto current = connection->current_local_description();
        current->ToString(&out);

        desc.type = ConvertSdpType(current->GetType());
        //TODO: Linux compatibility 
        desc.sdp = (char*)CoTaskMemAlloc(out.size() + 1);
        out.copy(desc.sdp, out.size());
        desc.sdp[out.size()] = '\0';
    }

#pragma warning(push)
#pragma warning(disable: 4715)
    RTCIceConnectionState PeerConnectionObject::GetIceCandidateState()
    {
        auto state = connection->ice_connection_state();
        switch (state)
        {
        case webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionNew:
            return RTCIceConnectionState::New;
        case webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionChecking:
            return RTCIceConnectionState::Checking;
        case webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionConnected:
            return RTCIceConnectionState::Connected;
        case webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionCompleted:
            return RTCIceConnectionState::Completed;
        case webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionFailed:
            return RTCIceConnectionState::Failed;
        case webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionDisconnected:
            return RTCIceConnectionState::Disconnected;
        case webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionClosed:
            return RTCIceConnectionState::Closed;
        case webrtc::PeerConnectionInterface::IceConnectionState::kIceConnectionMax:
            return RTCIceConnectionState::Max;
        }
    }

    RTCPeerConnectionState PeerConnectionObject::GetConnectionState()
    {
        auto state = connection->peer_connection_state();
        switch (state)
        {
        case webrtc::PeerConnectionInterface::PeerConnectionState::kClosed:
            return RTCPeerConnectionState::Closed;
        case webrtc::PeerConnectionInterface::PeerConnectionState::kConnected:
            return RTCPeerConnectionState::Connected;
        case webrtc::PeerConnectionInterface::PeerConnectionState::kConnecting:
            return RTCPeerConnectionState::Connecting;
        case webrtc::PeerConnectionInterface::PeerConnectionState::kDisconnected:
            return RTCPeerConnectionState::Disconnected;
        case webrtc::PeerConnectionInterface::PeerConnectionState::kFailed:
            return RTCPeerConnectionState::Failed;
        case webrtc::PeerConnectionInterface::PeerConnectionState::kNew:
            return RTCPeerConnectionState::New;
        }
    }
#pragma warning(pop)
}

