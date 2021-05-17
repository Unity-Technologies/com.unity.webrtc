using System;
using UnityEditor;
using UnityEngine;

namespace Unity.WebRTC
{
    internal class NvEnc
    {
        public enum Codec
        {
            H264,
            HEVC
        }

        public enum Caps
        {
            NV_ENC_CAPS_NUM_MAX_BFRAMES,
            NV_ENC_CAPS_SUPPORTED_RATECONTROL_MODES,
            NV_ENC_CAPS_SUPPORT_FIELD_ENCODING,
            NV_ENC_CAPS_SUPPORT_MONOCHROME,
            NV_ENC_CAPS_SUPPORT_FMO,
            NV_ENC_CAPS_SUPPORT_QPELMV,
            NV_ENC_CAPS_SUPPORT_BDIRECT_MODE,
            NV_ENC_CAPS_SUPPORT_CABAC,
            NV_ENC_CAPS_SUPPORT_ADAPTIVE_TRANSFORM,
            NV_ENC_CAPS_SUPPORT_STEREO_MVC,
            NV_ENC_CAPS_NUM_MAX_TEMPORAL_LAYERS,
            NV_ENC_CAPS_SUPPORT_HIERARCHICAL_PFRAMES,
            NV_ENC_CAPS_SUPPORT_HIERARCHICAL_BFRAMES,
            NV_ENC_CAPS_LEVEL_MAX,
            NV_ENC_CAPS_LEVEL_MIN,
            NV_ENC_CAPS_SEPARATE_COLOUR_PLANE,
            NV_ENC_CAPS_WIDTH_MAX,
            NV_ENC_CAPS_HEIGHT_MAX,
            NV_ENC_CAPS_SUPPORT_TEMPORAL_SVC,
            NV_ENC_CAPS_SUPPORT_DYN_RES_CHANGE,
            NV_ENC_CAPS_SUPPORT_DYN_BITRATE_CHANGE,
            NV_ENC_CAPS_SUPPORT_DYN_FORCE_CONSTQP,
            NV_ENC_CAPS_SUPPORT_DYN_RCMODE_CHANGE,
            NV_ENC_CAPS_SUPPORT_SUBFRAME_READBACK,
            NV_ENC_CAPS_SUPPORT_CONSTRAINED_ENCODING,
            NV_ENC_CAPS_SUPPORT_INTRA_REFRESH,
            NV_ENC_CAPS_SUPPORT_CUSTOM_VBV_BUF_SIZE,
            NV_ENC_CAPS_SUPPORT_DYNAMIC_SLICE_MODE,
            NV_ENC_CAPS_SUPPORT_REF_PIC_INVALIDATION,
            NV_ENC_CAPS_PREPROC_SUPPORT,
            NV_ENC_CAPS_ASYNC_ENCODE_SUPPORT,
            NV_ENC_CAPS_MB_NUM_MAX,
            NV_ENC_CAPS_MB_PER_SEC_MAX,
            NV_ENC_CAPS_SUPPORT_YUV444_ENCODE,
            NV_ENC_CAPS_SUPPORT_LOSSLESS_ENCODE,
            NV_ENC_CAPS_SUPPORT_SAO,
            NV_ENC_CAPS_SUPPORT_MEONLY_MODE,
            NV_ENC_CAPS_SUPPORT_LOOKAHEAD,
            NV_ENC_CAPS_SUPPORT_TEMPORAL_AQ,
            NV_ENC_CAPS_SUPPORT_10BIT_ENCODE,
            NV_ENC_CAPS_NUM_MAX_LTR_FRAMES,
            NV_ENC_CAPS_SUPPORT_WEIGHTED_PREDICTION,
            NV_ENC_CAPS_DYNAMIC_QUERY_ENCODER_CAPACITY,
            NV_ENC_CAPS_SUPPORT_BFRAME_REF_MODE,
            NV_ENC_CAPS_SUPPORT_EMPHASIS_LEVEL_MAP,
            NV_ENC_CAPS_WIDTH_MIN,
            NV_ENC_CAPS_HEIGHT_MIN,
            NV_ENC_CAPS_SUPPORT_MULTIPLE_REF_FRAMES,
            NV_ENC_CAPS_EXPOSED_COUNT
        }

        public static bool SupportedPlatdorm(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                    return true;
            }
            return false;
        }

        public static int GetCodecCapabilities(Codec codec, Caps caps)
        {
            if(codec == Codec.HEVC)
                throw new NotSupportedException("HEVC codec is currently not supported.");

                if (!NativeMethods.GetCodecCapabilities(codec, caps, out int value))
                throw new NotSupportedException("NvEnc is not supported on this device.");
            return value;
        }
    }
}
