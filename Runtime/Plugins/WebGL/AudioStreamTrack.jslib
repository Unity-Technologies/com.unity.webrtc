var UnityWebRTCAudioStreamTrack = {
  
  // To be implemented
  CreateAudioTrack: function () {
    if (!uwcom_audioContext) {
      uwcom_audioContext = new AudioContext;
    }
    var dest = uwcom_audioContext.createMediaStreamDestination();
    var audioTrack = dest.stream.getAudioTracks()[0];
    uwcom_addManageObj(audioTrack);
    return audioTrack.managePtr;
  },

  ProcessAudio: function (data, size) {
    // TODO
  }
};
mergeInto(LibraryManager.library, UnityWebRTCAudioStreamTrack);
