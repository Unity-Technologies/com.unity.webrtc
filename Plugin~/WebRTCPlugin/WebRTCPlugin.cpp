#include "pch.h"
#include "WebRTCPlugin.h"
#include "PeerConnectionObject.h"
#include "MediaStreamObserver.h"
#include "Context.h"
#include "Codec/EncoderFactory.h"

using namespace WebRTC;
namespace WebRTC
{
    DelegateDebugLog delegateDebugLog = nullptr;
    DelegateSetResolution delegateSetResolution = nullptr;

    void debugLog(const char* buf)
    {
        if (delegateDebugLog != nullptr)
        {
            if(rtc::ThreadManager::Instance()->IsMainThread())
            {
                delegateDebugLog(buf);
            }
        }
    }

    void SetResolution(int32* width, int32* length)
    {
        if (delegateSetResolution != nullptr)
        {
            delegateSetResolution(width, length);
        }
    }
}

extern "C"
{
    UNITY_INTERFACE_EXPORT bool GetHardwareEncoderSupport()
    {
        return EncoderFactory::GetHardwareEncoderSupport();
    }

    UNITY_INTERFACE_EXPORT UnityEncoderType ContextGetEncoderType(Context* context)
    {
        return context->GetEncoderType();
    }

    UNITY_INTERFACE_EXPORT webrtc::MediaStreamInterface* ContextCreateMediaStream(Context* context, const char* streamId)
    {
        return context->CreateMediaStream(streamId);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteMediaStream(Context* context, webrtc::MediaStreamInterface* stream)
    {
        context->DeleteMediaStream(stream);
    }

    UNITY_INTERFACE_EXPORT webrtc::MediaStreamTrackInterface* ContextCreateVideoTrack(Context* context, const char* label, void* rt, int32 width, int32 height, int32 bitRate)
    {
        return context->CreateVideoTrack(label, rt, width, height, bitRate);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteMediaStreamTrack(Context* context, webrtc::MediaStreamTrackInterface* track)
    {
        context->DeleteMediaStreamTrack(track);
    }

    UNITY_INTERFACE_EXPORT void ContextStopMediaStreamTrack(Context* context, webrtc::MediaStreamTrackInterface* track)
    {
        context->StopMediaStreamTrack(track);
    }

    UNITY_INTERFACE_EXPORT webrtc::MediaStreamTrackInterface* ContextCreateAudioTrack(Context* context, const char* label)
    {
        return context->CreateAudioTrack(label);
    }

    UNITY_INTERFACE_EXPORT bool MediaStreamAddTrack(webrtc::MediaStreamInterface* stream, webrtc::MediaStreamTrackInterface* track)
    {
        if (track->kind() == "audio")
        {
            return stream->AddTrack(static_cast<webrtc::AudioTrackInterface*>(track));
        }
        else
        {
            return stream->AddTrack(static_cast<webrtc::VideoTrackInterface*>(track));
        }
    }
    UNITY_INTERFACE_EXPORT bool MediaStreamRemoveTrack(webrtc::MediaStreamInterface* stream, webrtc::MediaStreamTrackInterface* track)
    {
        if (track->kind() == "audio")
        {
            return stream->RemoveTrack(static_cast<webrtc::AudioTrackInterface*>(track));
        }
        else
        {
            return stream->RemoveTrack(static_cast<webrtc::VideoTrackInterface*>(track));
        }
    }

    UNITY_INTERFACE_EXPORT char* MediaStreamGetID(webrtc::MediaStreamInterface* stream)
    {
        const auto idStr = stream->id();
        const auto id = static_cast<char*>(CoTaskMemAlloc(idStr.size() + sizeof(char)));
        idStr.copy(id, idStr.size());
        id[idStr.size()] = '\0';
        return id;
    }

    UNITY_INTERFACE_EXPORT void MediaStreamRegisterOnAddTrack(Context* context, webrtc::MediaStreamInterface* stream, DelegateMediaStreamOnAddTrack callback)
    {
        context->GetObserver(stream)->RegisterOnAddTrack(callback);
    }

    UNITY_INTERFACE_EXPORT void MediaStreamRegisterOnRemoveTrack(Context* context, webrtc::MediaStreamInterface* stream, DelegateMediaStreamOnRemoveTrack callback)
    {
        context->GetObserver(stream)->RegisterOnRemoveTrack(callback);
    }

    UNITY_INTERFACE_EXPORT webrtc::MediaStreamTrackInterface** MediaStreamGetVideoTracks(webrtc::MediaStreamInterface* stream, int* length)
    {
        auto tracksVector = stream->GetVideoTracks();
#pragma warning(suppress: 4267)
        *length = tracksVector.size();
        const auto buf = CoTaskMemAlloc(sizeof(webrtc::MediaStreamTrackInterface*) * tracksVector.size());
        const auto tracks = static_cast<webrtc::MediaStreamTrackInterface**>(buf);
        for (uint32_t i = 0; i < tracksVector.size(); i++)
        {
            tracks[i] = tracksVector[i].get();
        }
        return tracks;
    }

    UNITY_INTERFACE_EXPORT webrtc::MediaStreamTrackInterface** MediaStreamGetAudioTracks(webrtc::MediaStreamInterface* stream, int* length)
    {
        auto tracksVector = stream->GetAudioTracks();
#pragma warning(suppress: 4267)
        *length = tracksVector.size();
        const auto buf = CoTaskMemAlloc(sizeof(webrtc::MediaStreamTrackInterface*) * tracksVector.size());
        const auto tracks = static_cast<webrtc::MediaStreamTrackInterface**>(buf);
        for (uint32_t i = 0; i < tracksVector.size(); i++)
        {
            tracks[i] = tracksVector[i].get();
        }
        return tracks;
    }

    UNITY_INTERFACE_EXPORT TrackKind MediaStreamTrackGetKind(webrtc::MediaStreamTrackInterface* track)
    {
        const auto kindStr = track->kind();
        if (kindStr == "audio")
        {
            return TrackKind::Audio;
        }
        else
        {
            return TrackKind::Video;
        }
    }

    UNITY_INTERFACE_EXPORT webrtc::MediaStreamTrackInterface::TrackState MediaStreamTrackGetReadyState(webrtc::MediaStreamTrackInterface* track)
    {
        return track->state();
    }

    UNITY_INTERFACE_EXPORT char* MediaStreamTrackGetID(webrtc::MediaStreamTrackInterface* track)
    {
        const auto idStr = track->id();
        const auto id = static_cast<char*>(CoTaskMemAlloc(idStr.size() + sizeof(char)));
        idStr.copy(id, idStr.size());
        id[idStr.size()] = '\0';
        return id;
    }

    UNITY_INTERFACE_EXPORT bool MediaStreamTrackGetEnabled(webrtc::MediaStreamTrackInterface* track)
    {
        return track->enabled();
    }

    UNITY_INTERFACE_EXPORT void MediaStreamTrackSetEnabled(webrtc::MediaStreamTrackInterface* track, bool enabled)
    {
        track->set_enabled(enabled);
    }

    UNITY_INTERFACE_EXPORT void RegisterDebugLog(DelegateDebugLog func)
    {
        delegateDebugLog = func;
    }

    UNITY_INTERFACE_EXPORT void RegisterSetResolution(DelegateSetResolution func)
    {
        delegateSetResolution = func;
    }

    UNITY_INTERFACE_EXPORT Context* ContextCreate(int uid, UnityEncoderType encoderType)
    {
        auto ctx = ContextManager::GetInstance()->GetContext(uid);
        if (ctx != nullptr)
        {
            DebugLog("Already created context with ID %d", uid);
            return ctx;
        }
        ctx = ContextManager::GetInstance()->CreateContext(uid, encoderType);
        return ctx;
    }

    UNITY_INTERFACE_EXPORT void ContextDestroy(int uid)
    {
        ContextManager::GetInstance()->DestroyContext(uid);
    }

    UNITY_INTERFACE_EXPORT PeerConnectionObject* ContextCreatePeerConnection(Context* ctx)
    {
        return ctx->CreatePeerConnection();
    }

    UNITY_INTERFACE_EXPORT PeerConnectionObject* ContextCreatePeerConnectionWithConfig(Context* ctx, const char* conf)
    {
        return ctx->CreatePeerConnection(conf);
    }
    UNITY_INTERFACE_EXPORT void ContextDeletePeerConnection(Context* ctx, PeerConnectionObject* ptr)
    {
        ctx->DeletePeerConnection(ptr);
    }
    UNITY_INTERFACE_EXPORT void PeerConnectionClose(PeerConnectionObject* obj)
    {
        obj->Close();
    }

    UNITY_INTERFACE_EXPORT webrtc::RtpSenderInterface* PeerConnectionAddTrack(PeerConnectionObject* obj, webrtc::MediaStreamTrackInterface* track, webrtc::MediaStreamInterface* stream)
    {
        return obj->connection->AddTrack(rtc::scoped_refptr <webrtc::MediaStreamTrackInterface>(track), {stream->id()}).value().get();
    }

    UNITY_INTERFACE_EXPORT webrtc::RtpTransceiverInterface* PeerConnectionAddTransceiver(PeerConnectionObject* obj, webrtc::MediaStreamTrackInterface* track, webrtc::RtpTransceiverInit* init)
    {
        return obj->connection->AddTransceiver(track, *init).value().get();
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRemoveTrack(PeerConnectionObject* obj, webrtc::RtpSenderInterface* sender)
    {
        obj->connection->RemoveTrack(sender);
    }

    UNITY_INTERFACE_EXPORT webrtc::RTCErrorType PeerConnectionSetConfiguration(PeerConnectionObject* obj, const char* conf)
    {
        return obj->SetConfiguration(std::string(conf)); 
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionGetConfiguration(PeerConnectionObject* obj, char** conf, int* len)
    {
        std::string _conf;
        obj->GetConfiguration(_conf);
#pragma warning(suppress: 4267)
        *len = _conf.size();
        *conf = static_cast<char*>(::CoTaskMemAlloc(_conf.size() + sizeof(char)));
        _conf.copy(*conf, _conf.size());
        (*conf)[_conf.size()] = '\0';
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionSetRemoteDescription(PeerConnectionObject* obj, const RTCSessionDescription* desc)
    {
        obj->SetRemoteDescription(*desc);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionSetLocalDescription(PeerConnectionObject* obj, const RTCSessionDescription* desc)
    {
        obj->SetLocalDescription(*desc);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionCollectStats(PeerConnectionObject* obj)
    {
        obj->CollectStats();
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionGetLocalDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        obj->GetLocalDescription(*desc);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionCreateOffer(PeerConnectionObject* obj, const RTCOfferOptions* options)
    {
        obj->CreateOffer(*options);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionCreateAnswer(PeerConnectionObject* obj, const RTCAnswerOptions* options)
    {
        obj->CreateAnswer(*options);
    }

    UNITY_INTERFACE_EXPORT DataChannelObject* ContextCreateDataChannel(Context* ctx, PeerConnectionObject* obj, const char* label, const RTCDataChannelInit* options)
    {
        return ctx->CreateDataChannel(obj, label, *options);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteDataChannel(Context* ctx, DataChannelObject* channel)
    {
        ctx->DeleteDataChannel(channel);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterIceConnectionChange(PeerConnectionObject* obj, DelegateOnIceConnectionChange callback)
    {
        obj->RegisterIceConnectionChange(callback);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterOnIceCandidate(PeerConnectionObject*obj, DelegateIceCandidate callback)
    {
        obj->RegisterIceCandidate(callback);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterCallbackCollectStats(PeerConnectionObject* obj, DelegateCollectStats onGetStats)
    {
        obj->RegisterCallbackCollectStats(onGetStats);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterCallbackCreateSD(PeerConnectionObject* obj, DelegateCreateSDSuccess onSuccess, DelegateCreateSDFailure onFailure)
    {
        obj->RegisterCallbackCreateSD(onSuccess, onFailure);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterCallbackSetSD(PeerConnectionObject* obj, DelegateSetSDSuccess onSuccess, DelegateSetSDFailure onFailure)
    {
        obj->RegisterCallbackSetSD(onSuccess, onFailure);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionAddIceCandidate(PeerConnectionObject* obj, const RTCIceCandidate* candidate)
    {
        return obj->AddIceCandidate(*candidate);
    }

    UNITY_INTERFACE_EXPORT RTCPeerConnectionState PeerConnectionState(PeerConnectionObject* obj)
    {
        return obj->GetConnectionState();
    }

    UNITY_INTERFACE_EXPORT RTCIceConnectionState PeerConnectionIceConditionState(PeerConnectionObject* obj)
    {
        return obj->GetIceCandidateState();
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterOnDataChannel(PeerConnectionObject* obj, DelegateOnDataChannel callback)
    {
        obj->RegisterOnDataChannel(callback);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterOnRenegotiationNeeded(PeerConnectionObject* obj, DelegateOnRenegotiationNeeded callback)
    {
        obj->RegisterOnRenegotiationNeeded(callback);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterOnTrack(PeerConnectionObject* obj, DelegateOnTrack callback)
    {
        obj->RegisterOnTrack(callback);
    }
    UNITY_INTERFACE_EXPORT webrtc::MediaStreamTrackInterface* TransceiverGetTrack(webrtc::RtpTransceiverInterface* obj)
    {
        return obj->receiver()->track().get();
    }

    UNITY_INTERFACE_EXPORT webrtc::RtpTransceiverDirection TransceiverGetCurentDirection(webrtc::RtpTransceiverInterface* obj)
    {
        return obj->current_direction().value();
    }

    UNITY_INTERFACE_EXPORT webrtc::RtpReceiverInterface* TransceiverGetReceiver(webrtc::RtpTransceiverInterface* obj)
    {
        return obj->receiver().get();
    }

    UNITY_INTERFACE_EXPORT webrtc::RtpSenderInterface* TransceiverGetSender(webrtc::RtpTransceiverInterface* obj)
    {
        return obj->sender().get();
    }
    UNITY_INTERFACE_EXPORT int DataChannelGetID(DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->GetID();
    }

    UNITY_INTERFACE_EXPORT char* DataChannelGetLabel(DataChannelObject* dataChannelObj)
    {
        std::string tmp = dataChannelObj->GetLabel();
        auto label = static_cast<char*>(CoTaskMemAlloc(tmp.size() + sizeof(char)));
        tmp.copy(label, tmp.size());
        label[tmp.size()] = '\0';
        return label;
    }

    UNITY_INTERFACE_EXPORT void DataChannelSend(DataChannelObject* dataChannelObj, const char* msg)
    {
        dataChannelObj->Send(msg);
    }

    UNITY_INTERFACE_EXPORT void DataChannelSendBinary(DataChannelObject* dataChannelObj, const byte* msg, int len)
    {
        dataChannelObj->Send(msg, len);
    }

    UNITY_INTERFACE_EXPORT void DataChannelClose(DataChannelObject* dataChannelObj)
    {
        dataChannelObj->Close();
    }

    UNITY_INTERFACE_EXPORT void DataChannelRegisterOnMessage(DataChannelObject* dataChannelObj, DelegateOnMessage callback)
    {
        dataChannelObj->RegisterOnMessage(callback);
    }

    UNITY_INTERFACE_EXPORT void DataChannelRegisterOnOpen(DataChannelObject* dataChannelObj, DelegateOnOpen callback)
    {
        dataChannelObj->RegisterOnOpen(callback);
    }

    UNITY_INTERFACE_EXPORT void DataChannelRegisterOnClose(DataChannelObject* dataChannelObj, DelegateOnClose callback)
    {
        dataChannelObj->RegisterOnClose(callback);
    }

    UNITY_INTERFACE_EXPORT void SetCurrentContext(Context* context)
    {
        ContextManager::GetInstance()->curContext = context;
    }

    UNITY_INTERFACE_EXPORT void ProcessAudio(float* data, int32 size)
    {
        if (ContextManager::GetInstance()->curContext)
        {
            ContextManager::GetInstance()->curContext->ProcessAudioData(data, size);
        }
    }
}



