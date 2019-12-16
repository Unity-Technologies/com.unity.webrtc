#include "pch.h"
#include "NvEncoder.h"
#include "Context.h"
#include <cstring>
#include "GraphicsDevice/IGraphicsDevice.h"

#if _WIN32
#else
#include <dlfcn.h>
#endif

namespace WebRTC
{
    static void* s_hModule = nullptr;
    static std::unique_ptr<NV_ENCODE_API_FUNCTION_LIST> pNvEncodeAPI;
    static CodecInitializationResult initializationResult = CodecInitializationResult::NotInitialized;

    NvEncoder::NvEncoder(
        const NV_ENC_DEVICE_TYPE type,
        const NV_ENC_INPUT_RESOURCE_TYPE inputType,
        const int width, const int height, IGraphicsDevice* device)
    : width(width), height(height), m_device(device), m_deviceType(type), m_inputType(inputType)
    {
        LogPrint(StringFormat("width is %d, height is %d", width, height).c_str());
        checkf(width > 0 && height > 0, "Invalid width or height!");        
    }

    void NvEncoder::InitV()  {
        bool result = true;
        if (initializationResult == CodecInitializationResult::NotInitialized)
        {
            initializationResult = LoadCodec();
        }
        if(initializationResult != CodecInitializationResult::Success)
        {
            throw initializationResult;
        }
#pragma region open an encode session
        //open an encode session
        NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS openEncodeSessionExParams = { 0 };
        openEncodeSessionExParams.version = NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER;

        openEncodeSessionExParams.device = m_device->GetEncodeDevicePtrV();
        openEncodeSessionExParams.deviceType = m_deviceType;
        openEncodeSessionExParams.apiVersion = NVENCAPI_VERSION;
        errorCode = pNvEncodeAPI->nvEncOpenEncodeSessionEx(&openEncodeSessionExParams, &pEncoderInterface);
        checkf(NV_RESULT(errorCode), StringFormat("Unable to open NvEnc encode session %d", errorCode).c_str());
#pragma endregion
#pragma region set initialization parameters
        nvEncInitializeParams.version = NV_ENC_INITIALIZE_PARAMS_VER;
        nvEncInitializeParams.encodeWidth = width;
        nvEncInitializeParams.encodeHeight = height;
        nvEncInitializeParams.darWidth = width;
        nvEncInitializeParams.darHeight = height;
        nvEncInitializeParams.encodeGUID = NV_ENC_CODEC_H264_GUID;
        nvEncInitializeParams.presetGUID = NV_ENC_PRESET_LOW_LATENCY_HQ_GUID;
        nvEncInitializeParams.frameRateNum = frameRate;
        nvEncInitializeParams.frameRateDen = 1;
        nvEncInitializeParams.enablePTD = 1;
        nvEncInitializeParams.reportSliceOffsets = 0;
        nvEncInitializeParams.enableSubFrameWrite = 0;
        nvEncInitializeParams.encodeConfig = &nvEncConfig;
        nvEncInitializeParams.maxEncodeWidth = 3840;
        nvEncInitializeParams.maxEncodeHeight = 2160;
#pragma endregion
#pragma region get preset ocnfig and set it
        NV_ENC_PRESET_CONFIG presetConfig = { 0 };
        presetConfig.version = NV_ENC_PRESET_CONFIG_VER;
        presetConfig.presetCfg.version = NV_ENC_CONFIG_VER;
        errorCode = pNvEncodeAPI->nvEncGetEncodePresetConfig(pEncoderInterface, nvEncInitializeParams.encodeGUID, nvEncInitializeParams.presetGUID, &presetConfig);
        checkf(NV_RESULT(errorCode), StringFormat("Failed to select NVEncoder preset config %d", errorCode).c_str());
        std::memcpy(&nvEncConfig, &presetConfig.presetCfg, sizeof(NV_ENC_CONFIG));
        nvEncConfig.profileGUID = NV_ENC_H264_PROFILE_BASELINE_GUID;
        nvEncConfig.gopLength = nvEncInitializeParams.frameRateNum;
        nvEncConfig.rcParams.averageBitRate = bitRate;
        nvEncConfig.encodeCodecConfig.h264Config.idrPeriod = nvEncConfig.gopLength;

        nvEncConfig.encodeCodecConfig.h264Config.sliceMode = 0;
        nvEncConfig.encodeCodecConfig.h264Config.sliceModeData = 0;
        nvEncConfig.encodeCodecConfig.h264Config.repeatSPSPPS = 1;
        //Quality Control
        nvEncConfig.encodeCodecConfig.h264Config.level = NV_ENC_LEVEL_H264_51;
#pragma endregion
#pragma region get encoder capability
        NV_ENC_CAPS_PARAM capsParam = { 0 };
        capsParam.version = NV_ENC_CAPS_PARAM_VER;
        capsParam.capsToQuery = NV_ENC_CAPS_ASYNC_ENCODE_SUPPORT;
        int32 asyncMode = 0;
        errorCode = pNvEncodeAPI->nvEncGetEncodeCaps(pEncoderInterface, nvEncInitializeParams.encodeGUID, &capsParam, &asyncMode);
        checkf(NV_RESULT(errorCode), StringFormat("Failded to get NVEncoder capability params %d", errorCode).c_str());
        nvEncInitializeParams.enableEncodeAsync = 0;
#pragma endregion
#pragma region initialize hardware encoder session
        errorCode = pNvEncodeAPI->nvEncInitializeEncoder(pEncoderInterface, &nvEncInitializeParams);
        result = NV_RESULT(errorCode);
        checkf(result, StringFormat("Failed to initialize NVEncoder %d", errorCode).c_str());
#pragma endregion

        InitEncoderResources();
        isNvEncoderSupported = true;

    }
    NvEncoder::~NvEncoder()
    {
        ReleaseEncoderResources();
        if (pEncoderInterface)
        {
            errorCode = pNvEncodeAPI->nvEncDestroyEncoder(pEncoderInterface);
            checkf(NV_RESULT(errorCode), StringFormat("Failed to destroy NV encoder interface %d", errorCode).c_str());
            pEncoderInterface = nullptr;
        }
    }

