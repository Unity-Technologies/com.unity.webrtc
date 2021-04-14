#include "pch.h"
#include "WebRTCPlugin.h"
#include "PeerConnectionObject.h"
#include "MediaStreamObserver.h"
#include "SetSessionDescriptionObserver.h"
#include "Context.h"
#include "Codec/EncoderFactory.h"
#include "GraphicsDevice/GraphicsUtility.h"

#if defined(SUPPORT_VULKAN)
#include "GraphicsDevice/Vulkan/VulkanGraphicsDevice.h"
#endif

namespace unity
{
namespace webrtc
{
    DelegateSetResolution delegateSetResolution = nullptr;

    void SetResolution(int32* width, int32* length)
    {
        if (delegateSetResolution != nullptr)
        {
            delegateSetResolution(width, length);
        }
    }

    template<class T>
    T** ConvertPtrArrayFromRefPtrArray(
        std::vector<rtc::scoped_refptr<T>> vec, size_t* length)
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

    template<typename T>
    struct MarshallArray
    {
        int32_t length;
        T* values;

        T& operator[](int i) const
        {
            return values[i];
        }

        template<typename U>
        MarshallArray& operator=(const std::vector<U>& src)
        {
            length = static_cast<uint32_t>(src.size());
            values = static_cast<T*>(CoTaskMemAlloc(sizeof(T) * src.size()));

            for (size_t i = 0; i < src.size(); i++)
            {
                values[i] = src[i];
            }
            return *this;
        }
    };

    template<typename T, typename U>
    void ConvertArray(const MarshallArray<T>& src, std::vector<U>& dst)
    {
        dst.resize(src.length);
        for (size_t i = 0; i < dst.size(); i++)
        {
            dst[i] = src.values[i];
        }
    }

    template<typename T>
    struct Optional
    {
        bool hasValue;
        T value;

        template<typename U>
        Optional& operator=(const absl::optional<U>& src)
        {
            hasValue = src.has_value();
            if (hasValue)
            {
                value = static_cast<T>(src.value());
            }
            else
            {
                value = T();
            }
            return *this;
        }

        explicit operator const absl::optional<T>&() const
        {
            absl::optional<T> dst = absl::nullopt;
            if (hasValue)
                dst = value;
            return dst;
        }

        const T& value_or(const T& v) const
        {
            return hasValue ? value : v;
        }
    };

    template<typename T>
    absl::optional<T> ConvertOptional(const Optional<T>& value)
    {
        absl::optional<T> dst = absl::nullopt;
        if (value.hasValue)
        {
            dst = value.value;
        }
        return dst;
    }

    std::string ConvertSdp(const std::map<std::string, std::string>& map)
    {
        std::string str = "";
        for (const auto& pair : map)
        {
            if(!str.empty())
            {
                str += ";";
            }
            str += pair.first + "=" + pair.second;
        }
        return str;
    }

    std::vector<std::string> Split(const std::string& str, const std::string& delimiter)
    {
        std::vector<std::string> dst;
        std::string s = str;
        size_t pos = 0;
        while (true)
        {
            pos = s.find(delimiter);
            int length = pos;
            if(pos == std::string::npos)
                length = str.length();
            if (length == 0)
                break;
            dst.push_back(s.substr(0, length));
            if(pos == std::string::npos)
                break;
            s.erase(0, pos + delimiter.length());
        }
        return dst;
    }

    std::tuple<cricket::MediaType, std::string> ConvertMimeType(const std::string& mimeType)
    {
        const std::vector<std::string> vec = Split(mimeType, "/");
        const std::string kind = vec[0];
        const std::string name = vec[1];
        cricket::MediaType mediaType;
        if (kind == "video")
        {
            mediaType = cricket::MEDIA_TYPE_VIDEO;
        }
        else if (kind == "audio")
        {
            mediaType = cricket::MEDIA_TYPE_AUDIO;
        }
        return { mediaType, name };
    }

