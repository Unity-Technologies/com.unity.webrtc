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
  DeleteIceCandidate: function (candidatePtr) {
    if (!uwcom_existsCheck(candidatePtr, 'DeleteIceCandidate', 'iceCandidate')) return;
    delete UWManaged[candidatePtr];
  }
};
mergeInto(LibraryManager.library, UnityWebRTCIceCandidate);