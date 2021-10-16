var UnityWebRTCRtpTransceiver = {
  DeleteTransceiver: function (transceiverPtr) {
    if (!uwcom_existsCheck(transceiverPtr, 'DeleteTransceiver', 'transceiver')) return;
    delete UWManaged[transceiverPtr];
  },

  TransceiverGetDirection: function (transceiverPtr) {
    if (!uwcom_existsCheck(transceiverPtr, 'TransceiverGetDirection', 'transceiver')) return;
    var transceiver = UWManaged[transceiverPtr];
    return UWRTCRtpTransceiverDirection.indexOf(transceiver.direction);
  },

  TransceiverSetDirection: function (transceiverPtr, directionIdx) {
    if (!uwcom_existsCheck(transceiverPtr, 'TransceiverSetDirection', 'transceiver')) return;
    var transceiver = UWManaged[transceiverPtr];
    transceiver.direction = UWRTCRtpTransceiverDirection[directionIdx];
  },

  TransceiverGetCurrentDirection: function (transceiverPtr) {
    if (!uwcom_existsCheck(transceiverPtr, 'TransceiverGetCurrentDirection', 'transceiver')) return;
    var transceiver = UWManaged[transceiverPtr];
    return UWRTCRtpTransceiverDirection.indexOf(transceiver.currentDirection);
  },

  TransceiverGetReceiver: function (transceiverPtr) {
    if (!uwcom_existsCheck(transceiverPtr, 'TransceiverGetReceiver', 'transceiver')) return;
    var transceiver = UWManaged[transceiverPtr];
    uwcom_addManageObj(transceiver.receiver);
    return transceiver.receiver.managePtr;
  },

  TransceiverGetSender: function (transceiverPtr) {
    if (!uwcom_existsCheck(transceiverPtr, 'TransceiverGetSender', 'transceiver')) return;
    var transceiver = UWManaged[transceiverPtr];
    uwcom_addManageObj(transceiver.sender);
    return transceiver.sender.managePtr;
  },

  TransceiverSetCodecPreferences: function (transceiverPtr, codecsPtr) {
    if (!uwcom_existsCheck(transceiverPtr, 'TransceiverSetCodecPreferences', 'transceiver')) return UWRTCErrorType.indexOf("OperationErrorWithData");

    var transceiver = UWManaged[transceiverPtr];
    var codecsJson = Pointer_stringify(codecsPtr);
    var codecs = JSON.parse(codecsJson);

    const supportsSetCodecPreferences = window.RTCRtpTransceiver && 'setCodecPreferences' in window.RTCRtpTransceiver.prototype;
    if (supportsSetCodecPreferences) {
      try {
        transceiver.setCodecPreferences(codecs);
      } catch (err) {
        return UWRTCErrorType.indexOf("InvalidModification");
      }
    } 
    else return UWRTCErrorType.indexOf("UnsupportedOperation");
    return UWRTCErrorType.indexOf("None");
  },

  TransceiverStop: function (transceiverPtr) {
    if (!uwcom_existsCheck(transceiverPtr, 'TransceiverStop', 'transceiver')) return;
    try {
      var transceiver = UWManaged[transceiverPtr];
      transceiver.stop();
      return true;
    } catch (err) {
      return false;
    }
  }
};
mergeInto(LibraryManager.library, UnityWebRTCRtpTransceiver);