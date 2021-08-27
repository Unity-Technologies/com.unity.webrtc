#pragma once

namespace unity
{
namespace webrtc
{

    namespace webrtc = ::webrtc;

    class Context;
    class PeerConnectionObject;
    enum class RTCSdpType;
    enum class RTCPeerConnectionEventType;
    struct MediaStreamEvent;

    using DelegateDebugLog = void(*)(const char*);
    using DelegateSetResolution = void(*)(int32*, int32*);
    using DelegateMediaStreamOnAddTrack =
        void(*)(webrtc::MediaStreamInterface*, webrtc::MediaStreamTrackInterface*);
    using DelegateMediaStreamOnRemoveTrack =
        void(*)(webrtc::MediaStreamInterface*, webrtc::MediaStreamTrackInterface*);
    using DelegateSetSessionDescSuccess = void(*)(PeerConnectionObject*);
    using DelegateSetSessionDescFailure =
        void(*)(PeerConnectionObject*, webrtc::RTCErrorType, const char*);
    using DelegateAudioReceive =
        void(*)(webrtc::AudioTrackInterface* track,
            const void* audio_data,
            int size,
            int sample_rate,
            int number_of_channels,
            int number_of_frames);

    void debugLog(const char* buf);
    extern DelegateDebugLog delegateDebugLog;

    enum class RTCPeerConnectionState
    {
        New,
        Connecting,
        Connected,
        Disconnected,
        Failed,
        Closed
    };

    enum class RTCIceConnectionState
    {
        New,
        Checking,
        Connected,
        Completed,
        Failed,
        Disconnected,
        Closed,
        Max
    };

    enum class RTCSignalingState
    {
        Stable,
        HaveLocalOffer,
        HaveRemoteOffer,
        HaveLocalPranswer,
        HaveRemotePranswer,
        Closed
    };

    enum class RTCPeerConnectionEventType
    {
        ConnectionStateChange,
        DataChannel,
        IceCandidate,
        IceConnectionStateChange,
        Track
    };

    enum class RTCSdpType
    {
        Offer,
        PrAnswer,
        Answer,
        Rollback
    };

    enum class SdpSemanticsType
    {
        UnifiedPlan
    };

    enum class RTCErrorDetailType
    {
        DataChannelFailure,
        DtlsFailure,
        FingerprintFailure,
        IdpBadScriptFailure,
        IdpExecutionFailure,
        IdpLoadFailure,
        IdpNeedLogin,
        IdpTimeout,
        IdpTlsFailure,
        IdpTokenExpired,
        IdpTokenInvalid,
        SctpFailure,
        SdpSyntaxError,
        HardwareEncoderNotAvailable,
        HardwareEncoderError
    };

    enum class RTCIceCredentialType
    {
        Password,
        OAuth
    };

    enum class TrackKind
    {
        Audio,
        Video
    };

    struct RTCSessionDescription
    {
        RTCSdpType type;
        char* sdp;
    };

    struct RTCIceServer
    {
        char* credential;
        char* credentialType;
        char** urls;
        int urlsLength;
        char* username;
    };

    struct RTCConfiguration
    {
        RTCIceServer* iceServers;
        int iceServersLength;
        char* iceServerPolicy;
    };

    struct RTCIceCandidate
    {
        char* candidate;
        char* sdpMid;
        int sdpMLineIndex;
    };

    struct RTCOfferAnswerOptions
    {
        bool iceRestart;
        bool voiceActivityDetection;
    };
    
} // end namespace webrtc
} // end namespace unity
