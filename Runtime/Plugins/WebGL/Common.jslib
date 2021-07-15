var UnityWebRTCCommon = {
  $uwcom_logLevel: 0,
  $uwcom_managePtr: 0,
  $uwcom_localAudioTracks: {},
  $uwcom_localVideoTracks: {},
  $uwcom_remoteAudioTracks: {},
  $uwcom_remoteVideoTracks: {},
  $uwcom_audioContext: null,

  $uwcom_addManageObj: function (obj) {
    if (!obj.managePtr) {
      uwcom_managePtr++;
      obj.managePtr = uwcom_managePtr;
      UWManaged[obj.managePtr] = obj;
    }
    else if(!UWManaged[obj.managePtr]){
      UWManaged[obj.managePtr] = obj;
    }
  },
  $uwcom_strToPtr: function (str) {
    var len = lengthBytesUTF8(str) + 1;
    var ptr = _malloc(len);
    stringToUTF8(str, ptr, len);
    return ptr;
  },
  $uwcom_arrayToReturnPtr: function (arr, type) {
    var buf = (new type(arr)).buffer;
    var ui8a = new Uint8Array(buf);
    var ptr = _malloc(ui8a.byteLength + 4);
    HEAP32.set([arr.length], ptr >> 2);
    HEAPU8.set(ui8a, ptr + 4);
    setTimeout(function () {
      _free(ptr);
    }, 0);
    return ptr;
  },
  $uwcom_errorNo: function (err) {
    var errNo = UWRTCErrorType.indexOf(err.name);
    if (errNo === -1)
      errNo = 0;
    return errNo;
  },
  $uwcom_fixStatEnumValue: function (stat) {
    if (stat.type === 'codec') {
      if (stat.codecType) {
        stat.codecType = UWRTCCodecType.indexOf(stat.codecType);
        if (stat.codecType === -1) return false;
      }
    }
    if (stat.type === 'outbound-rtp') {
      if (stat.qualityLimitationReason) {
        stat.qualityLimitationReason = UWRTCQualityLimitationReason.indexOf(stat.qualityLimitationReason);
        if (stat.qualityLimitationReason === -1) return false;
      }
      if (stat.priority) {
        stat.priority = UWRTCPriorityType.indexOf(stat.priority);
        if (stat.priority === -1) return false;
      }
    }
    if (stat.type === 'media-source') {
      if (stat.kind) {
        stat.kind = UWMediaStreamTrackKind.indexOf(stat.kind);
        if (stat.kind === -1) return false;
      }
    }
    if (stat.type === 'data-channel') {
      if (stat.state) {
        stat.state = UWRTCDataChannelState.indexOf(stat.state);
        if (stat.state === -1) return false;
      }
    }
    if (stat.type === 'transport') {
      if (stat.iceRole) {
        stat.iceRole = UWRTCIceRole.indexOf(stat.iceRole);
        if (stat.iceRole === -1) return false;
      }
      if (stat.dtlsState) {
        stat.dtlsState = UWRTCDtlsTransportState.indexOf(stat.dtlsState);
        if (stat.dtlsState === -1) return false;
      }
      if (stat.iceState) {
        stat.iceState = UWRTCIceTransportState.indexOf(stat.iceState);
        if (stat.iceState === -1) return false;
      }
    }
    if (stat.type === 'local-candidate'
      || stat.type === 'remote-candidate') {
      if (stat.candidateType) {
        stat.candidateType = UWRTCIceCandidateType.indexOf(stat.candidateType);
        if (stat.candidateType === -1) return false;
      }
    }
    if (stat.type === 'candidate-pair') {
      if (stat.state) {
        stat.state = UWRTCStatsIceCandidatePairState.indexOf(stat.state);
        if (stat.state === -1) return false;
      }
    }
    stat.type = UWRTCStatsType.indexOf(stat.type);
    return true;
  },
  $uwcom_statsSerialize: function (stats) {
    var statsJsons = [];
    stats.forEach((function(stat) {
      if (uwcom_fixStatEnumValue(stat)) statsJsons.push(stat);
    }));
    var statsDataJson = JSON.stringify(statsJsons);
    var statsDataJsonPtr = uwcom_strToPtr(statsDataJson);
    return statsDataJsonPtr;
  },
  $uwcom_existsCheck: function (ptr, funcName, typeName) {
    var obj = UWManaged[ptr];
    if (obj) return true;
    console.error("[jslib] " + funcName + ": Unmanaged " + typeName + ". Ptr: " + ptr);
    return false;
  },
  $uwcom_getIdx: function (enum_, val) {
    enum_.indexOf()
  },
  $uwcom_debugLog: function (level, fileName, member, msg) {
    if (!level) return;
    var levelNo = ['', '', '', '', '', '', '', 'error', 'warning', 'log'].indexOf(level);
    if (levelNo === -1) return;
    if ((uwcom_logLevel > 0 && uwcom_logLevel <= 3 && levelNo > 0 && (levelNo - 6) <= uwcom_logLevel) ||
      (uwcom_logLevel > 6 && uwcom_logLevel <= 9 && levelNo > 6 && levelNo <= uwcom_logLevel)) {
        msg = '[JSLIB] ' + fileName + ' : ' + member + ' : ' + msg; 
      var levelPtr = uwcom_strToPtr(level); 
      var msgPtr = uwcom_strToPtr(msg);
      Module.dynCall_vii(uwevt_DebugLog, level, msgPtr);
      _free(levelPtr);
      _free(msgPtr);
    }
  },

  $UWManaged: {},

  $uwevt_DebugLog: null,
  $uwevt_PCOnIceCandidate: null,
  $uwevt_PCOnIceConnectionChange: null,
  $uwevt_PCOnConnectionStateChange: null,
  $uwevt_PCOnIceGatheringChange: null,
  $uwevt_PCOnNegotiationNeeded: null,
  $uwevt_PCOnDataChannel: null,
  $uwevt_PCOnTrack: null,
  $uwevt_PCOnRemoveTrack: null,
  $uwevt_MSOnAddTrack: null,
  $uwevt_MSOnRemoveTrack: null,
  $uwevt_DCOnTextMessage: null,
  $uwevt_DCOnBinaryMessage: null,
  $uwevt_DCOnOpen: null,
  $uwevt_DCOnClose: null,
  $uwevt_OnSetSessionDescSuccess: null,
  $uwevt_OnSetSessionDescFailure: null,
  $uwevt_OnSuccessCreateSessionDesc: null,
  $uwevt_OnFailureCreateSessionDesc: null,
  $uwevt_OnStatsDeliveredCallback: null,

  UnityWebRTCInit: function (logLevel) {

  },

  RegisterDebugLog: function (level, debugLogPtr) {
    uwcom_logLevel = level;
    uwevt_DebugLog = debugLogPtr;
  },

  StatsReportGetStatsList: function (reportPtr) {
    if (!uwcom_existsCheck(reportPtr, "StatsReportGetStatsList", "report")) return;
    var report = UWManaged[reportPtr];
    return uwcom_statsSerialize(report);
  },
  StatsGetJson: function (statsPtr) {

  },

};
autoAddDeps(UnityWebRTCCommon, '$uwcom_logLevel');
autoAddDeps(UnityWebRTCCommon, '$uwcom_debugLog');
autoAddDeps(UnityWebRTCCommon, '$uwcom_managePtr');
autoAddDeps(UnityWebRTCCommon, '$uwcom_localAudioTracks');
autoAddDeps(UnityWebRTCCommon, '$uwcom_localVideoTracks');
autoAddDeps(UnityWebRTCCommon, '$uwcom_remoteAudioTracks');
autoAddDeps(UnityWebRTCCommon, '$uwcom_remoteVideoTracks');
autoAddDeps(UnityWebRTCCommon, '$uwcom_audioContext');
autoAddDeps(UnityWebRTCCommon, '$UWManaged');

