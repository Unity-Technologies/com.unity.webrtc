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
    using DelegateMediaStreamOnAddTrack = void(*)(webrtc::MediaStreamInterface*, webrtc::MediaStreamTrackInterface*);
    using DelegateMediaStreamOnRemoveTrack = void(*)(webrtc::MediaStreamInterface*, webrtc::MediaStreamTrackInterface*);
    using DelegateSetSessionDescSuccess = void(*)(PeerConnectionObject*);
    using DelegateSetSessionDescFailure = void(*)(PeerConnectionObject*, webrtc::RTCErrorType, const char*);

    void debugLog(const char* buf);
    void SetResolution(int32* width, int32* length);
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

    struct RTCOfferOptions
    {
        bool iceRestart;
        bool offerToReceiveAudio;
        bool offerToReceiveVideo;
    };

    struct RTCDataChannelInit
    {
        bool reliable = false;
        bool ordered = true;
        int maxRetransmitTime = -1;
        int maxRetransmits = -1;
        char* protocol;
        bool negotiated = false;
        int id = -1;
    };

    struct RTCAnswerOptions
    {
        bool iceRestart;
    };
    
} // end namespace webrtc
} // end namespace unity
