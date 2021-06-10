var UnityWebRTCContext = {
  GetHardwareEncoderSupport: function () {
    return true;
  },

  ContextCreate__deps: ['$UWEncoderType'],
  ContextCreate: function (uid, encodeType) {
    var context = {
      id: uid,
      encodeType: UWEncoderType[encodeType]
    };
    uwcom_addManageObj(context);
    return context.managePtr;
  },

  ContextDestroy: function (uid) {
    var contextPtrs = Object.keys(UWManaged).filter(function (contextPtr) {
      if ('id' in UWManaged[contextPtr]) {
        return UWManaged[contextPtr].id === uid;
      } else
        return false;
    });
    if (contextPtrs.length > 1) {
      console.error('ContextDestroy: multiple Contexts with the same id');
    } else if (!contextPtrs.length) {
      console.error('ContextDestroy: There is no context with id = ' + uid.toString());
    }
    contextPtrs.forEach(function (contextPtr) {
      delete UWManaged[contextPtr];
    });
  },

  ContextGetEncoderType: function (contextPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextGetEncoderType', 'context')) return;
    var encodeTypeIdx = UWEncoderType.indexOf(context.encodeType);
    return encodeTypeIdx;
  },

  ContextCreatePeerConnection: function (contextPtr, conf) {
    if (!uwcom_existsCheck(contextPtr, 'ContextCreatePeerConnection', 'context')) return;
    return _CreatePeerConnection(conf);
  },

  ContextCreatePeerConnectionWithConfig: function (contextPtr, confPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextCreatePeerConnectionWithConfig', 'context')) return;
    return _CreatePeerConnectionWithConfig(confPtr);
  },

  ContextDeletePeerConnection: function (contextPtr, peerPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextDeletePeerConnection', 'context')) return;
    if (!uwcom_existsCheck(peerPtr, 'ContextDeletePeerConnection', 'peer')) return;
    var peer = UWManaged[peerPtr];
    if (peer.readyState !== 'closed' || peer.signalingState !== 'closed')
      peer.close();
    delete UWManaged[peerPtr];
  },

  PeerConnectionSetLocalDescription: function (contextPtr, peerPtr, typeIdx, sdpPtr) {
    if (!uwcom_existsCheck(contextPtr, 'PeerConnectionSetLocalDescription', 'context')) return 11; // OperationErrorWithData
    if (!uwcom_existsCheck(peerPtr, 'PeerConnectionSetLocalDescription', 'peer')) return 11; // OperationErrorWithData
    _PeerConnectionSetDescription(peerPtr, typeIdx, sdpPtr, 'Local');
    return 0;
  },

  PeerConnectionSetRemoteDescription: function (contextPtr, peerPtr, typeIdx, sdpPtr) {
    if (!uwcom_existsCheck(contextPtr, 'PeerConnectionSetRemoteDescription', 'context')) return 11; // OperationErrorWithData
    if (!uwcom_existsCheck(peerPtr, 'PeerConnectionSetRemoteDescription', 'peer')) return 11; // OperationErrorWithData
    _PeerConnectionSetDescription(peerPtr, typeIdx, sdpPtr, 'Remote');
    return 0;
  },

  PeerConnectionRegisterOnSetSessionDescSuccess: function (contextPtr, peerPtr, OnSetSessionDescSuccess) {
    if (!uwcom_existsCheck(contextPtr, 'PeerConnectionRegisterOnSetSessionDescSuccess', 'context')) return;
    if (!uwcom_existsCheck(peerPtr, 'PeerConnectionRegisterOnSetSessionDescSuccess', 'peer')) return;
    uwevt_OnSetSessionDescSuccess = OnSetSessionDescSuccess;
  },

  PeerConnectionRegisterOnSetSessionDescFailure: function (contextPtr, peerPtr, OnSetSessionDescFailure) {
    if (!uwcom_existsCheck(contextPtr, 'PeerConnectionRegisterOnSetSessionDescFailure', 'context')) return;
    if (!uwcom_existsCheck(peerPtr, 'PeerConnectionRegisterOnSetSessionDescFailure', 'peer')) return;
    uwevt_OnSetSessionDescFailure = OnSetSessionDescFailure;
  },

  ContextCreateDataChannel: function (contextPtr, peerPtr, labelPtr, optionsJsonPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextCreateDataChannel', 'context')) return;
    if (!uwcom_existsCheck(peerPtr, 'ContextCreateDataChannel', 'peer')) return;
    return _CreateDataChannel(peerPtr, labelPtr, optionsJsonPtr);
  },

  ContextDeleteDataChannel: function (contextPtr, dataChannelPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextDeleteDataChannel', 'context')) return;
    if (!uwcom_existsCheck(dataChannelPtr, 'ContextDeleteDataChannel', 'dataChannel')) return;
    delete UWManaged[dataChannelPtr];
  },

  ContextCreateMediaStream: function (contextPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextCreateMediaStream', 'context')) return;
    return _CreateMediaStream();
  },

  ContextDeleteMediaStream: function (contextPtr, streamPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextDeleteMediaStream', 'context')) return;
    if (!uwcom_existsCheck(streamPtr, 'ContextDeleteMediaStream', 'stream')) return;
    _DeleteMediaStream(streamPtr);
  },

  ContextRegisterMediaStreamObserver: function (contextPtr, streamPtr) {
    
  },
    
  ContextUnRegisterMediaStreamObserver: function (contextPtr, streamPtr) {

  },
    
  MediaStreamRegisterOnAddTrack: function (contextPtr, streamPtr, MediaStreamOnAddTrack) {
    if (!uwcom_existsCheck(contextPtr, 'MediaStreamRegisterOnAddTrack', 'context')) return;
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamRegisterOnAddTrack', 'stream')) return;
    uwevt_MSOnAddTrack = MediaStreamOnAddTrack;
  },

  MediaStreamRegisterOnRemoveTrack: function (contextPtr, streamPtr, MediaStreamOnRemoveTrack) {
    if (!uwcom_existsCheck(contextPtr, 'MediaStreamRegisterOnRemoveTrack', 'context')) return;
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamRegisterOnRemoveTrack', 'stream')) return;
    uwevt_MSOnRemoveTrack = MediaStreamOnRemoveTrack;
  },

  GetRenderEventFunc: function (contextPtr) {

  },

  GetUpdateTextureFunc: function (contextPtr) {

  },

  ContextCreateAudioTrack: function (contextPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextCreateAudioTrack', 'context')) return;
    return CreateAudioTrack();
  },

  ContextCreateVideoTrack: function (contextPtr, srcTexturePtr, dstTexturePtr, width, height) {
    if (!uwcom_existsCheck(contextPtr, 'ContextCreateVideoTrack', 'context')) return;
    return _CreateVideoTrack(srcTexturePtr, dstTexturePtr, width, height);
  },

  ContextStopMediaStreamTrack: function (contextPtr, trackPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextStopMediaStreamTrack', 'context')) return;
    if (!uwcom_existsCheck(trackPtr, 'ContextStopMediaStreamTrack', 'track')) return;
    var track = UWManaged[trackPtr];
    track.stop();
  },

  ContextDeleteMediaStreamTrack: function (trackPtr) {
    if (!uwcom_existsCheck(trackPtr, 'ContextDeleteMediaStreamTrack', 'track')) return;
    delete UWManaged[trackPtr];
  },

  // CreateVideoRenderer: function(contextPtr) {

  // },

  // DeleteVideoRenderer: function(contextPtr, sinkPtr) {

  // },

  ContextDeleteStatsReport: function (contextPtr, reportPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextDeleteStatsReport', 'context')) return;
    if (!uwcom_existsCheck(reportPtr, 'ContextDeleteStatsReport', 'report')) return;
    delete UWManaged[reportPtr];
  },

  // ContextSetVideoEncoderParameter: function(trackPtr, width, height, format, texturePtr) {

  // },

  // GetInitializationResult: function(contextPtr, trackPtr) {

  // },

  $UWContextGetCapabilities: function (senderReceiver, kindIdx) {
    var kind = UWMediaStreamTrackKind[kindIdx];
    var capabilities = senderReceiver.getCapabilities(kind);
    var capabilitiesJson = JSON.stringify(capabilities);
    var capabilitiesJsonPtr = uwcom_strToPtr(capabilitiesJson);
    return capabilitiesJsonPtr;
  },

  ContextGetSenderCapabilities: function (contextPtr, kindIdx) {
    if (!uwcom_existsCheck(contextPtr, 'ContextGetSenderCapabilities', 'context')) return;
    return UWContextGetCapabilities(RTCRtpSender, kindIdx);
  },

  ContextGetReceiverCapabilities: function (contextPtr, kindIdx) {
    if (!uwcom_existsCheck(contextPtr, 'ContextGetReceiverCapabilities', 'context')) return;
    return UWContextGetCapabilities(RTCRtpReceiver, kindIdx);
  }
};
autoAddDeps(UnityWebRTCContext, '$UWContextGetCapabilities');
mergeInto(LibraryManager.library, UnityWebRTCContext);