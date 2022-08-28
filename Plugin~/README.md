# Build Native Plugin

This guide will cover building and deploying the native plugin `com.unity.webrtc` depends on.

## Developing the native library

### Ubuntu 20.04

```powershell
sudo apt-get install zip unzip pkg-config python-is-python3
git clone https://github.com/aaron-stafford/com.unity.webrtc.git
cd com.unity.webrtc; bash BuildScripts~/build_libwebrtc_android.sh
```

## Developing the native plugin

### Ubuntu 20.04

```powershell
sudo apt-get update
sudo apt-get install -y zip unzip build-essential
wget https://github.com/Kitware/CMake/releases/download/v3.24.1/cmake-3.24.1-linux-x86_64.sh
bash cmake-3.24.1-linux-x86_64.sh --skip-license
echo 'PATH="$HOME/cmake-3.24.1-linux-x86_64/bin:$PATH"' >> .profile >> .profile;source .profile
git clone https://github.com/aaron-stafford/com.unity.webrtc.git
mkdir -p com.unity.webrtc/Plugin~/webrtc/lib/x86_64

# Uploaded .a files (artifacts from the native library build process)

# Download android NDK
unzip android-ndk-r25b-linux.zip
echo 'export ANDROID_NDK="$HOME/android-ndk-r25b"' >> .profile;source .profile

# The following line is a work around for a mis-match in the current tool chain.
rm android-ndk-r25b/toolchains/llvm/prebuilt/linux-x86_64/bin/ld
cd com.unity.webrtc; bash BuildScripts~/build_plugin_android.sh
```

For any other development environments, refer to [WebRTC for Unity development
instructions](https://github.com/Unity-Technologies/com.unity.webrtc/tree/develop/Plugin~)
