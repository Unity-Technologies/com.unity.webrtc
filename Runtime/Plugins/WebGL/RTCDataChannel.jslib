var UnityWebRTCDataChannel = {
    
  CreateDataChannel: function(peerPtr, labelPtr, optionsJsonPtr) {
    var peer = UWManaged[peerPtr];
    var label = Pointer_stringify(labelPtr);
    var optionsJson = Pointer_stringify(optionsJsonPtr);
    var options = JSON.parse(optionsJson);
    
    // Firefox doesn't like null values, so remove them.
    if(options.ordered.hasValue) options.ordered = options.ordered.value;
    else delete options.ordered;
    if(options.maxRetransmits.hasValue) options.maxRetransmits = options.maxRetransmits.value;
    else delete options.maxRetransmits;
    if(options.maxRetransmitTime.hasValue) options.maxRetransmitTime = options.maxRetransmitTime.value;
    else delete options.maxRetransmitTime;
    if(options.negotiated.hasValue) options.negotiated = options.negotiated.value;
    else delete options.negotiated;
    if(options.id.hasValue) options.id = options.id.value;
    else delete options.id;

    // Chrome (incorrectly?) accept maxRetransmits and maxRetransmitTime being set
    if (options.maxRetransmits && options.maxRetransmitTime) return 0;
    
    try {
        var dataChannel = peer.createDataChannel(label, options);
    }
    catch(err){
        console.log(err);   
        return 0;
    }
    
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
      if (!uwcom_existsCheck(this.managePtr, "onopen", "dataChannel")) return;
      Module.dynCall_vi(uwevt_DCOnOpen, this.managePtr);
    };
    dataChannel.onclose = function (evt) {
      if (!uwcom_existsCheck(this.managePtr, "onclose", "dataChannel")) return;
      Module.dynCall_vi(uwevt_DCOnClose, this.managePtr);
    };
    uwcom_addManageObj(dataChannel);
    return dataChannel.managePtr;
  },

  DataChannelGetID: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetID', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    if(dataChannel.id === null) return -1;
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
    if(dataChannel.maxRetransmits === null) return -1;
    return dataChannel.maxRetransmits;
  },

  DataChannelGetMaxRetransmitTime: function (dataChannelPtr) {
    if (!uwcom_existsCheck(dataChannelPtr, 'DataChannelGetMaxRetransmitTime', 'dataChannel')) return;
    var dataChannel = UWManaged[dataChannelPtr];
    if(dataChannel.maxPacketLifeTime === null) return -1;
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