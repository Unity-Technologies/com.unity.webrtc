#include "pch.h"
#include "WebRTCPlugin.h"
#include "PeerConnectionObject.h"
#include "MediaStreamObserver.h"
#include "SetSessionDescriptionObserver.h"
#include "Context.h"
#include "Codec/EncoderFactory.h"

namespace unity
{
namespace webrtc
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
    
} // end namespace webrtc
} // end namespace unity

using namespace unity::webrtc;
using namespace ::webrtc;

template<class T>
T** ConvertPtrArrayFromRefPtrArray(std::vector<rtc::scoped_refptr<T>> vec, size_t* length)
{
    *length = vec.size();
    const auto buf = CoTaskMemAlloc(sizeof(T*) * vec.size());
    const auto ret = static_cast<T**>(buf);
    std::copy(vec.begin(), vec.end(), ret);
    return ret;
}

template<typename T>
T* ConvertArray(std::vector<T> vec, size_t* length)
{
    *length = vec.size();
    size_t size = sizeof(T*) * vec.size();
    auto dst = CoTaskMemAlloc(size);
    auto src = vec.data();
    std::memcpy(dst, src, size);
    return static_cast<T*>(dst);
}

///
/// avoid compile erorr for vector<bool>
/// https://en.cppreference.com/w/cpp/container/vector_bool
bool* ConvertArray(std::vector<bool> vec, size_t* length)
{
    *length = vec.size();
    size_t size = sizeof(bool*) * vec.size();
    auto dst = CoTaskMemAlloc(size);
    bool* ret = static_cast<bool*>(dst);
    for (size_t i = 0; i < vec.size(); i++)
    {
        ret[i] = vec[i];
    }
    return ret;
}

