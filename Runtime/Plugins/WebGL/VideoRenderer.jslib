var UnityWebRTCVideoRenderer = {
  CreateVideoRenderer: function (contextPtr) {
  },

  CreateNativeTexture: function() {
    //console.log('nativeTexture');
    var texPtr = 0;
    for(var texPtr = 0; texPtr < GL.textures.length; texPtr++) {
      if(GL.textures[texPtr] === undefined)
        break;
    }
    var tex = GLctx.createTexture();
    tex.name = texPtr;
    GL.textures[texPtr] = tex;
    //console.log('nativeTexture' + texPtr);
    return texPtr;
  },
  
  GetVideoRendererId: function (sinkPtr) {
  },

  DeleteVideoRenderer: function (contextPtr, sinkPtr) {

  },

  UpdateRendererTexture: function (trackPtr, renderTexturePtr, needFlip) {
    // console.log('UpdateRendererTexture');
    if (!uwcom_existsCheck(trackPtr, 'UpdateRendererTexture', 'track')) return;
    if (!uwcom_remoteVideoTracks[trackPtr]) return;
    //console.log('UpdateRendererTexture', renderTexturePtr);
    var video = uwcom_remoteVideoTracks[trackPtr].video;
    var tex = GL.textures[renderTexturePtr];
    GLctx.bindTexture(GLctx.TEXTURE_2D, tex);
    if (!!needFlip){
      //GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
    }
    // For now: Flip every time, since we want the correct image transfered over WebRTC
    GLctx.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
    GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, video);
    GLctx.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
    // GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, video);
    GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MAG_FILTER, GLctx.LINEAR);
    GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
    GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
    GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
    GLctx.bindTexture(GLctx.TEXTURE_2D, null);
  },

  // Not used in WebGL, but is here to make the NativeMethods more maintainable. 
  VideoTrackAddOrUpdateSink: function(trackPtr, sinkPtr){

  },

  VideoTrackRemoveSink: function(trackPtr, sinkPtr){

  }
};
mergeInto(LibraryManager.library, UnityWebRTCVideoRenderer);