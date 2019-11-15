# Find libGLEW include path

set(GLEW_DIR "${CMAKE_SOURCE_DIR}/glew")

set(GLEW_INCLUDE_DIRS
    ${GLEW_DIR}/include
)

set(LIBRARY_PATHS
  ${GLEW_DIR}/lib
)

find_library(GLEW_STATIC_LIBRARY
  NAMES GLEW
  PATHS ${LIBRARY_PATHS}
)