//
// Created by ryan on 2022/12/30.
//

#ifndef AEC_DEBUG_H
#define AEC_DEBUG_H

#include <string>
#include <stdio.h>
#include <sstream>
#include "IUnityInterface.h"
extern "C"
{
//Create a callback delegate
typedef void(*FuncCallBack)(const char *message, int size);
static FuncCallBack callbackInstance = nullptr;
UNITY_INTERFACE_EXPORT void WebRtc_RegisterDebugCallback(FuncCallBack cb);
}

class Debug {
public:
    static void Log(const char *message);

    static void Log(const std::string message);

    static void Log(const int message);

    static void Log(const char message);

    static void Log(const float message);

    static void Log(const double message);

    static void Log(const bool message);

private:
    static void send_log(const std::stringstream &ss);
};

#endif //AEC_DEBUG_H
