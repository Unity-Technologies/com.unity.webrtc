// Unity Native Plugin API copyright © 2015 Unity Technologies ApS
//
// Licensed under the Unity Companion License for Unity - dependent projects--see[Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.Please review the license for details on these and other terms and conditions.

#pragma once
#include "IUnityInterface.h"

#include <stdint.h>

// Unity Profiler Native plugin API provides an ability to register callbacks for Unity Profiler events.
// The basic functionality includes:
// 1. Ability to create a Unity Profiler Marker.
// 2. Begin and end sample.
// 3. Register a thread for profiling.
// 4. Obtain an information if profiler is available and enabled.
//
//  Usage example:
//
//  #include <IUnityInterface.h>
//  #include <IUnityProfiler.h>
//
//  static IUnityProfiler* s_UnityProfiler = NULL;
//  static const UnityProfilerMarkerDesc* s_MyPluginMarker = NULL;
//  static bool s_IsDevelopmentBuild = false;
//
//  static void MyPluginWorkMethod()
//  {
//      if (s_IsDevelopmentBuild)
//          s_UnityProfiler->BeginSample(s_MyPluginMarker);
//
//      // Code I want to see in Unity Profiler as "MyPluginMethod".
//      // ...
//
//      if (s_IsDevelopmentBuild)
//          s_UnityProfiler->EndSample(s_MyPluginMarker);
//  }
//
//  extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
//  {
//      s_UnityProfiler = unityInterfaces->Get<IUnityProfiler>();
//      if (s_UnityProfiler == NULL)
//          return;
//      s_IsDevelopmentBuild = s_UnityProfiler->IsAvailable() != 0;
//      s_UnityProfiler->CreateMarker(&s_MyPluginMarker, "MyPluginMethod", kUnityProfilerCategoryOther, kUnityProfilerMarkerFlagDefault, 0);
//  }
//
//  extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
//  {
//  }


typedef uint32_t UnityProfilerMarkerId;

enum UnityBuiltinProfilerCategory_
{
    kUnityProfilerCategoryRender = 0,
    kUnityProfilerCategoryScripts = 1,
    kUnityProfilerCategoryManagedJobs = 2,
    kUnityProfilerCategoryBurstJobs = 3,
    kUnityProfilerCategoryGUI = 4,
    kUnityProfilerCategoryPhysics = 5,
    kUnityProfilerCategoryAnimation = 6,
    kUnityProfilerCategoryAI = 7,
    kUnityProfilerCategoryAudio = 8,
    kUnityProfilerCategoryAudioJob = 9,
    kUnityProfilerCategoryAudioUpdateJob = 10,
    kUnityProfilerCategoryVideo = 11,
    kUnityProfilerCategoryParticles = 12,
    kUnityProfilerCategoryGi = 13,
    kUnityProfilerCategoryNetwork = 14,
    kUnityProfilerCategoryLoading = 15,
    kUnityProfilerCategoryOther = 16,
    kUnityProfilerCategoryGC = 17,
    kUnityProfilerCategoryVSync = 18,
    kUnityProfilerCategoryOverhead = 19,
    kUnityProfilerCategoryPlayerLoop = 20,
    kUnityProfilerCategoryDirector = 21,
    kUnityProfilerCategoryVR = 22,
    kUnityProfilerCategoryAllocation = 23, kUnityProfilerCategoryMemory = 23,
    kUnityProfilerCategoryInternal = 24,
    kUnityProfilerCategoryFileIO = 25,
    kUnityProfilerCategoryUISystemLayout = 26,
    kUnityProfilerCategoryUISystemRender = 27,
    kUnityProfilerCategoryVFX = 28,
    kUnityProfilerCategoryBuildInterface = 29,
    kUnityProfilerCategoryInput = 30,
    kUnityProfilerCategoryVirtualTexturing = 31,
    kUnityProfilerCategoryGPU = 32,
    kUnityProfilerCategoryPhysics2D = 33,
    kUnityProfilerCategoryNetworkOperations = 34,
    kUnityProfilerCategoryUIDetails = 35,
    kUnityProfilerCategoryDebug = 36,
};
typedef uint16_t UnityProfilerCategoryId;

