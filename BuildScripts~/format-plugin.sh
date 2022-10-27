#!/bin/bash -eu

export SOLUTION_DIR=$(pwd)/Plugin~

# Install cmake-lang
pip3 install cmakelang

pushd $SOLUTION_DIR

# Check format
find . -name CMakeLists.txt -not -path "*/glad/*" | \
  xargs -I % cmake-format --check % -c .cmake-format

popd