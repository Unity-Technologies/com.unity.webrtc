#pragma once
#include "UnityEncoder.h"
#include "DummyAudioDevice.h"
#include "DummyVideoEncoder.h"
#include "PeerConnectionObject.h"
#include "UnityVideoCapturer.h"
#include "NvEncoder.h"

namespace WebRTC
{
    class Context;
    class PeerSDPObserver;
    class ContextManager
    {
    public:
        static ContextManager* GetInstance() { return &s_instance; }
     
        Context* GetContext(int uid);
        void DestroyContext(int uid);
        void SetCurContext(Context*);
        CodecInitializationResult GetCodecInitializationResult() const;

    public:
        using ContextPtr = std::unique_ptr<Context>;
        Context* curContext = nullptr;
        std::unique_ptr<NV_ENCODE_API_FUNCTION_LIST> pNvEncodeAPI;
        void* hModule = nullptr;
    private:
        ~ContextManager();
        CodecInitializationResult InitializeAndTryNvEnc();
        CodecInitializationResult LoadNvEncApi();
        CodecInitializationResult TryNvEnc();

        CodecInitializationResult codecInitializationResult;
        std::map<int, ContextPtr> m_contexts;
        static ContextManager s_instance;
    };

    class Context
    {
    public:
        explicit Context(int uid = -1);
        webrtc::MediaStreamInterface* CreateMediaStream(const std::string& stream_id);
        webrtc::MediaStreamTrackInterface* CreateVideoTrack(const std::string& label, UnityFrameBuffer* frameBuffer, int32 width, int32 height, int32 bitRate);
        webrtc::MediaStreamTrackInterface* CreateAudioTrack(const std::string& label);
        ~Context();

        PeerConnectionObject* CreatePeerConnection();
        PeerConnectionObject* CreatePeerConnection(const std::string& conf);
        void DeletePeerConnection(PeerConnectionObject* obj) { clients.erase(obj); }
        void EncodeFrame();
        void StopCapturer();
        void ProcessAudioData(const float* data, int32 size) { audioDevice->ProcessAudioData(data, size); }

        DataChannelObject* CreateDataChannel(PeerConnectionObject* obj, const char* label, const RTCDataChannelInit& options);
        void DeleteDataChannel(DataChannelObject* obj);

        std::map<DataChannelObject*, std::unique_ptr<DataChannelObject>> dataChannels;

    private:
        int m_uid;
        std::unique_ptr<rtc::Thread> workerThread;
        std::unique_ptr<rtc::Thread> signalingThread;
        std::map<PeerConnectionObject*, rtc::scoped_refptr<PeerConnectionObject>> clients;
        rtc::scoped_refptr<webrtc::PeerConnectionFactoryInterface> peerConnectionFactory;
        DummyVideoEncoderFactory* pDummyVideoEncoderFactory;
        std::map<const std::string, rtc::scoped_refptr<webrtc::MediaStreamInterface>> mediaStreamMap;
        std::list<rtc::scoped_refptr<webrtc::MediaStreamTrackInterface>> mediaSteamTrackList;

        std::list<UnityVideoCapturer*> nvVideoCapturerList;
        rtc::scoped_refptr<DummyAudioDevice> audioDevice;
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
