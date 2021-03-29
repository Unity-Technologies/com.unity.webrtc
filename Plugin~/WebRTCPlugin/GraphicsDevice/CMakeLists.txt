target_sources(WebRTCLib
  PRIVATE
    GraphicsDevice.cpp
    GraphicsDevice.h
    GraphicsDeviceType.h
    GraphicsUtility.cpp
    GraphicsUtility.h
    IGraphicsDevice.cpp
    IGraphicsDevice.h
    ITexture2D.cpp
    ITexture2D.h
)

if(Windows)
  add_subdirectory(Vulkan)
  add_subdirectory(D3D11)
  add_subdirectory(D3D12)
  add_subdirectory(Cuda)
elseif(iOS OR macOS)
  add_subdirectory(Metal)
elseif(Linux)
  add_subdirectory(Vulkan)
  add_subdirectory(OpenGL)
  add_subdirectory(Cuda)
elseif(Android)
  add_subdirectory(Vulkan)
  add_subdirectory(OpenGL) 
endif()
