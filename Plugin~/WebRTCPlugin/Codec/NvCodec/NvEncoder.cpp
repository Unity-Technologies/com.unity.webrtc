#include "pch.h"
#include "NvEncoder.h"
#include "Context.h"
#include <cstring>
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

#if _WIN32
#else
#include <dlfcn.h>
#endif

namespace unity
{
namespace webrtc
{

    static void* s_hModule = nullptr;
    static std::unique_ptr<NV_ENCODE_API_FUNCTION_LIST> pNvEncodeAPI = nullptr;

    NvEncoder::NvEncoder(
        const NV_ENC_DEVICE_TYPE type,
        const NV_ENC_INPUT_RESOURCE_TYPE inputType,
        const NV_ENC_BUFFER_FORMAT bufferFormat,
        const int width,
        const int height,
        IGraphicsDevice* device,
        UnityRenderingExtTextureFormat textureFormat)
    : m_width(width)
    , m_height(height)
    , m_device(device)
    , m_textureFormat(textureFormat)
    , m_deviceType(type)
    , m_inputType(inputType)
    , m_bufferFormat(bufferFormat)
    , m_clock(webrtc::Clock::GetRealTimeClock())
    {
    }

    void NvEncoder::InitV()
    {
        bool result = true;
        if (m_initializationResult == CodecInitializationResult::NotInitialized)
        {
            m_initializationResult = LoadCodec();
        }
        if(m_initializationResult != CodecInitializationResult::Success)
        {
            throw m_initializationResult;
        }
#pragma region open an encode session
        //open an encode session
        NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS openEncodeSessionExParams = { 0 };
        openEncodeSessionExParams.version = NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER;

        openEncodeSessionExParams.device = m_device->GetEncodeDevicePtrV();
        openEncodeSessionExParams.deviceType = m_deviceType;
        openEncodeSessionExParams.apiVersion = NVENCAPI_VERSION;

        errorCode = pNvEncodeAPI->nvEncOpenEncodeSessionEx(&openEncodeSessionExParams, &pEncoderInterface);

        if(!NV_RESULT(errorCode))
        {
            m_initializationResult = CodecInitializationResult::EncoderInitializationFailed;
            return;
        }

        checkf(NV_RESULT(errorCode), StringFormat("Unable to open NvEnc encode session %d", errorCode).c_str());
#pragma endregion
#pragma region set initialization parameters
        nvEncInitializeParams.version = NV_ENC_INITIALIZE_PARAMS_VER;
        nvEncInitializeParams.encodeWidth = m_width;
        nvEncInitializeParams.encodeHeight = m_height;
        nvEncInitializeParams.darWidth = m_width;
        nvEncInitializeParams.darHeight = m_height;
        nvEncInitializeParams.encodeGUID = NV_ENC_CODEC_H264_GUID;
        nvEncInitializeParams.presetGUID = NV_ENC_PRESET_LOW_LATENCY_HQ_GUID;
        nvEncInitializeParams.frameRateNum = m_frameRate;
        nvEncInitializeParams.frameRateDen = 1;
        nvEncInitializeParams.enablePTD = 1;
        nvEncInitializeParams.reportSliceOffsets = 0;
        nvEncInitializeParams.enableSubFrameWrite = 0;
        nvEncInitializeParams.encodeConfig = &nvEncConfig;

        // Note:: Encoder will not allow dynamic resolution change.
        // Please set values if you want to support dynamic resolution change.
        nvEncInitializeParams.maxEncodeWidth = 0;
        nvEncInitializeParams.maxEncodeHeight = 0;
#pragma endregion
#pragma region get preset config and set it
        NV_ENC_PRESET_CONFIG presetConfig = { 0 };
        presetConfig.version = NV_ENC_PRESET_CONFIG_VER;
        presetConfig.presetCfg.version = NV_ENC_CONFIG_VER;
        errorCode = pNvEncodeAPI->nvEncGetEncodePresetConfig(pEncoderInterface, nvEncInitializeParams.encodeGUID, nvEncInitializeParams.presetGUID, &presetConfig);
        checkf(NV_RESULT(errorCode), StringFormat("Failed to select NVEncoder preset config %d", errorCode).c_str());
        std::memcpy(&nvEncConfig, &presetConfig.presetCfg, sizeof(NV_ENC_CONFIG));
        nvEncConfig.profileGUID = NV_ENC_H264_PROFILE_BASELINE_GUID;
        nvEncConfig.gopLength = nvEncInitializeParams.frameRateNum;
        nvEncConfig.rcParams.rateControlMode = NV_ENC_PARAMS_RC_CBR_LOWDELAY_HQ;
        nvEncConfig.rcParams.averageBitRate =
            (static_cast<unsigned int>(5.0f *
            nvEncInitializeParams.encodeWidth *
            nvEncInitializeParams.encodeHeight) / (m_width * m_height)) * 100000;
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
        checkf(NV_RESULT(errorCode), StringFormat("Failed to get NVEncoder capability params %d", errorCode).c_str());
        nvEncInitializeParams.enableEncodeAsync = 0;
#pragma endregion
#pragma region initialize hardware encoder session
        errorCode = pNvEncodeAPI->nvEncInitializeEncoder(pEncoderInterface, &nvEncInitializeParams);
        result = NV_RESULT(errorCode);
        checkf(result, StringFormat("Failed to initialize NVEncoder %d", errorCode).c_str());
#pragma endregion
        InitEncoderResources();
        m_isNvEncoderSupported = true;
    }

