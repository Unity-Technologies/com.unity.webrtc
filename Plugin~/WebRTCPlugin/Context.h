#pragma once
#include "DummyAudioDevice.h"
#include "PeerConnectionObject.h"
#include "NvVideoCapturer.h"

namespace WebRTC
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
        Context* CreateContext(int uid, UnityEncoderType encoderType);
        void DestroyContext(int uid);
        void SetCurContext(Context*);
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
        UnityEncoderType type;

        VideoEncoderParameter(int width, int height, UnityEncoderType type) :width(width), height(height), type(type) { }
    };

    class Context
    {
    public:
        
        explicit Context(int uid = -1, UnityEncoderType encoderType = UnityEncoderHardware);
        ~Context();

        // Utility
        UnityEncoderType GetEncoderType() const;


        // MediaStream
        webrtc::MediaStreamInterface* CreateMediaStream(const std::string& streamId);
        void DeleteMediaStream(webrtc::MediaStreamInterface* stream);
        MediaStreamObserver* GetObserver(const webrtc::MediaStreamInterface* stream);


        // MediaStreamTrack
        webrtc::VideoTrackInterface* CreateVideoTrack(const std::string& label, void* frameBuffer, int32 width, int32 height, int32 bitRate);
        webrtc::AudioTrackInterface* CreateAudioTrack(const std::string& label);
        void DeleteMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        void StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        void ProcessAudioData(const float* data, int32 size);


        // PeerConnection
        PeerConnectionObject* CreatePeerConnection(const webrtc::PeerConnectionInterface::RTCConfiguration& config);
        void AddObserver(const webrtc::PeerConnectionInterface* connection, const rtc::scoped_refptr<SetSessionDescriptionObserver>& observer);
        void RemoveObserver(const webrtc::PeerConnectionInterface* connection);
        SetSessionDescriptionObserver* GetObserver(webrtc::PeerConnectionInterface* connection);
        void DeletePeerConnection(PeerConnectionObject* obj) { m_mapClients.erase(obj); }

        // DataChannel
        DataChannelObject* CreateDataChannel(PeerConnectionObject* obj, const char* label, const RTCDataChannelInit& options);
        void AddDataChannel(std::unique_ptr<DataChannelObject>& channel);
        void DeleteDataChannel(DataChannelObject* obj);

        
        // You must call these methods on Rendering thread.
        bool InitializeEncoder(IEncoder* encoder, webrtc::MediaStreamTrackInterface* track);
        // You must call these methods on Rendering thread.
        void EncodeFrame(webrtc::MediaStreamTrackInterface* track);
        const VideoEncoderParameter* GetEncoderParameter(const webrtc::MediaStreamTrackInterface* track);
        void SetEncoderParameter(const webrtc::MediaStreamTrackInterface* track, int width, int height, UnityEncoderType type);

    private:
        int m_uid;
        UnityEncoderType m_encoderType;
        std::unique_ptr<rtc::Thread> m_workerThread;
        std::unique_ptr<rtc::Thread> m_signalingThread;
        rtc::scoped_refptr<webrtc::PeerConnectionFactoryInterface> m_peerConnectionFactory;
        rtc::scoped_refptr<DummyAudioDevice> m_audioDevice;
        rtc::scoped_refptr<webrtc::AudioTrackInterface> m_audioTrack;
        std::list<rtc::scoped_refptr<webrtc::MediaStreamTrackInterface>> m_mediaSteamTrackList;
        std::map<const PeerConnectionObject*, rtc::scoped_refptr<PeerConnectionObject>> m_mapClients;
        std::map<const webrtc::MediaStreamTrackInterface*, NvVideoCapturer*> m_mapVideoCapturer;
        std::map<const std::string, rtc::scoped_refptr<webrtc::MediaStreamInterface>> m_mapMediaStream;
        std::map<const webrtc::MediaStreamInterface*, std::unique_ptr<MediaStreamObserver>> m_mapMediaStreamObserver;
        std::map<const webrtc::PeerConnectionInterface*, rtc::scoped_refptr<SetSessionDescriptionObserver>> m_mapSetSessionDescriptionObserver;
        std::map<const webrtc::MediaStreamTrackInterface*, std::unique_ptr<VideoEncoderParameter>> m_mapVideoEncoderParameter;
        std::map<const DataChannelObject*, std::unique_ptr<DataChannelObject>> m_mapDataChannels;
    };

    extern bool Convert(const std::string& str, webrtc::PeerConnectionInterface::RTCConfiguration& config);
    extern webrtc::SdpType ConvertSdpType(RTCSdpType type);
    extern RTCSdpType ConvertSdpType(webrtc::SdpType type);
}
