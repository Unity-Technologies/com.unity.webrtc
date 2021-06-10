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

    struct RTCAnswerOptions
    {
        bool iceRestart;
    };

    /// View over an existing buffer representing an audio frame, in the sense
    /// of a single group of contiguous audio data.
    struct AudioFrame {
        /// Pointer to the raw contiguous memory block holding the audio data in
        /// channel interleaved format. The length of the buffer is at least
        /// (|bits_per_sample_| / 8 * |channel_count_| * |sample_count_|) bytes.
        const void* data_;

        /// Number of bits per sample, often 8 or 16, for a single channel.
        std::uint32_t bits_per_sample_;

        /// Sampling rate, in Hertz (number of samples per second).
        std::uint32_t sampling_rate_hz_;

        /// Number of interleaved channels in a single audio sample.
        std::uint32_t channel_count_;

        /// Number of consecutive samples. The frame duration is given by the ratio
        /// |sample_count_| / |sampling_rate_hz_|.
        std::uint32_t sample_count_;
    };

    using DelegateAudioFrameObserverOnFrameReady = void(*)(const AudioFrame&);
    
} // end namespace webrtc
} // end namespace unity