    NvEncoder::~NvEncoder()
    {
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

        if (!LoadModule())
        {
            return CodecInitializationResult::DriverNotInstalled;
        }

        if(!CheckDriverVersion())
        {
            return CodecInitializationResult::DriverVersionDoesNotSupportAPI;
        }

        using NvEncodeAPICreateInstance_Type = NVENCSTATUS(NVENCAPI *)(NV_ENCODE_API_FUNCTION_LIST*);
#if defined(_WIN32)
        NvEncodeAPICreateInstance_Type NvEncodeAPICreateInstance = (NvEncodeAPICreateInstance_Type)GetProcAddress((HMODULE)s_hModule, "NvEncodeAPICreateInstance");
#else
        NvEncodeAPICreateInstance_Type NvEncodeAPICreateInstance = (NvEncodeAPICreateInstance_Type)dlsym(s_hModule, "NvEncodeAPICreateInstance");
#endif

        if (!NvEncodeAPICreateInstance)
        {
            RTC_LOG(LS_INFO) << "Cannot find NvEncodeAPICreateInstance() entry in NVENC library";
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

    bool NvEncoder::CheckDriverVersion()
    {
        using NvEncodeAPIGetMaxSupportedVersion_Type = NVENCSTATUS(NVENCAPI*)(uint32_t*);
#if defined(_WIN32)
        NvEncodeAPIGetMaxSupportedVersion_Type NvEncodeAPIGetMaxSupportedVersion =
            (NvEncodeAPIGetMaxSupportedVersion_Type)GetProcAddress((HMODULE)s_hModule, "NvEncodeAPIGetMaxSupportedVersion");
#else
        NvEncodeAPIGetMaxSupportedVersion_Type NvEncodeAPIGetMaxSupportedVersion =
            (NvEncodeAPIGetMaxSupportedVersion_Type)dlsym(s_hModule, "NvEncodeAPIGetMaxSupportedVersion");
#endif

        uint32_t version = 0;
        uint32_t currentVersion = (NVENCAPI_MAJOR_VERSION << 4) | NVENCAPI_MINOR_VERSION;
        NvEncodeAPIGetMaxSupportedVersion(&version);
        if (currentVersion > version)
        {
            RTC_LOG(LS_INFO) << "Current Driver Version does not support this NvEncodeAPI version, please upgrade driver";
            return false;
        }
        return true;
    }

    bool NvEncoder::LoadModule()
    {
        if (s_hModule != nullptr)
            return true;

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
            RTC_LOG(LS_INFO) << "NVENC library file is not found. Please ensure NV driver is installed";
            return false;
        }
        s_hModule = module;
        return true;
    }

    void NvEncoder::UnloadModule()
    {
        if (s_hModule)
        {
#if defined(_WIN32)
            FreeLibrary((HMODULE)s_hModule);
#else
            dlclose(s_hModule);
#endif
            s_hModule = nullptr;
        }
    }

    void NvEncoder::UpdateSettings()
    {
        bool settingChanged = false;
        if (nvEncConfig.rcParams.averageBitRate != m_targetBitrate)
        {
            nvEncConfig.rcParams.averageBitRate = m_targetBitrate;
            settingChanged = true;
        }
        if (nvEncInitializeParams.frameRateNum != m_frameRate)
        {
            // nvcodec do not allow a framerate over 240
            const uint32_t kMaxFramerate = 240;
            uint32_t targetFramerate = std::min(m_frameRate, kMaxFramerate);
            nvEncInitializeParams.frameRateNum = targetFramerate;
            settingChanged = true;
        }

        if (settingChanged)
        {
            NV_ENC_RECONFIGURE_PARAMS nvEncReconfigureParams;
            std::memcpy(&nvEncReconfigureParams.reInitEncodeParams, &nvEncInitializeParams, sizeof(nvEncInitializeParams));
            nvEncReconfigureParams.version = NV_ENC_RECONFIGURE_PARAMS_VER;
            errorCode = pNvEncodeAPI->nvEncReconfigureEncoder(pEncoderInterface, &nvEncReconfigureParams);
            checkf(NV_RESULT(errorCode), StringFormat("Failed to reconfigure encoder setting %d %d %d",
                errorCode, nvEncInitializeParams.frameRateNum, nvEncConfig.rcParams.averageBitRate).c_str());
        }
    }

    void NvEncoder::SetRates(uint32_t bitRate, int64_t frameRate)
    {
        m_frameRate = static_cast<uint32_t>(frameRate);
        m_targetBitrate = bitRate;
        isIdrFrame = true;
    }

    bool NvEncoder::CopyBuffer(void* frame)
    {
        const int curFrameNum = GetCurrentFrameCount() % bufferedFrameNum;
        const auto tex = m_renderTextures[curFrameNum];
        if (tex == nullptr)
            return false;
        m_device->CopyResourceFromNativeV(tex, frame);
        return true;
    }

    //entry for encoding a frame
    bool NvEncoder::EncodeFrame(int64_t timestamp_us)
    {
        UpdateSettings();
        uint32 bufferIndexToWrite = frameCount % bufferedFrameNum;
        Frame& frame = bufferedFrames[bufferIndexToWrite];
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
            picParams.encodePicFlags = NV_ENC_PIC_FLAG_FORCEIDR | NV_ENC_PIC_FLAG_FORCEINTRA;
            isIdrFrame = false;
        }
        errorCode = pNvEncodeAPI->nvEncEncodePicture(pEncoderInterface, &picParams);
        checkf(NV_RESULT(errorCode), StringFormat("Failed to encode frame, error is %d", errorCode).c_str());
#pragma endregion
        ProcessEncodedFrame(frame, timestamp_us);
        frameCount++;
        return true;
    }

