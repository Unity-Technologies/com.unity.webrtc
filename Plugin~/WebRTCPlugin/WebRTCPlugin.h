#pragma once

#include <api/frame_transformer_interface.h>
#include <api/media_stream_interface.h>
#include <api/rtc_error.h>

struct IUnityInterfaces;

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class Context;
    class PeerConnectionObject;
    class UnityVideoRenderer;
    class AudioTrackSinkAdapter;
    enum class RTCSdpType;
    enum class RTCPeerConnectionEventType;
    struct MediaStreamEvent;

    using DelegateDebugLog = void (*)(const char*, rtc::LoggingSeverity severity);
    using DelegateSetResolution = void (*)(int32_t*, int32_t*);
    using DelegateMediaStreamOnAddTrack = void (*)(MediaStreamInterface*, MediaStreamTrackInterface*);
    using DelegateMediaStreamOnRemoveTrack = void (*)(MediaStreamInterface*, MediaStreamTrackInterface*);
    using DelegateVideoFrameResize = void (*)(UnityVideoRenderer* renderer, int width, int height);
    using DelegateTransformedFrame = void (*)(FrameTransformerInterface*, TransformableFrameInterface*);

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

    class IGraphicsDevice;
    class ProfilerMarkerFactory;
    class Plugin
    {
    public:
        static IGraphicsDevice* GraphicsDevice();
        static ProfilerMarkerFactory* ProfilerMarkerFactory();
    };

} // end namespace webrtc
} // end namespace unity