typedef struct UnityProfilerCategoryDesc
{
    // Incremental category index.
    UnityProfilerCategoryId id;
    // Reserved.
    uint16_t reserved0;
    // Internally associated category color which is in 0xRRGGBBAA format.
    uint32_t rgbaColor;
    // NULL-terminated string which is associated with the category.
    const char* name;
} UnityProfilerCategoryDesc;

enum UnityProfilerMarkerFlag_
{
    kUnityProfilerMarkerFlagDefault = 0,

    kUnityProfilerMarkerFlagScriptUser = 1 << 1,         // Markers created with C# API.
    kUnityProfilerMarkerFlagScriptInvoke = 1 << 5,       // Runtime invocations with ScriptingInvocation::Invoke.
    kUnityProfilerMarkerFlagScriptEnterLeave = 1 << 6,   // Deep profiler.

    kUnityProfilerMarkerFlagAvailabilityEditor = 1 << 2, // Editor-only marker, doesn't present in dev and non-dev players.
    kUnityProfilerMarkerFlagAvailabilityNonDev = 1 << 3, // Non-development marker, is present everywhere including release builds.

    kUnityProfilerMarkerFlagWarning = 1 << 4,            // Indicates undesirable, performance-wise suboptimal code path.

    kUnityProfilerMarkerFlagCounter = 1 << 7,            // Marker is also used as a counter.

    kUnityProfilerMarkerFlagVerbosityDebug = 1 << 10,    // Internal debug markers - e.g. JobSystem Idle.
    kUnityProfilerMarkerFlagVerbosityInternal = 1 << 11, // Internal markers - e.g. Mutex/semaphore waits.
    kUnityProfilerMarkerFlagVerbosityAdvanced = 1 << 12  // Markers which are useful for advanced users - e.g. Loading.
};
typedef uint16_t UnityProfilerMarkerFlags;

enum UnityProfilerMarkerEventType_
{
    kUnityProfilerMarkerEventTypeBegin = 0,
    kUnityProfilerMarkerEventTypeEnd = 1,
    kUnityProfilerMarkerEventTypeSingle = 2
};
typedef uint16_t UnityProfilerMarkerEventType;

typedef struct UnityProfilerMarkerDesc
{
    // Per-marker callback chain pointer. Don't use.
    const void* callback;
    // Event id.
    UnityProfilerMarkerId id;
    // UnityProfilerMarkerFlag_ value.
    UnityProfilerMarkerFlags flags;
    // Category index the marker belongs to.
    UnityProfilerCategoryId categoryId;
    // NULL-terminated string which is associated with the marker.
    const char* name;
    // Metadata descriptions chain. Don't use.
    const void* metaDataDesc;
} UnityProfilerMarkerDesc;

enum UnityProfilerMarkerDataType_
{
    kUnityProfilerMarkerDataTypeNone = 0,
    kUnityProfilerMarkerDataTypeInstanceId = 1,
    kUnityProfilerMarkerDataTypeInt32 = 2,
    kUnityProfilerMarkerDataTypeUInt32 = 3,
    kUnityProfilerMarkerDataTypeInt64 = 4,
    kUnityProfilerMarkerDataTypeUInt64 = 5,
    kUnityProfilerMarkerDataTypeFloat = 6,
    kUnityProfilerMarkerDataTypeDouble = 7,
    kUnityProfilerMarkerDataTypeString = 8,
    kUnityProfilerMarkerDataTypeString16 = 9,
    kUnityProfilerMarkerDataTypeBlob8 = 11,
    kUnityProfilerMarkerDataTypeCount // Total count of data types
};
typedef uint8_t UnityProfilerMarkerDataType;

