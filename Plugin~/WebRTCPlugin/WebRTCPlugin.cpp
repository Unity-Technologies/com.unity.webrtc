#include "pch.h"

#include "Context.h"
#include "CreateSessionDescriptionObserver.h"
#include "EncodedStreamTransformer.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "MediaStreamObserver.h"
#include "PeerConnectionObject.h"
#include "SetLocalDescriptionObserver.h"
#include "SetRemoteDescriptionObserver.h"
#include "UnityAudioTrackSource.h"
#include "UnityLogStream.h"
#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wmissing-prototypes"

    std::string ConvertSdp(const std::map<std::string, std::string>& map)
    {
        std::string str = "";
        for (const auto& pair : map)
        {
            if (!str.empty())
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

        if (str.empty())
            return dst;

        while (true)
        {
            pos = s.find(delimiter);
            size_t length = pos;
            if (pos == std::string::npos)
                length = str.length();
            dst.push_back(s.substr(0, length));
            if (pos == std::string::npos)
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
        return std::make_tuple(mediaType, name);
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
    /// avoid compile error for vector<bool>
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

    template<class T>
    T** ConvertPtrArrayFromRefPtrArray(std::vector<rtc::scoped_refptr<T>> vec, size_t* length)
    {
        *length = vec.size();
        const auto buf = CoTaskMemAlloc(sizeof(T*) * vec.size());
        const auto ret = static_cast<T**>(buf);
        for (size_t i = 0; i < vec.size(); i++)
        {
            rtc::scoped_refptr<T> item = vec[i];
            ret[i] = item.get();
        }
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
        size_t length;
        T* values;

        T& operator[](size_t i) const { return values[i]; }

        template<typename U>
        MarshallArray& operator=(const std::vector<U>& src)
        {
            length = static_cast<int32_t>(src.size());
            values = static_cast<T*>(CoTaskMemAlloc(sizeof(T) * src.size()));

            for (size_t i = 0; i < src.size(); i++)
            {
                values[i] = src[i];
            }
            return *this;
        }

        template<typename U>
        MarshallArray& operator=(const rtc::ArrayView<U>& src)
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

#if defined(__clang__) || defined(__GNUC__)
        __attribute__((optnone))
#endif
        explicit
        operator const absl::optional<T>() const
        {
            absl::optional<T> dst = absl::nullopt;
            if (hasValue)
                dst = value;
            return dst;
        }

        const T& value_or(const T& v) const { return hasValue ? value : v; }
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

    template<typename T>
    const char** StatsMemberGetMapStringValue(const std::map<std::string, T>& map, T** values, size_t* length)
    {
        std::vector<const char*> vc;
        std::vector<T> vv;

        for (auto const& pair : map)
        {
            vc.push_back(ConvertString(pair.first));
            vv.push_back(pair.second);
        }
        *values = ConvertArray(vv, length);
        return ConvertArray(vc, length);
    }
} // end namespace webrtc
} // end namespace unity

using namespace unity::webrtc;
using namespace ::webrtc;

extern "C"
{
    UNITY_INTERFACE_EXPORT MediaStreamInterface* ContextCreateMediaStream(Context* context, const char* streamId)
    {
        rtc::scoped_refptr<MediaStreamInterface> stream = context->CreateMediaStream(streamId);
        context->AddRefPtr(stream);
        return stream.get();
    }

    UNITY_INTERFACE_EXPORT void ContextRegisterMediaStreamObserver(Context* context, MediaStreamInterface* stream)
    {
        context->RegisterMediaStreamObserver(stream);
    }

    UNITY_INTERFACE_EXPORT void ContextUnRegisterMediaStreamObserver(Context* context, MediaStreamInterface* stream)
    {
        context->UnRegisterMediaStreamObserver(stream);
    }

    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface*
    ContextCreateVideoTrack(Context* context, const char* label, webrtc::VideoTrackSourceInterface* source)
    {
        rtc::scoped_refptr<VideoTrackInterface> track = context->CreateVideoTrack(label, source);
        context->AddRefPtr(track);
        return track.get();
    }

    UNITY_INTERFACE_EXPORT void
    ContextStopMediaStreamTrack(Context* context, ::webrtc::MediaStreamTrackInterface* track)
    {
        context->StopMediaStreamTrack(track);
    }

    UNITY_INTERFACE_EXPORT webrtc::VideoTrackSourceInterface* ContextCreateVideoTrackSource(Context* context)
    {
        rtc::scoped_refptr<VideoTrackSourceInterface> source = context->CreateVideoSource();
        context->AddRefPtr(source);
        return source.get();
    }

    UNITY_INTERFACE_EXPORT webrtc::AudioSourceInterface* ContextCreateAudioTrackSource(Context* context)
    {
        rtc::scoped_refptr<AudioSourceInterface> source = context->CreateAudioSource();
        context->AddRefPtr(source);
        return source.get();
    }

    UNITY_INTERFACE_EXPORT webrtc::MediaStreamTrackInterface*
    ContextCreateAudioTrack(Context* context, const char* label, webrtc::AudioSourceInterface* source)
    {
        rtc::scoped_refptr<AudioTrackInterface> track = context->CreateAudioTrack(label, source);
        context->AddRefPtr(track);
        return track.get();
    }

    UNITY_INTERFACE_EXPORT void ContextAddRefPtr(Context* context, rtc::RefCountInterface* ptr)
    {
        context->AddRefPtr(ptr);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteRefPtr(Context* context, rtc::RefCountInterface* ptr)
    {
        context->RemoveRefPtr(ptr);
    }

    UNITY_INTERFACE_EXPORT EncodedStreamTransformer*
    ContextCreateFrameTransformer(Context* context, DelegateTransformedFrame callback)
    {
        rtc::scoped_refptr<EncodedStreamTransformer> transformer = rtc::make_ref_counted<EncodedStreamTransformer>();
        context->AddRefPtr(transformer);
        return transformer.get();
    }

    UNITY_INTERFACE_EXPORT bool MediaStreamAddTrack(MediaStreamInterface* stream, MediaStreamTrackInterface* track)
    {
        if (track->kind() == "audio")
        {
            return stream->AddTrack(rtc::scoped_refptr<AudioTrackInterface>(static_cast<AudioTrackInterface*>(track)));
        }
        else
        {
            return stream->AddTrack(rtc::scoped_refptr<VideoTrackInterface>(static_cast<VideoTrackInterface*>(track)));
        }
    }
    UNITY_INTERFACE_EXPORT bool MediaStreamRemoveTrack(MediaStreamInterface* stream, MediaStreamTrackInterface* track)
    {
        if (track->kind() == "audio")
        {
            return stream->RemoveTrack(
                rtc::scoped_refptr<AudioTrackInterface>(static_cast<AudioTrackInterface*>(track)));
        }
        else
        {
            return stream->RemoveTrack(
                rtc::scoped_refptr<VideoTrackInterface>(static_cast<VideoTrackInterface*>(track)));
        }
    }

    UNITY_INTERFACE_EXPORT char* MediaStreamGetID(MediaStreamInterface* stream) { return ConvertString(stream->id()); }

    UNITY_INTERFACE_EXPORT void MediaStreamRegisterOnAddTrack(
        Context* context, MediaStreamInterface* stream, DelegateMediaStreamOnAddTrack callback)
    {
        context->GetObserver(stream)->RegisterOnAddTrack(callback);
    }

    UNITY_INTERFACE_EXPORT void MediaStreamRegisterOnRemoveTrack(
        Context* context, MediaStreamInterface* stream, DelegateMediaStreamOnRemoveTrack callback)
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

    UNITY_INTERFACE_EXPORT VideoTrackSourceInterface*
    ContextGetVideoSource(Context* context, VideoTrackInterface* track)
    {
        return track->GetSource();
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

    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface::TrackState
    MediaStreamTrackGetReadyState(MediaStreamTrackInterface* track)
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

    UNITY_INTERFACE_EXPORT UnityVideoRenderer*
    CreateVideoRenderer(Context* context, DelegateVideoFrameResize callback, bool needFlipVertical)
    {
        return context->CreateVideoRenderer(callback, needFlipVertical);
    }

    UNITY_INTERFACE_EXPORT uint32_t GetVideoRendererId(UnityVideoRenderer* sink) { return sink->GetId(); }

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

    UNITY_INTERFACE_EXPORT void
    RegisterDebugLog(DelegateDebugLog func, bool enableNativeLog, rtc::LoggingSeverity loggingSeverity)
    {
        delegateDebugLog = func;
        if (func != nullptr && enableNativeLog)
        {
            UnityLogStream::AddLogStream(func, loggingSeverity);
        }
        else if (func == nullptr)
        {
            UnityLogStream::RemoveLogStream();
        }
    }

    UNITY_INTERFACE_EXPORT Context* ContextCreate(int uid)
    {
        auto ctx = ContextManager::GetInstance()->GetContext(uid);
        if (ctx != nullptr)
        {
            DebugLog("Already created context with ID %d", uid);
            return ctx;
        }
        ContextDependencies dependencies;
        dependencies.device = Plugin::GraphicsDevice();
        dependencies.profiler = Plugin::ProfilerMarkerFactory();
        ctx = ContextManager::GetInstance()->CreateContext(uid, dependencies);
        return ctx;
    }

    UNITY_INTERFACE_EXPORT void ContextDestroy(int uid) { ContextManager::GetInstance()->DestroyContext(uid); }

    UNITY_INTERFACE_EXPORT PeerConnectionObject* ContextCreatePeerConnection(Context* context)
    {
        PeerConnectionInterface::RTCConfiguration config;
        config.sdp_semantics = SdpSemantics::kUnifiedPlan;
        config.enable_implicit_rollback = true;
        return context->CreatePeerConnection(config);
    }

    UNITY_INTERFACE_EXPORT PeerConnectionObject*
    ContextCreatePeerConnectionWithConfig(Context* context, const char* conf)
    {
        PeerConnectionInterface::RTCConfiguration config;
        if (!Convert(conf, config))
            return nullptr;

        config.sdp_semantics = SdpSemantics::kUnifiedPlan;
        config.enable_implicit_rollback = true;
        return context->CreatePeerConnection(config);
    }

    UNITY_INTERFACE_EXPORT void ContextDeletePeerConnection(Context* context, PeerConnectionObject* obj)
    {
        obj->Close();
        context->DeletePeerConnection(obj);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionClose(PeerConnectionObject* obj) { obj->Close(); }

    UNITY_INTERFACE_EXPORT void PeerConnectionRestartIce(PeerConnectionObject* obj) { obj->connection->RestartIce(); }

    UNITY_INTERFACE_EXPORT RTCErrorType PeerConnectionAddTrack(
        PeerConnectionObject* obj, MediaStreamTrackInterface* track, const char* streamId, RtpSenderInterface** sender)
    {
        std::vector<std::string> streams;
        if (streamId)
            streams.push_back(streamId);

        auto result = obj->connection->AddTrack(rtc::scoped_refptr<MediaStreamTrackInterface>(track), streams);
        if (result.ok())
        {
            *sender = result.value().get();
        }
        return result.error().type();
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
            if (rid != nullptr)
                dst.rid = std::string(rid);
            return dst;
        }
    };

    struct RTCRtpTransceiverInit
    {
        RtpTransceiverDirection direction;
        MarshallArray<RTCRtpEncodingParameters> sendEncodings;
        MarshallArray<MediaStreamInterface*> streams;

        operator RtpTransceiverInit() const
        {
            RtpTransceiverInit dst = {};
            dst.direction = direction;
            dst.send_encodings.resize(sendEncodings.length);
            for (size_t i = 0; i < dst.send_encodings.size(); i++)
            {
                dst.send_encodings[i] = sendEncodings[i];
            }
            dst.stream_ids.resize(streams.length);
            for (size_t i = 0; i < dst.stream_ids.size(); i++)
            {
                dst.stream_ids[i] = streams[i]->id();
            }
            return dst;
        }
    };

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface*
    PeerConnectionAddTransceiver(PeerConnectionObject* obj, MediaStreamTrackInterface* track)
    {
        auto result = obj->connection->AddTransceiver(rtc::scoped_refptr<MediaStreamTrackInterface>(track));
        if (!result.ok())
            return nullptr;

        return result.value().get();
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface* PeerConnectionAddTransceiverWithInit(
        PeerConnectionObject* obj, MediaStreamTrackInterface* track, const RTCRtpTransceiverInit* init)
    {
        auto result = obj->connection->AddTransceiver(rtc::scoped_refptr<MediaStreamTrackInterface>(track), *init);
        if (!result.ok())
            return nullptr;

        return result.value().get();
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface*
    PeerConnectionAddTransceiverWithType(PeerConnectionObject* obj, cricket::MediaType type)
    {
        auto result = obj->connection->AddTransceiver(type);
        if (!result.ok())
            return nullptr;

        return result.value().get();
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface* PeerConnectionAddTransceiverWithTypeAndInit(
        PeerConnectionObject* obj, cricket::MediaType type, const RTCRtpTransceiverInit* init)
    {
        auto result = obj->connection->AddTransceiver(type, *init);
        if (!result.ok())
            return nullptr;

        return result.value().get();
    }

    UNITY_INTERFACE_EXPORT RTCErrorType PeerConnectionRemoveTrack(PeerConnectionObject* obj, RtpSenderInterface* sender)
    {
        webrtc::RTCError error = obj->connection->RemoveTrackOrError(rtc::scoped_refptr<RtpSenderInterface>(sender));
        return error.type();
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

    UNITY_INTERFACE_EXPORT PeerConnectionStatsCollectorCallback* PeerConnectionGetStats(PeerConnectionObject* obj)
    {
        rtc::scoped_refptr<PeerConnectionStatsCollectorCallback> callback =
            PeerConnectionStatsCollectorCallback::Create(obj);
        obj->connection->GetStats(callback.get());
        return callback.get();
    }

    UNITY_INTERFACE_EXPORT PeerConnectionStatsCollectorCallback*
    PeerConnectionSenderGetStats(PeerConnectionObject* obj, RtpSenderInterface* sender)
    {
        rtc::scoped_refptr<PeerConnectionStatsCollectorCallback> callback =
            PeerConnectionStatsCollectorCallback::Create(obj);
        obj->connection->GetStats(rtc::scoped_refptr<RtpSenderInterface>(sender), callback);
        return callback.get();
    }

    UNITY_INTERFACE_EXPORT PeerConnectionStatsCollectorCallback*
    PeerConnectionReceiverGetStats(PeerConnectionObject* obj, RtpReceiverInterface* receiver)
    {
        rtc::scoped_refptr<PeerConnectionStatsCollectorCallback> callback =
            PeerConnectionStatsCollectorCallback::Create(obj);
        obj->connection->GetStats(rtc::scoped_refptr<RtpReceiverInterface>(receiver), callback);
        return callback.get();
    }

    UNITY_INTERFACE_EXPORT const RTCStats**
    ContextGetStatsList(Context* context, const RTCStatsReport* report, size_t* length, uint32_t** types)
    {
        return context->GetStatsList(report, length, types);
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteStatsReport(Context* context, const RTCStatsReport* report)
    {
        context->DeleteStatsReport(report);
    }

    UNITY_INTERFACE_EXPORT const char* StatsGetJson(const RTCStats* stats) { return ConvertString(stats->ToJson()); }

    UNITY_INTERFACE_EXPORT int64_t StatsGetTimestamp(const RTCStats* stats) { return stats->timestamp().us(); }

    UNITY_INTERFACE_EXPORT const char* StatsGetId(const RTCStats* stats) { return ConvertString(stats->id()); }

    UNITY_INTERFACE_EXPORT uint32_t StatsGetType(const RTCStats* stats) { return statsTypes.at(stats->type()); }

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
        return *member->cast_to<RTCStatsMember<bool>>();
    }

    UNITY_INTERFACE_EXPORT int32_t StatsMemberGetInt(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<RTCStatsMember<int32_t>>();
    }

    UNITY_INTERFACE_EXPORT uint32_t StatsMemberGetUnsignedInt(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<RTCStatsMember<uint32_t>>();
    }

    UNITY_INTERFACE_EXPORT int64_t StatsMemberGetLong(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<RTCStatsMember<int64_t>>();
    }

    UNITY_INTERFACE_EXPORT uint64_t StatsMemberGetUnsignedLong(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<RTCStatsMember<uint64_t>>();
    }

    UNITY_INTERFACE_EXPORT double StatsMemberGetDouble(const RTCStatsMemberInterface* member)
    {
        return *member->cast_to<RTCStatsMember<double>>();
    }

    UNITY_INTERFACE_EXPORT const char* StatsMemberGetString(const RTCStatsMemberInterface* member)
    {
        return ConvertString(member->ValueToString());
    }

    UNITY_INTERFACE_EXPORT bool* StatsMemberGetBoolArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<RTCStatsMember<std::vector<bool>>>(), length);
    }

    UNITY_INTERFACE_EXPORT int32_t* StatsMemberGetIntArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<RTCStatsMember<std::vector<int>>>(), length);
    }

    UNITY_INTERFACE_EXPORT uint32_t*
    StatsMemberGetUnsignedIntArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<RTCStatsMember<std::vector<uint32_t>>>(), length);
    }

    UNITY_INTERFACE_EXPORT int64_t* StatsMemberGetLongArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<RTCStatsMember<std::vector<int64_t>>>(), length);
    }

    UNITY_INTERFACE_EXPORT uint64_t*
    StatsMemberGetUnsignedLongArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<RTCStatsMember<std::vector<uint64_t>>>(), length);
    }

    UNITY_INTERFACE_EXPORT double* StatsMemberGetDoubleArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        return ConvertArray(*member->cast_to<RTCStatsMember<std::vector<double>>>(), length);
    }

    UNITY_INTERFACE_EXPORT const char** StatsMemberGetStringArray(const RTCStatsMemberInterface* member, size_t* length)
    {
        std::vector<std::string> vec = *member->cast_to<RTCStatsMember<std::vector<std::string>>>();
        std::vector<const char*> vc;
        std::transform(vec.begin(), vec.end(), std::back_inserter(vc), ConvertString);
        return ConvertArray(vc, length);
    }

    UNITY_INTERFACE_EXPORT const char**
    StatsMemberGetMapStringUint64(const RTCStatsMemberInterface* member, uint64_t** values, size_t* length)
    {
        std::map<std::string, uint64_t> map = *member->cast_to<RTCStatsMember<std::map<std::string, uint64_t>>>();
        return StatsMemberGetMapStringValue(map, values, length);
    }

    UNITY_INTERFACE_EXPORT const char**
    StatsMemberGetMapStringDouble(const RTCStatsMemberInterface* member, double** values, size_t* length)
    {
        std::map<std::string, double> map = *member->cast_to<RTCStatsMember<std::map<std::string, double>>>();
        return StatsMemberGetMapStringValue(map, values, length);
    }

    UNITY_INTERFACE_EXPORT RTCStatsMemberInterface::Type StatsMemberGetType(const RTCStatsMemberInterface* member)
    {
        return member->type();
    }

    UNITY_INTERFACE_EXPORT SetLocalDescriptionObserver* PeerConnectionSetLocalDescription(
        PeerConnectionObject* obj, const RTCSessionDescription* desc, RTCErrorType* errorType, char* error[])
    {
        std::string error_;
        auto observer = SetLocalDescriptionObserver::Create(obj);
        *errorType = obj->SetLocalDescription(*desc, observer, error_);
        *error = ConvertString(error_);
        return observer.get();
    }

    UNITY_INTERFACE_EXPORT SetLocalDescriptionObserver* PeerConnectionSetLocalDescriptionWithoutDescription(
        PeerConnectionObject* obj, RTCErrorType* errorType, char* error[])
    {
        std::string error_;
        auto observer = SetLocalDescriptionObserver::Create(obj);
        *errorType = obj->SetLocalDescriptionWithoutDescription(observer, error_);
        *error = ConvertString(error_);
        return observer.get();
    }

    UNITY_INTERFACE_EXPORT SetRemoteDescriptionObserver* PeerConnectionSetRemoteDescription(
        PeerConnectionObject* obj, const RTCSessionDescription* desc, RTCErrorType* errorType, char* error[])
    {
        std::string error_;
        auto observer = SetRemoteDescriptionObserver::Create(obj);
        *errorType = obj->SetRemoteDescription(*desc, observer, error_);
        *error = ConvertString(error_);
        return observer.get();
    }

    UNITY_INTERFACE_EXPORT bool
    PeerConnectionGetLocalDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->local_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool
    PeerConnectionGetRemoteDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->remote_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool
    PeerConnectionGetPendingLocalDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->pending_local_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool
    PeerConnectionGetPendingRemoteDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->pending_remote_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool
    PeerConnectionGetCurrentLocalDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->current_local_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT bool
    PeerConnectionGetCurrentRemoteDescription(PeerConnectionObject* obj, RTCSessionDescription* desc)
    {
        return obj->GetSessionDescription(obj->connection->current_remote_description(), *desc);
    }

    UNITY_INTERFACE_EXPORT RtpReceiverInterface**
    PeerConnectionGetReceivers(Context* context, PeerConnectionObject* obj, size_t* length)
    {
        auto receivers = obj->connection->GetReceivers();
        return ConvertPtrArrayFromRefPtrArray<RtpReceiverInterface>(receivers, length);
    }

    UNITY_INTERFACE_EXPORT RtpSenderInterface**
    PeerConnectionGetSenders(Context* context, PeerConnectionObject* obj, size_t* length)
    {
        auto senders = obj->connection->GetSenders();
        return ConvertPtrArrayFromRefPtrArray<RtpSenderInterface>(senders, length);
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverInterface**
    PeerConnectionGetTransceivers(Context* context, PeerConnectionObject* obj, size_t* length)
    {
        auto transceivers = obj->connection->GetTransceivers();
        return ConvertPtrArrayFromRefPtrArray<RtpTransceiverInterface>(transceivers, length);
    }

    UNITY_INTERFACE_EXPORT unity::webrtc::CreateSessionDescriptionObserver*
    PeerConnectionCreateOffer(Context* context, PeerConnectionObject* obj, const RTCOfferAnswerOptions* options)
    {
        auto observer = unity::webrtc::CreateSessionDescriptionObserver::Create(obj);
        obj->CreateOffer(*options, observer.get());
        return observer.get();
    }

    UNITY_INTERFACE_EXPORT unity::webrtc::CreateSessionDescriptionObserver*
    PeerConnectionCreateAnswer(Context* context, PeerConnectionObject* obj, const RTCOfferAnswerOptions* options)
    {
        auto observer = unity::webrtc::CreateSessionDescriptionObserver::Create(obj);
        obj->CreateAnswer(*options, observer.get());
        return observer.get();
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

    UNITY_INTERFACE_EXPORT DataChannelInterface* ContextCreateDataChannel(
        Context* ctx, PeerConnectionObject* obj, const char* label, const RTCDataChannelInit* options)
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

    UNITY_INTERFACE_EXPORT void ContextDeleteDataChannel(Context* ctx, DataChannelInterface* channel)
    {
        ctx->DeleteDataChannel(channel);
    }

    UNITY_INTERFACE_EXPORT void
    PeerConnectionRegisterIceConnectionChange(PeerConnectionObject* obj, DelegateOnIceConnectionChange callback)
    {
        obj->RegisterIceConnectionChange(callback);
    }

    UNITY_INTERFACE_EXPORT void
    PeerConnectionRegisterIceGatheringChange(PeerConnectionObject* obj, DelegateOnIceGatheringChange callback)
    {
        obj->RegisterIceGatheringChange(callback);
    }

    UNITY_INTERFACE_EXPORT void
    PeerConnectionRegisterConnectionStateChange(PeerConnectionObject* obj, DelegateOnConnectionStateChange callback)
    {
        obj->RegisterConnectionStateChange(callback);
    }

    UNITY_INTERFACE_EXPORT void
    PeerConnectionRegisterOnIceCandidate(PeerConnectionObject* obj, DelegateIceCandidate callback)
    {
        obj->RegisterIceCandidate(callback);
    }

    UNITY_INTERFACE_EXPORT void StatsCollectorRegisterCallback(DelegateCollectStats callback)
    {
        PeerConnectionStatsCollectorCallback::RegisterOnGetStats(callback);
    }

    UNITY_INTERFACE_EXPORT void CreateSessionDescriptionObserverRegisterCallback(DelegateCreateSessionDesc callback)
    {
        unity::webrtc::CreateSessionDescriptionObserver::RegisterCallback(callback);
    }

    UNITY_INTERFACE_EXPORT void SetLocalDescriptionObserverRegisterCallback(DelegateSetLocalDesc callback)
    {
        unity::webrtc::SetLocalDescriptionObserver::RegisterCallback(callback);
    }

    UNITY_INTERFACE_EXPORT void SetRemoteDescriptionObserverRegisterCallback(DelegateSetRemoteDesc callback)
    {
        unity::webrtc::SetRemoteDescriptionObserver::RegisterCallback(callback);
    }

    UNITY_INTERFACE_EXPORT void SetTransformedFrameRegisterCallback(DelegateTransformedFrame callback)
    {
        unity::webrtc::EncodedStreamTransformer::RegisterCallback(callback);
    }

    UNITY_INTERFACE_EXPORT bool
    PeerConnectionAddIceCandidate(PeerConnectionObject* obj, const IceCandidateInterface* candidate)
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

        Candidate& operator=(const cricket::Candidate& obj)
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

    UNITY_INTERFACE_EXPORT RTCErrorType
    CreateIceCandidate(const RTCIceCandidateInit* options, IceCandidateInterface** candidate)
    {
        SdpParseError error;
        IceCandidateInterface* _candidate =
            CreateIceCandidate(options->sdpMid, options->sdpMLineIndex, options->candidate, &error);
        if (_candidate == nullptr)
            return RTCErrorType::INVALID_PARAMETER;
        *candidate = _candidate;
        return RTCErrorType::NONE;
    }

    UNITY_INTERFACE_EXPORT void DeleteIceCandidate(IceCandidateInterface* candidate) { delete candidate; }

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

    UNITY_INTERFACE_EXPORT PeerConnectionInterface::IceConnectionState
    PeerConnectionIceConditionState(PeerConnectionObject* obj)
    {
        return obj->connection->ice_connection_state();
    }

    UNITY_INTERFACE_EXPORT PeerConnectionInterface::SignalingState
    PeerConnectionSignalingState(PeerConnectionObject* obj)
    {
        return obj->connection->signaling_state();
    }

    UNITY_INTERFACE_EXPORT PeerConnectionInterface::IceGatheringState
    PeerConnectionIceGatheringState(PeerConnectionObject* obj)
    {
        return obj->connection->ice_gathering_state();
    }

    UNITY_INTERFACE_EXPORT void
    PeerConnectionRegisterOnDataChannel(PeerConnectionObject* obj, DelegateOnDataChannel callback)
    {
        obj->RegisterOnDataChannel(callback);
    }

    UNITY_INTERFACE_EXPORT void
    PeerConnectionRegisterOnRenegotiationNeeded(PeerConnectionObject* obj, DelegateOnRenegotiationNeeded callback)
    {
        obj->RegisterOnRenegotiationNeeded(callback);
    }

    UNITY_INTERFACE_EXPORT void PeerConnectionRegisterOnTrack(PeerConnectionObject* obj, DelegateOnTrack callback)
    {
        obj->RegisterOnTrack(callback);
    }

    UNITY_INTERFACE_EXPORT void
    PeerConnectionRegisterOnRemoveTrack(PeerConnectionObject* obj, DelegateOnRemoveTrack callback)
    {
        obj->RegisterOnRemoveTrack(callback);
    }

    UNITY_INTERFACE_EXPORT bool
    TransceiverGetCurrentDirection(RtpTransceiverInterface* transceiver, RtpTransceiverDirection* direction)
    {
        if (transceiver->current_direction().has_value())
        {
            *direction = transceiver->current_direction().value();
            return true;
        }
        return false;
    }

    UNITY_INTERFACE_EXPORT RTCErrorType TransceiverStop(RtpTransceiverInterface* transceiver)
    {
        auto error = transceiver->StopStandard();
        return error.type();
    }

    UNITY_INTERFACE_EXPORT RtpTransceiverDirection TransceiverGetDirection(RtpTransceiverInterface* transceiver)
    {
        return transceiver->direction();
    }

    UNITY_INTERFACE_EXPORT RTCErrorType
    TransceiverSetDirection(RtpTransceiverInterface* transceiver, RtpTransceiverDirection direction)
    {
        RTCError error = transceiver->SetDirectionWithError(direction);
        return error.type();
    }

    struct RTCRtpCodecCapability
    {
        char* mimeType;
        Optional<int32_t> clockRate;
        Optional<int32_t> channels;
        char* sdpFmtpLine;

        RTCRtpCodecCapability& operator=(const RtpCodecCapability& obj)
        {
            this->mimeType = ConvertString(obj.mime_type());
            this->clockRate = obj.clock_rate;
            this->channels = obj.num_channels;
            this->sdpFmtpLine = ConvertString(ConvertSdp(obj.parameters));
            return *this;
        }
    };

    UNITY_INTERFACE_EXPORT RTCErrorType
    TransceiverSetCodecPreferences(RtpTransceiverInterface* transceiver, RTCRtpCodecCapability* codecs, size_t length)
    {
        std::vector<RtpCodecCapability> _codecs(length);
        for (size_t i = 0; i < length; i++)
        {
            std::string mimeType = ConvertString(codecs[i].mimeType);
            std::tie(_codecs[i].kind, _codecs[i].name) = ConvertMimeType(mimeType);
            _codecs[i].clock_rate = ConvertOptional(codecs[i].clockRate);
            _codecs[i].num_channels = ConvertOptional(codecs[i].channels);
            _codecs[i].parameters = ConvertSdp(codecs[i].sdpFmtpLine);
        }
        auto error = transceiver->SetCodecPreferences(_codecs);
        if (error.type() != RTCErrorType::NONE)
            RTC_LOG(LS_ERROR) << error.message();
        return error.type();
    }

    UNITY_INTERFACE_EXPORT char* TransceiverGetMid(RtpTransceiverInterface* transceiver)
    {
        auto mid = transceiver->mid();
        if (!mid.has_value())
        {
            return nullptr;
        }
        return ConvertString(mid.value());
    }

    UNITY_INTERFACE_EXPORT RtpReceiverInterface* TransceiverGetReceiver(RtpTransceiverInterface* transceiver)
    {
        return transceiver->receiver().get();
    }

    UNITY_INTERFACE_EXPORT RtpSenderInterface* TransceiverGetSender(RtpTransceiverInterface* transceiver)
    {
        return transceiver->sender().get();
    }

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
            id = static_cast<uint16_t>(src.id);
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
            dst.encodings[i].max_bitrate_bps =
                static_cast<absl::optional<int>>(ConvertOptional(src->encodings[i].maxBitrate));
            dst.encodings[i].min_bitrate_bps =
                static_cast<absl::optional<int>>(ConvertOptional(src->encodings[i].minBitrate));
            dst.encodings[i].max_framerate =
                static_cast<absl::optional<double>>(ConvertOptional(src->encodings[i].maxFramerate));
            dst.encodings[i].scale_resolution_down_by = ConvertOptional(src->encodings[i].scaleResolutionDownBy);
            if (src->encodings[i].rid != nullptr)
                dst.encodings[i].rid = std::string(src->encodings[i].rid);
        }
        const ::webrtc::RTCError error = sender->SetParameters(dst);
        return error.type();
    }

    struct RTCRtpHeaderExtensionCapability
    {
        char* uri;

        RTCRtpHeaderExtensionCapability& operator=(const RtpHeaderExtensionCapability& obj)
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

    UNITY_INTERFACE_EXPORT void
    ContextGetSenderCapabilities(Context* context, TrackKind trackKind, RTCRtpCapabilities** parameters)
    {
        RtpCapabilities src;
        cricket::MediaType type = trackKind == TrackKind::Audio ? cricket::MEDIA_TYPE_AUDIO : cricket::MEDIA_TYPE_VIDEO;
        context->GetRtpSenderCapabilities(type, &src);

        RTCRtpCapabilities* dst = static_cast<RTCRtpCapabilities*>(CoTaskMemAlloc(sizeof(RTCRtpCapabilities)));
        *dst = src;
        *parameters = dst;
    }

    UNITY_INTERFACE_EXPORT void
    ContextGetReceiverCapabilities(Context* context, TrackKind trackKind, RTCRtpCapabilities** parameters)
    {
        RtpCapabilities src;
        cricket::MediaType type = trackKind == TrackKind::Audio ? cricket::MEDIA_TYPE_AUDIO : cricket::MEDIA_TYPE_VIDEO;
        context->GetRtpReceiverCapabilities(type, &src);

        RTCRtpCapabilities* dst = static_cast<RTCRtpCapabilities*>(CoTaskMemAlloc(sizeof(RTCRtpCapabilities)));
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

    UNITY_INTERFACE_EXPORT void SenderSetTransform(RtpSenderInterface* sender, FrameTransformerInterface* transformer)
    {
        sender->SetEncoderToPacketizerFrameTransformer(rtc::scoped_refptr<FrameTransformerInterface>(transformer));
    }

    UNITY_INTERFACE_EXPORT MediaStreamTrackInterface* ReceiverGetTrack(RtpReceiverInterface* receiver)
    {
        return receiver->track().get();
    }

    UNITY_INTERFACE_EXPORT MediaStreamInterface** ReceiverGetStreams(RtpReceiverInterface* receiver, size_t* length)
    {
        return ConvertPtrArrayFromRefPtrArray<MediaStreamInterface>(receiver->streams(), length);
    }

    UNITY_INTERFACE_EXPORT int DataChannelGetID(DataChannelInterface* channel) { return channel->id(); }

    struct RtpSource
    {
        Optional<uint8_t> audioLevel;
        uint8_t sourceType;
        uint32_t source;
        uint32_t rtpTimestamp;
        int64_t timestamp;

        RtpSource(const webrtc::RtpSource& src)
        {
            audioLevel = src.audio_level();
            rtpTimestamp = src.rtp_timestamp();
            source = src.source_id();
            sourceType = static_cast<uint8_t>(src.source_type());
            timestamp = src.timestamp_ms();
        }
    };

    UNITY_INTERFACE_EXPORT ::RtpSource* ReceiverGetSources(RtpReceiverInterface* receiver, size_t* length)
    {
        auto sources = receiver->GetSources();
        if (sources.empty())
            return nullptr;

        std::vector<::RtpSource> result;
        std::transform(sources.begin(), sources.end(), std::back_inserter(result), [](webrtc::RtpSource source) {
            return source;
        });
        return ConvertArray(result, length);
    }

    UNITY_INTERFACE_EXPORT void
    ReceiverSetTransform(RtpReceiverInterface* receiver, FrameTransformerInterface* transformer)
    {
        receiver->SetDepacketizerToDecoderFrameTransformer(rtc::scoped_refptr<FrameTransformerInterface>(transformer));
    }

    UNITY_INTERFACE_EXPORT char* DataChannelGetLabel(DataChannelInterface* channel)
    {
        return ConvertString(channel->label());
    }

    UNITY_INTERFACE_EXPORT char* DataChannelGetProtocol(DataChannelInterface* channel)
    {
        return ConvertString(channel->protocol());
    }

    UNITY_INTERFACE_EXPORT uint16_t DataChannelGetMaxRetransmits(DataChannelInterface* channel)
    {
        return channel->maxRetransmits();
    }

    UNITY_INTERFACE_EXPORT uint16_t DataChannelGetMaxRetransmitTime(DataChannelInterface* channel)
    {
        return channel->maxRetransmitTime();
    }

    UNITY_INTERFACE_EXPORT bool DataChannelGetOrdered(DataChannelInterface* channel) { return channel->ordered(); }

    UNITY_INTERFACE_EXPORT uint64_t DataChannelGetBufferedAmount(DataChannelInterface* channel)
    {
        return channel->buffered_amount();
    }

    UNITY_INTERFACE_EXPORT bool DataChannelGetNegotiated(DataChannelInterface* channel)
    {
        return channel->negotiated();
    }

    UNITY_INTERFACE_EXPORT DataChannelInterface::DataState DataChannelGetReadyState(DataChannelInterface* channel)
    {
        return channel->state();
    }

    UNITY_INTERFACE_EXPORT void DataChannelSend(DataChannelInterface* channel, const char* data)
    {
        channel->Send(webrtc::DataBuffer(std::string(data)));
    }

    UNITY_INTERFACE_EXPORT void DataChannelSendBinary(DataChannelInterface* channel, const byte* data, int length)
    {
        rtc::CopyOnWriteBuffer buf(data, static_cast<size_t>(length));
        channel->Send(webrtc::DataBuffer(buf, true));
    }

    UNITY_INTERFACE_EXPORT void DataChannelClose(DataChannelInterface* channel) { channel->Close(); }

    UNITY_INTERFACE_EXPORT void
    DataChannelRegisterOnMessage(Context* context, DataChannelInterface* channel, DelegateOnMessage callback)
    {
        context->GetDataChannelObject(channel)->RegisterOnMessage(callback);
    }

    UNITY_INTERFACE_EXPORT void
    DataChannelRegisterOnOpen(Context* context, DataChannelInterface* channel, DelegateOnOpen callback)
    {
        context->GetDataChannelObject(channel)->RegisterOnOpen(callback);
    }

    UNITY_INTERFACE_EXPORT void
    DataChannelRegisterOnClose(Context* context, DataChannelInterface* channel, DelegateOnClose callback)
    {
        context->GetDataChannelObject(channel)->RegisterOnClose(callback);
    }

    UNITY_INTERFACE_EXPORT void SetCurrentContext(Context* context)
    {
        ContextManager::GetInstance()->curContext = context;
    }

    UNITY_INTERFACE_EXPORT void AudioSourceProcessLocalAudio(
        UnityAudioTrackSource* source,
        float* audio_data,
        int32 sample_rate,
        int32 number_of_channels,
        int32 number_of_frames)
    {
        if (source != nullptr)
        {
            source->PushAudioData(
                audio_data,
                sample_rate,
                static_cast<size_t>(number_of_channels),
                static_cast<size_t>(number_of_frames));
        }
    }

    UNITY_INTERFACE_EXPORT AudioTrackSinkAdapter* ContextCreateAudioTrackSink(Context* context)
    {
        return context->CreateAudioTrackSinkAdapter();
    }

    UNITY_INTERFACE_EXPORT void ContextDeleteAudioTrackSink(Context* context, AudioTrackSinkAdapter* sink)
    {
        return context->DeleteAudioTrackSinkAdapter(sink);
    }

    UNITY_INTERFACE_EXPORT void AudioTrackAddSink(AudioTrackInterface* track, AudioTrackSinkInterface* sink)
    {
        track->AddSink(sink);
    }

    UNITY_INTERFACE_EXPORT void AudioTrackRemoveSink(AudioTrackInterface* track, AudioTrackSinkInterface* sink)
    {
        track->RemoveSink(sink);
    }

    UNITY_INTERFACE_EXPORT void
    AudioTrackSinkProcessAudio(AudioTrackSinkAdapter* sink, float* data, size_t length, int channels, int sampleRate)
    {
        sink->ProcessAudio(data, length, static_cast<size_t>(channels), sampleRate);
    }

    UNITY_INTERFACE_EXPORT uint32_t FrameGetTimestamp(TransformableFrameInterface* frame)
    {
        return frame->GetTimestamp();
    }

    UNITY_INTERFACE_EXPORT uint32_t FrameGetSsrc(TransformableFrameInterface* frame) { return frame->GetSsrc(); }

    UNITY_INTERFACE_EXPORT bool VideoFrameIsKeyFrame(TransformableVideoFrameInterface* frame)
    {
        return frame->IsKeyFrame();
    }

    struct RTCVideoFrameMetadata
    {
        Optional<int64_t> frameId;
        uint16_t width;
        uint16_t height;
        int spacialIndex;
        int temporalIndex;
        MarshallArray<int64_t> dependencies;
    };

    UNITY_INTERFACE_EXPORT RTCVideoFrameMetadata* VideoFrameGetMetadata(TransformableVideoFrameInterface* frame)
    {
        RTCVideoFrameMetadata* data =
            static_cast<RTCVideoFrameMetadata*>(CoTaskMemAlloc(sizeof(RTCVideoFrameMetadata)));

        auto metadata = frame->GetMetadata();

        data->frameId = metadata.GetFrameId();
        data->width = metadata.GetWidth();
        data->height = metadata.GetHeight();
        data->spacialIndex = metadata.GetSpatialIndex();
        data->temporalIndex = metadata.GetTemporalIndex();
        data->dependencies = metadata.GetFrameDependencies();

        return data;
    }

    UNITY_INTERFACE_EXPORT void
    FrameTransformerSendFrameToSink(EncodedStreamTransformer* transformer, TransformableFrameInterface* frame)
    {
        transformer->SendFrameToSink(std::unique_ptr<TransformableFrameInterface>(frame));
    }

    UNITY_INTERFACE_EXPORT void FrameGetData(TransformableFrameInterface* frame, const uint8_t** data, size_t* size)
    {
        auto data_ = frame->GetData();
        *size = data_.size();
        *data = data_.data();
    }

    UNITY_INTERFACE_EXPORT void FrameSetData(TransformableFrameInterface* frame, const uint8_t* data, size_t size)
    {
        frame->SetData(rtc::ArrayView<const uint8_t>(data, size));
    }
#pragma clang diagnostic pop
}
