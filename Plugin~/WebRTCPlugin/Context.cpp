#include "pch.h"
#include "WebRTCPlugin.h"
#include "Context.h"
#include "GraphicsDevice/GraphicsDevice.h"
#include "Codec/EncoderFactory.h"
#include "DummyVideoEncoder.h"


namespace WebRTC
{
    ContextManager ContextManager::s_instance;
    bool ContextManager::s_use_software_encoder = false;

    Context* ContextManager::GetContext(int uid)
    {
        auto it = s_instance.m_contexts.find(uid);
        if (it != s_instance.m_contexts.end()) {
            DebugLog("Using already created context with ID %d", uid);
            return it->second.get();
        }

        auto ctx = new Context(uid);
        s_instance.m_contexts[uid].reset(ctx);
        DebugLog("Register context with ID %d", uid);
        return ctx;
    }
    CodecInitializationResult Context::GetCodecInitializationResult()
    {
        return nvVideoCapturer->GetCodecInitializationResult();
    }

    void ContextManager::SetCurContext(Context* context)
    {
        curContext = context;
    }

    void ContextManager::DestroyContext(int uid)
    {
        auto it = s_instance.m_contexts.find(uid);
        if (it != s_instance.m_contexts.end()) {
            DebugLog("Unregister context with ID %d", uid);
            s_instance.m_contexts.erase(it);
        }
    }

    ContextManager::~ContextManager()
    {
        if (m_contexts.size()) {
            DebugWarning("%lu remaining context(s) registered", m_contexts.size());
        }
        m_contexts.clear();
    }

    void Convert(const std::string& str, webrtc::PeerConnectionInterface::RTCConfiguration& config)
    {
        config = webrtc::PeerConnectionInterface::RTCConfiguration{};
        Json::Reader jsonReader;
        Json::Value configJson;
        jsonReader.parse(str, configJson);
        Json::Value iceServersJson = configJson["iceServers"];
        if (!iceServersJson)
            return;
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
            if (!iceServerJson["username"].isNull())
            {
                iceServer.password = iceServerJson["credential"].asString();
            }
            config.servers.push_back(iceServer);
        }
        config.sdp_semantics = webrtc::SdpSemantics::kUnifiedPlan;
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
        }
        throw std::invalid_argument("Unknown SdpType");
    }
