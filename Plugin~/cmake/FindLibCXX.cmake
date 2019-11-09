set(LIBRARY_PATHS
  /usr/lib/x86_64-linux-gnu
 )

set(LIBCXX_INCLUDE_DIRS
  /usr/include/c++/v1
)

set(CMAKE_FIND_LIBRARY_SUFFIXES ".a" ".so")

find_library(CXX_LIBRARY
  NAMES c++
  PATHS ${LIBRARY_PATHS}
)

find_library(CXXABI_LIBRARY
  NAMES c++abi
  PATHS ${LIBRARY_PATHS}
)

set(LIBCXX_LIBRARIES
  ${CXX_LIBRARY}
  ${CXXABI_LIBRARY}
)

set(CMAKE_FIND_LIBRARY_SUFFIXES ".a")

find_library(CXX_STATIC_LIBRARY
  NAMES c++
  PATHS ${LIBRARY_PATHS}
)

find_library(CXXABI_STATIC_LIBRARY
  NAMES c++abi
  PATHS ${LIBRARY_PATHS}
)

set(LIBCXX_STATIC_LIBRARIES
  ${CXX_STATIC_LIBRARY}
  ${CXXABI_STATIC_LIBRARY}
)