var UnityWebRTCMediaStream = {
  
  // Note: MediaStream creates a read-only id field, so we cannot set it.
  // Using custom 'guid' field instead.
  CreateMediaStream: function (labelPtr) {
    var label = Pointer_stringify(labelPtr);
    var stream = new MediaStream();
    stream.guid = label;
    stream.onaddtrack = function (evt) {
      uwcom_addManageObj(evt.track);
      console.log('stream.ontrack' + evt.track.managePtr);
      Module.dynCall_vii(uwevt_MSOnAddTrack, this.managePtr, evt.track.managePtr);
    };
    stream.onremovetrack = function (evt) {
      if (!evt.track.managePtr) {
          console.warn('track does not own managePtr');
        return;
      }
      if (!uwcom_existsCheck(evt.track.managePtr, 'stream.onremovetrack', 'track')) return;
      Module.dynCall_vii(uwevt_MSOnRemoveTrack, this.managePtr, evt.track.managePtr);
    };

    uwcom_addManageObj(stream);
    uwcom_debugLog('log', 'RTCPeerConnection.jslib', 'CreateMediaStream', stream.managePtr);
    return stream.managePtr;
  },

  MediaStreamAddUserMedia: function (streamPtr, constraints){
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamAddUserMedia', 'stream')) return;
    uwcom_debugLog('log', 'MediaStream.jslib', 'AddUserMedia', streamPtr);
    
    var stream = UWManaged[streamPtr];
    var optionsJson = Pointer_stringify(constraints);
    var options = JSON.parse(optionsJson);
    
    navigator.mediaDevices.getUserMedia(options)
      .then(function(usermedia){
        usermedia.getTracks().forEach(function(track){
          uwcom_addManageObj(track);
          _MediaStreamAddTrack(stream.managePtr, track.managePtr);
        })
      })
      .catch(function(err) {
        console.error(err);
      });
  },
  
  DeleteMediaStream: function(streamPtr) {
    var stream = UWManaged[streamPtr];
    stream.getTracks().forEach(function(track) {
      track.stop();
      stream.removeTrack(track);
      track = null;
    });
    stream = null;
    delete UWManaged[streamPtr];
  },

  MediaStreamGetID: function (streamPtr) {
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamGetID', 'stream')) return;
    var stream = UWManaged[streamPtr];
    var id = stream.guid || stream.id;
    var streamIdPtr = uwcom_strToPtr(id);
    return streamIdPtr;
  },

  MediaStreamGetVideoTracks: function (streamPtr) {
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamGetVideoTracks', 'stream')) return;
    var stream = UWManaged[streamPtr];
    var tracks = stream.getVideoTracks();
    var ptrs = [];
    tracks.forEach(function (track) {
      uwcom_addManageObj(track);
      ptrs.push(track.managePtr);
    });
    var ptr = uwcom_arrayToReturnPtr(ptrs, Int32Array);
    return ptr;
  },

  MediaStreamGetAudioTracks: function (streamPtr) {
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamGetAudioTracks', 'stream')) return;
    var stream = UWManaged[streamPtr];
    var tracks = stream.getAudioTracks();
    var ptrs = [];
    tracks.forEach(function (track) {
      uwcom_addManageObj(track);
      ptrs.push(track.managePtr);
    });
    var ptr = uwcom_arrayToReturnPtr(ptrs, Int32Array);
    return ptr;
  },

  MediaStreamAddTrack: function (streamPtr, trackPtr) {
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamAddTrack', 'stream')) return;
    if (!uwcom_existsCheck(trackPtr, 'MediaStreamAddTrack', 'track')) return;
    var stream = UWManaged[streamPtr];
    var track = UWManaged[trackPtr];
    try {
      stream.addTrack(track);
      Module.dynCall_vii(uwevt_MSOnAddTrack, stream.managePtr, track.managePtr);
      return true;
    } catch(err){
      return false;
    }
    // try {
    //   console.log('MediaStreamAddTrack:' + streamPtr + ':' + trackPtr);
    //   stream.addTrack(track);
    //   var video = document.createElement('video');
    //   video.id = 'video_' + track.managePtr.toString();
    //   video.muted = true;
    //   //video.style.display = 'none';
    //   video.srcObject = stream;
    //   document.body.appendChild(video);
    //   video.style.width = '300px';
    //   video.style.height = '200px';
    //   video.style.position = 'absolute';
    //   video.style.left = video.style.top = 0;
    //   uwcom_remoteVideoTracks[track.managePtr] = {
    //     track: track,
    //     video: video,
    //     playing: false
    //   };
    //   video.onplaying = function(){
    //     uwcom_remoteVideoTracks[track.managePtr].playing = true;
    //   }
    //   video.play();
    //   Module.dynCall_vii(uwevt_MSOnAddTrack, stream.managePtr, track.managePtr);
    //   return true;
    // } catch (err) {
    //     console.log('MediaStreamAddTrack: ' + err.message);
    //   return false;
    // }
  },

  MediaStreamRemoveTrack: function (streamPtr, trackPtr) {
    if (!uwcom_existsCheck(streamPtr, 'MediaStreamRemoveTrack', 'stream')) return;
    if (!uwcom_existsCheck(trackPtr, 'MediaStreamRemoveTrack', 'track')) return;
    var stream = UWManaged[streamPtr];
    var track = UWManaged[trackPtr];
    try {
      stream.removeTrack(track);
      return true;
    } catch (err) {
      console.log('MediaStreamRemoveTrack: ' + err.message);
      return false;
    }
  }
};
mergeInto(LibraryManager.library, UnityWebRTCMediaStream);