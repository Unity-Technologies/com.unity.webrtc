# Changelog
All notable changes to the webrtc package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added the ability to execute pending native tasks manually from the main thread.

## [2.4.0-exp.4] - 2021-08-19

### Added

- mac M1 architecture native support
- Audio stream rendering support
- Add two scenes (`Audio` and `MultiAudioReceive`) into the package sample
- Add `RTCAudioSourceStats` and `VideoSourceStats` class

### Changed

- Add the audio waveform graph to `MultiplePeerConnections` scene in the sample

### Fixed 

- Fix the crash on Windows with Vulkan API on `VideoReceieSample` 
- Fix the crash when calling `WebRTC.Initialize` twice
- Fix the error in the build process on `Unity2021.2`

## [2.4.0-exp.3] - 2021-06-08

### Changed

- Add options of the incoming video in `VideoReceive` sample to test video capture modules on the device

### Fixed

- Fix the validation for the color space of the incoming video
- Fix the crash when accessing the property `RTCRtpSender.Track`

## [2.4.0-exp.2] - 2021-05-21

### Fixed

- Fix the color space of the RenderTexture for streaming when using Vulkan API
- Fix sample scenes
- Add a short version string to info.plist of ios framework

### Changed

- Add the validation of the streaming texture size on Android
- Add the validation of the  streaming when using NvCodec 
- Use the software video decoder when disabling hardware acceleration

## [2.4.0-exp.1] - 2021-04-23

### Added

- Android ARM64 platform support
- Added a sample scene "Menu" which developers can go back and forth between sample scenes
- Added a sample scene "PerfectNegotiation"
- Added the software encoder on Linux support when using OpenGL Core graphics API 
- Added the `RestartIce` method to the `RTCPeerConnection` class
- Added the `Streams` property to the `RTCRtpReceiver` class

### Changed

