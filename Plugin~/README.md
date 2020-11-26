# Native Plugin

This guide will cover building and deploying the native plugin `com.unity.webrtc` depends on.

## Developing environment

Install dependencies to make development environment.

### How to install dependencies (Windows)

On windows, [chocolatey](https://chocolatey.org/) is used to install.

```powershell
# Install CUDA
choco install cuda --version=10.1

# Install Windows SDK
choco install -y vcredist2010 vcredist2013 vcredist140 windows-sdk-10-version-1809-all

# Install Vulkan
choco install -y wget
wget https://vulkan.lunarg.com/sdk/download/1.1.121.2/windows/VulkanSDK-1.1.121.2-Installer.exe -O C:/Windows/Temp/VulkanSDK.exe
C:/Windows/Temp/VulkanSDK.exe /S

# Install CMake 3.18.0
choco install cmake -y --version 3.18.0

# Setting up environment variables
setx CUDA_PATH "C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v10.1" /m
setx VULKAN_SDK "C:\VulkanSDK\1.1.121.2" /m
```

### How to install dependencies (Ubuntu18.04)

The below commands shows the build process developing environment on Ubuntu 18.04.

```bash
# Install libc++-dev libc++abi-dev clang vulkan-utils libvulkan1 libvulkan-dev
sudo apt-get install -y libc++-dev libc++abi-dev clang vulkan-utils libvulkan1 libvulkan-dev

# Install libc++, libc++abi clang glut
sudo apt update
sudo apt install -y clang freeglut3-dev

# Install CUDA SDK
sudo apt-key adv --fetch-keys http://developer.download.nvidia.com/compute/cuda/repos/ubuntu1804/x86_64/7fa2af80.pub
wget http://developer.download.nvidia.com/compute/cuda/repos/ubuntu1804/x86_64/cuda-repo-ubuntu1804_10.1.243-1_amd64.deb
sudo dpkg -i cuda-repo-ubuntu1804_10.1.243-1_amd64.deb
sudo apt update
sudo apt install -y cuda

# Install CMake 3.18.0
sudo apt install -y libssl-dev
sudo apt purge -y cmake
wget https://github.com/Kitware/CMake/releases/download/v3.18.0/cmake-3.18.0.tar.gz
tar xvf cmake-3.18.0.tar.gz
cd cmake-3.18.0
./bootstrap && make && sudo make install
```

### How to install dependencies (macOS)

On macOS, [homebrew](https://brew.sh/) is used to install CMake. XCode version **11.0.0 or higher** is used but **Xcode 12 would not work well**.

```bash
# Install CMake
brew install cmake
```

### How to install dependencies (iOS)

On macOS, [homebrew](https://brew.sh/) is used to install CMake. XCode version **11.0.0 or higher** is used but **Xcode 12 would not work well**.

```bash
# Install CMake
brew install cmake
```

### Embedding libwebrtc

The plugin relies on [libwebrtc](https://chromium.googlesource.com/external/webrtc/), so building it requires a static libwebrtc link. `webrtc-win.zip` can be found on the [Github Release](https://github.com/Unity-Technologies/com.unity.webrtc/releases) page. If you want to build the library yourself, build script can be found below `BuildScript~` folder.

 <img src="../Documentation~/images/libwebrtc_github_release.png" width=600 align=center>

Download the zip on Github Release page.
Extract the files from the zip, and place them in the `Plugin~` folder.

<img src="../Documentation~/images/deploy_libwebrtc.png" width=500 align=center>

## Build plugin

To build plugin, you need to execute CMake command in the `Plugin~` folder.

### Windows

```bash
# Visual Studio 2017
cmake . -G "Visual Studio 15 2017" -A x64 -B "build"
cmake --build build --config Release --target WebRTCPlugin

# Visual Studio 2019
cmake . -G "Visual Studio 16 2019" -A x64 -B "build"
cmake --build build --config Release --target WebRTCPlugin
```

### macOS
```bash
cmake . -G Xcode -B build
cmake --build build --config Release --target WebRTCPlugin
```

### Linux
```bash
cmake -D CMAKE_C_COMPILER="clang"         \
      -D CMAKE_CXX_COMPILER="clang++"     \
      -D CMAKE_CXX_FLAGS="-stdlib=libc++" \
      -B build
      .
cmake --build build --config Release --target WebRTCPlugin
```

### iOS
```bash
cmake -G Xcode                                 \
  -D CMAKE_SYSTEM_NAME=iOS                     \
  -D "CMAKE_OSX_ARCHITECTURES=arm64;x86_64"    \
  -D CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
  -D CMAKE_XCODE_ATTRIBUTE_ENABLE_BITCODE=YES  \
  .

# for iOS simulator
xcodebuild -sdk iphonesimulator -configuration Release

# for iOS device
xcodebuild -sdk iphoneos -configuration Release

# If you want to make Universal framework, you need to use lipo command 
# to combine two binaries

```


## Debug

The `WebRTC` project properties must be adjusted to match your environment in order to build the plugin. 

Set the Unity.exe path under `Command` and the project path under `Command Arguments`. Once set, during debugging the Unity Editor will run and breakpoints will be enabled.  

<img src="../Documentation~/images/command_config_vs2017.png" width=600 align=center>

### Deploying the Plugin

When you run the build, `webrtc.dll` will be placed in `Packages\com.unity.webrtc\Runtime\Plugins\x86_64`. You should then be able to verify the following settings in the Unity Inspector window. 

<img src="../Documentation~/images/inspector_webrtc_plugin.png" width=400 align=center>