    CodecInitializationResult NvEncoder::LoadCodec()
    {
        pNvEncodeAPI = std::make_unique<NV_ENCODE_API_FUNCTION_LIST>();
        pNvEncodeAPI->version = NV_ENCODE_API_FUNCTION_LIST_VER;
#if defined(_WIN32)
#if defined(_WIN64)
        HMODULE module = LoadLibrary(TEXT("nvEncodeAPI64.dll"));
#else
        HMODULE module = LoadLibrary(TEXT("nvEncodeAPI.dll"));
#endif
#else
        void *module = dlopen("libnvidia-encode.so.1", RTLD_LAZY);
#endif

        if (module == nullptr)
        {
            LogPrint("NVENC library file is not found. Please ensure NV driver is installed");
            return CodecInitializationResult::DriverNotInstalled;
        }
        s_hModule = module;

        using NvEncodeAPIGetMaxSupportedVersion_Type = NVENCSTATUS(NVENCAPI *)(uint32_t*);
#if defined(_WIN32)
        NvEncodeAPIGetMaxSupportedVersion_Type NvEncodeAPIGetMaxSupportedVersion = (NvEncodeAPIGetMaxSupportedVersion_Type)GetProcAddress(module, "NvEncodeAPIGetMaxSupportedVersion");
#else
        NvEncodeAPIGetMaxSupportedVersion_Type NvEncodeAPIGetMaxSupportedVersion = (NvEncodeAPIGetMaxSupportedVersion_Type)dlsym(module, "NvEncodeAPIGetMaxSupportedVersion");
#endif

        uint32_t version = 0;
        uint32_t currentVersion = (NVENCAPI_MAJOR_VERSION << 4) | NVENCAPI_MINOR_VERSION;
        NvEncodeAPIGetMaxSupportedVersion(&version);
        if (currentVersion > version)
        {
            LogPrint("Current Driver Version does not support this NvEncodeAPI version, please upgrade driver");
            return CodecInitializationResult::DriverVersionDoesNotSupportAPI;
        }

        using NvEncodeAPICreateInstance_Type = NVENCSTATUS(NVENCAPI *)(NV_ENCODE_API_FUNCTION_LIST*);
#if defined(_WIN32)
        NvEncodeAPICreateInstance_Type NvEncodeAPICreateInstance = (NvEncodeAPICreateInstance_Type)GetProcAddress(module, "NvEncodeAPICreateInstance");
#else
        NvEncodeAPICreateInstance_Type NvEncodeAPICreateInstance = (NvEncodeAPICreateInstance_Type)dlsym(module, "NvEncodeAPICreateInstance");
#endif

        if (!NvEncodeAPICreateInstance)
        {
            LogPrint("Cannot find NvEncodeAPICreateInstance() entry in NVENC library");
            return CodecInitializationResult::APINotFound;
        }
        bool result = (NvEncodeAPICreateInstance(pNvEncodeAPI.get()) == NV_ENC_SUCCESS);
        checkf(result, "Unable to create NvEnc API function list");
        if (!result)
        {
            return CodecInitializationResult::APINotFound;
        }
        return CodecInitializationResult::Success;
    }

