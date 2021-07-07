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
    if (!uwcom_existsCheck(candPtr, "DeleteIceCandidate", "iceCandidate")) return;
    var candidate = UWManaged[candPtr];
    var ret = {};
    ret.candidate = candidate.candidate;
    ret.component = candidate.component;
    ret.foundation = candidate.foundation;
    ret.ip = candidate.ip;
    ret.port = candidate.port;
    ret.priority = candidate.priority;
    ret.address = candidate.address;
    ret.protocol = candidate.protocol;
    ret.relatedAddress = candidate.relatedAddress;
    ret.sdpMid = candidate.sdpMid;
    ret.sdpMLineIndex = candidate.sdpMLineIndex;
    ret.tcpType = candidate.tcpType;
    ret.type = candidate.type;
    ret.usernameFragment = candidate.usernameFragment;
    // TODO: Use uwcom_strToPtr()
    var json = JSON.stringify(ret);
    var bufferSize = lengthBytesUTF8(json) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(json, buffer, bufferSize);
    return buffer;
  }, 
  
  DeleteIceCandidate: function (candidatePtr) {
    if (!uwcom_existsCheck(candidatePtr, 'DeleteIceCandidate', 'iceCandidate')) return;
    delete UWManaged[candidatePtr];
  }
};
mergeInto(LibraryManager.library, UnityWebRTCIceCandidate);