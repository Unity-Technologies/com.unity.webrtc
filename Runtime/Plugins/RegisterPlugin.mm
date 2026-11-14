#import "UnityAppController.h"
#include "Unity/IUnityGraphics.h"

bool g_registeredRenderingPlugin = false;

extern "C"
{
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityWebRTCPluginLoad(
    IUnityInterfaces* unityInterfaces);
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityWebRTCPluginUnload();
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RegisterRenderingWebRTCPlugin()
{
    if(g_registeredRenderingPlugin)
        return;
    UnityRegisterRenderingPluginV5(&UnityWebRTCPluginLoad, &UnityWebRTCPluginUnload);
    g_registeredRenderingPlugin = true;
}
}