enum UnityProfilerMarkerDataUnit_
{
    kUnityProfilerMarkerDataUnitUndefined = 0,
    kUnityProfilerMarkerDataUnitTimeNanoseconds = 1,
    kUnityProfilerMarkerDataUnitBytes = 2,
    kUnityProfilerMarkerDataUnitCount = 3,
    kUnityProfilerMarkerDataUnitPercent = 4,
    kUnityProfilerMarkerDataUnitFrequencyHz = 5,
};
typedef uint8_t UnityProfilerMarkerDataUnit;

typedef struct UnityProfilerMarkerData
{
    UnityProfilerMarkerDataType type;
    uint8_t reserved0;
    uint16_t reserved1;
    uint32_t size;
    const void* ptr;
} UnityProfilerMarkerData;

enum UnityProfilerFlowEventType_
{
    // Starts flow chain for a current profiler marker scope (__enclosing__ scope).
    // Mark the scheduler function with "begin" flow to connect it later to execution function on another thread.
    kUnityProfilerFlowEventTypeBegin = 0,
    // The flow continues with the next sample.
    // Marks the __next__ profiler sample connected to the sample which started the flow (kUnityProfilerFlowEventTypeBegin) or previous (in time) kUnityProfilerFlowEventTypeNext flow event.
    // All parallel flow instances are equivalent and connected to the same previous in time kUnityProfilerFlowEventTypeBegin or kUnityProfilerFlowEventTypeNext events and next (also in time) kUnityProfilerFlowEventTypeNext event.
    kUnityProfilerFlowEventTypeParallelNext = 1,
    // Ends flow started by kUnityProfilerFlowEventTypeBegin. Usually represents a sync point (SyncFence)
    // Marks the __enclosing__ sample as endpoint.
    kUnityProfilerFlowEventTypeEnd = 2,
    // The flow continues with the next sample.
    // Marks the __next__ profiler sample connected to the sample which started the flow (kUnityProfilerFlowEventTypeBegin).
    kUnityProfilerFlowEventTypeNext = 3,
};
typedef uint8_t UnityProfilerFlowEventType;

enum UnityProfilerCounterFlags_
{
    kUnityProfilerCounterFlagNone = 0,
    // Automatic flush of counter value to recorder or profiler at the end of a frame
    kUnityProfilerCounterFlushOnEndOfFrame = 1 << 1,
    // Automatic reset counter value to zero on flush
    kUnityProfilerCounterFlagResetToZeroOnFlush = 1 << 2,
    // Use atomic to access counters value. Don't use
    kUnityProfilerCounterFlagAtomic = 1 << 3,
    // Pull-style counter. Don't use
    kUnityProfilerCounterFlagGetter = 1 << 4
};
typedef uint16_t UnityProfilerCounterFlags;


typedef uint64_t UnityProfilerThreadId;

typedef void (*UnityProfilerCounterStatePtrCallback)(void* userData);

#ifdef __cplusplus
template<typename T> struct UnityProfilerDataUnitHelper;
template<> struct UnityProfilerDataUnitHelper<int32_t> { static const UnityProfilerMarkerDataType GetProfilerType() { return kUnityProfilerMarkerDataTypeInt32; } };
template<> struct UnityProfilerDataUnitHelper<uint32_t> { static const UnityProfilerMarkerDataType GetProfilerType() { return kUnityProfilerMarkerDataTypeUInt32; } };
template<> struct UnityProfilerDataUnitHelper<int64_t> { static const UnityProfilerMarkerDataType GetProfilerType() { return kUnityProfilerMarkerDataTypeInt64; } };
template<> struct UnityProfilerDataUnitHelper<uint64_t> { static const UnityProfilerMarkerDataType GetProfilerType() { return kUnityProfilerMarkerDataTypeUInt64; } };
template<> struct UnityProfilerDataUnitHelper<float> { static const UnityProfilerMarkerDataType GetProfilerType() { return kUnityProfilerMarkerDataTypeFloat; } };
template<> struct UnityProfilerDataUnitHelper<double> { static const UnityProfilerMarkerDataType GetProfilerType() { return kUnityProfilerMarkerDataTypeDouble; } };