- Unity 2020.3 support
- Upgrade libwebrtc [m89](https://groups.google.com/g/discuss-webrtc/c/Zrsn2hi8FV0/m/KIbn0EZPBQAJ)
- Changed the argument type of the `RTCPeerConnection.CreateOffer` method and the `RTCPeerConnection.CreateAnswer` method

### Fixed

- Fixed crash for accessing properties of `RTCDataChannel` instance
- Fixed crash when using the invalid graphics format to stream video on macOS

## [2.3.3-preview] - 2021-02-26

### Added

- Added `OnConnectionStateChange` event to the `RTCPeerConnection` class

### Fixed

- Fixed a crash bug that occurs when accessing `MediaStreamTrack` properties after disposing of `RTCPeerConnection`

## [2.3.2-preview] - 2021-02-12

### Changed

- Changed `Audio.CaptureStream` method to allow setting of audio track label.

### Fixed

- Fixed memory leaks in native code.
- Fixed a crash bug when access an instance after disposed of it.
- Fixed `MediaStream.GetVideoStreamTrack` method and `MediaStream.GetVideoStreamTrack` method to return a correct value.
- Fixed `RTCRtpTransceiver.Receiver` property and `RTCRtpTransceiver.Sender` property to return a correct value.

## [2.3.1-preview] - 2021-01-07

### Fixed

- Fixed `RTCIceCandidate.candidate` property in order to return a correct SDP formatted string.

## [2.3.0-preview] - 2020-12-28

### Added 

- Supported iOS platform
- Supported H.264 HW decoder (VideoToolbox) on macOS
- Added `GetCapabilities` method to the `RTCRtpSender` class and the `RTCRtpReceiver` class
- Added `SetCodecPreferences` method to the `RTCRtpTransceiver` class
- Added two samples (`ChangeCodecs`, `TrickleIce`)
- Added properties to the `RTCIceCandidate` class
- Added properties tp the `RTCDataChannelInit` class

### Changed

- Changed `RTCIceCandidate` type from `struct` to `class`
- Changed `RTCIceCandidateInit` type from `struct` to `class`
- Changed `RTCDataChannelInit` type from `struct` to `class`
- Changed argumments of the `RTCPeerConnection.AddIceCandidate` method
```csharp
// old
public void AddIceCandidate(ref RTCIceCandidate candidate);
// new
public bool AddIceCandidate(RTCIceCandidate candidate);
```

- Changed arguments of the `RTCPeerConnection.CreateDataChannel` method
```csharp
// old
public RTCDataChannel CreateDataChannel(string label, ref RTCDataChannelInit options);
// new
public RTCDataChannel CreateDataChannel(string label RTCDataChannelInit options = null);
```

## [2.2.1-preview] - 2020-11-13

### Added

- Added a `Bandwidth` sample

### Fixed

- Fixed the receiver of video streaming with Vulkan API
- Fixed a crash bug when the application ended using Vulkan API
- Fixed a crash bug of the standalone build using Vulkan API
- Fixed bugs that occur on Linux not installed NVIDIA driver
- Fixed a bug of the `VideoReceive` sample

## [2.2.0-preview] - 2020-10-26

### Added

- Software decoder support
- Hardware encoder (VideoToolbox) support on macOS
- Vulkan API support on Linux and Windows
- Linux IL2CPP support
- Add WebRTC samples (`MultiplePeerConnections`, `MultiVideoReceive`, `MungeSDP`, `VideoReceive`)

### Changed

- Upgrade libwebrtc m85
- Upgrade NVIDIA Codec SDK 9.1
- Changed `RTCPeerConnection` behaviour to throw exceptions when pass invalid arguments to `SetLocalDescription`, `SetRemoteDescription` 

## [2.1.3-preview] - 2020-09-28

### Changed

- Add "minBitrate" parameter to `RTCRtpEncodingParameters` class.

## [2.1.2-preview] - 2020-09-14

### Changed

- Erase Japanese documentation due to migrating to internal translation system.

## [2.1.1-preview] - 2020-09-11

### Fixed

- Fixed an issue where the `RTCRtpSender.SetParameters` API did not work properly
- Removed ZWSP(zero-width-space) in C# code

## [2.1.0-preview] - 2020-08-24

### Added

- Added statistics window in Unity editor to allow checking the operation of WebRTC
- Added `RTCPeerConnection.GetStats` API which collect statistics of WebRTC
- Added `RTCRtpSender.SetParameters` and `RTCRtpSender.GetParameters` to adjustment streaming video quality
- Added `RTCDataChannel.ReadyState` which shows the state of the channel

### Fixed

- Fixed a issue which video stream remains with bad quality after a short network degradation

## [2.0.5-preview] - 2020-07-30

### Fixed

- Upgrade libwebrtc m84 to fix security issue (https://bugs.chromium.org/p/project-zero/issues/detail?id=2034)

## [2.0.4-preview] - 2020-07-10

### Fixed

- Fix a crash bug when dispose a video track

## [2.0.3-preview] - 2020-06-05

### Fixed

- Fix the memory leak when using DirectX12

## [2.0.2-preview] - 2020-05-14

### Fixed

- Fix the crash when using the incorrect parameter to as the argument of `RTCDataChannel` constructor
- Fix the crash when initializing the hardware encoder failed
- Fix the editor freeze bug when recompiling scripts
- Fixed documents

## [2.0.1-preview] - 2020-05-01

### Fixed

- Fixed versioning issue

## [2.0.0-preview] - 2020-04-30

### Added

- Multi camera support
- DirectX 12 API support
- Published VideoStreamTrack API
- Published AudioStreamTrack API

## [1.1.2-preview] - 2020-03-19

### Fixed

- Fix OpenGL color order

## [1.1.1-preview] - 2020-02-28

### Fixed

- Fix DLL import error

## [1.1.0-preview] - 2020-02-25

### Added

- IL2CPP support
- Linux OpenGL hardware encoder support
- Mac OS Metal software encoder support
- Windows DirectX11 software encoder support

### Changed

- Changed `Audio.Update` method to public

## [1.0.1-preview] - 2019-09-22

### Fixed

- Fixed documents

## [1.0.0-preview] - 2019-08-22

### Added

- Added tooltips

### Changed

- Renamed sample folders

## [0.2.0-preview] - 2019-07-30

### Changed

- Output logs when NVCodec failed to initialize

## [0.1.0-preview] - 2019-07-02

- Initial Release
