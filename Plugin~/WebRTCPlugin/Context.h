#pragma once

#include <mutex>

#include "AudioTrackSinkAdapter.h"
#include "DummyAudioDevice.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "PeerConnectionObject.h"
#include "UnityVideoRenderer.h"
#include "UnityVideoTrackSource.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    const std::map<std::string, uint32_t> statsTypes = {
        { "codec", 0 },
        { "inbound-rtp", 1 },
        { "outbound-rtp", 2 },
        { "remote-inbound-rtp", 3 },
        { "remote-outbound-rtp", 4 },
        { "media-source", 5 },
        { "media-playout", 6 },
        { "peer-connection", 7 },
        { "data-channel", 8 },
        { "transport", 9 },
        { "candidate-pair", 10 },
        { "local-candidate", 11 },
        { "remote-candidate", 12 },
        { "certificate", 13 },
        // todo: If the following types are deleted from rtcstats_objects.h, delete them as well.
        { "stream", 21 },
        { "track", 22 }
    };

    class IGraphicsDevice;
    class ProfilerMarkerFactory;
    struct ContextDependencies
    {
        IGraphicsDevice* device;
        ProfilerMarkerFactory* profiler;
    };

    class Context;
    class MediaStreamObserver;
    class SetSessionDescriptionObserver;
    class ContextManager
    {
    public:
        static ContextManager* GetInstance();
        ~ContextManager();

        Context* GetContext(int uid) const;
        Context* CreateContext(int uid, ContextDependencies& dependencies);
        void DestroyContext(int uid);
        void SetCurContext(Context*);
        bool Exists(Context* context);
        using ContextPtr = std::unique_ptr<Context>;
        Context* curContext = nullptr;
        std::mutex mutex;

    private:
        std::map<int, ContextPtr> m_contexts;
        static std::unique_ptr<ContextManager> s_instance;
    };

    class Context
    {
    public:
        explicit Context(ContextDependencies& dependencies);
        ~Context();

        bool ExistsRefPtr(const rtc::RefCountInterface* ptr) const
        {
            return m_mapRefPtr.find(ptr) != m_mapRefPtr.end();
        }
        template<typename T>
        void AddRefPtr(rtc::scoped_refptr<T> refptr)
        {
            m_mapRefPtr.emplace(refptr.get(), refptr);
        }
        void AddRefPtr(rtc::RefCountInterface* ptr) { m_mapRefPtr.emplace(ptr, ptr); }

        template<typename T>
        void RemoveRefPtr(rtc::scoped_refptr<T>& refptr)
        {
            std::lock_guard<std::mutex> lock(mutex);
            m_mapRefPtr.erase(refptr.get());
        }
        template<typename T>
        void RemoveRefPtr(T* ptr)
        {
            std::lock_guard<std::mutex> lock(mutex);
            m_mapRefPtr.erase(ptr);
        }

        // MediaStream
        rtc::scoped_refptr<MediaStreamInterface> CreateMediaStream(const std::string& streamId);
        void RegisterMediaStreamObserver(webrtc::MediaStreamInterface* stream);
        void UnRegisterMediaStreamObserver(webrtc::MediaStreamInterface* stream);
        MediaStreamObserver* GetObserver(const webrtc::MediaStreamInterface* stream);

        // Audio Source
        rtc::scoped_refptr<AudioSourceInterface> CreateAudioSource();
        // Audio Renderer
        AudioTrackSinkAdapter* CreateAudioTrackSinkAdapter();
        void DeleteAudioTrackSinkAdapter(AudioTrackSinkAdapter* sink);

        // Video Source
        rtc::scoped_refptr<UnityVideoTrackSource> CreateVideoSource();

        // MediaStreamTrack
        rtc::scoped_refptr<VideoTrackInterface>
        CreateVideoTrack(const std::string& label, webrtc::VideoTrackSourceInterface* source);
        rtc::scoped_refptr<AudioTrackInterface>
        CreateAudioTrack(const std::string& label, webrtc::AudioSourceInterface* source);
        void StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);

        // PeerConnection
        PeerConnectionObject* CreatePeerConnection(const webrtc::PeerConnectionInterface::RTCConfiguration& config);
        void DeletePeerConnection(PeerConnectionObject* obj);

        // StatsReport
        std::mutex mutexStatsReport;
        void AddStatsReport(const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report);
        const RTCStats** GetStatsList(const RTCStatsReport* report, size_t* length, uint32_t** types);
        void DeleteStatsReport(const webrtc::RTCStatsReport* report);

        // DataChannel
        DataChannelInterface*
        CreateDataChannel(PeerConnectionObject* obj, const char* label, const DataChannelInit& options);
        void AddDataChannel(rtc::scoped_refptr<DataChannelInterface> channel, PeerConnectionObject& pc);
        DataChannelObject* GetDataChannelObject(const DataChannelInterface* channel);
        void DeleteDataChannel(DataChannelInterface* channel);

        // Renderer
        UnityVideoRenderer* CreateVideoRenderer(DelegateVideoFrameResize callback, bool needFlipVertical);
        std::shared_ptr<UnityVideoRenderer> GetVideoRenderer(uint32_t id);
        void DeleteVideoRenderer(UnityVideoRenderer* renderer);

        // RtpSender
        void GetRtpSenderCapabilities(cricket::MediaType kind, RtpCapabilities* capabilities) const;

        // RtpReceiver
        void GetRtpReceiverCapabilities(cricket::MediaType kind, RtpCapabilities* capabilities) const;

        // AudioDevice
        rtc::scoped_refptr<DummyAudioDevice> GetAudioDevice() const { return m_audioDevice; }

        // mutex;
        std::mutex mutex;

    private:
        std::unique_ptr<rtc::Thread> m_workerThread;
        std::unique_ptr<rtc::Thread> m_signalingThread;
        std::unique_ptr<TaskQueueFactory> m_taskQueueFactory;
        rtc::scoped_refptr<webrtc::PeerConnectionFactoryInterface> m_peerConnectionFactory;
        rtc::scoped_refptr<DummyAudioDevice> m_audioDevice;
        std::vector<rtc::scoped_refptr<const webrtc::RTCStatsReport>> m_listStatsReport;
        std::map<const PeerConnectionObject*, std::unique_ptr<PeerConnectionObject>> m_mapClients;
        std::map<const webrtc::MediaStreamInterface*, std::unique_ptr<MediaStreamObserver>> m_mapMediaStreamObserver;
        std::map<const DataChannelInterface*, std::unique_ptr<DataChannelObject>> m_mapDataChannels;
        std::map<const uint32_t, std::shared_ptr<UnityVideoRenderer>> m_mapVideoRenderer;
        std::map<const AudioTrackSinkAdapter*, std::unique_ptr<AudioTrackSinkAdapter>> m_mapAudioTrackAndSink;
        std::map<const rtc::RefCountInterface*, rtc::scoped_refptr<rtc::RefCountInterface>> m_mapRefPtr;

        static uint32_t s_rendererId;
        static uint32_t GenerateRendererId();
    };

    extern bool Convert(const std::string& str, webrtc::PeerConnectionInterface::RTCConfiguration& config);
} // end namespace webrtc
} // end namespace unity