autoAddDeps(UnityWebRTCCommon, '$uwevt_DebugLog');
autoAddDeps(UnityWebRTCCommon, '$uwevt_PCOnIceCandidate');
autoAddDeps(UnityWebRTCCommon, '$uwevt_PCOnIceConnectionChange');
autoAddDeps(UnityWebRTCCommon, '$uwevt_PCOnConnectionStateChange');
autoAddDeps(UnityWebRTCCommon, '$uwevt_PCOnIceGatheringChange');
autoAddDeps(UnityWebRTCCommon, '$uwevt_PCOnNegotiationNeeded');
autoAddDeps(UnityWebRTCCommon, '$uwevt_PCOnDataChannel');
autoAddDeps(UnityWebRTCCommon, '$uwevt_PCOnTrack');
autoAddDeps(UnityWebRTCCommon, '$uwevt_PCOnRemoveTrack');
autoAddDeps(UnityWebRTCCommon, '$uwevt_MSOnAddTrack');
autoAddDeps(UnityWebRTCCommon, '$uwevt_MSOnRemoveTrack');
autoAddDeps(UnityWebRTCCommon, '$uwevt_DCOnTextMessage');
autoAddDeps(UnityWebRTCCommon, '$uwevt_DCOnBinaryMessage');
autoAddDeps(UnityWebRTCCommon, '$uwevt_DCOnOpen');
autoAddDeps(UnityWebRTCCommon, '$uwevt_DCOnClose');
autoAddDeps(UnityWebRTCCommon, '$uwevt_OnSetSessionDescSuccess');
autoAddDeps(UnityWebRTCCommon, '$uwevt_OnSetSessionDescFailure');
autoAddDeps(UnityWebRTCCommon, '$uwevt_OnSuccessCreateSessionDesc');
autoAddDeps(UnityWebRTCCommon, '$uwevt_OnFailureCreateSessionDesc');
autoAddDeps(UnityWebRTCCommon, '$uwevt_OnStatsDeliveredCallback');

autoAddDeps(UnityWebRTCCommon, '$uwcom_addManageObj');
autoAddDeps(UnityWebRTCCommon, '$uwcom_strToPtr');
autoAddDeps(UnityWebRTCCommon, '$uwcom_arrayToReturnPtr');
autoAddDeps(UnityWebRTCCommon, '$uwcom_errorNo');
autoAddDeps(UnityWebRTCCommon, '$uwcom_fixStatEnumValue');
autoAddDeps(UnityWebRTCCommon, '$uwcom_statsSerialize');
autoAddDeps(UnityWebRTCCommon, '$uwcom_existsCheck');

mergeInto(LibraryManager.library, UnityWebRTCCommon);
