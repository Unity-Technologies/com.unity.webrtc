var UnityWebRTCIceCandidate = {
  CreateNativeRTCIceCandidate: function (candPtr, sdpMidPtr, sdpMLineIndex) {
    var cand = Pointer_stringify(candPtr);
    var sdpMid = Pointer_stringify(sdpMidPtr);
    var candidate = new RTCIceCandidate({
      candidate: cand,
      sdpMid: sdpMid,
      sdpMLineIndex: sdpMLineIndex
    });
    uwcom_addManageObj(candidate);
    return candidate.managePtr;
  },

  IceCandidateGetCandidate: function (candPtr){
    if (!uwcom_existsCheck(candPtr, "IceCandidateGetCandidate", "iceCandidate")) return;
    var candidate = UWManaged[candPtr];
    var ret = {};
    ret.candidate = candidate.candidate;
    ret.component =  UWRTCIceComponent.indexOf(candidate.component);
    ret.foundation = candidate.foundation;
    ret.ip = candidate.ip;
    ret.port = candidate.port;
    ret.priority = candidate.priority;
    ret.address = candidate.address;
    ret.protocol = candidate.protocol;
    ret.relatedAddress = candidate.relatedAddress;    // TODO: https://developer.mozilla.org/en-US/docs/Web/API/RTCIceCandidate/relatedAddress - Is null for host candidates
    ret.sdpMid = candidate.sdpMid;
    ret.sdpMLineIndex = candidate.sdpMLineIndex;
    ret.tcpType = candidate.tcpType;
    ret.type = candidate.type;
    ret.usernameFragment = candidate.usernameFragment;  //TODO: https://developer.mozilla.org/en-US/docs/Web/API/RTCIceCandidate/usernameFragment - This can be null?
    var json = JSON.stringify(ret);
    return uwcom_strToPtr(json);
  },

  IceCandidateGetSdp: function(candidatePtr){
    if (!uwcom_existsCheck(candidatePtr, "IceCandidateGetSdp", "iceCandidate")) return;
    candidate = UWManaged[candidatePtr];
    return uwcom_strToPtr(candidate.candidate);
  },

  IceCandidateGetSdpMid: function(candidatePtr){
    if (!uwcom_existsCheck(candidatePtr, "IceCandidateGetSdpMid", "iceCandidate")) return;
    return uwcom_strToPtr(candidate.sdpMid);
  },

  IceCandidateGetSdpLineIndex: function(candidatePtr){
    if (!uwcom_existsCheck(candidatePtr, "IceCandidateGetSdpMid", "iceCandidate")) return;
    return candidate.sdpMLineIndex;
  },
  
  DeleteIceCandidate: function (candidatePtr) {
    if (!uwcom_existsCheck(candidatePtr, 'DeleteIceCandidate', 'iceCandidate')) return;
    delete UWManaged[candidatePtr];
  }
};
mergeInto(LibraryManager.library, UnityWebRTCIceCandidate);