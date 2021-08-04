#include "pch.h"
#include "WebRTCPlugin.h"
#include "Context.h"

#include "UnityAudioEncoderFactory.h"
#include "UnityAudioDecoderFactory.h"

#if defined(UNITY_WIN) || defined(UNITY_LINUX)
#include "Codec/NvCodec/NvEncoder.h"
#endif

#include "AudioTrackSinkAdapter.h"
#include "DummyVideoEncoder.h"
#include "MediaStreamObserver.h"
#include "SetSessionDescriptionObserver.h"
#include "UnityAudioTrackSource.h"
#include "UnityVideoEncoderFactory.h"
#include "UnityVideoDecoderFactory.h"
#include "UnityVideoTrackSource.h"

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

    ContextManager ContextManager::s_instance;

    Context* ContextManager::GetContext(int uid) const
    {
        auto it = s_instance.m_contexts.find(uid);
        if (it != s_instance.m_contexts.end()) {
            return it->second.get();
        }
        return nullptr;
    }

    Context* ContextManager::CreateContext(int uid, UnityEncoderType encoderType, bool forTest)
    {
        auto it = s_instance.m_contexts.find(uid);
        if (it != s_instance.m_contexts.end()) {
            DebugLog("Using already created context with ID %d", uid);
            return nullptr;
        }
        auto ctx = new Context(uid, encoderType, forTest);
        s_instance.m_contexts[uid].reset(ctx);
        return ctx;
    }

    void ContextManager::SetCurContext(Context* context)
    {
        curContext = context;
    }

    bool ContextManager::Exists(Context *context)
    {
        for(auto it = s_instance.m_contexts.begin(); it != s_instance.m_contexts.end(); ++it)
        {
            if(it->second.get() == context)
                return true;
        }
        return false;
    }

    void ContextManager::DestroyContext(int uid)
    {
        auto it = s_instance.m_contexts.find(uid);
        if (it != s_instance.m_contexts.end())
        {
            s_instance.m_contexts.erase(it);
            DebugLog("Unregistered context with ID %d", uid);
        }
    }

    ContextManager::~ContextManager()
    {
        if (m_contexts.size()) {
            DebugWarning("%lu remaining context(s) registered", m_contexts.size());
        }
        m_contexts.clear();
    }

#pragma region open an encode session
    uint32_t Context::s_encoderId = 0;
    uint32_t Context::GenerateUniqueId() { return s_encoderId++; }
#pragma endregion 

    bool Convert(const std::string& str, webrtc::PeerConnectionInterface::RTCConfiguration& config)
    {
        config = webrtc::PeerConnectionInterface::RTCConfiguration{};
        Json::CharReaderBuilder builder;
        const std::unique_ptr<Json::CharReader> reader(builder.newCharReader());
        Json::Value configJson;
        Json::String err;
        auto ok = reader->parse(str.c_str(), str.c_str() + static_cast<int>(str.length()), &configJson, &err);
        if (!ok)
        {
            //json parse failed.
            return false;
        }

        Json::Value iceServersJson = configJson["iceServers"];
        if (!iceServersJson)
            return false;
        for (auto iceServerJson : iceServersJson)
        {
            webrtc::PeerConnectionInterface::IceServer iceServer;
            for (auto url : iceServerJson["urls"])
            {
                iceServer.urls.push_back(url.asString());
            }
            if (!iceServerJson["username"].isNull())
            {
                iceServer.username = iceServerJson["username"].asString();
            }
            if (!iceServerJson["credential"].isNull())
            {
                iceServer.password = iceServerJson["credential"].asString();
            }
            config.servers.push_back(iceServer);
        }
        Json::Value iceTransportPolicy = configJson["iceTransportPolicy"];
        if(iceTransportPolicy["hasValue"].asBool())
        {
            config.type = static_cast<PeerConnectionInterface::IceTransportsType>(iceTransportPolicy["value"].asInt());
        }
        Json::Value enableDtlsSrtp = configJson["enableDtlsSrtp"];
        if (enableDtlsSrtp["hasValue"].asBool())
        {
            config.enable_dtls_srtp = enableDtlsSrtp["value"].asBool();
        }
        Json::Value iceCandidatePoolSize = configJson["iceCandidatePoolSize"];
        if (iceCandidatePoolSize["hasValue"].asBool())
        {
            config.ice_candidate_pool_size = iceCandidatePoolSize["value"].asInt();
        }
        Json::Value bundlePolicy = configJson["bundlePolicy"];
        if (bundlePolicy["hasValue"].asBool())
        {
            config.bundle_policy = static_cast<PeerConnectionInterface::BundlePolicy>(bundlePolicy["value"].asInt());
        }
        config.sdp_semantics = webrtc::SdpSemantics::kUnifiedPlan;
        config.enable_implicit_rollback = true;
        return true;
    }
