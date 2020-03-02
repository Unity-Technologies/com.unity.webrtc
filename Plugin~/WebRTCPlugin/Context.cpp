#include "pch.h"
#include "WebRTCPlugin.h"
#include "Context.h"
#include "GraphicsDevice/GraphicsDevice.h"
#include "DummyVideoEncoder.h"
#include "VideoCaptureTrackSource.h"

namespace WebRTC
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

    Context* ContextManager::CreateContext(int uid, UnityEncoderType encoderType)
    {
        auto it = s_instance.m_contexts.find(uid);
        if (it != s_instance.m_contexts.end()) {
            DebugLog("Using already created context with ID %d", uid);
            return nullptr;
        }
        auto ctx = new Context(uid, encoderType);
        s_instance.m_contexts[uid].reset(ctx);
        return ctx;
    }

    /*
    CodecInitializationResult Context::GetCodecInitializationResult()
    {
        return nvVideoCapturer->GetCodecInitializationResult();
    }
    */

    void ContextManager::SetCurContext(Context* context)
    {
        curContext = context;
    }

    void ContextManager::DestroyContext(int uid)
    {
        auto it = s_instance.m_contexts.find(uid);
        if (it != s_instance.m_contexts.end()) {
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

    void Convert(const std::string& str, webrtc::PeerConnectionInterface::RTCConfiguration& config)
    {
        config = webrtc::PeerConnectionInterface::RTCConfiguration{};
        Json::CharReaderBuilder builder;
        const std::unique_ptr<Json::CharReader> reader(builder.newCharReader());
        Json::Value configJson;
        Json::String err;
        bool ok = reader->parse(str.c_str(), str.c_str() + static_cast<int>(str.length()), &configJson, &err);
        if (!ok)
        {
            //json parse faild.
            return;
        }

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
        default:
            throw std::invalid_argument("Unknown SdpType");
        }
    }
#pragma warning(pop)

    Context::Context(int uid, UnityEncoderType encoderType)
        : m_uid(uid)
        , m_encoderType(encoderType)
    {
        workerThread.reset(new rtc::Thread(rtc::SocketServer::CreateDefault()));
        workerThread->Start();
        signalingThread.reset(new rtc::Thread(rtc::SocketServer::CreateDefault()));
        signalingThread->Start();

        rtc::InitializeSSL();

        audioDevice = new rtc::RefCountedObject<DummyAudioDevice>();

#if defined(SUPPORT_METAL) && defined(SUPPORT_SOFTWARE_ENCODER)
        //Always use SoftwareEncoder on Mac for now.
        std::unique_ptr<webrtc::VideoEncoderFactory> videoEncoderFactory = webrtc::CreateBuiltinVideoEncoderFactory();
#else
        std::unique_ptr<webrtc::VideoEncoderFactory> videoEncoderFactory =
            m_encoderType == UnityEncoderType::UnityEncoderHardware ?
            std::make_unique<DummyVideoEncoderFactory>() : webrtc::CreateBuiltinVideoEncoderFactory();
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
//        videoTracks.clear();

        videoCapturerList.clear();
        //mediaStreamMap.clear();

        workerThread->Quit();
        workerThread.reset();
        signalingThread->Quit();
        signalingThread.reset();
    }

    bool Context::InitializeEncoder(IGraphicsDevice* device)
    {
        /*
        if(!nvVideoCapturer->InitializeEncoder(device, m_encoderType))
        {
            return false;
        }
        nvVideoCapturer->StartEncoder();
        */
        for (const auto& entry : videoCapturerList)
        {
            if (!entry.second->InitializeEncoder(device, m_encoderType))
            {
                return false;
            }
            entry.second->StartEncoder();
        }

        return true;
    }
    void Context::EncodeFrame()
    {
        for (const auto& entry : videoCapturerList)
        {
            entry.second->EncodeVideoData();
        }
    }
    void Context::FinalizeEncoder()
    {
        //nvVideoCapturer->FinalizeEncoder();
        for (const auto& entry : videoCapturerList)
        {
            entry.second->FinalizeEncoder();
        }
    }

    UnityEncoderType Context::GetEncoderType() const
    {
        return m_encoderType;
    }

    webrtc::MediaStreamInterface* Context::CreateMediaStream(const std::string& streamId)
    {
        return peerConnectionFactory->CreateLocalMediaStream(streamId);
        /*
        if (mediaStreamMap.count(streamId) == 0)
        {
            mediaStreamMap[streamId] = peerConnectionFactory->CreateLocalMediaStream(streamId);
        }
        return mediaStreamMap[streamId];
        */
    }

    void Context::DeleteMediaStream(webrtc::MediaStreamInterface* stream)
    {
        /*
        const auto streamId = stream->id();
        if (mediaStreamMap.count(streamId) > 0)
        {
            mediaStreamMap.erase(streamId);
        }
        */
    }

    webrtc::MediaStreamTrackInterface* Context::CreateVideoTrack(const std::string& label, void* frameBuffer, int32 width, int32 height, int32 bitRate)
    {
        //void* pUnityEncoder = //pDummyVideoEncoderFactory->CreatePlatformEncoder(WebRTC::Nvidia, width, height, bitRate);
        auto pUnityVideoCapturer = std::make_unique<NvVideoCapturer>();
        //pUnityVideoCapturer->InitializeEncoder();
        //pDummyVideoEncoderFactory->AddCapturer(pUnityVideoCapturer);
        pUnityVideoCapturer->SetFrameBuffer(frameBuffer);

        const auto source(WebRTC::VideoCapturerTrackSource::Create(workerThread.get(), std::move(pUnityVideoCapturer), false));
        const auto videoTrack = peerConnectionFactory->CreateVideoTrack(label, source);
        //auto videoTrack = peerConnectionFactory->CreateVideoTrack(label, peerConnectionFactory->CreateVideoSource(pUnityVideoCapturer));
        //pUnityVideoCapturer->StartEncoder();

        // TODO:: Create dictionary to impletement StopMediaStreamTrack API
        // videoCapturerList[videoTrack] = pUnityVideoCapturer.get();
        //mediaStreamTrackList.push_back(videoTrack);
        return videoTrack.get();
    }

    void Context::StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track)
    {
        //auto videoTrack = static_cast<webrtc::VideoTrackInterface*>(track);
        if(videoCapturerList.count(track) > 0)
        {
            videoCapturerList[track]->Stop();
        }
    }

    webrtc::MediaStreamTrackInterface* Context::CreateAudioTrack(const std::string& label)
    {
        //avoid optimization specially for voice
        cricket::AudioOptions audioOptions;
        audioOptions.auto_gain_control = false;
        audioOptions.noise_suppression = false;
        audioOptions.highpass_filter = false;
        //TODO: label and stream id should be maintained in some way for multi-stream
        return peerConnectionFactory->CreateAudioTrack(label, peerConnectionFactory->CreateAudioSource(audioOptions));
    }

    /*
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
    */

    /*
    void Context::DeleteAudioStream(webrtc::MediaStreamInterface* stream)
    {
        audioStream.release();
    }
    */

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
