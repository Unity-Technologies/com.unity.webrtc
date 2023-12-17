var UnityWebRTCRtpReceiver = {
  DeleteReceiver: function (receiverPtr) {
    if (!uwcom_existsCheck(receiverPtr, 'DeleteReceiver', 'receiver')) return;
    delete UWManaged[receiverPtr];
  },

  ReceiverGetTrack: function (receiverPtr) {
    if (!uwcom_existsCheck(receiverPtr, 'ReceiverGetTrack', 'receiver')) return;
    var receiver = UWManaged[receiverPtr];
    uwcom_addManageObj(receiver.track);
    return receiver.track.managePtr;
  },
  
  ReceiverGetStreams: function(receiverPtr, length){
    
  }
};
mergeInto(LibraryManager.library, UnityWebRTCRtpReceiver);