#pragma once
#include "DummyAudioDevice.h"
#include "PeerConnectionObject.h"
#include "NvVideoCapturer.h"

namespace WebRTC
{
    class Context;
    class PeerSDPObserver;
    class IGraphicsDevice;
    class MediaStreamObserver;
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

        // MediaStream
        webrtc::MediaStreamInterface* CreateMediaStream(const std::string& stream_id);
        void DeleteMediaStream(webrtc::MediaStreamInterface* stream);
        MediaStreamObserver* GetObserver(const webrtc::MediaStreamInterface* stream);


        webrtc::MediaStreamTrackInterface* CreateVideoTrack(const std::string& label, void* frameBuffer, int32 width, int32 height, int32 bitRate);
        webrtc::MediaStreamTrackInterface* CreateAudioTrack(const std::string& label);
        void DeleteMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        PeerConnectionObject* CreatePeerConnection();
        PeerConnectionObject* CreatePeerConnection(const std::string& conf);
        void DeletePeerConnection(PeerConnectionObject* obj) { clients.erase(obj); }
        UnityEncoderType GetEncoderType() const;

        // You must call these methods on Rendering thread.
        bool InitializeEncoder(IGraphicsDevice* device, webrtc::MediaStreamTrackInterface* track);
        void EncodeFrame(webrtc::MediaStreamTrackInterface* track);
        void FinalizeEncoder(webrtc::MediaStreamTrackInterface* track);
        //

        void StopMediaStreamTrack(webrtc::MediaStreamTrackInterface* track);
        void ProcessAudioData(const float* data, int32 size);

        DataChannelObject* CreateDataChannel(PeerConnectionObject* obj, const char* label, const RTCDataChannelInit& options);
        void DeleteDataChannel(DataChannelObject* obj);

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
    };

    class PeerSDPObserver : public webrtc::SetSessionDescriptionObserver
    {
    public:
        static PeerSDPObserver* Create(PeerConnectionObject* obj);
        virtual void OnSuccess();
        virtual void OnFailure(const std::string& error);
    protected:
        PeerSDPObserver() {}
        ~PeerSDPObserver() {}
    private:
        PeerConnectionObject* m_obj;
    };  // class PeerSDPObserver

    extern void Convert(const std::string& str, webrtc::PeerConnectionInterface::RTCConfiguration& config);
    extern webrtc::SdpType ConvertSdpType(RTCSdpType type);
    extern RTCSdpType ConvertSdpType(webrtc::SdpType type);
}