template<typename T> struct UnityProfilerCounter;
#endif

// Available since 2021.2
UNITY_DECLARE_INTERFACE(IUnityProfilerV2)
{
    void BeginSample(const UnityProfilerMarkerDesc * markerDesc)
    {
        (this->EmitEvent)(markerDesc, kUnityProfilerMarkerEventTypeBegin, 0, NULL);
    }

    void BeginSample(const UnityProfilerMarkerDesc * markerDesc, uint16_t eventDataCount, const UnityProfilerMarkerData * eventData)
    {
        (this->EmitEvent)(markerDesc, kUnityProfilerMarkerEventTypeBegin, eventDataCount, eventData);
    }

    void EndSample(const UnityProfilerMarkerDesc * markerDesc)
    {
        (this->EmitEvent)(markerDesc, kUnityProfilerMarkerEventTypeEnd, 0, NULL);
    }

    // Create instrumentation event.
    // \param markerDesc is a pointer to marker description struct.
    // \param eventType is an event type - UnityProfilerMarkerEventType_.
    // \param eventDataCount is an event metadata count passed in eventData. Must be less than eventDataCount specified in CreateMarker.
    // \param eventData is a metadata array of eventDataCount elements.
    void(UNITY_INTERFACE_API * EmitEvent)(const UnityProfilerMarkerDesc * markerDesc, UnityProfilerMarkerEventType eventType, uint16_t eventDataCount, const UnityProfilerMarkerData * eventData);

    // Returns 1 if Unity Profiler is enabled, 0 overwise.
    int(UNITY_INTERFACE_API * IsEnabled)();

    // Returns 1 if Unity Profiler is available, 0 overwise.
    // Profiler is available only in Development builds. Release builds have Unity Profiler compiled out.
    // However individual markers can be available even in Release builds (e.g. GC.Collect) and be collected with
    // Recorder API or forwarded to platform and other profilers (via IUnityProfilerCallbacks::RegisterMarkerEventCallback API).
    // You can choose whenever you want or not emit profiler event in Development and Release builds using the cached result of this method.
    int(UNITY_INTERFACE_API * IsAvailable)();

    // Creates a new Unity Profiler marker.
    // \param desc Pointer to the const UnityProfilerMarkerDesc* which is set to the created marker in a case of a succesful execution.
    // \param name Marker name to be displayed in Unity Profiler.
    // \param flags Marker flags. One of UnityProfilerMarkerFlag_ enum. Use kUnityProfilerMarkerFlagDefault if not sure.
    // \param eventDataCount Maximum count of potential metadata parameters count.
    // \return 0 on success and non-zero in case of error.
    int(UNITY_INTERFACE_API * CreateMarker)(const UnityProfilerMarkerDesc * *desc, const char* name, UnityProfilerCategoryId category, UnityProfilerMarkerFlags flags, int eventDataCount);

    // Set a metadata description for the Unity Profiler marker.
    // \param markerDesc is a pointer to marker description struct.
    // \param index metadata index to set name for.
    // \param metadataType Data type. Must be of UnityProfilerMarkerDataType_ enum.
    // \param name Metadata name.
    // \return 0 on success and non-zero in case of error.
    int(UNITY_INTERFACE_API * SetMarkerMetadataName)(const UnityProfilerMarkerDesc * desc, int index, const char* metadataName, UnityProfilerMarkerDataType metadataType, UnityProfilerMarkerDataUnit metadataUnit);

    // Creates a new Unity Profiler category.
    // \param category is a pointer to UnityProfilerCategoryId variable which is set to the created category id in case of a succesful execution.
    // \param name Category name to be displayed in Unity Profiler.
    // \return 0 on success and non-zero in case of error.
    int(UNITY_INTERFACE_API * CreateCategory)(UnityProfilerCategoryId *category, const char* name, uint32_t unused);

    // Registers current thread with Unity Profiler.
    // Has no effect in Release Players.
    // \param threadId Optional Unity Profiler thread identifier which it written on successful method call. Can be used with UnregisterThread.
    // \param groupName Thread group name. Unity Profiler aggregates threads with the same group.
    // \param name Thread name.
    // \return 0 on success and non-zero in case of error.
    int(UNITY_INTERFACE_API * RegisterThread)(UnityProfilerThreadId * threadId, const char* groupName, const char* name);

    // Unregisters current thread from Unity Profiler and cleans up all associated memory.
    // Has no effect in Release Players.
    // \param threadId Unity Profiler thread identifier obtained with RegisterThread call. Use 0 to cleanup the current thread.
    // \return 0 on success and non-zero in case of error.
    int(UNITY_INTERFACE_API * UnregisterThread)(UnityProfilerThreadId threadId);

#ifdef __cplusplus
    // Creates a new Unity Profiler counter wrapper object.
    // \param category Counter marker category which assignes marker to one of visualized groups on the profiler chart.
    // \param name Counter marker name to be displayed in Unity Profiler.
    // \param valueUnit Data unit, defines how data will be visualized. Must be of UnityProfilerMarkerDataUnit_ enum.
    // \param activateFunc Callback called when new coutner data receiver is connected.
    // \param deactivateFunc Callback called when coutner data receiver is disconnected.
    // \param userData User data pointer passed to activate and deactivate callbacks.
    // \return UnityProfilerCounter counter struct.
    template<typename T>
    UnityProfilerCounter<T> CreateCounter(UnityProfilerCategoryId category, const char* name, UnityProfilerMarkerDataUnit dataUnit,
        UnityProfilerCounterStatePtrCallback activateFunc = nullptr, UnityProfilerCounterStatePtrCallback deactivateFunc = nullptr, void* userData = nullptr)
    {
        return UnityProfilerCounter<T>(
            this,
            this->CreateCounterValue(
                category,
                name,
                kUnityProfilerMarkerFlagCounter,
                UnityProfilerDataUnitHelper<T>::GetProfilerType(),
                dataUnit,
                sizeof(T),
                kUnityProfilerCounterFlagNone,
                activateFunc,
                deactivateFunc,
                userData
            )
        );
    }

#endif

    // Creates a new Unity Profiler counter.
    // \param category Counter marker category which assignes marker to one of visualized groups on the profiler chart.
    // \param name Counter marker name to be displayed in Unity Profiler.
    // \param flags Counter marker flags. One of UnityProfilerMarkerFlag_ enum. Use kUnityProfilerMarkerFlagDefault if not sure.
    // \param valueType Data type. Must be of UnityProfilerMarkerDataType_ enum.
    // \param valueUnit Data unit, defines how data will be visualized. Must be of UnityProfilerMarkerDataUnit_ enum.
    // \param valueSize Data size, must be less or equal to 8.
    // \param counterFlags Counter flags. One of UnityProfilerCounterFlags_ enum. Use kUnityProfilerCounterFlagNone if not sure.
    // \param activateFunc Callback called when new coutner data receiver is connected.
    // \param deactivateFunc Callback called when coutner data receiver is disconnected.
    // \param userData User data pointer passed to activate and deactivate callbacks.
    // \return CounterValue object address on success and null in case of error.
    void* (UNITY_INTERFACE_API* CreateCounterValue)(
        UnityProfilerCategoryId category,
        const char* name,
        UnityProfilerMarkerFlags flags,
        UnityProfilerMarkerDataType valueType,
        UnityProfilerMarkerDataUnit valueUnit,
        size_t valueSize,
        UnityProfilerCounterFlags counterFlags,
        UnityProfilerCounterStatePtrCallback activateFunc,
        UnityProfilerCounterStatePtrCallback deactivateFunc,
        void* userData
    );

    void(UNITY_INTERFACE_API * FlushCounterValue)(void* counter);
};
UNITY_REGISTER_INTERFACE_GUID(0xB957E0189CB6A30BULL, 0x83CE589AE85B9068ULL, IUnityProfilerV2)

