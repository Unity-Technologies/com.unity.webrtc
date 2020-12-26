#if UNITY_WEBRTC_ENABLE_PROFILING_CORE

using Unity.Profiling;

namespace Unity.WebRTC.Profiling
{
    public class ProfilerCounterCollection
    {
        public static readonly ProfilerCategory ProfilerCategory = ProfilerCategory.Scripts;

        public static readonly ProfilerCounter<uint> packetsReceived =
            new ProfilerCounter<uint>(ProfilerCategory,
                "packetsReceived", ProfilerMarkerDataUnit.Undefined);


        public void AddCounter()
        {

        }

        public void RemoveCounter()
        {

        }

    }
}

#endif