#pragma warning(push)
#pragma warning(disable: 4715)
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
        default:
            throw std::invalid_argument("Unknown SdpType");
        }
    }
#pragma warning(pop)

    Context::Context(int uid, UnityEncoderType encoderType, bool forTest)
        : m_uid(uid)
        , m_encoderType(encoderType)
    {
        m_workerThread.reset(new rtc::Thread(rtc::SocketServer::CreateDefault()));
        m_workerThread->Start();
        m_signalingThread.reset(new rtc::Thread(rtc::SocketServer::CreateDefault()));
        m_signalingThread->Start();

        rtc::InitializeSSL();

        m_workerThread->Invoke<void>(
            RTC_FROM_HERE,
            [this]()
            {
                m_audioDevice = new rtc::RefCountedObject<DummyAudioDevice>();
            });

        std::unique_ptr<webrtc::VideoEncoderFactory> videoEncoderFactory =
            m_encoderType == UnityEncoderType::UnityEncoderHardware ?
            std::make_unique<UnityVideoEncoderFactory>(static_cast<IVideoEncoderObserver*>(this)) :
            webrtc::CreateBuiltinVideoEncoderFactory();

        std::unique_ptr<webrtc::VideoDecoderFactory> videoDecoderFactory =
            m_encoderType == UnityEncoderType::UnityEncoderHardware ?
            std::make_unique<UnityVideoDecoderFactory>(forTest) :
            webrtc::CreateBuiltinVideoDecoderFactory();

        rtc::scoped_refptr<AudioEncoderFactory> audioEncoderFactory = CreateAudioEncoderFactory();
        rtc::scoped_refptr<AudioDecoderFactory>  audioDecoderFactory = CreateAudioDecoderFactory();

        m_peerConnectionFactory = CreatePeerConnectionFactory(
                                m_workerThread.get(),
                                m_workerThread.get(),
                                m_signalingThread.get(),
                                m_audioDevice,
                                audioEncoderFactory,
                                audioDecoderFactory,
                                std::move(videoEncoderFactory),
                                std::move(videoDecoderFactory),
                                nullptr,
                                nullptr);
    }

    Context::~Context()
    {
        {
            std::lock_guard<std::mutex> lock(mutex);

            m_peerConnectionFactory = nullptr;
            m_workerThread->Invoke<void>(
                RTC_FROM_HERE,
                [this]()
                {
                    m_audioDevice = nullptr;
                });
            m_mapIdAndEncoder.clear();
            m_mapClients.clear();

            // check count of refptr to avoid to forget disposing
            RTC_DCHECK_EQ(m_mapRefPtr.size(), 0);

            m_mapRefPtr.clear();
            m_mapMediaStreamObserver.clear();
            m_mapSetSessionDescriptionObserver.clear();
            m_mapVideoEncoderParameter.clear();
            m_mapDataChannels.clear();
            m_mapVideoRenderer.clear();

            m_workerThread->Quit();
            m_workerThread.reset();
            m_signalingThread->Quit();
            m_signalingThread.reset();
        }
    }

    UnityVideoTrackSource* Context::GetVideoSource(const MediaStreamTrackInterface* track)
    {
        if (!ExistsRefPtr(track))
        {
            RTC_LOG(LS_INFO) << "track is not found";
            return nullptr;
        }

        const VideoTrackInterface* videoTrack = static_cast<const VideoTrackInterface*>(track);
        webrtc::VideoTrackSourceInterface* source = videoTrack->GetSource();

        if (!ExistsRefPtr(source))
        {
            RTC_LOG(LS_INFO) << "source is not found";
            return nullptr;
        }
        return static_cast<UnityVideoTrackSource*>(source);
    }

    bool Context::InitializeEncoder(IEncoder* encoder, MediaStreamTrackInterface* track)
    {
        UnityVideoTrackSource* source = GetVideoSource(track);
        if (source == nullptr)
            return false;
        if (encoder->GetCodecInitializationResult() != CodecInitializationResult::Success)
            return false;

        source->SetEncoder(encoder);

        uint32_t id = GenerateUniqueId();
        encoder->SetEncoderId(id);
        m_mapIdAndEncoder[id] = encoder;
        return true;
    }

    bool Context::FinalizeEncoder(IEncoder* encoder)
    {
        m_mapIdAndEncoder.erase(encoder->Id());
        return true;
    }

    const VideoEncoderParameter* Context::GetEncoderParameter(const MediaStreamTrackInterface* track)
    {
        return m_mapVideoEncoderParameter[track].get();
    }

    void Context::SetEncoderParameter(
        const MediaStreamTrackInterface* track,
        int width,
        int height,
        UnityRenderingExtTextureFormat textureFormat,
        void* textureHandle)
    {
        m_mapVideoEncoderParameter[track] =
            std::make_unique<VideoEncoderParameter>(
                width, height, textureFormat, textureHandle);
    }

    void Context::SetKeyFrame(uint32_t id)
    {
        if (m_mapIdAndEncoder.count(id))
        {
            m_mapIdAndEncoder[id]->SetIdrFrame();
        }
    }

    void Context::SetRates(uint32_t id, uint32_t bitRate, int64_t frameRate)
    {
        if(m_mapIdAndEncoder.count(id))
        {
            m_mapIdAndEncoder[id]->SetRates(bitRate, frameRate);
        }
    }

    UnityEncoderType Context::GetEncoderType() const
    {
        return m_encoderType;
    }

    CodecInitializationResult Context::GetInitializationResult(MediaStreamTrackInterface* track)
    {
        UnityVideoTrackSource* source = GetVideoSource(track);
        if (source != nullptr)
            return source->GetCodecInitializationResult();
        return CodecInitializationResult::NotInitialized;
    }

    webrtc::MediaStreamInterface* Context::CreateMediaStream(const std::string& streamId)
    {
        rtc::scoped_refptr<webrtc::MediaStreamInterface> stream =
            m_peerConnectionFactory->CreateLocalMediaStream(streamId);
        AddRefPtr(stream);
        return stream;
    }

    void Context::RegisterMediaStreamObserver(webrtc::MediaStreamInterface* stream)
    {
        m_mapMediaStreamObserver[stream] = std::make_unique<MediaStreamObserver>(stream, this);
    }

    void Context::UnRegisterMediaStreamObserver(webrtc::MediaStreamInterface* stream)
    {
        m_mapMediaStreamObserver.erase(stream);
    }

    MediaStreamObserver* Context::GetObserver(
        const webrtc::MediaStreamInterface* stream)
    {
        return m_mapMediaStreamObserver[stream].get();
    }

    VideoTrackSourceInterface* Context::CreateVideoSource()
    {
        rtc::scoped_refptr<UnityVideoTrackSource> source =
            new rtc::RefCountedObject<UnityVideoTrackSource>(false, nullptr);

        AddRefPtr(source);
        return source;
    }

    webrtc::VideoTrackInterface* Context::CreateVideoTrack(
        const std::string& label, VideoTrackSourceInterface* source)
    {
        const rtc::scoped_refptr<VideoTrackInterface> track =
            m_peerConnectionFactory->CreateVideoTrack(label, source);

        AddRefPtr(track);
        return track;
    }

    void Context::StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track)
    {
        // todo:(kazuki)
    }

    webrtc::AudioSourceInterface* Context::CreateAudioSource()
    {
        //avoid optimization specially for voice
        cricket::AudioOptions audioOptions;
        audioOptions.auto_gain_control = false;
        audioOptions.noise_suppression = false;
        audioOptions.highpass_filter = false;

        const rtc::scoped_refptr<UnityAudioTrackSource> source =
            UnityAudioTrackSource::Create(audioOptions);

        AddRefPtr(source);
        return source;
    }

    AudioTrackInterface* Context::CreateAudioTrack(const std::string& label, webrtc::AudioSourceInterface* source)
    {
        const rtc::scoped_refptr<AudioTrackInterface> track =
            m_peerConnectionFactory->CreateAudioTrack(label, source);

        AddRefPtr(track);
        return track;
    }


    void Context::RegisterAudioReceiveCallback(
        AudioTrackInterface* track, DelegateAudioReceive callback)
    {
        if (m_mapAudioTrackAndSink.find(track) != m_mapAudioTrackAndSink.end())
        {
            RTC_LOG(LS_WARNING) << "The callback is already registered.";
            return;
        }

        std::unique_ptr<AudioTrackSinkAdapter> sink =
            std::make_unique<AudioTrackSinkAdapter>(track, callback);

        track->AddSink(sink.get());
        m_mapAudioTrackAndSink[track] = std::move(sink);
    }

    void Context::UnregisterAudioReceiveCallback(
        AudioTrackInterface* track)
    {
        if (m_mapAudioTrackAndSink.find(track) == m_mapAudioTrackAndSink.end())
            return;

        AudioTrackSinkInterface* sink = m_mapAudioTrackAndSink[track].get();
        track->RemoveSink(sink);
        m_mapAudioTrackAndSink.erase(track);
    }

    void Context::AddStatsReport(const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report)
    {
        m_listStatsReport.push_back(report);
    }

    void Context::DeleteStatsReport(const webrtc::RTCStatsReport* report)
    {
        auto found = std::find_if(m_listStatsReport.begin(), m_listStatsReport.end(),
             [report](rtc::scoped_refptr<const webrtc::RTCStatsReport> it){ return it.get() == report; });
        m_listStatsReport.erase(found);
	}

    DataChannelObject* Context::CreateDataChannel(PeerConnectionObject* obj, const char* label, const DataChannelInit& options)
    {
        auto channel = obj->connection->CreateDataChannel(label, &options);
        if (channel == nullptr)
            return nullptr;
        auto dataChannelObj = std::make_unique<DataChannelObject>(channel, *obj);
        DataChannelObject* ptr = dataChannelObj.get();
        m_mapDataChannels[ptr] = std::move(dataChannelObj);
        return ptr;
    }

    void Context::AddDataChannel(std::unique_ptr<DataChannelObject> channel) {
        const auto ptr = channel.get();
        m_mapDataChannels[ptr] = std::move(channel);
    }

    void Context::DeleteDataChannel(DataChannelObject* obj)
    {
        if (m_mapDataChannels.count(obj) > 0)
        {
            m_mapDataChannels.erase(obj);
        }
    }

    void Context::AddObserver(const webrtc::PeerConnectionInterface* connection, const rtc::scoped_refptr<SetSessionDescriptionObserver>& observer)
    {
        m_mapSetSessionDescriptionObserver[connection] = observer;
    }

    void Context::RemoveObserver(const webrtc::PeerConnectionInterface* connection)
    {
        m_mapSetSessionDescriptionObserver.erase(connection);
    }

    PeerConnectionObject* Context::CreatePeerConnection(
            const webrtc::PeerConnectionInterface::RTCConfiguration& config)
    {
        rtc::scoped_refptr<PeerConnectionObject> obj =
                new rtc::RefCountedObject<PeerConnectionObject>(*this);
        PeerConnectionDependencies dependencies(obj);
        obj->connection = m_peerConnectionFactory->CreatePeerConnection(
                config, std::move(dependencies));
        if (obj->connection == nullptr)
            return nullptr;
        const PeerConnectionObject* ptr = obj.get();
        m_mapClients[ptr] = std::move(obj);
        return m_mapClients[ptr].get();
    }

    void Context::DeletePeerConnection(PeerConnectionObject *obj)
    {
        m_mapClients.erase(obj);
    }

    SetSessionDescriptionObserver* Context::GetObserver(webrtc::PeerConnectionInterface* connection)
    {
        return m_mapSetSessionDescriptionObserver[connection];
    }

    uint32_t Context::s_rendererId = 0;
    uint32_t Context::GenerateRendererId() { return s_rendererId++; }

    UnityVideoRenderer* Context::CreateVideoRenderer()
    {
        auto rendererId = GenerateRendererId();
        auto renderer = std::make_shared<UnityVideoRenderer>(rendererId);
        m_mapVideoRenderer[rendererId] = renderer;
        return m_mapVideoRenderer[rendererId].get();
    }

    std::shared_ptr<UnityVideoRenderer> Context::GetVideoRenderer(uint32_t id)
    {
        return m_mapVideoRenderer[id];
    }

    void Context::DeleteVideoRenderer(UnityVideoRenderer* renderer)
    {
        m_mapVideoRenderer.erase(renderer->GetId());
        renderer = nullptr;
    }

    void Context::GetRtpSenderCapabilities(
        cricket::MediaType kind, RtpCapabilities* capabilities) const
    {
        *capabilities = m_peerConnectionFactory->GetRtpSenderCapabilities(kind);
    }

    void Context::GetRtpReceiverCapabilities(
        cricket::MediaType kind, RtpCapabilities* capabilities) const
    {
        *capabilities = m_peerConnectionFactory->GetRtpReceiverCapabilities(kind);
    }

} // end namespace webrtc
} // end namespace unity
