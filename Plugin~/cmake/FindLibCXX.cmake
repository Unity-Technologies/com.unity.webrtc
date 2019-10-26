set(LIBRARY_PATHS
  /usr/lib/x86_64-linux-gnu
 )

set(LIBCXX_INCLUDE_DIRS
  /usr/include/c++/v1
)


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