    void NvEncoder::UnloadCodec()
    {
        if (s_hModule)
        {
#if _WIN32
            FreeLibrary((HMODULE)s_hModule);
#else
            dlclose(s_hModule);
#endif
            s_hModule = nullptr;
        }
        initializationResult = CodecInitializationResult::NotInitialized;
    }

    CodecInitializationResult NvEncoder::InitializationResult()
    {
        return initializationResult;
    }

    void NvEncoder::UpdateSettings()
    {
        bool settingChanged = false;
        if (nvEncConfig.rcParams.averageBitRate != bitRate)
        {
            nvEncConfig.rcParams.averageBitRate = bitRate;
            settingChanged = true;
        }
        if (nvEncInitializeParams.frameRateNum != frameRate)
        {
            nvEncInitializeParams.frameRateNum = frameRate;
            settingChanged = true;
        }

        if (settingChanged)
        {
            NV_ENC_RECONFIGURE_PARAMS nvEncReconfigureParams;
            std::memcpy(&nvEncReconfigureParams.reInitEncodeParams, &nvEncInitializeParams, sizeof(nvEncInitializeParams));
            nvEncReconfigureParams.version = NV_ENC_RECONFIGURE_PARAMS_VER;
            errorCode = pNvEncodeAPI->nvEncReconfigureEncoder(pEncoderInterface, &nvEncReconfigureParams);
            checkf(NV_RESULT(errorCode), StringFormat("Failed to reconfigure encoder setting %d", errorCode).c_str());
        }
    }
    void NvEncoder::SetRate(uint32 rate)
    {
#pragma warning (suppress: 4018)
        if (rate < lastBitRate)
        {
#pragma warning(suppress: 4018)
            bitRate = rate > minBitRate ? rate : minBitRate;
            lastBitRate = bitRate;
        }
    }

    bool NvEncoder::CopyBuffer(void* frame)
    {
        const int curFrameNum = GetCurrentFrameCount() % bufferedFrameNum;
        const auto tex = renderTextures[curFrameNum];
        if (tex == nullptr)
            return false;
        m_device->CopyResourceFromNativeV(tex, frame);
        return true;
    }

    //entry for encoding a frame
    bool NvEncoder::EncodeFrame()
    {
        UpdateSettings();
        uint32 bufferIndexToWrite = frameCount % bufferedFrameNum;
        Frame& frame = bufferedFrames[bufferIndexToWrite];
#pragma region set frame params
        //no free buffer, skip this frame
        if (frame.isEncoding)
        {
            return false;
        }
        frame.isEncoding = true;
#pragma endregion
#pragma region configure per-frame encode parameters
        NV_ENC_PIC_PARAMS picParams = { 0 };
        picParams.version = NV_ENC_PIC_PARAMS_VER;
        picParams.pictureStruct = NV_ENC_PIC_STRUCT_FRAME;
        picParams.inputBuffer = frame.inputFrame.mappedResource;
        picParams.bufferFmt = frame.inputFrame.bufferFormat;
        picParams.inputWidth = nvEncInitializeParams.encodeWidth;
        picParams.inputHeight = nvEncInitializeParams.encodeHeight;
        picParams.outputBitstream = frame.outputFrame;
        picParams.inputTimeStamp = frameCount;
#pragma endregion
#pragma region start encoding
        if (isIdrFrame)
        {
            picParams.encodePicFlags |= NV_ENC_PIC_FLAG_FORCEIDR;
        }
        isIdrFrame = false;
        errorCode = pNvEncodeAPI->nvEncEncodePicture(pEncoderInterface, &picParams);
        checkf(NV_RESULT(errorCode), StringFormat("Failed to encode frame, error is %d", errorCode).c_str());
#pragma endregion
        ProcessEncodedFrame(frame);
        frameCount++;
        return true;
    }

