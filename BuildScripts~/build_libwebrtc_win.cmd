@echo off

if not exist depot_tools (
git clone --depth 1 https://chromium.googlesource.com/chromium/tools/depot_tools.git
)

set PATH=%cd%\depot_tools;%PATH%
set WEBRTC_VERSION=4103
set DEPOT_TOOLS_WIN_TOOLCHAIN=0
set CPPFLAGS=/WX-
set GYP_GENERATORS=ninja,msvs-ninja
set GYP_MSVS_VERSION=2017
set OUTPUT_DIR=out
set ARTIFACTS_DIR=%cd%\artifacts

cmd /k fetch.bat webrtc

cd src
cmd /k git.bat config --system core.longpaths true
cmd /k git.bat checkout  refs/remotes/branch-heads/%WEBRTC_VERSION%
cd ..

cmd /k gclient.bat sync -f

REM add jsoncpp
patch "src\BUILD.gn" < "BuildScripts~\add_jsoncpp.patch"

REM install pywin32
cmd /k %cd%\depot_tools\bootstrap-3_8_0_chromium_8_bin\python\bin\python.exe -m pip install pywin32

cmd /k gn.bat gen %OUTPUT_DIR% --root="src" --args="is_debug=false is_clang=false target_cpu=\"x64\" rtc_include_tests=false rtc_build_examples=false rtc_use_h264=false symbol_level=0 enable_iterator_debugging=false"

REM add json.obj in link list of webrtc.ninja
powershell -File ".\BuildScripts~\ReplaceText.ps1" "%OUTPUT_DIR%\obj\webrtc.ninja" "obj/rtc_base/rtc_base/crc32.obj" "obj/rtc_base/rtc_base/crc32.obj obj/rtc_base/rtc_json/json.obj"
type "%OUTPUT_DIR%\obj\webrtc.ninja"

REM update LIB_TO_LICENSES_DICT in generate_licenses.py
powershell -File ".\BuildScripts~\ReplaceText.ps1" "src\tools_webrtc\libs\generate_licenses.py" "'ow2_asm': []," "'ow2_asm': [], 'winsdk_samples': [], 'googletest': ['third_party/googletest/src/LICENSE'], 'nasm': ['third_party/nasm/LICENSE'], "
type "src\tools_webrtc\libs\generate_licenses.py"

ninja.exe -C %OUTPUT_DIR%

REM generate license
call python.bat .\src\tools_webrtc\libs\generate_licenses.py --target //:default %OUTPUT_DIR% %OUTPUT_DIR%

REM unescape license
powershell -File .\Unescape.ps1 %OUTPUT_DIR%\LICENSE.md			

REM copy header
xcopy src\*.h %ARTIFACTS_DIR%\include /C /S /I /F /H

REM copy lib
mkdir %ARTIFACTS_DIR%\lib
for %%G in (webrtc.lib audio_decoder_opus.lib webrtc_opus.lib) do forfiles /P "%cd%\%OUTPUT_DIR%" /S /M %%G /C "cmd /c copy @path %ARTIFACTS_DIR%\lib"

REM copy license
copy %OUTPUT_DIR%\LICENSE.md %ARTIFACTS_DIR%

REM create zip
cd %ARTIFACTS_DIR%
7z a -tzip webrtc-win.zip *