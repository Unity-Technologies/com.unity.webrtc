var UnityWebRTCDataChannel = {
    
  CreateDataChannel: function(peerPtr, labelPtr, optionsJsonPtr) {
    var peer = UWManaged[peerPtr];
    var label = Pointer_stringify(labelPtr);
    var optionsJson = Pointer_stringify(optionsJsonPtr);
    var options = JSON.parse(optionsJson);
    var dataChannel = peer.createDataChannel(label, options);
    dataChannel.onmessage = function (evt) {
      if (typeof evt.data === 'string') {
        var msgPtr = uwcom_strToPtr(evt.data);
        Module.dynCall_vii(uwevt_DCOnTextMessage, this.managePtr, msgPtr);
      } else {
        var msgPtr = uwcom_arrayToReturnPtr(evt.data, Uint8Array);
        Module.dynCall_vii(uwevt_DCOnBinaryMessage, this.managePtr, msgPtr);
      }
    };
    dataChannel.onopen = function (evt) {
      Module.dynCall_vi(uwevt_DCOnOpen, this.managePtr);
    };
    dataChannel.onclose = function (evt) {
      Module.dynCall_vi(uwevt_DCOnClose, this.managePtr);
    };
    uwcom_addManageObj(dataChannel);
    return dataChannel.managePtr;
  },

  DataChannelGetID: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetID', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    return dataChannel.id;
  },

  DataChannelGetLabel: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetLabel', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    var labelPtr = uwcom_strToPtr(dataChannel.label);
    return labelPtr;
  },

  DataChannelGetProtocol: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetProtocol', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    var protocolPtr = uwcom_strToPtr(dataChannel.protocol);
    return protocolPtr;
  },

  DataChannelGetMaxRetransmits: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetMaxRetransmits', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    return dataChannel.maxRetransmits;
  },

  DataChannelGetMaxRetransmitTime: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetMaxRetransmitTime', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    return dataChannel.maxPacketLifeTime;
  },

  DataChannelGetOrdered: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetOrdered', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    return dataChannel.ordered;
  },

  DataChannelGetBufferedAmount: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetBufferedAmount', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    return dataChannel.bufferedAmount;
  },

  DataChannelGetNegotiated: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetNegotiated', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    return dataChannel.negotiated;
  },

  DataChannelGetReadyState: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetReadyState', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    var readyStateIdx = UWRTCDataChannelState.indexOf(dataChannel.readyState);
    return readyStateIdx;
  },

  DataChannelRegisterOnMessage: function (dataChannelPtr, DataChannelNativeOnMessage) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelRegisterOnMessage', 'dataChannel')) return;
    uwevt_DCOnBinaryMessage = DataChannelNativeOnMessage;
  },

  DataChannelRegisterOnTextMessage: function (dataChannelPtr, DataChannelNativeOnTextMessage) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelRegisterOnTextMessage', 'dataChannel')) return;
    uwevt_DCOnTextMessage = DataChannelNativeOnTextMessage;
  },

  DataChannelRegisterOnOpen: function (dataChannelPtr, DataChannelNativeOnOpen) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelRegisterOnOpen', 'dataChannel')) return;
    uwevt_DCOnOpen = DataChannelNativeOnOpen;
  },
    
  DataChannelRegisterOnClose: function (dataChannelPtr, DataChannelNativeOnClose) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelRegisterOnClose', 'dataChannel')) return;
    uwevt_DCOnClose = DataChannelNativeOnClose;
  },

  DataChannelSend: function (dataChannelPtr, textMsgPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelSend', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    var textMsg = Pointer_stringify(textMsgPtr);
    dataChannel.send(textMsg);
  },

  DataChannelSendBinary: function (dataChannelPtr, binaryMsgPtr, size) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelSendBinary', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    var binaryMsg = HEAPU8.subarray(binaryMsgPtr, binaryMsgPtr + size);
    dataChannel.send(binaryMsg);
  },

  DataChannelClose: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelClose', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    dataChannel.close();
  }
};
mergeInto(LibraryManager.library, UnityWebRTCDataChannel);