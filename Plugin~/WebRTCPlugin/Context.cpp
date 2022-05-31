#include "pch.h"

#include <api/create_peerconnection_factory.h>
#include <api/task_queue/default_task_queue_factory.h>
#include <rtc_base/ssl_adapter.h>
#include <rtc_base/strings/json.h>

#include "AudioTrackSinkAdapter.h"
#include "Context.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "MediaStreamObserver.h"
#include "SetSessionDescriptionObserver.h"
#include "UnityAudioDecoderFactory.h"
#include "UnityAudioEncoderFactory.h"
#include "UnityAudioTrackSource.h"
#include "UnityVideoDecoderFactory.h"
#include "UnityVideoEncoderFactory.h"
#include "UnityVideoTrackSource.h"
#include "WebRTCPlugin.h"

#if CUDA_PLATFORM
#include "Logger.h"
simplelogger::Logger* logger = simplelogger::LoggerFactory::CreateConsoleLogger();
#endif

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{
    std::unique_ptr<ContextManager> ContextManager::s_instance;

    ContextManager* ContextManager::GetInstance()
    {
        if (s_instance == nullptr)
        {
            s_instance = std::make_unique<ContextManager>();
        }
        return s_instance.get();
    }

    Context* ContextManager::GetContext(int uid) const
    {
        auto it = s_instance->m_contexts.find(uid);
        if (it != s_instance->m_contexts.end())
        {
            return it->second.get();
        }
        return nullptr;
    }

    Context* ContextManager::CreateContext(int uid, ContextDependencies& dependencies)
    {
        auto it = s_instance->m_contexts.find(uid);
        if (it != s_instance->m_contexts.end())
        {
            DebugLog("Using already created context with ID %d", uid);
            return nullptr;
        }
        s_instance->m_contexts[uid] = std::make_unique<Context>(dependencies);
        return s_instance->m_contexts[uid].get();
    }

    void ContextManager::SetCurContext(Context* context) { curContext = context; }

    bool ContextManager::Exists(Context* context)
    {
        for (auto it = s_instance->m_contexts.begin(); it != s_instance->m_contexts.end(); ++it)
        {
            if (it->second.get() == context)
                return true;
        }
        return false;
    }

    void ContextManager::DestroyContext(int uid)
    {
        auto it = s_instance->m_contexts.find(uid);
        if (it != s_instance->m_contexts.end())
        {
            s_instance->m_contexts.erase(it);
        }
    }

    ContextManager::~ContextManager()
    {
        if (m_contexts.size())
        {
            DebugWarning("%lu remaining context(s) registered", m_contexts.size());
        }
        m_contexts.clear();
    }

    bool Convert(const std::string& str, PeerConnectionInterface::RTCConfiguration& config)
    {
        config = PeerConnectionInterface::RTCConfiguration {};
        Json::CharReaderBuilder builder;
        const std::unique_ptr<Json::CharReader> reader(builder.newCharReader());
        Json::Value configJson;
        Json::String err;
        auto ok = reader->parse(str.c_str(), str.c_str() + static_cast<int>(str.length()), &configJson, &err);
        if (!ok)
        {
            // json parse failed.
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
        if (iceTransportPolicy["hasValue"].asBool())
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

    Context::Context(ContextDependencies& dependencies)
        : m_workerThread(rtc::Thread::CreateWithSocketServer())
        , m_signalingThread(rtc::Thread::CreateWithSocketServer())
        , m_taskQueueFactory(CreateDefaultTaskQueueFactory())
    {
        m_workerThread->Start();
        m_signalingThread->Start();

        rtc::InitializeSSL();

        m_audioDevice = m_workerThread->Invoke<rtc::scoped_refptr<DummyAudioDevice>>(
            RTC_FROM_HERE, [&]() { return new rtc::RefCountedObject<DummyAudioDevice>(m_taskQueueFactory.get()); });

        std::unique_ptr<webrtc::VideoEncoderFactory> videoEncoderFactory =
            std::make_unique<UnityVideoEncoderFactory>(dependencies.device, dependencies.profiler);

        std::unique_ptr<webrtc::VideoDecoderFactory> videoDecoderFactory =
            std::make_unique<UnityVideoDecoderFactory>(dependencies.device, dependencies.profiler);

        rtc::scoped_refptr<AudioEncoderFactory> audioEncoderFactory = CreateAudioEncoderFactory();
        rtc::scoped_refptr<AudioDecoderFactory> audioDecoderFactory = CreateAudioDecoderFactory();

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
            m_workerThread->Invoke<void>(RTC_FROM_HERE, [this]() { m_audioDevice = nullptr; });
            m_mapClients.clear();

            // check count of refptr to avoid to forget disposing
            RTC_DCHECK_EQ(m_mapRefPtr.size(), 0);

            m_mapRefPtr.clear();
            m_mapMediaStreamObserver.clear();
            m_mapSetSessionDescriptionObserver.clear();
            m_mapDataChannels.clear();
            m_mapVideoRenderer.clear();

            m_workerThread->Quit();
            m_workerThread.reset();
            m_signalingThread->Quit();
            m_signalingThread.reset();
        }
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

    MediaStreamObserver* Context::GetObserver(const webrtc::MediaStreamInterface* stream)
    {
        return m_mapMediaStreamObserver[stream].get();
    }

    VideoTrackSourceInterface* Context::CreateVideoSource()
    {
        const rtc::scoped_refptr<UnityVideoTrackSource> source =
            new rtc::RefCountedObject<UnityVideoTrackSource>(false, absl::nullopt, m_taskQueueFactory.get());
        AddRefPtr(source);
        return source;
    }

    webrtc::VideoTrackInterface* Context::CreateVideoTrack(const std::string& label, VideoTrackSourceInterface* source)
    {
        const rtc::scoped_refptr<VideoTrackInterface> track = m_peerConnectionFactory->CreateVideoTrack(label, source);
        AddRefPtr(track);
        return track;
    }

    void Context::StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track)
    {
        // todo:(kazuki)
    }

    webrtc::AudioSourceInterface* Context::CreateAudioSource()
    {
        // avoid optimization specially for voice
        cricket::AudioOptions audioOptions;
        audioOptions.auto_gain_control = false;
        audioOptions.noise_suppression = false;
        audioOptions.highpass_filter = false;

        const rtc::scoped_refptr<UnityAudioTrackSource> source = UnityAudioTrackSource::Create(audioOptions);

        AddRefPtr(source);
        return source;
    }

    AudioTrackInterface* Context::CreateAudioTrack(const std::string& label, webrtc::AudioSourceInterface* source)
    {
        const rtc::scoped_refptr<AudioTrackInterface> track = m_peerConnectionFactory->CreateAudioTrack(label, source);
        AddRefPtr(track);
        return track;
    }

    AudioTrackSinkAdapter* Context::CreateAudioTrackSinkAdapter()
    {
        auto sink = std::make_unique<AudioTrackSinkAdapter>();
        AudioTrackSinkAdapter* ptr = sink.get();
        m_mapAudioTrackAndSink.emplace(ptr, std::move(sink));
        return ptr;
    }

    void Context::DeleteAudioTrackSinkAdapter(AudioTrackSinkAdapter* sink) { m_mapAudioTrackAndSink.erase(sink); }

    void Context::AddStatsReport(const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report)
    {
        m_listStatsReport.push_back(report);
    }

    void Context::DeleteStatsReport(const webrtc::RTCStatsReport* report)
    {
        auto found = std::find_if(
            m_listStatsReport.begin(),
            m_listStatsReport.end(),
            [report](rtc::scoped_refptr<const webrtc::RTCStatsReport> it) { return it.get() == report; });
        m_listStatsReport.erase(found);
    }

    DataChannelInterface*
    Context::CreateDataChannel(PeerConnectionObject* obj, const char* label, const DataChannelInit& options)
    {
        const rtc::scoped_refptr<DataChannelInterface> channel = obj->connection->CreateDataChannel(label, &options);

        if (channel == nullptr)
            return nullptr;

        AddDataChannel(channel, *obj);
        return channel;
    }

    void Context::AddDataChannel(DataChannelInterface* channel, PeerConnectionObject& pc)
    {
        auto dataChannelObj = std::make_unique<DataChannelObject>(channel, pc);
        m_mapDataChannels[channel] = std::move(dataChannelObj);
    }

    DataChannelObject* Context::GetDataChannelObject(const DataChannelInterface* channel)
    {
        return m_mapDataChannels[channel].get();
    }

    void Context::DeleteDataChannel(DataChannelInterface* channel)
    {
        if (m_mapDataChannels.count(channel) > 0)
        {
            m_mapDataChannels.erase(channel);
        }
    }

    void Context::AddObserver(
        const webrtc::PeerConnectionInterface* connection,
        const rtc::scoped_refptr<SetSessionDescriptionObserver>& observer)
    {
        m_mapSetSessionDescriptionObserver[connection] = observer;
    }

    void Context::RemoveObserver(const webrtc::PeerConnectionInterface* connection)
    {
        m_mapSetSessionDescriptionObserver.erase(connection);
    }

    PeerConnectionObject* Context::CreatePeerConnection(const webrtc::PeerConnectionInterface::RTCConfiguration& config)
    {
        rtc::scoped_refptr<PeerConnectionObject> obj = new rtc::RefCountedObject<PeerConnectionObject>(*this);
        PeerConnectionDependencies dependencies(obj);
        auto connection = m_peerConnectionFactory->CreatePeerConnectionOrError(config, std::move(dependencies));
        if (!connection.ok())
        {
            RTC_LOG(LS_ERROR) << connection.error().message();
            return nullptr;
        }
        obj->connection = connection.MoveValue();
        const PeerConnectionObject* ptr = obj.get();
        m_mapClients[ptr] = std::move(obj);
        return m_mapClients[ptr].get();
    }

    void Context::DeletePeerConnection(PeerConnectionObject* obj) { m_mapClients.erase(obj); }

    SetSessionDescriptionObserver* Context::GetObserver(webrtc::PeerConnectionInterface* connection)
    {
        return m_mapSetSessionDescriptionObserver[connection];
    }

    uint32_t Context::s_rendererId = 0;
    uint32_t Context::GenerateRendererId() { return s_rendererId++; }

    UnityVideoRenderer* Context::CreateVideoRenderer(DelegateVideoFrameResize callback, bool needFlipVertical)
    {
        auto rendererId = GenerateRendererId();
        auto renderer = std::make_shared<UnityVideoRenderer>(rendererId, callback, needFlipVertical);
        m_mapVideoRenderer[rendererId] = renderer;
        return m_mapVideoRenderer[rendererId].get();
    }

    std::shared_ptr<UnityVideoRenderer> Context::GetVideoRenderer(uint32_t id) { return m_mapVideoRenderer[id]; }

    void Context::DeleteVideoRenderer(UnityVideoRenderer* renderer)
    {
        m_mapVideoRenderer.erase(renderer->GetId());
        renderer = nullptr;
    }

    void Context::GetRtpSenderCapabilities(cricket::MediaType kind, RtpCapabilities* capabilities) const
    {
        *capabilities = m_peerConnectionFactory->GetRtpSenderCapabilities(kind);
    }

    void Context::GetRtpReceiverCapabilities(cricket::MediaType kind, RtpCapabilities* capabilities) const
    {
        *capabilities = m_peerConnectionFactory->GetRtpReceiverCapabilities(kind);
    }

} // end namespace webrtc
} // end namespace unity
