target_sources(
  WebRTCLib
  PRIVATE OpenGLGraphicsDevice.cpp OpenGLGraphicsDevice.h OpenGLTexture2D.cpp
          OpenGLTexture2D.h OpenGLContext.cpp OpenGLContext.h)

if(Linux)
  add_subdirectory(implib)
endif()