    //get encoded frame
    void NvEncoder::ProcessEncodedFrame(Frame& frame)
    {
        //The frame hasn't been encoded, something wrong
        if (!frame.isEncoding)
        {
            return;
        }
        frame.isEncoding = false;
#pragma region retrieve encoded frame from output buffer
        NV_ENC_LOCK_BITSTREAM lockBitStream = { 0 };
        lockBitStream.version = NV_ENC_LOCK_BITSTREAM_VER;
        lockBitStream.outputBitstream = frame.outputFrame;
        lockBitStream.doNotWait = nvEncInitializeParams.enableEncodeAsync;
        errorCode = pNvEncodeAPI->nvEncLockBitstream(pEncoderInterface, &lockBitStream);
        checkf(NV_RESULT(errorCode), StringFormat("Failed to lock bit stream, error is %d", errorCode).c_str());
        if (lockBitStream.bitstreamSizeInBytes)
        {
            frame.encodedFrame.resize(lockBitStream.bitstreamSizeInBytes);
            std::memcpy(frame.encodedFrame.data(), lockBitStream.bitstreamBufferPtr, lockBitStream.bitstreamSizeInBytes);
        }
        errorCode = pNvEncodeAPI->nvEncUnlockBitstream(pEncoderInterface, frame.outputFrame);
        checkf(NV_RESULT(errorCode), StringFormat("Failed to unlock bit stream, error is %d", errorCode).c_str());
        frame.isIdrFrame = lockBitStream.pictureType == NV_ENC_PIC_TYPE_IDR;
#pragma endregion


        rtc::scoped_refptr<FrameBuffer> buffer = new rtc::RefCountedObject<FrameBuffer>(width, height, frame.encodedFrame);
        int64 timestamp = rtc::TimeMillis();
        webrtc::VideoFrame videoFrame{buffer, webrtc::VideoRotation::kVideoRotation_0, timestamp};
        videoFrame.set_ntp_time_ms(timestamp);
        CaptureFrame(videoFrame);
    }

    NV_ENC_REGISTERED_PTR NvEncoder::RegisterResource(NV_ENC_INPUT_RESOURCE_TYPE inputType, void *buffer)
    {
        NV_ENC_REGISTER_RESOURCE registerResource = { NV_ENC_REGISTER_RESOURCE_VER };
        const auto bufferFormat = NV_ENC_BUFFER_FORMAT_ARGB;
        registerResource.resourceType = inputType;
        registerResource.resourceToRegister = buffer;

        if (!registerResource.resourceToRegister)
            LogPrint("resource is not initialized");
        registerResource.width = width;
        registerResource.height = height;
        if (inputType !=NV_ENC_INPUT_RESOURCE_TYPE_CUDAARRAY)
        {
            registerResource.pitch = GetWidthInBytes(bufferFormat, width);          
        } else{
            registerResource.pitch = width;            
        }
        registerResource.bufferFormat = bufferFormat;
        registerResource.bufferUsage = NV_ENC_INPUT_IMAGE;
        errorCode = pNvEncodeAPI->nvEncRegisterResource(pEncoderInterface, &registerResource);
        checkf(NV_RESULT(errorCode), StringFormat("nvEncRegisterResource error is %d", errorCode).c_str());
        return registerResource.registeredResource;
    }
    void NvEncoder::MapResources(InputFrame& inputFrame)
    {
        NV_ENC_MAP_INPUT_RESOURCE mapInputResource = { 0 };
        mapInputResource.version = NV_ENC_MAP_INPUT_RESOURCE_VER;
        mapInputResource.registeredResource = inputFrame.registeredResource;
        errorCode = pNvEncodeAPI->nvEncMapInputResource(pEncoderInterface, &mapInputResource);
        checkf(NV_RESULT(errorCode), StringFormat("nvEncMapInputResource error is %d", errorCode).c_str());
        inputFrame.mappedResource = mapInputResource.mappedResource;
    }
    NV_ENC_OUTPUT_PTR NvEncoder::InitializeBitstreamBuffer()
    {
        NV_ENC_CREATE_BITSTREAM_BUFFER createBitstreamBuffer = { 0 };
        createBitstreamBuffer.version = NV_ENC_CREATE_BITSTREAM_BUFFER_VER;
        errorCode = pNvEncodeAPI->nvEncCreateBitstreamBuffer(pEncoderInterface, &createBitstreamBuffer);
        checkf(NV_RESULT(errorCode), StringFormat("nvEncCreateBitstreamBuffer error is %d", errorCode).c_str());
        return createBitstreamBuffer.bitstreamBuffer;
    }
    void NvEncoder::InitEncoderResources()
    {
        for (uint32 i = 0; i < bufferedFrameNum; i++)
        {
            renderTextures[i] = m_device->CreateDefaultTextureV(width, height);
            void* buffer = AllocateInputResourceV(renderTextures[i]);

            Frame& frame = bufferedFrames[i];
            frame.inputFrame.registeredResource = RegisterResource(m_inputType, buffer);
            frame.inputFrame.bufferFormat = NV_ENC_BUFFER_FORMAT_ARGB;
            MapResources(frame.inputFrame);
            frame.outputFrame = InitializeBitstreamBuffer();
        }
    }

