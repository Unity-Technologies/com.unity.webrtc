# Copyright 2020-2022 NVIDIA Corporation.  All rights reserved.
#
# Please refer to the NVIDIA end user license agreement (EULA) associated
# with this source code for terms and conditions that govern your use of
# this software. Any use, reproduction, disclosure, or distribution of
# this software and related documentation outside the terms of the EULA
# is strictly prohibited.

# 3.7 is required for FindVulkan module support in CMake.
cmake_minimum_required(VERSION 3.7)

project(NvCodec)

# Set C++11 for all projects and disable non-standard extensions
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

set(CMAKE_INSTALL_PREFIX .)

# Check for WSL 
if (EXISTS /usr/lib/wsl/lib)
    set(WSL_FLAG TRUE)
endif()

set(NVCODEC_PUBLIC_INTERFACE_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../Interface)
set(NVCODEC_UTILS_DIR ${CMAKE_CURRENT_SOURCE_DIR}/Utils)
set(NV_CODEC_DIR ${CMAKE_CURRENT_SOURCE_DIR}/NvCodec)
set(NV_ENC_DIR ${CMAKE_CURRENT_SOURCE_DIR}/NvCodec/NvEncoder)
set(NV_DEC_DIR ${CMAKE_CURRENT_SOURCE_DIR}/NvCodec/NvDecoder)
set(NV_APPENC_COMMON_DIR ${CMAKE_CURRENT_SOURCE_DIR}/AppEncode/Common)
set(NV_APPDEC_COMMON_DIR  ${CMAKE_CURRENT_SOURCE_DIR}/AppDecode/Common)

 if(CMAKE_SIZEOF_VOID_P EQUAL 8)
    set(NVCODEC_SAMPLES_INSTALL_DIR ${CMAKE_BINARY_DIR})
else()
    set(NVCODEC_SAMPLES_INSTALL_DIR ${CMAKE_BINARY_DIR})
endif()

if(WIN32)
    if(CMAKE_SIZEOF_VOID_P EQUAL 8)
        Set(CUVID_LIB ${CMAKE_CURRENT_SOURCE_DIR}/../Lib/x64/nvcuvid.lib)
        set(NVENCODEAPI_LIB ${CMAKE_CURRENT_SOURCE_DIR}/../Lib/x64/nvencodeapi.lib)
    else()
        Set(CUVID_LIB ${CMAKE_CURRENT_SOURCE_DIR}/../Lib/Win32/nvcuvid.lib)
        set(NVENCODEAPI_LIB ${CMAKE_CURRENT_SOURCE_DIR}/../Lib/Win32/nvencodeapi.lib)
    endif()
else()
    find_library(CUVID_LIB nvcuvid)
    find_library(NVENCODEAPI_LIB nvidia-encode)
endif()

if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
    find_package(PkgConfig REQUIRED)
    pkg_check_modules(PC_AVCODEC REQUIRED IMPORTED_TARGET libavcodec)
    pkg_check_modules(PC_AVFORMAT REQUIRED IMPORTED_TARGET libavformat)
    pkg_check_modules(PC_AVUTIL REQUIRED IMPORTED_TARGET libavutil)
    pkg_check_modules(PC_SWRESAMPLE REQUIRED IMPORTED_TARGET libswresample)

    set(NV_FFMPEG_HDRS ${PC_AVCODEC_INCLUDE_DIRS})
    find_library(AVCODEC_LIBRARY NAMES avcodec
            HINTS
            ${PC_AVCODEC_LIBDIR}
            ${PC_AVCODEC_LIBRARY_DIRS}
            )
    find_library(AVFORMAT_LIBRARY NAMES avformat
            HINTS
            ${PC_AVFORMAT_LIBDIR}
            ${PC_AVFORMAT_LIBRARY_DIRS}
            )
    find_library(AVUTIL_LIBRARY NAMES avutil
            HINTS
            ${PC_AVUTIL_LIBDIR}
            ${PC_AVUTIL_LIBRARY_DIRS}
            )
    find_library(SWRESAMPLE_LIBRARY NAMES swresample
            HINTS
            ${PC_SWRESAMPLE_LIBDIR}
            ${PC_SWRESAMPLE_LIBRARY_DIRS}
            )
    set(AVCODEC_LIB ${AVCODEC_LIBRARY})
    set(AVFORMAT_LIB ${AVFORMAT_LIBRARY})
    set(AVUTIL_LIB ${AVUTIL_LIBRARY})
    set(SWRESAMPLE_LIB ${SWRESAMPLE_LIBRARY})
endif()


if(WIN32)

    if (NOT DEFINED AGILITY_SDK_BIN)
        message(WARNING "Agility SDK binary path not configured. Excluding AppEncD3D12")
        message(WARNING "Please get Agility SDK and set AGILITY_SDK_BIN cmake variable to point to the directory containing D3D12Core.dll for your platform")
    elseif(NOT DEFINED AGILITY_SDK_VER)
        message(WARNING "Agility SDK version not specified. Excluding AppEncD3D12")
        message(WARNING "Please set AGILITY_SDK_VER cmake variable to the Agility SDK version you are using")
    else()
        # Exclude if D3D12Core.dll is not found
        if (NOT EXISTS "${AGILITY_SDK_BIN}/D3D12Core.dll")
            message(WARNING "Couldnt find D3D12Core.dll at AGILITY_SDK_BIN (${AGILITY_SDK_BIN})")
            message(WARNING "Excluding AppEncD3D12")
        else()
            add_subdirectory(AppEncode/AppEncD3D12)        
        endif()
    endif()
    
    add_subdirectory(AppEncode/AppEncD3D11)
    add_subdirectory(AppEncode/AppEncD3D9)
    add_subdirectory(AppDecode/AppDecD3D)
endif()

if (NOT WSL_FLAG)
    add_subdirectory(AppDecode/AppDecGL)
    add_subdirectory(AppEncode/AppMotionEstimationVkCuda)
    #Need only linux Makefile for this
    if (NOT WIN32)
        add_subdirectory(AppEncode/AppEncGL)
    endif()
endif()

add_subdirectory(AppEncode/AppEncCuda)
add_subdirectory(AppEncode/AppEncDec)
add_subdirectory(AppEncode/AppEncLowLatency)
add_subdirectory(AppEncode/AppEncME)
add_subdirectory(AppEncode/AppEncPerf)
add_subdirectory(AppEncode/AppEncQual)
add_subdirectory(AppTranscode/AppTrans)
add_subdirectory(AppTranscode/AppTransOneToN)
add_subdirectory(AppTranscode/AppTransPerf)
add_subdirectory(AppDecode/AppDec)
add_subdirectory(AppDecode/AppDecImageProvider)
add_subdirectory(AppDecode/AppDecLowLatency)
add_subdirectory(AppDecode/AppDecMem)
add_subdirectory(AppDecode/AppDecMultiFiles)
add_subdirectory(AppDecode/AppDecMultiInput)
add_subdirectory(AppDecode/AppDecPerf)