#ifdef __cplusplus
struct UnityProfilerCounterValue
{
    UnityProfilerCounterValue()
        : m_Profiler(nullptr)
        , m_Value(nullptr)
    {
    }

    UnityProfilerCounterValue(IUnityProfilerV2* profiler, void* value)
        : m_Profiler(profiler)
        , m_Value(value)
    {
    }

    template<typename T>
    inline T& Value() { return *static_cast<T*>(m_Value); }

    template<typename T>
    inline const T& Value() const { return *static_cast<T*>(m_Value); }

    inline void Flush()
    {
        m_Profiler->FlushCounterValue(m_Value);
    }

private:
    void* m_Value;
    IUnityProfilerV2* m_Profiler;
};

template<typename T>
struct UnityProfilerCounter : public UnityProfilerCounterValue
{
    UnityProfilerCounter() {}
    UnityProfilerCounter(IUnityProfilerV2* profiler, void* value) : UnityProfilerCounterValue(profiler, value) {}

    T& operator*() { return Value<T>(); }
    const T& operator*() const { return Value<T>(); }

    operator T&() { return Value<T>(); }
    operator T() const { return Value<T>(); }

    T& operator=(const T& s)
    {
        Value<T>() = s;
        return Value<T>();
    }

    T operator++(int)
    {
        T copy(Value<T>());
        ++(Value<T>());
        return copy;
    }

