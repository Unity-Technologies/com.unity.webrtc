var UnityWebRTCVideoStreamTrack = {
  CreateVideoTrack: function (srcPtr, dstPtr, width, height) {
    var cnv = document.createElement('canvas');
    cnv.width = width;
    cnv.height = height;
    var ctx = cnv.getContext('2d');
    var imgData = ctx.createImageData(width, height);
    var stream = cnv.captureStream();
    var track = stream.getVideoTracks()[0];
    var srcTexture = GL.textures[srcPtr];
    var dstTexture = GL.textures[dstPtr];
    var frameBuffer = GLctx.createFramebuffer();
    GLctx.bindFramebuffer(GLctx.FRAMEBUFFER, frameBuffer);
    GLctx.framebufferTexture2D(
      GLctx.FRAMEBUFFER,
      GLctx.COLOR_ATTACHMENT0,
      GLctx.TEXTURE_2D,
      srcTexture,
      0
    );
    var canRead = (GLctx.checkFramebufferStatus(GLctx.FRAMEBUFFER) === GLctx.FRAMEBUFFER_COMPLETE);
    GLctx.bindFramebuffer(GLctx.FRAMEBUFFER, null);
    var localVideoData = {
      cnv: cnv,
      ctx: ctx,
      imgData: imgData,
      width: width,
      height: height,
      dstTexture: dstTexture,
      stream: stream,
      canRead: canRead,
      buffer: new Uint8Array(width * height * 4),
      lineBuffer: new Uint8Array(width * 4),
      frameBuffer: frameBuffer
    };

    //uwcom_addManageObj(stream);
    uwcom_addManageObj(track);
    uwcom_localVideoTracks[track.managePtr] = localVideoData;
    //console.log('localVideoData', track.managePtr);
    return track.managePtr;
  },

  // Not finished, don't delete.
  $readPixelsAsync: function (data) {
    var w = data.width;
    var h = data.height;
    var buffer = data.buffer;
    var lineBuffer = data.lineBuffer;
    var frameBuffer = data.frameBuffer;
    var imgData = data.imgData;
    var cnv = data.cnv;
    var ctx = data.ctx;
    var dstTexture = data.dstTexture;
    var buf = GLctx.createBuffer();
    GLctx.bindTexture(GLctx.TEXTURE_2D, dstTexture);
    GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, cnv);
    GLctx.bindBuffer(GLctx.PIXEL_PACK_BUFFER, buf);
    GLctx.bufferData(GLctx.PIXEL_PACK_BUFFER, buffer.byteLength, GLctx.STREAM_READ);
    GLctx.readPixels(0, 0, w, h, GLctx.RGBA, GLctx.UNSIGNED_BYTE, 0);
    GLctx.bindBuffer(GLctx.PIXEL_PACK_BUFFER, null);

    var sync = GLctx.fenceSync(GLctx.SYNC_GPU_COMMANDS_COMPLETE, 0);
    if (!sync) {
      return null;
    }

    GLctx.flush();

    return clientWaitAsync(sync, 0, 10).then(function () {
      GLctx.deleteSync(sync);

      GLctx.bindBuffer(GLctx.PIXEL_PACK_BUFFER, buf);
      GLctx.getBufferSubData(GLctx.PIXEL_PACK_BUFFER, 0, buffer);
      GLctx.bindBuffer(GLctx.PIXEL_PACK_BUFFER, null);
      GLctx.deleteBuffer(buf);

      imgData.data.set(buffer);
      ctx.putImageData(imgData, 0, 0);

      GLctx.bindTexture(GLctx.TEXTURE_2D, dstTexture);
      GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, cnv);
      //GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, cnv);
      GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MAG_FILTER, GLctx.LINEAR);
      GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
      GLctx.generateMipmap(GLctx.TEXTURE_2D);
      GLctx.bindTexture(GLctx.TEXTURE_2D, null);
    });
  },

  // Not finished, don't delete.
  $clientWaitAsync: function (sync, flags, interval_ms) {
    return new Promise(function (resolve, reject) {
      var check = function() {
        var res = GLctx.clientWaitSync(sync, flags, 0);
        if (res === GLctx.WAIT_FAILED) {
          reject();
          return;
        }
        if (res === GLctx.TIMEOUT_EXPIRED) {
          setTimeout(check, interval_ms);
          return;
        }
        resolve();
      };
      check();
    });
  },

  RenderLocalVideotrack__deps: ['$readPixelsAsync', '$clientWaitAsync'],
  RenderLocalVideotrack: function (trackPtr, needFlip) {
    var data = uwcom_localVideoTracks[trackPtr];
    // console.log('RenderLocalVideotrack', trackPtr, data);
    if (!data) return;
    // readPixelsAsync(data);
    // return;
    var w = data.width;
    var h = data.height;
    var buffer = data.buffer;
    var lineBuffer = data.lineBuffer;
    var frameBuffer = data.frameBuffer;
    var imgData = data.imgData;
    var cnv = data.cnv;
    var ctx = data.ctx;
    var dstTexture = data.dstTexture;

    if (data.canRead) {
      GLctx.bindFramebuffer(GLctx.FRAMEBUFFER, frameBuffer);
      GLctx.readPixels(0, 0, w, h, GLctx.RGBA, GLctx.UNSIGNED_BYTE, buffer);
      GLctx.bindFramebuffer(GLctx.FRAMEBUFFER, null);

      // var halfHeight = h / 2 | 0;
      // var bytesPerRow = w;
      // for (var y = 0; y < halfHeight; y++) {
      //   var topOffset = y * bytesPerRow;
      //   var bottomOffset = (h - y - 1) * bytesPerRow;
      //   data.lineBuffer.set(buffer.subarray(topOffset, topOffset + bytesPerRow));
      //   buffer.copyWithin(topOffset, bottomOffset, bottomOffset + bytesPerRow);
      //   buffer.set(lineBuffer, bottomOffset);
      // }

      imgData.data.set(buffer);
      ctx.putImageData(imgData, 0, 0);
      
      // For now: Flip every time, since we want the correct image transfered over WebRTC
      ctx.globalCompositeOperation = 'copy';
      ctx.scale(1,-1);
      ctx.translate(0, -imgData.height);
      ctx.drawImage(cnv,0,0);
      ctx.setTransform(1,0,0,1,0,0);
      ctx.globalCompositeOperation = 'source-over';
      
      GLctx.bindTexture(GLctx.TEXTURE_2D, dstTexture);
      GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, cnv);
      //GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, cnv);
      GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MAG_FILTER, GLctx.LINEAR);
      GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
      GLctx.generateMipmap(GLctx.TEXTURE_2D);
      GLctx.bindTexture(GLctx.TEXTURE_2D, null);
    }
  },
  
};
mergeInto(LibraryManager.library, UnityWebRTCVideoStreamTrack);