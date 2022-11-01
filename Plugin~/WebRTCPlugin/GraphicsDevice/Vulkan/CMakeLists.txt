target_sources(
  WebRTCLib
  PRIVATE ListOfVulkanFunctions.inl
          LoadVulkanFunctions.cpp
          LoadVulkanFunctions.h
          UnityVulkanInitCallback.cpp
          UnityVulkanInitCallback.h
          VulkanGraphicsDevice.cpp
          VulkanGraphicsDevice.h
          VulkanTexture2D.cpp
          VulkanTexture2D.h
          VulkanUtility.cpp
          VulkanUtility.h)