    UnityProfilerCounter<T>& operator++()
    {
        ++(Value<T>());
        return *this;
    }

    T operator--(int)
    {
        T copy(Value<T>());
        --(Value<T>());
        return copy;
    }

    UnityProfilerCounter& operator--()
    {
        --(Value<T>());
        return *this;
    }

    UnityProfilerCounter& operator-=(T& s)
    {
        (Value<T>()) -= s;
        return *this;
    }

    UnityProfilerCounter& operator+=(T& s)
    {
        (Value<T>()) += s;
        return *this;
    }
};
#endif

// Available since 2020.1
UNITY_DECLARE_INTERFACE(IUnityProfiler)
{
    void BeginSample(const UnityProfilerMarkerDesc* markerDesc)
    {
        (this->EmitEvent)(markerDesc, kUnityProfilerMarkerEventTypeBegin, 0, NULL);
    }

    void BeginSample(const UnityProfilerMarkerDesc* markerDesc, uint16_t eventDataCount, const UnityProfilerMarkerData* eventData)
    {
        (this->EmitEvent)(markerDesc, kUnityProfilerMarkerEventTypeBegin, eventDataCount, eventData);
    }

    void EndSample(const UnityProfilerMarkerDesc* markerDesc)
    {
        (this->EmitEvent)(markerDesc, kUnityProfilerMarkerEventTypeEnd, 0, NULL);
    }

    void(UNITY_INTERFACE_API * EmitEvent)(const UnityProfilerMarkerDesc* markerDesc, UnityProfilerMarkerEventType eventType, uint16_t eventDataCount, const UnityProfilerMarkerData* eventData);

    int(UNITY_INTERFACE_API * IsEnabled)();
    int(UNITY_INTERFACE_API * IsAvailable)();

    int(UNITY_INTERFACE_API * CreateMarker)(const UnityProfilerMarkerDesc** desc, const char* name, UnityProfilerCategoryId category, UnityProfilerMarkerFlags flags, int eventDataCount);
    int(UNITY_INTERFACE_API * SetMarkerMetadataName)(const UnityProfilerMarkerDesc* desc, int index, const char* metadataName, UnityProfilerMarkerDataType metadataType, UnityProfilerMarkerDataUnit metadataUnit);

    int(UNITY_INTERFACE_API * RegisterThread)(UnityProfilerThreadId* threadId, const char* groupName, const char* name);
    int(UNITY_INTERFACE_API * UnregisterThread)(UnityProfilerThreadId threadId);
};
UNITY_REGISTER_INTERFACE_GUID(0x2CE79ED8316A4833ULL, 0x87076B2013E1571FULL, IUnityProfiler)
