target_sources(WebRTCLib
  PRIVATE
    VulkanGraphicsDevice.cpp
    VulkanGraphicsDevice.h
    VulkanTexture2D.cpp
    VulkanTexture2D.h
    VulkanUtility.cpp
    VulkanUtility.h
)

if(Windows OR Linux)
  target_sources(WebRTCLib
    PRIVATE
      UnityVulkanInitCallback.cpp
      UnityVulkanInitCallback.h
  )
endif()