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
        webrtc::MediaStreamTrackInterface* CreateVideoTrack(const std::string& label, void* frameBuffer, int32 width, int32 height, int32 bitRate);
        webrtc::MediaStreamTrackInterface* CreateAudioTrack(const std::string& label);
        void DeleteMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        void StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        void ProcessAudioData(const float* data, int32 size);


        // PeerConnection
        PeerConnectionObject* CreatePeerConnection(const webrtc::PeerConnectionInterface::RTCConfiguration& config);
        void AddObserver(const webrtc::PeerConnectionInterface* connection, const rtc::scoped_refptr<SetSessionDescriptionObserver>& observer);
        void RemoveObserver(const webrtc::PeerConnectionInterface* connection);
        SetSessionDescriptionObserver* GetObserver(webrtc::PeerConnectionInterface* connection);
        void DeletePeerConnection(PeerConnectionObject* obj) { clients.erase(obj); }

        // DataChannel
        DataChannelObject* CreateDataChannel(PeerConnectionObject* obj, const char* label, const RTCDataChannelInit& options);
        void DeleteDataChannel(DataChannelObject* obj);

        
        // You must call these methods on Rendering thread.
        bool InitializeEncoder(IGraphicsDevice* device, webrtc::MediaStreamTrackInterface* track);
        // You must call these methods on Rendering thread.
        void EncodeFrame(webrtc::MediaStreamTrackInterface* track);
        // You must call these methods on Rendering thread.
        void FinalizeEncoder(webrtc::MediaStreamTrackInterface* track);


        std::map<DataChannelObject*, std::unique_ptr<DataChannelObject>> dataChannels;

    private:
        int m_uid;
        UnityEncoderType m_encoderType;
        std::unique_ptr<rtc::Thread> workerThread;
        std::unique_ptr<rtc::Thread> signalingThread;
        std::map<PeerConnectionObject*, rtc::scoped_refptr<PeerConnectionObject>> clients;
        rtc::scoped_refptr<webrtc::PeerConnectionFactoryInterface> peerConnectionFactory;
        rtc::scoped_refptr<DummyAudioDevice> audioDevice;
        rtc::scoped_refptr<webrtc::AudioTrackInterface> audioTrack;
        std::map<webrtc::MediaStreamTrackInterface*, NvVideoCapturer*> videoCapturerList;
        std::map<const std::string, rtc::scoped_refptr<webrtc::MediaStreamInterface>> mediaStreamMap;
        std::list<rtc::scoped_refptr<webrtc::MediaStreamTrackInterface>> mediaSteamTrackList;
        std::map<const webrtc::MediaStreamInterface*, MediaStreamObserver*> m_mapMediaStreamObserver;
        std::map<const webrtc::PeerConnectionInterface*, rtc::scoped_refptr<SetSessionDescriptionObserver>> m_mapSetSessionDescriptionObserver;
    };

    extern void Convert(const std::string& str, webrtc::PeerConnectionInterface::RTCConfiguration& config);
    extern webrtc::SdpType ConvertSdpType(RTCSdpType type);
    extern RTCSdpType ConvertSdpType(webrtc::SdpType type);
}
