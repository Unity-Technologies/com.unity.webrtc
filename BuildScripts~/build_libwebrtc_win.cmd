@echo off

if not exist depot_tools (
  git clone --depth 1 https://chromium.googlesource.com/chromium/tools/depot_tools.git
)

set COMMAND_DIR=%~dp0
set PATH=%cd%\depot_tools;%PATH%
set WEBRTC_VERSION=5615
set DEPOT_TOOLS_WIN_TOOLCHAIN=0
set GYP_GENERATORS=ninja,msvs-ninja
set GYP_MSVS_VERSION=2019
set OUTPUT_DIR=out
set ARTIFACTS_DIR=%cd%\artifacts
set PYPI_URL=https://artifactory.prd.it.unity3d.com/artifactory/api/pypi/pypi/simple
set vs2019_install=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional

if not exist src (
  call fetch.bat --nohooks webrtc
  cd src
  call git.bat config --system core.longpaths true
  call git.bat checkout  refs/remotes/branch-heads/%WEBRTC_VERSION%
  cd ..
  call gclient.bat sync -D --force --reset
)

rem add jsoncpp
patch -N "src\BUILD.gn" < "%COMMAND_DIR%\patches\add_jsoncpp.patch"

rem fix towupper
patch -N "src\modules\desktop_capture\win\full_screen_win_application_handler.cc" < "%COMMAND_DIR%\patches\fix_towupper.patch"

rem fix abseil
patch -N "src\third_party\abseil-cpp/absl/base/config.h" < "%COMMAND_DIR%\patches\fix_abseil.patch"

mkdir "%ARTIFACTS_DIR%\lib"

setlocal enabledelayedexpansion

for %%i in (x64) do (
  mkdir "%ARTIFACTS_DIR%/lib/%%i"
  for %%j in (true false) do (

    rem generate ninja for release
    call gn.bat gen %OUTPUT_DIR% --root="src" ^
      --args="is_debug=%%j is_clang=true target_cpu=\"%%i\" use_custom_libcxx=false rtc_include_tests=false rtc_build_examples=false rtc_use_h264=false symbol_level=0 enable_iterator_debugging=false use_cxx17=true"

    rem build
    call ninja.bat -C %OUTPUT_DIR% webrtc

    set filename=
    if true==%%j (
      set filename=webrtcd.lib
    ) else (
      set filename=webrtc.lib
    )

    rem copy static library for release build
    copy "%OUTPUT_DIR%\obj\webrtc.lib" "%ARTIFACTS_DIR%\lib\%%i\!filename!"
  )
)

endlocal

rem generate license
call python.bat "%cd%\src\tools_webrtc\libs\generate_licenses.py" ^
  --target :webrtc %OUTPUT_DIR% %OUTPUT_DIR%

rem unescape license
powershell -File "%COMMAND_DIR%\Unescape.ps1" "%OUTPUT_DIR%\LICENSE.md"

rem copy header
xcopy src\*.h "%ARTIFACTS_DIR%\include" /C /S /I /F /H

rem copy license
copy "%OUTPUT_DIR%\LICENSE.md" "%ARTIFACTS_DIR%"

rem create zip
cd %ARTIFACTS_DIR%
7z a -tzip webrtc-win.zip *