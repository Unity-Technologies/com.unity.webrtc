#pragma once
#include <mutex>
#include "DummyAudioDevice.h"
#include "DummyVideoEncoder.h"
#include "PeerConnectionObject.h"
#include "UnityVideoRenderer.h"
#include "Codec/IEncoder.h"

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

    class Context;
    class IGraphicsDevice;
    class MediaStreamObserver;
    class UnityVideoTrackSource;
    class SetSessionDescriptionObserver;
    class ContextManager
    {
    public:
        static ContextManager* GetInstance() { return &s_instance; }
     
        Context* GetContext(int uid) const;
        Context* CreateContext(int uid, UnityEncoderType encoderType);
        void DestroyContext(int uid);
        void SetCurContext(Context*);
        bool Exists(Context* context);
        using ContextPtr = std::unique_ptr<Context>;
        Context* curContext = nullptr;
    private:
        ~ContextManager();
        std::map<int, ContextPtr> m_contexts;
        static ContextManager s_instance;
    };

    struct VideoEncoderParameter
    {
        int width;
        int height;
        VideoEncoderParameter(int width, int height) :width(width), height(height) { }
    };

    class Context : public IVideoEncoderObserver
    {
    public:
        
        explicit Context(int uid = -1, UnityEncoderType encoderType = UnityEncoderHardware);
        ~Context();

        // Utility
        UnityEncoderType GetEncoderType() const;
        CodecInitializationResult GetInitializationResult(webrtc::MediaStreamTrackInterface* track);

        // MediaStream
        webrtc::MediaStreamInterface* CreateMediaStream(const std::string& streamId);
        void DeleteMediaStream(webrtc::MediaStreamInterface* stream);
        MediaStreamObserver* GetObserver(const webrtc::MediaStreamInterface* stream);


        // MediaStreamTrack
        webrtc::VideoTrackInterface* CreateVideoTrack(
            const std::string& label, void* frame, UnityGfxRenderer gfxRenderer);
        webrtc::AudioTrackInterface* CreateAudioTrack(const std::string& label);
        void DeleteMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        void StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        void ProcessAudioData(const float* data, int32 size);
        UnityVideoTrackSource* GetVideoSource(const MediaStreamTrackInterface* track);

        // PeerConnection
        PeerConnectionObject* CreatePeerConnection(const webrtc::PeerConnectionInterface::RTCConfiguration& config);
        void AddObserver(const webrtc::PeerConnectionInterface* connection, const rtc::scoped_refptr<SetSessionDescriptionObserver>& observer);
        void RemoveObserver(const webrtc::PeerConnectionInterface* connection);
        SetSessionDescriptionObserver* GetObserver(webrtc::PeerConnectionInterface* connection);
        void DeletePeerConnection(PeerConnectionObject* obj) { m_mapClients.erase(obj); }

        // StatsReport
        void AddStatsReport(const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report);
        void DeleteStatsReport(const webrtc::RTCStatsReport* report);
    
        // DataChannel
        DataChannelObject* CreateDataChannel(PeerConnectionObject* obj, const char* label, const RTCDataChannelInit& options);
        void AddDataChannel(std::unique_ptr<DataChannelObject>& channel);
        void DeleteDataChannel(DataChannelObject* obj);

        // Renderer
        UnityVideoRenderer* CreateVideoRenderer();
        UnityVideoRenderer* GetVideoRenderer(uint32_t id);
        void DeleteVideoRenderer(UnityVideoRenderer* renderer);

        // You must call these methods on Rendering thread.
        bool InitializeEncoder(IEncoder* encoder, webrtc::MediaStreamTrackInterface* track);
        bool FinalizeEncoder(IEncoder* encoder);
        // You must call these methods on Rendering thread.
        bool EncodeFrame(webrtc::MediaStreamTrackInterface* track);
        const VideoEncoderParameter* GetEncoderParameter(const webrtc::MediaStreamTrackInterface* track);
        void SetEncoderParameter(const webrtc::MediaStreamTrackInterface* track, int width, int height);

        // mutex;
        std::mutex mutex;

    private:
        int m_uid;
        UnityEncoderType m_encoderType;
        std::unique_ptr<rtc::Thread> m_workerThread;
        std::unique_ptr<rtc::Thread> m_signalingThread;
        rtc::scoped_refptr<webrtc::PeerConnectionFactoryInterface> m_peerConnectionFactory;
        rtc::scoped_refptr<DummyAudioDevice> m_audioDevice;
        rtc::scoped_refptr<webrtc::AudioTrackInterface> m_audioTrack;
        std::list<rtc::scoped_refptr<webrtc::MediaStreamTrackInterface>> m_mediaSteamTrackList;
        std::vector<rtc::scoped_refptr<const webrtc::RTCStatsReport>> m_listStatsReport;
        std::map<const PeerConnectionObject*, rtc::scoped_refptr<PeerConnectionObject>> m_mapClients;
        std::map<const std::string, rtc::scoped_refptr<webrtc::MediaStreamInterface>> m_mapMediaStream;
        std::map<const webrtc::MediaStreamInterface*, std::unique_ptr<MediaStreamObserver>> m_mapMediaStreamObserver;
        std::map<const webrtc::PeerConnectionInterface*, rtc::scoped_refptr<SetSessionDescriptionObserver>> m_mapSetSessionDescriptionObserver;
        std::map<const webrtc::MediaStreamTrackInterface*, std::unique_ptr<VideoEncoderParameter>> m_mapVideoEncoderParameter;
        std::map<const DataChannelObject*, std::unique_ptr<DataChannelObject>> m_mapDataChannels;
        std::map<const uint32_t, std::unique_ptr<UnityVideoRenderer>> m_mapVideoRenderer;
 
        // todo(kazuki): remove map after moving hardware encoder instance to DummyVideoEncoder.
        std::map<const uint32_t, IEncoder*> m_mapIdAndEncoder;

        // todo(kazuki): remove these callback methods by moving hardware encoder instance to DummyVideoEncoder.
        //               attention point is multi-threaded opengl implementation with nvcodec.
        void SetKeyFrame(uint32_t id) override;
        void SetRates(uint32_t id, uint32_t bitRate, int64_t frameRate) override;

        // todo(kazuki): static variable to set id each encoder.
        static uint32_t s_encoderId;
        static uint32_t GenerateUniqueId();

        static uint32_t s_rendererId;
        static uint32_t GenerateRendererId();
    };

    extern bool Convert(const std::string& str, webrtc::PeerConnectionInterface::RTCConfiguration& config);
    extern webrtc::SdpType ConvertSdpType(RTCSdpType type);
    extern RTCSdpType ConvertSdpType(webrtc::SdpType type);

} // end namespace webrtc
} // end namespace unity
