if(iOS)
  set(FRAMEWORK_LIBS
    "-framework Foundation"
    "-framework AVFoundation"
    "-framework CoreServices"
    "-framework CoreAudio"
    "-framework CoreVideo"    
    "-framework CoreMedia"
    "-framework CoreGraphics"
    "-framework AudioToolbox"
    "-framework VideoToolbox"
    "-framework Metal"
    "-framework UIKit"
  )
else()
  find_library(CORE_FOUNDATION Foundation)
  find_library(AV_FOUNDATION AVFoundation)
  find_library(CORE_SERVICES CoreServices)
  find_library(APPLICATION_SERVICES ApplicationServices)
  find_library(CORE_AUDIO CoreAudio)
  find_library(CORE_VIDEO CoreVideo)
  find_library(CORE_MEDIA CoreMedia)
  find_library(AUDIO_TOOLBOX AudioToolbox)
  find_library(VIDEO_TOOLBOX VideoToolbox)
  find_library(METAL Metal)

  set(FRAMEWORK_LIBS
    ${CORE_FOUNDATION}
    ${AV_FOUNDATION}
    ${APPLICATION_SERVICES}
    ${CORE_SERVICES}
    ${CORE_AUDIO}
    ${CORE_VIDEO}
    ${CORE_MEDIA}
    ${AUDIO_TOOLBOX}
    ${VIDEO_TOOLBOX}
    ${METAL}
  )
endif()