# Build Native Plugin

This guide will cover building and deploying the native plugin `com.unity.webrtc` depends on.

## Developing environment

Install dependencies to make development environment.

### Windows

On windows, first, you need to install **Visual Studio 2022**. The build process use the **clang** compiler. To install clang, see [MSDN](https://docs.microsoft.com/en-us/cpp/build/clang-support-msbuild?view=msvc-170). And [chocolatey](https://chocolatey.org/) is used to install.

```powershell
# Install CUDA
choco install cuda --version=11.0.3

# Install Windows SDK
# WARNING: If you have versions of Windows SDK earlier than Version 22H2,
# compiling the plugin will fail. Make sure to uninstall earlier versions.
choco install -y vcredist2010 vcredist2013 vcredist140 windows-sdk-11-version-22h2-all

# Install Vulkan
choco install vulkan-sdk --version=1.2.182.0

# Install CMake 3.24.3
choco install cmake -y --version=3.24.3

# Install 7zip (used to extract Google's webrtc library after download)
choco install 7zip

# Setting up environment variables
setx CUDA_PATH "C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v11.0" /m
setx VULKAN_SDK "C:\VulkanSDK\1.2.182.0" /m
```

### Ubuntu

The below commands shows the build process developing environment on **Ubuntu 16.04**.

```bash
# Install clang 11
wget -O - https://apt.llvm.org/llvm-snapshot.gpg.key|sudo apt-key add -
sudo apt-add-repository "deb http://apt.llvm.org/xenial/ llvm-toolchain-xenial-11 main"
sudo apt update
sudo apt install -y clang-11 lld-11

# Install stdlibc++9 for support GLIBCXX_3.4.26
sudo add-apt-repository ppa:ubuntu-toolchain-r/test
sudo apt update
sudo apt install -y g++-9
sudo ln -sf g++-9 /usr/bin/g++

# Install other packages
sudo apt install -y vulkan-utils libvulkan1 libvulkan-dev libglfw3-dev ninja-build

# Install glad2
sudo pip install git+https://github.com/dav1dde/glad.git@glad2#egg=glad2

# Install CUDA SDK
wget https://developer.download.nvidia.com/compute/cuda/repos/ubuntu1604/x86_64/cuda-ubuntu1604.pin
sudo mv cuda-ubuntu1604.pin /etc/apt/preferences.d/cuda-repository-pin-600
sudo apt-key adv --fetch-keys http://developer.download.nvidia.com/compute/cuda/repos/ubuntu1804/x86_64/3bf863cc.pub
sudo apt-key adv --fetch-keys http://developer.download.nvidia.com/compute/cuda/repos/ubuntu1804/x86_64/7fa2af80.pub
sudo add-apt-repository "deb https://developer.download.nvidia.com/compute/cuda/repos/ubuntu1604/x86_64/ /"
sudo apt update
sudo apt install -y cuda-toolkit-11-0

# Install CMake 3.22.3
sudo apt install -y libssl-dev
sudo apt purge -y cmake
wget https://github.com/Kitware/CMake/releases/download/v3.24.3/cmake-3.24.3.tar.gz
tar xvf cmake-3.24.3.tar.gz
cd cmake-3.24.3
./bootstrap && make && sudo make install
```

### macOS

On macOS, [homebrew](https://brew.sh/) is used to install CMake. XCode version **11.0.0 or higher** is used but **Xcode 12 would not work well**.

```bash
# Install CMake
brew install cmake
```

### iOS

On macOS, [homebrew](https://brew.sh/) is used to install CMake. XCode version **11.0.0 or higher** is used but **Xcode 12 would not work well**.

### Android

On Ubuntu (**WSL2** on Windows is also working well), 

```bash
# Download Android NDK r21b
wget https://dl.google.com/android/repository/android-ndk-r21b-linux-x86_64.zip

# Unzip the downloaded NDK file to home directory
unzip android-ndk-r21b-linux-x86_64.zip -d ~/

# Set Android NDK root path to `ANDROID_NDK` environment variable
echo "export ANDROID_NDK=~/android-ndk-r21b/" >> ~/.profile

# Install CMake 3.24.3
sudo apt install -y libssl-dev
sudo apt purge -y cmake
wget https://github.com/Kitware/CMake/releases/download/v3.24.3/cmake-3.24.3.tar.gz
tar xvf cmake-3.24.3.tar.gz
cd cmake-3.24.3
./bootstrap && make && sudo make install

# Install pkg-config, zip
sudo apt install -y pkg-config zip
```

## Build plugin

To build plugin, you need to execute command in the `BuildScripts~` folder.

- [BuildScripts~/build_plugin_android.sh](../BuildScripts~/build_plugin_android.sh)
- [BuildScripts~/build_plugin_mac.sh](../BuildScripts~/build_plugin_mac.sh)
- [BuildScripts~/build_plugin_ios.sh](../BuildScripts~/build_plugin_ios.sh)
- [BuildScripts~/build_plugin_linux.sh](../BuildScripts~/build_plugin_linux.sh)
- [BuildScripts~/build_plugin_win.cmd](../BuildScripts~/build_plugin_win.cmd)
    - Note: If you encounter `LNK1120`, `LNK2001` or `LNK2019` errors while running this build script, it's possible that you may need to open `Plugin~/build64/webrtc.sln` and build from within Visual Studio 2019 instead. You can also use it for development. ([#441](https://github.com/Unity-Technologies/com.unity.webrtc/issues/441))

Alternatively, after the script has been run, a project ready for your IDE or other build tools is ready for you to use/build with (the name of the folder differs based on the target platform, check the script for more details).

### Deploying the Plugin

When you run the build, `webrtc.dll` will be placed in `Packages\com.unity.webrtc\Runtime\Plugins\x86_64`. You should then be able to verify the following settings in the Unity Inspector window.

**WARNING:** If "Load on startup" is not ticked, your editor will crash when running your project. This may become unticked after you make a change to the plugin. ([#444](https://github.com/Unity-Technologies/com.unity.webrtc/issues/444))

<img src="../Documentation~/images/inspector_webrtc_plugin.png" width=400 align=center>

## Debug

The `WebRTC` project properties must be adjusted to match your environment in order to build the plugin. 

Set the Unity.exe path under `Command` and the project path under `Command Arguments`. Once set, during debugging the Unity Editor will run and breakpoints will be enabled.  

<img src="../Documentation~/images/command_config_vs2017.png" width=600 align=center>