    std::map<std::string, std::string> ConvertSdp(const std::string& src)
    {
        std::map<std::string, std::string> map;
        std::vector<std::string> vec = Split(src, ";");

        for (const auto& str : vec)
        {
            std::vector<std::string> pair = Split(str, "=");
            map.emplace(pair[0], pair[1]);
        }
        return map;
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

} // end namespace webrtc
} // end namespace unity

using namespace unity::webrtc;
using namespace ::webrtc;

extern "C"
{
    UNITY_INTERFACE_EXPORT bool GetHardwareEncoderSupport()
    {
#if CUDA_PLATFORM
        IGraphicsDevice* device = GraphicsUtility::GetGraphicsDevice();
        if(!device->IsCudaSupport())
        {
            return false;
        }
#endif
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

    UNITY_INTERFACE_EXPORT void ContextSetVideoEncoderParameter(
        Context* context, MediaStreamTrackInterface* track, int width, int height,
        UnityRenderingExtTextureFormat textureFormat, void* textureHandle)
    {
        context->SetEncoderParameter(track, width, height, textureFormat, textureHandle);
    }

    UNITY_INTERFACE_EXPORT MediaStreamInterface* ContextCreateMediaStream(Context* context, const char* streamId)
    {
        return context->CreateMediaStream(streamId);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteMediaStream(Context* context, MediaStreamInterface* stream)
    {
        context->DeleteMediaStream(stream);
    }

    UNITY_INTERFACE_EXPORT void ContextRegisterMediaStreamObserver(Context* context, MediaStreamInterface* stream)
    {
        context->RegisterMediaStreamObserver(stream);
    }

    UNITY_INTERFACE_EXPORT void ContextUnRegisterMediaStreamObserver(Context* context, MediaStreamInterface* stream)
    {
        context->UnRegisterMediaStreamObserver(stream);
    }

    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface* ContextCreateVideoTrack(Context* context, const char* label)
    {
        return context->CreateVideoTrack(label);
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

    UNITY_INTERFACE_EXPORT UnityVideoRenderer* CreateVideoRenderer(Context* context)
    {
        return context->CreateVideoRenderer();
    }

    UNITY_INTERFACE_EXPORT uint32_t GetVideoRendererId(UnityVideoRenderer* sink)
    {
        return sink->GetId();
    }

    UNITY_INTERFACE_EXPORT void DeleteVideoRenderer(Context* context, UnityVideoRenderer* sink)
    {
        context->DeleteVideoRenderer(sink);
    }

    UNITY_INTERFACE_EXPORT void VideoTrackAddOrUpdateSink(VideoTrackInterface* track, UnityVideoRenderer* sink)
    {
        track->AddOrUpdateSink(sink, rtc::VideoSinkWants());
    }

    UNITY_INTERFACE_EXPORT void VideoTrackRemoveSink(VideoTrackInterface* track, UnityVideoRenderer* sink)
    {
        track->RemoveSink(sink);
    }

    UNITY_INTERFACE_EXPORT void RegisterDebugLog(DelegateDebugLog func)
    {
        delegateDebugLog = func;
    }

    UNITY_INTERFACE_EXPORT void RegisterSetResolution(DelegateSetResolution func)
    {
        delegateSetResolution = func;
    }

    UNITY_INTERFACE_EXPORT Context* ContextCreate(int uid, UnityEncoderType encoderType, bool useDirectAudio)
    {
        auto ctx = ContextManager::GetInstance()->GetContext(uid);
        if (ctx != nullptr)
        {
            DebugLog("Already created context with ID %d", uid);
            return ctx;
        }
        ctx = ContextManager::GetInstance()->CreateContext(uid, encoderType, useDirectAudio);
        return ctx;
    }

    UNITY_INTERFACE_EXPORT void ContextDestroy(int uid)
    {
        ContextManager::GetInstance()->DestroyContext(uid);
    }

    PeerConnectionObject* _ContextCreatePeerConnection(
        Context* context, const PeerConnectionInterface::RTCConfiguration& config)
    {
        const auto obj = context->CreatePeerConnection(config);
        if (obj == nullptr)
            return nullptr;
        const auto observer = unity::webrtc::SetSessionDescriptionObserver::Create(obj);
        context->AddObserver(obj->connection, observer);
        return obj;
    }

    UNITY_INTERFACE_EXPORT PeerConnectionObject* ContextCreatePeerConnection(Context* context)
    {
        PeerConnectionInterface::RTCConfiguration config;
        config.sdp_semantics = SdpSemantics::kUnifiedPlan;
        config.enable_implicit_rollback = true;
        return _ContextCreatePeerConnection(context, config);
    }

    UNITY_INTERFACE_EXPORT PeerConnectionObject* ContextCreatePeerConnectionWithConfig(Context* context, const char* conf)
    {
        PeerConnectionInterface::RTCConfiguration config;
        if (!Convert(conf, config))
            return nullptr;

        config.sdp_semantics = SdpSemantics::kUnifiedPlan;
        config.enable_implicit_rollback = true;
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

    UNITY_INTERFACE_EXPORT void PeerConnectionRestartIce(PeerConnectionObject* obj)
    {
        obj->connection->RestartIce();
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

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface* PeerConnectionAddTransceiverWithType(PeerConnectionObject* obj, cricket::MediaType type)
    {
        return obj->connection->AddTransceiver(type).value().get();
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface* PeerConnectionAddTransceiverWithTypeAndInit(PeerConnectionObject* obj, cricket::MediaType type, RtpTransceiverInit* init)
    {
        return obj->connection->AddTransceiver(type, *init).value().get();
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

    UNITY_INTERFACE_EXPORT RTCErrorType PeerConnectionSetLocalDescription(
        Context* context, PeerConnectionObject* obj, const RTCSessionDescription* desc, char* error[])
    {
        std::string error_;
        RTCErrorType errorType = obj->SetLocalDescription(
            *desc, context->GetObserver(obj->connection), error_);
        *error = ConvertString(error_);
        return errorType;
    }

    UNITY_INTERFACE_EXPORT RTCErrorType PeerConnectionSetLocalDescriptionWithoutDescription(Context* context, PeerConnectionObject* obj, char* error[])
    {
        std::string error_;
        RTCErrorType errorType = obj->SetLocalDescriptionWithoutDescription(context->GetObserver(obj->connection), error_);
        *error = ConvertString(error_);
        return errorType;
    }

    UNITY_INTERFACE_EXPORT RTCErrorType PeerConnectionSetRemoteDescription(
        Context* context, PeerConnectionObject* obj, const RTCSessionDescription* desc, char* error[])
    {
        std::string error_;
        RTCErrorType errorType = obj->SetRemoteDescription(
            *desc, context->GetObserver(obj->connection), error_);
        *error = ConvertString(error_);
        return errorType;
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

    UNITY_INTERFACE_EXPORT void PeerConnectionCreateOffer(PeerConnectionObject* obj, const RTCOfferAnswerOptions* options)
    {
        obj->CreateOffer(*options);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionCreateAnswer(PeerConnectionObject* obj, const RTCOfferAnswerOptions* options)
    {
        obj->CreateAnswer(*options);
    }

    struct RTCDataChannelInit
    {
        Optional<bool> ordered;
        Optional<int32_t> maxRetransmitTime;
        Optional<int32_t> maxRetransmits;
        char* protocol;
        Optional<bool> negotiated;
        Optional<int32_t> id;
    };

    UNITY_INTERFACE_EXPORT DataChannelObject* ContextCreateDataChannel(Context* ctx, PeerConnectionObject* obj, const char* label, const RTCDataChannelInit* options)
    {
        DataChannelInit _options;
        _options.ordered = options->ordered.value_or(true);
        _options.maxRetransmitTime = static_cast<absl::optional<int32_t>>(options->maxRetransmitTime);
        _options.maxRetransmits = static_cast<absl::optional<int32_t>>(options->maxRetransmits);
        _options.protocol = options->protocol == nullptr ? "" : options->protocol;
        _options.negotiated = options->negotiated.value_or(false);
        _options.id = options->id.value_or(-1);

        return ctx->CreateDataChannel(obj, label, _options);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteDataChannel(Context* ctx, DataChannelObject* channel)
    {
        ctx->DeleteDataChannel(channel);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterIceConnectionChange(PeerConnectionObject* obj, DelegateOnIceConnectionChange callback)
    {
        obj->RegisterIceConnectionChange(callback);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterIceGatheringChange(PeerConnectionObject* obj, DelegateOnIceGatheringChange callback)
    {
        obj->RegisterIceGatheringChange(callback);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterConnectionStateChange(PeerConnectionObject* obj, DelegateOnConnectionStateChange callback)
    {
        obj->RegisterConnectionStateChange(callback);
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

    UNITY_INTERFACE_EXPORT bool PeerConnectionAddIceCandidate(PeerConnectionObject* obj, const IceCandidateInterface* candidate)
    {
        return obj->connection->AddIceCandidate(candidate);
    }

    struct RTCIceCandidateInit
    {
        char* candidate;
        char* sdpMid;
        int32_t sdpMLineIndex;
    };

    struct Candidate
    {
        char* candidate;
        int32_t component;
        char* foundation;
        char* ip;
        uint16_t port;
        uint32_t priority;
        char* address;
        char* protocol;
        char* relatedAddress;
        uint16_t relatedPort;
        char* tcpType;
        char* type;
        char* usernameFragment;

        Candidate& operator =(const cricket::Candidate& obj)
        {
            candidate = ConvertString(obj.ToString());
            component = obj.component();
            foundation = ConvertString(obj.foundation());
            ip = ConvertString(obj.address().ipaddr().ToString());
            port = obj.address().port();
            priority = obj.priority();
            address = ConvertString(obj.address().ToString());
            protocol = ConvertString(obj.protocol());
            relatedAddress = ConvertString(obj.related_address().ToString());
            relatedPort = obj.related_address().port();
            tcpType = ConvertString(obj.tcptype());
            type = ConvertString(obj.type());
            usernameFragment = ConvertString(obj.username());
            return *this;
        }
    };

    UNITY_INTERFACE_EXPORT RTCErrorType CreateIceCandidate(const RTCIceCandidateInit* options, IceCandidateInterface** candidate)
    {
        SdpParseError error;
        IceCandidateInterface* _candidate = CreateIceCandidate(options->sdpMid, options->sdpMLineIndex, options->candidate, &error);
        if (_candidate == nullptr)
            return RTCErrorType::INVALID_PARAMETER;
        *candidate = _candidate;
        return RTCErrorType::NONE;
    }

    UNITY_INTERFACE_EXPORT void DeleteIceCandidate(IceCandidateInterface* candidate)
    {
        delete candidate;
    }

    UNITY_INTERFACE_EXPORT void IceCandidateGetCandidate(const IceCandidateInterface* candidate, Candidate* dst)
    {
        *dst = candidate->candidate();
    }

    UNITY_INTERFACE_EXPORT int32_t IceCandidateGetSdpLineIndex(const IceCandidateInterface* candidate)
    {
        return candidate->sdp_mline_index();
    }

    UNITY_INTERFACE_EXPORT const char* IceCandidateGetSdp(const IceCandidateInterface* candidate)
    {
        std::string str;
        if (!candidate->ToString(&str))
            return nullptr;
        return ConvertString(str);
    }

    UNITY_INTERFACE_EXPORT const char* IceCandidateGetSdpMid(const IceCandidateInterface* candidate)
    {
        return ConvertString(candidate->sdp_mid());
    }

    UNITY_INTERFACE_EXPORT PeerConnectionInterface::PeerConnectionState PeerConnectionState(PeerConnectionObject* obj)
    {
        return obj->connection->peer_connection_state();
    }

    UNITY_INTERFACE_EXPORT PeerConnectionInterface::IceConnectionState PeerConnectionIceConditionState(PeerConnectionObject* obj)
    {
        return obj->connection->ice_connection_state();
    }

    UNITY_INTERFACE_EXPORT PeerConnectionInterface::SignalingState PeerConnectionSignalingState(PeerConnectionObject* obj)
    {
        return obj->connection->signaling_state();
    }

    UNITY_INTERFACE_EXPORT PeerConnectionInterface::IceGatheringState PeerConnectionIceGatheringState(PeerConnectionObject* obj)
    {
        return obj->connection->ice_gathering_state();
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

    UNITY_INTERFACE_EXPORT RtpTransceiverDirection TransceiverGetDirection(RtpTransceiverInterface* transceiver)
    {
        return transceiver->direction();
    }

    UNITY_INTERFACE_EXPORT void TransceiverSetDirection(RtpTransceiverInterface* transceiver, RtpTransceiverDirection direction)
    {
        transceiver->SetDirection(direction);
    }

    struct RTCRtpCodecCapability
    {
        char* mimeType;
        Optional<int32_t> clockRate;
        Optional<int32_t> channels;
        char* sdpFmtpLine;

        RTCRtpCodecCapability& operator = (const RtpCodecCapability& obj)
        {
            this->mimeType = ConvertString(obj.mime_type());
            this->clockRate = obj.clock_rate;
            this->channels = obj.num_channels;
            this->sdpFmtpLine = ConvertString(ConvertSdp(obj.parameters));
            return *this;
        }
    };
     
    UNITY_INTERFACE_EXPORT RTCErrorType TransceiverSetCodecPreferences(RtpTransceiverInterface* transceiver, RTCRtpCodecCapability* codecs, size_t length)
    {
        std::vector<RtpCodecCapability> _codecs(length);
        for(int i = 0; i < length; i++)
        {
            std::string mimeType = ConvertString(codecs[i].mimeType);
            std::tie(_codecs[i].kind, _codecs[i].name) = ConvertMimeType(mimeType);
            _codecs[i].clock_rate = ConvertOptional(codecs[i].clockRate);
            _codecs[i].num_channels = ConvertOptional(codecs[i].channels);
            _codecs[i].parameters = ConvertSdp(codecs[i].sdpFmtpLine);
        }
        return transceiver->SetCodecPreferences(_codecs).type();
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
        Optional<uint64_t> maxBitrate;
        Optional<uint64_t> minBitrate;
        Optional<uint32_t> maxFramerate;
        Optional<double> scaleResolutionDownBy;
        char* rid;

        RTCRtpEncodingParameters& operator=(const RtpEncodingParameters& obj)
        {
            active = obj.active;
            maxBitrate = obj.max_bitrate_bps;
            minBitrate = obj.min_bitrate_bps;
            maxFramerate = obj.max_framerate;
            scaleResolutionDownBy = obj.scale_resolution_down_by;
            rid = ConvertString(obj.rid);
            return *this;
        }

        operator RtpEncodingParameters() const
        {
            RtpEncodingParameters dst = {};
            dst.active = active;
            dst.max_bitrate_bps = static_cast<absl::optional<int>>(ConvertOptional(maxBitrate));
            dst.min_bitrate_bps = static_cast<absl::optional<int>>(ConvertOptional(minBitrate));
            dst.max_framerate = static_cast<absl::optional<double>>(ConvertOptional(maxFramerate));
            dst.scale_resolution_down_by = ConvertOptional(scaleResolutionDownBy);
            if(rid != nullptr)
                dst.rid = std::string(rid);
            return dst;
        }
    };

    struct RTCRtpCodecParameters
    {
        int payloadType;
        char* mimeType;
        Optional<uint64_t> clockRate;
        Optional<uint16_t> channels;
        char* sdpFmtpLine;

        RTCRtpCodecParameters& operator=(const RtpCodecParameters& src)
        {
            payloadType = src.payload_type;
            mimeType = ConvertString(src.mime_type());
            clockRate = src.clock_rate;
            channels = src.num_channels;
            sdpFmtpLine = ConvertString(ConvertSdp(src.parameters));
            return *this;
        }
    };

    struct RTCRtpExtension
    {
        char* uri;
        uint16_t id;
        bool encrypted;

        RTCRtpExtension& operator=(const RtpExtension& src)
        {
            uri = ConvertString(src.uri);
            id = src.id;
            encrypted = src.encrypt;
            return *this;
        }
    };

    struct RTCRtcpParameters
    {
        char* cname;
        bool reducedSize;

        RTCRtcpParameters& operator=(const RtcpParameters& src)
        {
            cname = ConvertString(src.cname);
            reducedSize = src.reduced_size;
            return *this;
        }
    };

    struct RTCRtpSendParameters
    {
        MarshallArray<RTCRtpEncodingParameters> encodings;
        char* transactionId;
        MarshallArray<RTCRtpCodecParameters> codecs;
        MarshallArray<RTCRtpExtension> headerExtensions;
        RTCRtcpParameters rtcp;

        RTCRtpSendParameters& operator=(const RtpParameters& src)
        {
            encodings = src.encodings;
            transactionId = ConvertString(src.transaction_id);
            codecs = src.codecs;
            headerExtensions = src.header_extensions;
            rtcp = src.rtcp;
            return *this;
        }
    };

    UNITY_INTERFACE_EXPORT void SenderGetParameters(RtpSenderInterface* sender, RTCRtpSendParameters** parameters)
    {
        const RtpParameters src = sender->GetParameters();
        RTCRtpSendParameters* dst = static_cast<RTCRtpSendParameters*>(CoTaskMemAlloc(sizeof(RTCRtpSendParameters)));
        *dst = src;
        *parameters = dst;
    }

    UNITY_INTERFACE_EXPORT RTCErrorType SenderSetParameters(RtpSenderInterface* sender, const RTCRtpSendParameters* src)
    {
        RtpParameters dst = sender->GetParameters();

        for (size_t i = 0; i < dst.encodings.size(); i++)
        {
            dst.encodings[i].active = src->encodings[i].active;
            dst.encodings[i].max_bitrate_bps = static_cast<absl::optional<int>>(ConvertOptional(src->encodings[i].maxBitrate));
            dst.encodings[i].min_bitrate_bps = static_cast<absl::optional<int>>(ConvertOptional(src->encodings[i].minBitrate));
            dst.encodings[i].max_framerate = static_cast<absl::optional<double>>(ConvertOptional(src->encodings[i].maxFramerate));
            dst.encodings[i].scale_resolution_down_by = ConvertOptional(src->encodings[i].scaleResolutionDownBy);
            if(src->encodings[i].rid != nullptr)
                dst.encodings[i].rid = std::string(src->encodings[i].rid);
        }
        const ::webrtc::RTCError error = sender->SetParameters(dst);
        return error.type();
    }

    struct RTCRtpHeaderExtensionCapability
    {
        char* uri;

        RTCRtpHeaderExtensionCapability& operator = (const RtpHeaderExtensionCapability& obj)
        {
            this->uri = ConvertString(obj.uri);
            return *this;
        }
    };

    struct RTCRtpCapabilities
    {
        MarshallArray<RTCRtpCodecCapability> codecs;
        MarshallArray<RTCRtpHeaderExtensionCapability> extensionHeaders;

        RTCRtpCapabilities& operator=(const RtpCapabilities& src)
        {
            codecs = src.codecs;
            extensionHeaders = src.header_extensions;
            return *this;
        }
    };

    UNITY_INTERFACE_EXPORT void ContextGetSenderCapabilities(
        Context* context, TrackKind trackKind, RTCRtpCapabilities** parameters)
    {
        RtpCapabilities src;
        cricket::MediaType type =
            trackKind == TrackKind::Audio ?
            cricket::MEDIA_TYPE_AUDIO : cricket::MEDIA_TYPE_VIDEO;
        context->GetRtpSenderCapabilities(type, &src);

        RTCRtpCapabilities* dst =
            static_cast<RTCRtpCapabilities*>(CoTaskMemAlloc(sizeof(RTCRtpCapabilities)));
        *dst = src;
        *parameters = dst;
    }

    UNITY_INTERFACE_EXPORT void ContextGetReceiverCapabilities(
        Context* context, TrackKind trackKind, RTCRtpCapabilities** parameters)
    {
        RtpCapabilities src;
        cricket::MediaType type =
            trackKind == TrackKind::Audio ?
            cricket::MEDIA_TYPE_AUDIO : cricket::MEDIA_TYPE_VIDEO;
        context->GetRtpReceiverCapabilities(type, &src);

        RTCRtpCapabilities* dst =
            static_cast<RTCRtpCapabilities*>(CoTaskMemAlloc(sizeof(RTCRtpCapabilities)));
        *dst = src;
        *parameters = dst;
    }

    UNITY_INTERFACE_EXPORT bool SenderReplaceTrack(RtpSenderInterface* sender, MediaStreamTrackInterface* track)
    {
        return sender->SetTrack(track);
    }

    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface* SenderGetTrack(RtpSenderInterface* sender)
    {
        return sender->track().get();
    }

    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface* ReceiverGetTrack(RtpReceiverInterface* receiver)
    {
        return receiver->track().get();
    }

    UNITY_INTERFACE_EXPORT MediaStreamInterface** ReceiverGetStreams(RtpReceiverInterface* receiver, size_t* length)
    {
        return ConvertPtrArrayFromRefPtrArray<MediaStreamInterface>(receiver->streams(), length);
    }

    UNITY_INTERFACE_EXPORT int DataChannelGetID(DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->dataChannel->id();
    }

    UNITY_INTERFACE_EXPORT char* DataChannelGetLabel(DataChannelObject* dataChannelObj)
    {
        return ConvertString(dataChannelObj->dataChannel->label());
    }

    UNITY_INTERFACE_EXPORT char* DataChannelGetProtocol(DataChannelObject* dataChannelObj)
    {
        return ConvertString(dataChannelObj->dataChannel->protocol());
    }

    UNITY_INTERFACE_EXPORT uint16_t DataChannelGetMaxRetransmits(DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->dataChannel->maxRetransmits();
    }

    UNITY_INTERFACE_EXPORT uint16_t DataChannelGetMaxRetransmitTime(DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->dataChannel->maxRetransmitTime();
    }

    UNITY_INTERFACE_EXPORT bool DataChannelGetOrdered(DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->dataChannel->ordered();
    }

    UNITY_INTERFACE_EXPORT uint64_t DataChannelGetBufferedAmount(DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->dataChannel->buffered_amount();
    }

    UNITY_INTERFACE_EXPORT bool DataChannelGetNegotiated(DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->dataChannel->negotiated();
    }

    UNITY_INTERFACE_EXPORT DataChannelInterface::DataState DataChannelGetReadyState(
        DataChannelObject* dataChannelObj)
    {
        return dataChannelObj->dataChannel->state();
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

