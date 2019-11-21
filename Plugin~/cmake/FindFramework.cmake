
find_library(CORE_FOUNDATION Foundation)
find_library(APPLICATION_SERVICES ApplicationServices)
find_library(CORE_SERVICES CoreServices)
find_library(CORE_AUDIO CoreAudio)
find_library(AUDIO_TOOLBOX AudioToolbox)
find_library(METAL Metal)

set(FRAMEWORK_LIBS
  ${CORE_FOUNDATION}
  ${APPLICATION_SERVICES}
  ${CORE_SERVICES}
  ${CORE_AUDIO}
  ${AUDIO_TOOLBOX}
  ${METAL}
)