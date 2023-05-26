#!/bin/bash -eu

export SOLUTION_DIR=$(pwd)/Plugin~

# Install clang-format
sudo apt install clang-format-11

# Install cmake-lang
sudo pip3 install cmakelang

pushd $SOLUTION_DIR

# Check native code format
find . -type f \( -name "*.cpp" -or -name '*.mm' -or -name "*.h" \) \
  ! -path "./libcxx/*" \
  ! -path "./NvCodec/*" \
  ! -path "./unity/*" \
  ! -path "./gl3w/*" \
  ! -path "./webrtc/*" \
  | xargs -I % clang-format-11 -style=file --dry-run --Werror %

# Check CMakeLists.txt format
find . -name CMakeLists.txt ! -path "*/glad/*" | \
  xargs -I % cmake-format --check % -c .cmake-format

popd