#pragma warning(pop)

    Context::Context(int uid)
        : m_uid(uid)
    {
        workerThread.reset(new rtc::Thread());
        workerThread->Start();
        signalingThread.reset(new rtc::Thread());
        signalingThread->Start();

        rtc::InitializeSSL();

        audioDevice = new rtc::RefCountedObject<DummyAudioDevice>();
        nvVideoCapturerUnique = std::make_unique<NvVideoCapturer>();

        //[TODO-sin: 2020-1-16] Since we are giving the pointer to peerConnectionFactory in CreateVideoStream(), 
        //we shouldn't store the raw pointer here.
        nvVideoCapturer = nvVideoCapturerUnique.get();

#if defined(SUPPORT_METAL) && defined(SUPPORT_SOFTWARE_ENCODER)
        //Always use SoftwareEncoder on Mac for now.
        std::unique_ptr<webrtc::VideoEncoderFactory> videoEncoderFactory = webrtc::CreateBuiltinVideoEncoderFactory();
#else
        std::unique_ptr<webrtc::VideoEncoderFactory> videoEncoderFactory = ContextManager::s_use_software_encoder ? webrtc::CreateBuiltinVideoEncoderFactory() : std::make_unique<DummyVideoEncoderFactory>(nvVideoCapturer);
#endif
        
        peerConnectionFactory = webrtc::CreatePeerConnectionFactory(
                                workerThread.get(),
                                workerThread.get(),
                                signalingThread.get(),
                                audioDevice,
                                webrtc::CreateAudioEncoderFactory<webrtc::AudioEncoderOpus>(),
                                webrtc::CreateAudioDecoderFactory<webrtc::AudioDecoderOpus>(),
                                std::move(videoEncoderFactory),
                                webrtc::CreateBuiltinVideoDecoderFactory(),
                                nullptr,
                                nullptr);
    }

    Context::~Context()
    {
        dataChannels.clear();
        clients.clear();
        peerConnectionFactory = nullptr;
        audioTrack = nullptr;
        videoTracks.clear();
        audioStream = nullptr;
        videoStreams.clear();

        workerThread->Quit();
        workerThread.reset();
        signalingThread->Quit();
        signalingThread.reset();
    }

    bool Context::InitializeEncoder(IGraphicsDevice* device)
    {
        if(!nvVideoCapturer->InitializeEncoder(device))
        {
            return false;
        }
        nvVideoCapturer->StartEncoder();
        return true;
    }
    void Context::EncodeFrame()
    {
        nvVideoCapturer->EncodeVideoData();
    }
    void Context::FinalizeEncoder()
    {
        nvVideoCapturer->FinalizeEncoder();
    }

    webrtc::MediaStreamInterface* Context::CreateVideoStream(void* frameBuffer, int width, int height)
    {
        //TODO: label and stream id should be maintained in some way for multi-stream
        auto videoTrack = peerConnectionFactory->CreateVideoTrack(
            "video", peerConnectionFactory->CreateVideoSource(std::move(nvVideoCapturerUnique)));

        videoTracks[frameBuffer] = videoTrack;

        auto videoStream = peerConnectionFactory->CreateLocalMediaStream("video");
        videoStream->AddTrack(videoTrack);
        videoStreams.push_back(videoStream);
        nvVideoCapturer->SetFrameBuffer(frameBuffer);
        nvVideoCapturer->SetSize(width, height);
        return videoStream.get();
    }

    void Context::DeleteVideoStream(webrtc::MediaStreamInterface* stream)
    {
        auto item = std::find(videoStreams.begin(), videoStreams.end(), stream);
        videoStreams.erase(item);
    }

    webrtc::MediaStreamInterface* Context::CreateAudioStream()
    {
        //avoid optimization specially for voice
        cricket::AudioOptions audioOptions;
        audioOptions.auto_gain_control = false;
        audioOptions.noise_suppression = false;
        audioOptions.highpass_filter = false;
        //TODO: label and stream id should be maintained in some way for multi-stream
        audioTrack = peerConnectionFactory->CreateAudioTrack("audio", peerConnectionFactory->CreateAudioSource(audioOptions));
        audioStream = peerConnectionFactory->CreateLocalMediaStream("audio");
        audioStream->AddTrack(audioTrack);

        return audioStream.get();
    }

    void Context::DeleteAudioStream(webrtc::MediaStreamInterface* stream)
    {
        audioStream.release();
    }

    void Context::ProcessAudioData(const float* data, int32 size)
    {
        audioDevice->ProcessAudioData(data, size);
    }

    DataChannelObject* Context::CreateDataChannel(PeerConnectionObject* obj, const char* label, const RTCDataChannelInit& options)
    {
        webrtc::DataChannelInit config;
        config.reliable = options.reliable;
        config.ordered = options.ordered;
        config.maxRetransmitTime = options.maxRetransmitTime;
        config.maxRetransmits = options.maxRetransmits;
        config.protocol = options.protocol;
        config.negotiated = options.negotiated;

        auto channel = obj->connection->CreateDataChannel(label, &config);
        auto dataChannelObj = std::make_unique<DataChannelObject>(channel, *obj);
        auto ptr = dataChannelObj.get();
        dataChannels[ptr] = std::move(dataChannelObj);
        return ptr;
    }

    void Context::DeleteDataChannel(DataChannelObject* obj)
    {
        if (dataChannels.count(obj) > 0)
        {
            dataChannels.erase(obj);
        }
    }

    PeerSDPObserver* PeerSDPObserver::Create(PeerConnectionObject* obj)
    {
        auto observer = new rtc::RefCountedObject<PeerSDPObserver>();
        observer->m_obj = obj;
        return observer;
    }

    void PeerSDPObserver::OnSuccess()
    {
        m_obj->onSetSDSuccess(m_obj);
    }

    void PeerSDPObserver::OnFailure(const std::string& error)
    {
        m_obj->onSetSDFailure(m_obj);
    }
}
