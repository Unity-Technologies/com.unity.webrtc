#pragma once
#include <mutex>

#include "AudioTrackSinkAdapter.h"
#include "DummyAudioDevice.h"
#include "DummyVideoEncoder.h"
#include "PeerConnectionObject.h"
#include "UnityVideoRenderer.h"
#include "UnityVideoTrackSource.h"
#include "Codec/IEncoder.h"

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

    class Context;
    class IGraphicsDevice;
    class MediaStreamObserver;
    class SetSessionDescriptionObserver;
    class ContextManager
    {
    public:
        static ContextManager* GetInstance() { return &s_instance; }
     
        Context* GetContext(int uid) const;
        Context* CreateContext(int uid, UnityEncoderType encoderType, bool forTest);
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
        UnityRenderingExtTextureFormat textureFormat;
        void* textureHandle;
        VideoEncoderParameter(
            int width, int height, UnityRenderingExtTextureFormat textureFormat, void* textureHandle)
            : width(width)
            , height(height)
            , textureFormat(textureFormat)
            , textureHandle(textureHandle)
        {
        }
    };

    class Context : public IVideoEncoderObserver
    {
    public:
        
        explicit Context(int uid = -1, UnityEncoderType encoderType = UnityEncoderHardware, bool forTest = false);
        ~Context();

        // Utility
        UnityEncoderType GetEncoderType() const;
        CodecInitializationResult GetInitializationResult(webrtc::MediaStreamTrackInterface* track);

        template <typename T>
        bool ExistsRefPtr(T* ptr) {  return m_mapRefPtr.find(ptr) != m_mapRefPtr.end(); }
        template <typename T>
        void AddRefPtr(rtc::scoped_refptr<T> refptr) { m_mapRefPtr.emplace(refptr.get(), refptr); }
        template <typename T>
        void AddRefPtr(T* ptr) { m_mapRefPtr.emplace(ptr, ptr); }
        template <typename T>
        void RemoveRefPtr(rtc::scoped_refptr<T>& refptr)
        {
            std::lock_guard<std::mutex> lock(mutex);
            m_mapRefPtr.erase(refptr.get());
        }
        template <typename T>
        void RemoveRefPtr(T* ptr)
        {
            std::lock_guard<std::mutex> lock(mutex);
            m_mapRefPtr.erase(ptr);
        }

        // MediaStream
        webrtc::MediaStreamInterface* CreateMediaStream(const std::string& streamId);
        void RegisterMediaStreamObserver(webrtc::MediaStreamInterface* stream);
        void UnRegisterMediaStreamObserver(webrtc::MediaStreamInterface* stream);
        MediaStreamObserver* GetObserver(const webrtc::MediaStreamInterface* stream);

        // Audio Source
        webrtc::AudioSourceInterface* CreateAudioSource();

        // Video Source
        webrtc::VideoTrackSourceInterface* CreateVideoSource();

        // MediaStreamTrack
        webrtc::VideoTrackInterface* CreateVideoTrack(const std::string& label, webrtc::VideoTrackSourceInterface* source);
        webrtc::AudioTrackInterface* CreateAudioTrack(const std::string& label, webrtc::AudioSourceInterface* source);
        void StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        UnityVideoTrackSource* GetVideoSource(const MediaStreamTrackInterface* track);

        void RegisterAudioReceiveCallback(
            AudioTrackInterface* track, DelegateAudioReceive callback);
        void UnregisterAudioReceiveCallback(AudioTrackInterface* track);

        // PeerConnection
        PeerConnectionObject* CreatePeerConnection(const webrtc::PeerConnectionInterface::RTCConfiguration& config);
        void DeletePeerConnection(PeerConnectionObject* obj);
        void AddObserver(const webrtc::PeerConnectionInterface* connection, const rtc::scoped_refptr<SetSessionDescriptionObserver>& observer);
        void RemoveObserver(const webrtc::PeerConnectionInterface* connection);
        SetSessionDescriptionObserver* GetObserver(webrtc::PeerConnectionInterface* connection);

        // StatsReport
        void AddStatsReport(const rtc::scoped_refptr<const webrtc::RTCStatsReport>& report);
        void DeleteStatsReport(const webrtc::RTCStatsReport* report);
    
        // DataChannel
        DataChannelObject* CreateDataChannel(PeerConnectionObject* obj, const char* label, const DataChannelInit& options);
        void AddDataChannel(std::unique_ptr<DataChannelObject> channel);
        void DeleteDataChannel(DataChannelObject* obj);

        // Renderer
        UnityVideoRenderer* CreateVideoRenderer();
        std::shared_ptr<UnityVideoRenderer> GetVideoRenderer(uint32_t id);
        void DeleteVideoRenderer(UnityVideoRenderer* renderer);

        // RtpSender
        void GetRtpSenderCapabilities(
            cricket::MediaType kind, RtpCapabilities* capabilities) const;

        // RtpReceiver
        void GetRtpReceiverCapabilities(
            cricket::MediaType kind, RtpCapabilities* capabilities) const;

        // You must call these methods on Rendering thread.
        bool InitializeEncoder(IEncoder* encoder, webrtc::MediaStreamTrackInterface* track);
        bool FinalizeEncoder(IEncoder* encoder);
        // You must call these methods on Rendering thread.
        const VideoEncoderParameter* GetEncoderParameter(const webrtc::MediaStreamTrackInterface* track);
        void SetEncoderParameter(const MediaStreamTrackInterface* track, int width, int height,
            UnityRenderingExtTextureFormat format, void* textureHandle);

        // mutex;
        std::mutex mutex;

    private:
        int m_uid;
        UnityEncoderType m_encoderType;
        std::unique_ptr<rtc::Thread> m_workerThread;
        std::unique_ptr<rtc::Thread> m_signalingThread;
        rtc::scoped_refptr<webrtc::PeerConnectionFactoryInterface> m_peerConnectionFactory;
        rtc::scoped_refptr<DummyAudioDevice> m_audioDevice;
        std::vector<rtc::scoped_refptr<const webrtc::RTCStatsReport>> m_listStatsReport;
        std::map<const PeerConnectionObject*, rtc::scoped_refptr<PeerConnectionObject>> m_mapClients;
        std::map<const webrtc::MediaStreamInterface*, std::unique_ptr<MediaStreamObserver>> m_mapMediaStreamObserver;
        std::map<const webrtc::PeerConnectionInterface*, rtc::scoped_refptr<SetSessionDescriptionObserver>> m_mapSetSessionDescriptionObserver;
        std::map<const webrtc::MediaStreamTrackInterface*, std::unique_ptr<VideoEncoderParameter>> m_mapVideoEncoderParameter;
        std::map<const DataChannelObject*, std::unique_ptr<DataChannelObject>> m_mapDataChannels;
        std::map<const uint32_t, std::shared_ptr<UnityVideoRenderer>> m_mapVideoRenderer;
        std::map<webrtc::AudioTrackInterface*, std::unique_ptr<AudioTrackSinkAdapter>> m_mapAudioTrackAndSink;
        std::map<const rtc::RefCountInterface*, rtc::scoped_refptr<rtc::RefCountInterface>> m_mapRefPtr;

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