    //get encoded frame
    void NvEncoder::ProcessEncodedFrame(Frame& frame, int64_t timestamp_us)
    {
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
#pragma endregion
        const rtc::scoped_refptr<FrameBuffer> buffer =
            new rtc::RefCountedObject<FrameBuffer>(
                m_width, m_height, frame.encodedFrame, m_encoderId);
        const int64_t now_us = rtc::TimeMicros();
        const int64_t translated_camera_time_us =
            timestamp_aligner_.TranslateTimestamp(
                timestamp_us,
                now_us);

        webrtc::VideoFrame::Builder builder =
            webrtc::VideoFrame::Builder()
            .set_video_frame_buffer(buffer)
            .set_timestamp_us(translated_camera_time_us)
            .set_timestamp_rtp(0)
            .set_ntp_time_ms(rtc::TimeMillis());

        CaptureFrame(builder.build());
    }

    NV_ENC_REGISTERED_PTR NvEncoder::RegisterResource(NV_ENC_INPUT_RESOURCE_TYPE inputType, void *buffer)
    {
        NV_ENC_REGISTER_RESOURCE registerResource = { NV_ENC_REGISTER_RESOURCE_VER };
        registerResource.resourceType = inputType;
        registerResource.resourceToRegister = buffer;

        if (!registerResource.resourceToRegister)
            RTC_LOG(LS_INFO) << "resource is not initialized";
        registerResource.width = m_width;
        registerResource.height = m_height;
        if (inputType != NV_ENC_INPUT_RESOURCE_TYPE_CUDAARRAY)
        {
            registerResource.pitch = GetWidthInBytes(m_bufferFormat, m_width);          
        } else{
            registerResource.pitch = m_width;            
        }
        registerResource.bufferFormat = m_bufferFormat;
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
            m_renderTextures[i] = m_device->CreateDefaultTextureV(m_width, m_height, m_textureFormat);
            void* buffer = AllocateInputResourceV(m_renderTextures[i]);
            m_buffers.push_back(buffer);
            Frame& frame = bufferedFrames[i];
            frame.inputFrame.registeredResource = RegisterResource(m_inputType, buffer);
            frame.inputFrame.bufferFormat = m_bufferFormat;
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
    void NvEncoder::ReleaseEncoderResources() {
        for (Frame &frame : bufferedFrames) {
            ReleaseFrameInputBuffer(frame);
            if (frame.outputFrame != nullptr) {
                errorCode = pNvEncodeAPI->nvEncDestroyBitstreamBuffer(pEncoderInterface, frame.outputFrame);
                checkf(NV_RESULT(errorCode),
                       StringFormat("Failed to destroy output buffer bit stream %d", errorCode).c_str());
                frame.outputFrame = nullptr;
            }
        }

        for (auto &renderTexture : m_renderTextures) {
            delete renderTexture;
            renderTexture = nullptr;
        }
        for (auto &buffer : m_buffers)
        {
            ReleaseInputResourceV(buffer);
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
    
} // end namespace webrtc
} // end namespace unity
