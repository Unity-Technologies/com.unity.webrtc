//
// Created by ryan on 2022/12/30.
//
#include "Debug.h"

#include<stdio.h>
#include <string.h>
#include <sstream>
#include "IUnityInterface.h"
//-------------------------------------------------------------------
void Debug::Log(const char *message) {
    if (callbackInstance != nullptr)
        callbackInstance(message, (int) strlen(message));
}

void Debug::Log(const std::string message) {
    const char *tmsg = message.c_str();
    if (callbackInstance != nullptr)
        callbackInstance(tmsg, (int) strlen(tmsg));
}

void Debug::Log(const int message) {
    std::stringstream ss;
    ss << message;
    send_log(ss);
}

void Debug::Log(const char message) {
    std::stringstream ss;
    ss << message;
    send_log(ss);
}

void Debug::Log(const float message) {
    std::stringstream ss;
    ss << message;
    send_log(ss);
}

void Debug::Log(const double message) {
    std::stringstream ss;
    ss << message;
    send_log(ss);
}

void Debug::Log(const bool message) {
    std::stringstream ss;
    if (message)
        ss << "true";
    else
        ss << "false";

    send_log(ss);
}

void Debug::send_log(const std::stringstream &ss) {
    const std::string tmp = ss.str();
    const char *tmsg = tmp.c_str();
    if (callbackInstance != nullptr)
        callbackInstance(tmsg,(int) strlen(tmsg));
}
//-------------------------------------------------------------------

//Create a callback delegate
extern "C" void WebRtc_RegisterDebugCallback(FuncCallBack cb) {
    callbackInstance = cb;
}