char* ConvertString(const std::string str)
{
    const size_t size = str.size();
    char* ret = static_cast<char*>(CoTaskMemAlloc(size + sizeof(char)));
    str.copy(ret, size);
    ret[size] = '\0';
    return ret;
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

    UNITY_INTERFACE_EXPORT CodecInitializationResult GetInitializationResult(Context* context, MediaStreamTrackInterface* track)
    {
        return context->GetInitializationResult(track);
    }

    UNITY_INTERFACE_EXPORT void ContextSetVideoEncoderParameter(Context* context, MediaStreamTrackInterface* track, int width, int height, UnityEncoderType type)
    {
        context->SetEncoderParameter(track, width, height);
    }

    UNITY_INTERFACE_EXPORT MediaStreamInterface* ContextCreateMediaStream(Context* context, const char* streamId)
    {
        return context->CreateMediaStream(streamId);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteMediaStream(Context* context, MediaStreamInterface* stream)
    {
        context->DeleteMediaStream(stream);
    }

    UNITY_INTERFACE_EXPORT::webrtc::MediaStreamTrackInterface* ContextCreateVideoTrack(Context* context, const char* label, void* rt, int32 width, int32 height)
    {
        return context->CreateVideoTrack(label, rt);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteMediaStreamTrack(Context* context, ::webrtc::MediaStreamTrackInterface* track)
    {
        context->DeleteMediaStreamTrack(track);
    }

    UNITY_INTERFACE_EXPORT void ContextStopMediaStreamTrack(Context* context, ::webrtc::MediaStreamTrackInterface* track)
    {
        context->StopMediaStreamTrack(track);
    }

    UNITY_INTERFACE_EXPORT::webrtc::MediaStreamTrackInterface* ContextCreateAudioTrack(Context* context, const char* label)
    {
        return context->CreateAudioTrack(label);
    }

    UNITY_INTERFACE_EXPORT bool MediaStreamAddTrack(MediaStreamInterface* stream, MediaStreamTrackInterface* track)
    {
        if (track->kind() == "audio")
        {
            return stream->AddTrack(static_cast<AudioTrackInterface*>(track));
        }
        else
        {
            return stream->AddTrack(static_cast<VideoTrackInterface*>(track));
        }
    }
    UNITY_INTERFACE_EXPORT bool MediaStreamRemoveTrack(MediaStreamInterface* stream, MediaStreamTrackInterface* track)
    {
        if (track->kind() == "audio")
        {
            return stream->RemoveTrack(static_cast<AudioTrackInterface*>(track));
        }
        else
        {
            return stream->RemoveTrack(static_cast<VideoTrackInterface*>(track));
        }
    }

    UNITY_INTERFACE_EXPORT char* MediaStreamGetID(MediaStreamInterface* stream)
    {
        return ConvertString(stream->id());
    }

    UNITY_INTERFACE_EXPORT void MediaStreamRegisterOnAddTrack(Context* context, MediaStreamInterface* stream, DelegateMediaStreamOnAddTrack callback)
    {
        context->GetObserver(stream)->RegisterOnAddTrack(callback);
    }

    UNITY_INTERFACE_EXPORT void MediaStreamRegisterOnRemoveTrack(Context* context, MediaStreamInterface* stream, DelegateMediaStreamOnRemoveTrack callback)
    {
        context->GetObserver(stream)->RegisterOnRemoveTrack(callback);
    }

    UNITY_INTERFACE_EXPORT VideoTrackInterface** MediaStreamGetVideoTracks(MediaStreamInterface* stream, size_t* length)
    {
        return ConvertPtrArrayFromRefPtrArray<VideoTrackInterface>(stream->GetVideoTracks(), length);
    }

    UNITY_INTERFACE_EXPORT AudioTrackInterface** MediaStreamGetAudioTracks(MediaStreamInterface* stream, size_t* length)
    {
        return ConvertPtrArrayFromRefPtrArray<AudioTrackInterface>(stream->GetAudioTracks(), length);
    }

    UNITY_INTERFACE_EXPORT TrackKind MediaStreamTrackGetKind(MediaStreamTrackInterface* track)
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

    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface::TrackState MediaStreamTrackGetReadyState(MediaStreamTrackInterface* track)
    {
        return track->state();
    }

    UNITY_INTERFACE_EXPORT char* MediaStreamTrackGetID(MediaStreamTrackInterface* track)
    {
        return ConvertString(track->id());
    }

    UNITY_INTERFACE_EXPORT bool MediaStreamTrackGetEnabled(MediaStreamTrackInterface* track)
    {
        return track->enabled();
    }

    UNITY_INTERFACE_EXPORT void MediaStreamTrackSetEnabled(MediaStreamTrackInterface* track, bool enabled)
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

    PeerConnectionObject* _ContextCreatePeerConnection(Context* context, const PeerConnectionInterface::RTCConfiguration& config)
    {
        const auto obj = context->CreatePeerConnection(config);
        const auto observer = unity::webrtc::SetSessionDescriptionObserver::Create(obj);
        context->AddObserver(obj->connection, observer);
        return obj;
    }

    UNITY_INTERFACE_EXPORT PeerConnectionObject* ContextCreatePeerConnection(Context* context)
    {
        PeerConnectionInterface::RTCConfiguration config;
        config.sdp_semantics = SdpSemantics::kUnifiedPlan;
        return _ContextCreatePeerConnection(context, config);
    }

    UNITY_INTERFACE_EXPORT PeerConnectionObject* ContextCreatePeerConnectionWithConfig(Context* context, const char* conf)
    {
        PeerConnectionInterface::RTCConfiguration config;
        Convert(conf, config);
        return _ContextCreatePeerConnection(context, config);
    }

    UNITY_INTERFACE_EXPORT void ContextDeletePeerConnection(Context* context, PeerConnectionObject* obj)
    {
        obj->Close();
        context->RemoveObserver(obj->connection);
        context->DeletePeerConnection(obj);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionClose(PeerConnectionObject* obj)
    {
        obj->Close();
    }

    UNITY_INTERFACE_EXPORT RtpSenderInterface* PeerConnectionAddTrack(PeerConnectionObject* obj, MediaStreamTrackInterface* track, const char* streamId)
    {
        return obj->connection->AddTrack(rtc::scoped_refptr <MediaStreamTrackInterface>(track), { streamId }).value().get();
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface* PeerConnectionAddTransceiver(PeerConnectionObject* obj, MediaStreamTrackInterface* track)
    {
        return obj->connection->AddTransceiver(track).value().get();
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface* PeerConnectionAddTransceiverWithInit(PeerConnectionObject* obj, MediaStreamTrackInterface* track, RtpTransceiverInit* init)
    {
        return obj->connection->AddTransceiver(track, *init).value().get();
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRemoveTrack(PeerConnectionObject* obj, RtpSenderInterface* sender)
    {
        obj->connection->RemoveTrack(sender);
    }

    UNITY_INTERFACE_EXPORT RTCErrorType PeerConnectionSetConfiguration(PeerConnectionObject* obj, const char* conf)
    {
        return obj->SetConfiguration(std::string(conf));
    }

    UNITY_INTERFACE_EXPORT char* PeerConnectionGetConfiguration(PeerConnectionObject* obj)
    {
        const std::string str = obj->GetConfiguration();
        return ConvertString(str);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionSetRemoteDescription(Context* context, PeerConnectionObject* obj, const RTCSessionDescription* desc)
    {
        obj->SetRemoteDescription(*desc, context->GetObserver(obj->connection));
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionGetStats(PeerConnectionObject* obj)
    {
        obj->connection->GetStats(PeerConnectionStatsCollectorCallback::Create(obj));
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionSenderGetStats(PeerConnectionObject* obj, RtpSenderInterface* selector)
    {
        obj->connection->GetStats(selector, PeerConnectionStatsCollectorCallback::Create(obj));
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionReceiverGetStats(PeerConnectionObject* obj, RtpReceiverInterface* receiver)
    {
        obj->connection->GetStats(receiver, PeerConnectionStatsCollectorCallback::Create(obj));
    }


    const std::map<std::string, byte> statsTypes =
    {
        { "codec", 0 },
        { "inbound-rtp", 1 },
        { "outbound-rtp", 2 },
        { "remote-inbound-rtp", 3 },
        { "remote-outbound-rtp", 4 },
        { "media-source", 5 },
        { "csrc", 6 },
        { "peer-connection", 7 },
        { "data-channel", 8 },
        { "stream", 9 },
        { "track", 10 },
        { "transceiver", 11 },
        { "sender", 12 },
        { "receiver", 13 },
        { "transport", 14 },
        { "sctp-transport", 15 },
        { "candidate-pair", 16 },
        { "local-candidate", 17 },
        { "remote-candidate", 18 },
        { "certificate", 19 },
        { "ice-server", 20 }
    };

    UNITY_INTERFACE_EXPORT const RTCStats** StatsReportGetStatsList(const RTCStatsReport* report, size_t* length, byte** types)
    {
        const size_t size = report->size();
        *length = size;
        *types = static_cast<byte*>(CoTaskMemAlloc(sizeof(byte) * size));
        void* buf = CoTaskMemAlloc(sizeof(RTCStats*) * size);
        const RTCStats** ret = static_cast<const RTCStats**>(buf);
        if(size == 0)
        {
            return ret;
        }
        int i = 0;
        for (const auto& stats : *report)
        {
            ret[i] = &stats;
            (*types)[i] = statsTypes.at(stats.type());
            i++;
        }
        return ret;
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteStatsReport(Context* context, const RTCStatsReport* report)
    {
        context->DeleteStatsReport(report);
    }

    UNITY_INTERFACE_EXPORT const char* StatsGetJson(const RTCStats* stats)
    {
        return ConvertString(stats->ToJson());
    }

    UNITY_INTERFACE_EXPORT int64_t StatsGetTimestamp(const RTCStats* stats)
    {
        return stats->timestamp_us();
    }

    UNITY_INTERFACE_EXPORT const char* StatsGetId(const RTCStats* stats)
    {
        return ConvertString(stats->id());
    }

    UNITY_INTERFACE_EXPORT byte StatsGetType(const RTCStats* stats)
    {
        return statsTypes.at(stats->type());
    }

    UNITY_INTERFACE_EXPORT const RTCStatsMemberInterface** StatsGetMembers(const RTCStats* stats, size_t* length)
    {
        return ConvertArray(stats->Members(), length);
    }

    UNITY_INTERFACE_EXPORT bool StatsMemberIsDefined(const RTCStatsMemberInterface* member)
    {
        return member->is_defined();
    }

    UNITY_INTERFACE_EXPORT const char* StatsMemberGetName(const RTCStatsMemberInterface* member)
    {
        return ConvertString(std::string(member->name()));
    }

    UNITY_INTERFACE_EXPORT bool StatsMemberGetBool(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<::webrtc::RTCStatsMember<bool>>();
    }

    UNITY_INTERFACE_EXPORT int32_t StatsMemberGetInt(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<::webrtc::RTCStatsMember<int32_t>>();
    }

    UNITY_INTERFACE_EXPORT uint32_t StatsMemberGetUnsignedInt(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<::webrtc::RTCStatsMember<uint32_t>>();
    }

    UNITY_INTERFACE_EXPORT int64_t StatsMemberGetLong(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<::webrtc::RTCStatsMember<uint64_t>>();
    }

    UNITY_INTERFACE_EXPORT uint64_t StatsMemberGetUnsignedLong(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<::webrtc::RTCStatsMember<uint64_t>>();
    }

    UNITY_INTERFACE_EXPORT double StatsMemberGetDouble(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<::webrtc::RTCStatsMember<double>>();
    }

    UNITY_INTERFACE_EXPORT const char* StatsMemberGetString(const RTCStatsMemberInterface* member)
    {
        return ConvertString(member->ValueToString());
    }

    UNITY_INTERFACE_EXPORT bool* StatsMemberGetBoolArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<::webrtc::RTCStatsMember<std::vector<bool>>>(), length);
    }

    UNITY_INTERFACE_EXPORT int32_t* StatsMemberGetIntArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<::webrtc::RTCStatsMember<std::vector<int>>>(), length);
    }

    UNITY_INTERFACE_EXPORT uint32_t* StatsMemberGetUnsignedIntArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<::webrtc::RTCStatsMember<std::vector<uint32_t>>>(), length);
    }

    UNITY_INTERFACE_EXPORT int64_t* StatsMemberGetLongArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<::webrtc::RTCStatsMember<std::vector<int64_t>>>(), length);
    }

    UNITY_INTERFACE_EXPORT uint64_t* StatsMemberGetUnsignedLongArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<::webrtc::RTCStatsMember<std::vector<uint64_t>>>(), length);
    }

    UNITY_INTERFACE_EXPORT double* StatsMemberGetDoubleArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<::webrtc::RTCStatsMember<std::vector<double>>>(), length);
    }

    UNITY_INTERFACE_EXPORT const char** StatsMemberGetStringArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        std::vector<std::string> vec = *member->cast_to<::webrtc::RTCStatsMember<std::vector<std::string>>>();
        std::vector<const char*>  vc;
        std::transform(vec.begin(), vec.end(), std::back_inserter(vc), ConvertString);
        return ConvertArray(vc, length);
    }

    UNITY_INTERFACE_EXPORT RTCStatsMemberInterface::Type StatsMemberGetType(const RTCStatsMemberInterface* member)
    {
        return member->type();
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionSetLocalDescription(Context* context, PeerConnectionObject* obj, const RTCSessionDescription* desc)
    {
        obj->SetLocalDescription(*desc, context->GetObserver(obj->connection));
    }

    UNITY_INTERFACE_EXPORT bool PeerConnectionGetLocalDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->local_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool PeerConnectionGetRemoteDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->remote_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool PeerConnectionGetPendingLocalDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->pending_local_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool PeerConnectionGetPendingRemoteDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->pending_remote_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool PeerConnectionGetCurrentLocalDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->current_local_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool PeerConnectionGetCurrentRemoteDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->current_remote_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT RtpReceiverInterface** PeerConnectionGetReceivers(PeerConnectionObject* obj, size_t* length)
    {
        return ConvertPtrArrayFromRefPtrArray<RtpReceiverInterface>(obj->connection->GetReceivers(), length);
    }

    UNITY_INTERFACE_EXPORT RtpSenderInterface** PeerConnectionGetSenders(PeerConnectionObject* obj, size_t* length)
    {
        return ConvertPtrArrayFromRefPtrArray<RtpSenderInterface>(obj->connection->GetSenders(), length);
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface** PeerConnectionGetTransceivers(PeerConnectionObject* obj, size_t* length)
    {
        return ConvertPtrArrayFromRefPtrArray<RtpTransceiverInterface>(obj->connection->GetTransceivers(), length);
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

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterCallbackCollectStats(Context* context, DelegateCollectStats onGetStats)
    {
        PeerConnectionStatsCollectorCallback::RegisterOnGetStats(onGetStats);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterCallbackCreateSD(PeerConnectionObject* obj, DelegateCreateSDSuccess onSuccess, DelegateCreateSDFailure onFailure)
    {
        obj->RegisterCallbackCreateSD(onSuccess, onFailure);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterOnSetSessionDescSuccess(Context* context, PeerConnectionObject* obj, DelegateSetSessionDescSuccess onSuccess)
    {
        context->GetObserver(obj->connection)->RegisterDelegateOnSuccess(onSuccess);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterOnSetSessionDescFailure(Context* context, PeerConnectionObject* obj, DelegateSetSessionDescFailure onFailure)
    {
        context->GetObserver(obj->connection)->RegisterDelegateOnFailure(onFailure);
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

    UNITY_INTERFACE_EXPORT RTCSignalingState PeerConnectionSignalingState(PeerConnectionObject* obj)
    {
        return obj->GetSignalingState();
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
    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface* TransceiverGetTrack(RtpTransceiverInterface* transceiver)
    {
        return transceiver->receiver()->track().get();
    }

    UNITY_INTERFACE_EXPORT bool TransceiverGetCurrentDirection(RtpTransceiverInterface* transceiver, RtpTransceiverDirection* direction)
    {
        if (transceiver->current_direction().has_value())
        {
            *direction = transceiver->current_direction().value();
            return true;
        }
        return false;
    }

    UNITY_INTERFACE_EXPORT void TransceiverStop(RtpTransceiverInterface* transceiver)
    {
        transceiver->Stop();
    }

    UNITY_INTERFACE_EXPORT RtpReceiverInterface* TransceiverGetReceiver(RtpTransceiverInterface* transceiver)
    {
        return transceiver->receiver().get();
    }

    UNITY_INTERFACE_EXPORT RtpSenderInterface* TransceiverGetSender(RtpTransceiverInterface* transceiver)
    {
        return transceiver->sender().get();
    }

    struct RTCRtpEncodingParameters
    {
        bool active;
        bool hasValueMaxBitrate;
        uint64_t maxBitrate;
        bool hasValueMaxFramerate;
        uint32_t maxFramerate;
        bool hasValueScaleResolutionDownBy;
        double scaleResolutionDownBy;
        char* rid;
    };

    struct RTCRtpSendParameters
    {
        uint32_t encodingsLength;
        RTCRtpEncodingParameters* encodings;
        char* transactionId;
    };

    UNITY_INTERFACE_EXPORT void SenderGetParameters(RtpSenderInterface* sender, RTCRtpSendParameters** parameters)
    {
        const RtpParameters src = sender->GetParameters();
        RTCRtpSendParameters* dst = static_cast<RTCRtpSendParameters*>(CoTaskMemAlloc(sizeof(RTCRtpSendParameters)));
        dst->encodingsLength = static_cast<uint32_t>(src.encodings.size());
        dst->encodings = static_cast<RTCRtpEncodingParameters*>(CoTaskMemAlloc(sizeof(RTCRtpEncodingParameters) * src.encodings.size()));

        for(int i = 0; i < src.encodings.size(); i++)
        {
            dst->encodings[i].active = src.encodings[i].active;
            dst->encodings[i].hasValueMaxBitrate = src.encodings[i].max_bitrate_bps.has_value();
            dst->encodings[i].maxBitrate = src.encodings[i].max_bitrate_bps.value_or(0);
            dst->encodings[i].hasValueMaxFramerate = src.encodings[i].max_framerate.has_value();
            dst->encodings[i].maxFramerate = src.encodings[i].max_framerate.value_or(0);
            dst->encodings[i].hasValueScaleResolutionDownBy = src.encodings[i].scale_resolution_down_by.has_value();
            dst->encodings[i].scaleResolutionDownBy = src.encodings[i].scale_resolution_down_by.value_or(0);
            dst->encodings[i].rid = ConvertString(src.encodings[i].rid);
        }
        dst->transactionId = ConvertString(src.transaction_id);
        *parameters = dst;
    }

    UNITY_INTERFACE_EXPORT RTCErrorType SenderSetParameters(RtpSenderInterface* sender, const RTCRtpSendParameters* src)
    {
        RtpParameters dst = sender->GetParameters();

        for (int i = 0; i < dst.encodings.size(); i++)
        {
            dst.encodings[i].active = src->encodings[i].active;
            if(src->encodings[i].hasValueMaxBitrate)
                dst.encodings[i].max_bitrate_bps = static_cast<int>(src->encodings[i].maxBitrate);
            if (src->encodings[i].hasValueMaxFramerate)
                dst.encodings[i].max_framerate = static_cast<int>(src->encodings[i].maxFramerate);
            if (src->encodings[i].hasValueScaleResolutionDownBy)
                dst.encodings[i].scale_resolution_down_by = src->encodings[i].scaleResolutionDownBy;
            if(src->encodings[i].rid != nullptr)
                dst.encodings[i].rid = std::string(src->encodings[i].rid);
        }
        const ::webrtc::RTCError error = sender->SetParameters(dst);
        return error.type();
    }

    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface* SenderGetTrack(RtpSenderInterface* sender)
    {
        return sender->track();
    }

    UNITY_INTERFACE_EXPORT int DataChannelGetID(DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->GetID();
    }

    UNITY_INTERFACE_EXPORT char* DataChannelGetLabel(DataChannelObject* dataChannelObj)
    {
        return ConvertString(dataChannelObj->GetLabel());
    }

    UNITY_INTERFACE_EXPORT DataChannelInterface::DataState DataChannelGetReadyState(
        DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->GetReadyState();
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

