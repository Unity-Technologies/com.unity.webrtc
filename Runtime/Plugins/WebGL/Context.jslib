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
  
  
  // TODO
  ContextAddRefPtr: function (ptr){
    
  },
  
  // TODO
  ContextDeleteRefPtr: function(ptr){
    
  },
  
  // TODO
  ContextCreateAudioTrackSource: function(contextPtr){
    if (!uwcom_existsCheck(contextPtr, "ContextCreateAudioTrackSource", "context")) return;
    const audioTrackSource = {};
    uwcom_addManageObj(audioTrackSource);
    return audioTrackSource.managePtr;
  },
  
  // TODO
  ContextCreateVideoTrackSource: function(contextPtr){
    if (!uwcom_existsCheck(contextPtr, "ContextCreateVideoTrackSource", "context")) return;
    const videoTrackSource = {};
    uwcom_addManageObj(videoTrackSource);
    return videoTrackSource.managePtr;
  },
  
  ContextGetEncoderType: function (contextPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextGetEncoderType', 'context')) return;
    var context = UWManaged[contextPtr];
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
    //if (!uwcom_existsCheck(contextPtr, 'ContextDeletePeerConnection', 'context')) return;
    if (!uwcom_existsCheck(peerPtr, 'ContextDeletePeerConnection', 'peer')) return;
    var peer = UWManaged[peerPtr];
    if (peer.readyState !== 'closed' || peer.signalingState !== 'closed')
      peer.close();
    delete UWManaged[peerPtr];
  },

  PeerConnectionSetLocalDescription: function (contextPtr, peerPtr, typeIdx, sdpPtr) {
    if (!uwcom_existsCheck(contextPtr, 'PeerConnectionSetLocalDescription', 'context')) return 11; // OperationErrorWithData
    if (!uwcom_existsCheck(peerPtr, 'PeerConnectionSetLocalDescription', 'peer')) return 11; // OperationErrorWithData
    return _PeerConnectionSetDescription(peerPtr, typeIdx, sdpPtr, 'Local');
  },

  PeerConnectionSetRemoteDescription: function (contextPtr, peerPtr, typeIdx, sdpPtr) {
    if (!uwcom_existsCheck(contextPtr, 'PeerConnectionSetRemoteDescription', 'context')) return 11; // OperationErrorWithData
    if (!uwcom_existsCheck(peerPtr, 'PeerConnectionSetRemoteDescription', 'peer')) return 11; // OperationErrorWithData
    return _PeerConnectionSetDescription(peerPtr, typeIdx, sdpPtr, 'Remote');
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
    //if (!uwcom_existsCheck(contextPtr, 'ContextDeleteDataChannel', 'context')) return;
    if (!uwcom_existsCheck(dataChannelPtr, 'ContextDeleteDataChannel', 'dataChannel')) return;
    delete UWManaged[dataChannelPtr];
  },

  ContextCreateMediaStream: function (contextPtr, labelPtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextCreateMediaStream', 'context')) return;
    return _CreateMediaStream(labelPtr);
  },

  ContextDeleteMediaStream: function (contextPtr, streamPtr) {
    //if (!uwcom_existsCheck(contextPtr, 'ContextDeleteMediaStream', 'context')) return;
    if (!uwcom_existsCheck(streamPtr, 'ContextDeleteMediaStream', 'stream')) return;
    _DeleteMediaStream(streamPtr);
  },

  ContextRegisterMediaStreamObserver: function (contextPtr, streamPtr) {
    if (!uwcom_existsCheck(contextPtr, 'MediaStreamRegisterOnAddTrack', 'context')) return;
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamRegisterOnAddTrack', 'stream')) return;
    
    var stream = UWManaged[streamPtr];
    stream.onaddtrack = (function(evt) {
      uwcom_addManageObj(evt.track);
      Module.dynCall_vii(uwevt_MSOnAddTrack, stream.managePtr, evt.track.managePtr);
    });
    stream.onremovetrack = (function(evt) {
      if (!uwcom_existsCheck(evt.track.managePtr, "stream.onremovetrack", "track")) return;
      Module.dynCall_vii(uwevt_MSOnRemoveTrack, stream.managePtr, evt.track.managePtr);
    });
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

  ContextCreateAudioTrack: function (contextPtr, labelPtr, sourcePtr) {
    if (!uwcom_existsCheck(contextPtr, 'ContextCreateAudioTrack', 'context')) return;
    return _CreateAudioTrack(labelPtr, sourcePtr);
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

  ContextDeleteMediaStreamTrack: function (contextPtr, trackPtr) {
    if (!uwcom_existsCheck(trackPtr, 'ContextDeleteMediaStreamTrack', 'track')) return;
    var track = UWManaged[trackPtr];

    // Not sure how js garbage collection works, remove/disable/stop all attributes inside the track object?
    if(track.kind === "video"){
      if(uwcom_localVideoTracks[trackPtr]){
        delete uwcom_localVideoTracks[trackPtr];
      }

      if(uwcom_remoteVideoTracks[trackPtr]){
        uwcom_remoteVideoTracks[trackPtr].video.remove();
        uwcom_remoteVideoTracks[trackPtr].track.stop();
        delete uwcom_remoteVideoTracks[trackPtr];
      }
    }
    delete UWManaged[trackPtr];
  },

  ContextRegisterAudioReceiveCallback: function (contextPtr, trackPtr, AudioTrackOnReceive){
    
  },

  ContextUnregisterAudioReceiveCallback: function (contextPtr, trackPtr){
    
  },
  
  // CreateVideoRenderer: function(contextPtr) {

  // },

  // DeleteVideoRenderer: function(contextPtr, sinkPtr) {

  // },

  ContextDeleteStatsReport: function (contextPtr, reportPtr) {
    //if (!uwcom_existsCheck(contextPtr, 'ContextDeleteStatsReport', 'context')) return;
    if (!uwcom_existsCheck(reportPtr, 'ContextDeleteStatsReport', 'report')) return;
    delete UWManaged[reportPtr];
  },

  // ContextSetVideoEncoderParameter: function(trackPtr, width, height, format, texturePtr) {

  // },

  // GetInitializationResult: function(contextPtr, trackPtr) {

  // },

  $UWContextGetCapabilities: function (senderReceiver, kindIdx) {
    var kind = UWMediaStreamTrackKind[kindIdx];
    var capabilities = {codecs:[], headerExtensions: []};
    const supportsSetCodecPreferences = window.RTCRtpTransceiver && 'setCodecPreferences' in window.RTCRtpTransceiver.prototype;
    if(supportsSetCodecPreferences) capabilities = senderReceiver.getCapabilities(kind);
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