    void NvEncoder::ReleaseFrameInputBuffer(Frame& frame)
    {
        if(frame.inputFrame.mappedResource)
        {
            errorCode = pNvEncodeAPI->nvEncUnmapInputResource(pEncoderInterface, frame.inputFrame.mappedResource);
            checkf(NV_RESULT(errorCode), StringFormat("Failed to unmap input resource %d", errorCode).c_str());
            frame.inputFrame.mappedResource = nullptr;
        }

        if(frame.inputFrame.registeredResource)
        {
            errorCode = pNvEncodeAPI->nvEncUnregisterResource(pEncoderInterface, frame.inputFrame.registeredResource);
            checkf(NV_RESULT(errorCode), StringFormat("Failed to unregister input buffer resource %d", errorCode).c_str());
            frame.inputFrame.registeredResource = nullptr;
        }
    }
    void NvEncoder::ReleaseEncoderResources()
    {
        for (Frame& frame : bufferedFrames)
        {
            ReleaseFrameInputBuffer(frame);
            errorCode = pNvEncodeAPI->nvEncDestroyBitstreamBuffer(pEncoderInterface, frame.outputFrame);
            checkf(NV_RESULT(errorCode), StringFormat("Failed to destroy output buffer bit stream %d", errorCode).c_str());
            frame.outputFrame = nullptr;
        }
    }
    uint32_t NvEncoder::GetNumChromaPlanes(const NV_ENC_BUFFER_FORMAT bufferFormat)
    {
        switch (bufferFormat)
        {
            case NV_ENC_BUFFER_FORMAT_NV12:
            case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
                return 1;
            case NV_ENC_BUFFER_FORMAT_YV12:
            case NV_ENC_BUFFER_FORMAT_IYUV:
            case NV_ENC_BUFFER_FORMAT_YUV444:
            case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
                return 2;
            case NV_ENC_BUFFER_FORMAT_ARGB:
            case NV_ENC_BUFFER_FORMAT_ARGB10:
            case NV_ENC_BUFFER_FORMAT_AYUV:
            case NV_ENC_BUFFER_FORMAT_ABGR:
            case NV_ENC_BUFFER_FORMAT_ABGR10:
                return 0;
            default:
                return -1;
        }
    }
    uint32_t NvEncoder::GetChromaHeight(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaHeight)
    {
        switch (bufferFormat)
        {
            case NV_ENC_BUFFER_FORMAT_YV12:
            case NV_ENC_BUFFER_FORMAT_IYUV:
            case NV_ENC_BUFFER_FORMAT_NV12:
            case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
                return (lumaHeight + 1)/2;
            case NV_ENC_BUFFER_FORMAT_YUV444:
            case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
                return lumaHeight;
            case NV_ENC_BUFFER_FORMAT_ARGB:
            case NV_ENC_BUFFER_FORMAT_ARGB10:
            case NV_ENC_BUFFER_FORMAT_AYUV:
            case NV_ENC_BUFFER_FORMAT_ABGR:
            case NV_ENC_BUFFER_FORMAT_ABGR10:
                return 0;
            default:
                return 0;
        }
    }
    uint32_t NvEncoder::GetWidthInBytes(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t width)
    {
        switch (bufferFormat) {
            case NV_ENC_BUFFER_FORMAT_NV12:
            case NV_ENC_BUFFER_FORMAT_YV12:
            case NV_ENC_BUFFER_FORMAT_IYUV:
            case NV_ENC_BUFFER_FORMAT_YUV444:
                return width;
            case NV_ENC_BUFFER_FORMAT_YUV420_10BIT:
            case NV_ENC_BUFFER_FORMAT_YUV444_10BIT:
                return width * 2;
            case NV_ENC_BUFFER_FORMAT_ARGB:
            case NV_ENC_BUFFER_FORMAT_ARGB10:
            case NV_ENC_BUFFER_FORMAT_AYUV:
            case NV_ENC_BUFFER_FORMAT_ABGR:
            case NV_ENC_BUFFER_FORMAT_ABGR10:
                return width * 4;
            default:
                return 0;
        }
    }
}
