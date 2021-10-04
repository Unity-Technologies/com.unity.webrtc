var UnityWebRTCAudioStreamTrack = {
  
  // To be implemented
  CreateAudioTrack: function (labelPtr, sourcePtr) {
    if (!uwcom_audioContext) {
      uwcom_audioContext = new AudioContext;
    }
    var dest = uwcom_audioContext.createMediaStreamDestination();
    var audioTrack = dest.stream.getAudioTracks()[0];
    uwcom_addManageObj(audioTrack);
    audioTrack.guid = Pointer_stringify(labelPtr);
    return audioTrack.managePtr;
  },

  ProcessAudio: function (data, size) {
    // TODO
  }
};
mergeInto(LibraryManager.library, UnityWebRTCAudioStreamTrack);
