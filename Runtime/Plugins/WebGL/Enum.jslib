var UnityWebRTCEnum = {
  // WebRTC
  $UWRTCSdpSemantics: [ // Not defined in the API
    'plan-b',
    'unified-plan'
  ],
  $UWRTCIceCredentialType: [
    'password',
    'oauth'
  ],
  $UWRTCIceTransportPolicy: [
    '',
    'relay',
    '',
    'all'
  ],
  $UWRTCBundlePolicy: [
    'balanced',
    'max-compat',
    'max-bundle'
  ],
  $UWRTCRtcpMuxPolicy: [
    'require'
  ],
  $UWRTCSignalingState: [
    'stable',
    'have-local-offer',
    'have-remote-offer',
    'have-local-pranswer',
    'have-remote-pranswer',
    'closed'
  ],
  $UWRTCIceGatheringState: [
    'new',
    'gathering',
    'complete'
  ],
  $UWRTCPeerConnectionState: [
    'new',
    'connecting',
    'connected',
    'disconnected',
    'failed',
    'closed'
  ],
  $UWRTCIceConnectionState: [
    'new',
    'checking',
    'connected',
    'completed',
    'failed',
    'disconnected',
    'closed',
    'max'
  ],
  $UWRTCSdpType: [
    'offer',
    'pranswer',
    'answer',
    'rollback'
  ],
  $UWRTCIceProtocol: [
    'udp',
    'tcp'
  ],
  $UWRTCIceTcpCandidateType: [
    'active',
    'passive',
    'so'
  ],
  $UWRTCIceCandidateType: [
    'host',
    'srflx',
    'prflx',
    'relay'
  ],
  $UWRTCRtpTransceiverDirection: [
    'sendrecv',
    'sendonly',
    'recvonly',
    'inactive',
    'stopped'
  ],
  $UWRTCDtlsTransportState: [
    'new',
    'connecting',
    'connected',
    'closed',
    'failed'
  ],
  $UWRTCIceGathererState: [
    'new',
    'gathering',
    'complete'
  ],
  $UWRTCIceTransportState: [
    'new',
    'checking',
    'connected',
    'completed',
    'disconnected',
    'failed',
    'closed'
  ],
  $UWRTCIceRole: [
    'unknown',
    'controlling',
    'controlled'
  ],
  $UWRTCIceComponent: [
    'none',
    'rtp',
    'rtcp'
  ],
  $UWRTCSctpTransportState: [
    'connecting',
    'connected',
    'closed'
  ],
  $UWRTCDataChannelState: [
    'connecting',
    'open',
    'closing',
    'closed'
  ],
  $UWRTCErrorDetailType: [
    'data-channel-failure',
    'dtls-failure',
    'fingerprint-failure',
    'hardware-encoder-error',
    'hardware-encoder-not-available',
    'sdp-syntax-error',
    'sctp-failure'
  ],
  // Media Capture and Streams
  $UWMediaStreamTrackState: [
    'live',
    'ended'
  ],
  $UWVideoFacingModeEnum: [
    'user',
    'environment',
    'left',
    'right'
  ],
  $UWVideoResizeModeEnum: [
    'none',
    'crop-and-scale'
  ],
  $UWMediaStreamTrackKind: [ // Not defined in the API
    'audio',
    'video'
  ],
  $UWMediaDeviceKind: [
    'audioinput',
    'audiooutput',
    'videoinput'
  ],
  // Identifiers for WebRTC's Statistics
  $UWRTCStatsType: [
    'codec',
    'inbound-rtp',
    'outbound-rtp',
    'remote-inbound-rtp',
    'remote-outbound-rtp',
    'media-source',
    'csrc',
    'peer-connection',
    'data-channel',
    'stream',
    'track',
    'transceiver',
    'sender',
    'receiver',
    'transport',
    'sctp-transport',
    'candidate-pair',
    'local-candidate',
    'remote-candidate',
    'certificate',
    'ice-server'
  ],
  $UWRTCCodecType: [
    'encode',
    'decode'
  ],
  $UWRTCQualityLimitationReason: [
    'none',
    'cpu',
    'bandwidth',
    'other'
  ],
  $UWRTCStatsIceCandidatePairState: [
    'frozen',
    'waiting',
    'in-progress',
    'failed',
    'succeeded'
  ],
  // WebRTC Priority Control
  $UWRTCPriorityType: [
    'very-low',
    'low',
    'medium',
    'high'
  ],
  // Unity WebRTC
  $UWEncoderType: [
    'software',
    'hardware'
  ],
  $UWRTCErrorType: [
    'None',
    'UnsupportedOperation',
    'UnsupportedParameter',
    'InvalidParameter',
    'InvalidRange',
    'SyntaxError',
    'InvalidState',
    'InvalidModification',
    'NetworkError',
    'ResourceExhausted',
    'InternalError',
    'OperationErrorWithData'
  ]
}
autoAddDeps(UnityWebRTCEnum, '$UWRTCSdpSemantics');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceCredentialType');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceTransportPolicy');
autoAddDeps(UnityWebRTCEnum, '$UWRTCBundlePolicy');
autoAddDeps(UnityWebRTCEnum, '$UWRTCRtcpMuxPolicy');
autoAddDeps(UnityWebRTCEnum, '$UWRTCSignalingState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceGatheringState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCPeerConnectionState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceConnectionState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCSdpType');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceProtocol');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceTcpCandidateType');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceCandidateType');
autoAddDeps(UnityWebRTCEnum, '$UWRTCRtpTransceiverDirection');
autoAddDeps(UnityWebRTCEnum, '$UWRTCDtlsTransportState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceGathererState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceTransportState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceRole');
autoAddDeps(UnityWebRTCEnum, '$UWRTCIceComponent');
autoAddDeps(UnityWebRTCEnum, '$UWRTCSctpTransportState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCDataChannelState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCErrorDetailType');
autoAddDeps(UnityWebRTCEnum, '$UWMediaStreamTrackState');
autoAddDeps(UnityWebRTCEnum, '$UWVideoFacingModeEnum');
autoAddDeps(UnityWebRTCEnum, '$UWVideoResizeModeEnum');
autoAddDeps(UnityWebRTCEnum, '$UWMediaStreamTrackKind');
autoAddDeps(UnityWebRTCEnum, '$UWMediaDeviceKind');
autoAddDeps(UnityWebRTCEnum, '$UWRTCStatsType');
autoAddDeps(UnityWebRTCEnum, '$UWRTCCodecType');
autoAddDeps(UnityWebRTCEnum, '$UWRTCQualityLimitationReason');
autoAddDeps(UnityWebRTCEnum, '$UWRTCStatsIceCandidatePairState');
autoAddDeps(UnityWebRTCEnum, '$UWRTCPriorityType');
autoAddDeps(UnityWebRTCEnum, '$UWEncoderType');
autoAddDeps(UnityWebRTCEnum, '$UWRTCErrorType');
mergeInto(LibraryManager.library, UnityWebRTCEnum);