var UnityWebRTCAudioStreamTrack = {
  CreateAudioTrack: function () {
    // TODO
    if (!uwcom_audioContext) {
      uwcom_audioContext = new AudioContext();
    }
    var audioTrack = {};
    uwcom_addManageObj(audioTrack);
    return audioTrack.managePtr;
  },

  ProcessAudio: function (data, size) {
    // TODO
  }
};
mergeInto(LibraryManager.library, UnityWebRTCAudioStreamTrack);
