// Unity Native Plugin API copyright © 2015 Unity Technologies ApS
//
// Licensed under the Unity Companion License for Unity - dependent projects--see[Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.Please review the license for details on these and other terms and conditions.

#pragma once
#include "IUnityInterface.h"

/// The type of the log message
enum UnityLogType
{
    /// UnityLogType used for Errors.
    kUnityLogTypeError = 0,
    /// UnityLogType used for Warnings.
    kUnityLogTypeWarning = 2,
    /// UnityLogType used for regular log messages.
    kUnityLogTypeLog = 3,
    /// UnityLogType used for Exceptions.
    kUnityLogTypeException = 4,
};

#define UNITY_WRAP_CODE(CODE_) do { CODE_; } while (0)
#define UNITY_LOG(PTR_, MSG_) UNITY_WRAP_CODE((PTR_)->Log(kUnityLogTypeLog, MSG_, __FILE__, __LINE__))
#define UNITY_LOG_WARNING(PTR_, MSG_) UNITY_WRAP_CODE((PTR_)->Log(kUnityLogTypeWarning, MSG_, __FILE__, __LINE__))
#define UNITY_LOG_ERROR(PTR_, MSG_) UNITY_WRAP_CODE((PTR_)->Log(kUnityLogTypeError, MSG_, __FILE__, __LINE__))

UNITY_DECLARE_INTERFACE(IUnityLog)
{
    // Writes information message to Unity log.
    // \param type type log channel type which defines importance of the message.
    // \param message UTF-8 null terminated string.
    // \param fileName UTF-8 null terminated string with file name of the point where message is generated.
    // \param fileLine integer file line number of the point where message is generated.
    void(UNITY_INTERFACE_API * Log)(UnityLogType type, const char* message, const char *fileName, const int fileLine);
};
UNITY_REGISTER_INTERFACE_GUID(0x9E7507fA5B444D5DULL, 0x92FB979515EA83FCULL, IUnityLog)
