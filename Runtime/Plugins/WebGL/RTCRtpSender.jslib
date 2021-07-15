var UnityWebRTCRtpSender = {
  DeleteSender: function (senderPtr) {
    if (!uwcom_existsCheck(senderPtr, 'DeleteSender', 'sender')) return;
    delete UWManaged[senderPtr];
  },

  SenderGetTrack: function (senderPtr) {
    if (!uwcom_existsCheck(senderPtr, 'SenderGetTrack', 'sender')) return;
    var sender = UWManaged[senderPtr];
    if(sender.track){
      uwcom_addManageObj(sender.track);
      return sender.track.managePtr;
    }
  },

  SenderGetParameters: function (senderPtr) {
    if (!uwcom_existsCheck(senderPtr, 'SenderGetParameters', 'sender')) return;
    var sender = UWManaged[senderPtr];
    var parameters = sender.getParameters();
    var parametersJson = JSON.stringify(parameters);
    var parametersJsonPtr = uwcom_strToPtr(parametersJson);
    return parametersJsonPtr;
  },

  SenderSetParameters: function (senderPtr, parametersJsonPtr) {
    if (!uwcom_existsCheck(senderPtr, 'SenderSetParameters', 'sender')) return;
    var sender = UWManaged[senderPtr];
    var parametersJson = Pointer_stringify(parametersJsonPtr);
    var parameters = JSON.parse(parametersJson);
    sender.setParameters(parameters).then(function () {
      // TODO Send correct RTCErrorType.
    });
  },

  SenderReplaceTrack: function (senderPtr, trackPtr) {
    if (!uwcom_existsCheck(senderPtr, 'SenderReplaceTrack', 'sender')) return;
    if (!uwcom_existsCheck(trackPtr, 'SenderReplaceTrack', 'track')) return;
    var sender = UWManaged[senderPtr];
    var track = UWManaged[trackPtr];
    sender.replaceTrack(track);
  }
};
mergeInto(LibraryManager.library, UnityWebRTCRtpSender);