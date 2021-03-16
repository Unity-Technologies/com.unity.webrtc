#include "IUnityInterface.h"

bool g_registeredRenderingPlugin = false;

typedef void (UNITY_INTERFACE_API* PluginLoadFunc)(IUnityInterfaces* unityInterfaces);
typedef void (UNITY_INTERFACE_API* PluginUnloadFunc)();

extern "C"
{
void UnityRegisterRenderingPlugin(PluginLoadFunc loadPlugin, PluginUnloadFunc unloadPlugin);

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityWebRTCPluginLoad(
    IUnityInterfaces* unityInterfaces);
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityWebRTCPluginUnload();
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RegisterRenderingWebRTCPlugin()
{
    if(g_registeredRenderingPlugin)
        return;
    UnityRegisterRenderingPlugin(&UnityWebRTCPluginLoad, &UnityWebRTCPluginUnload);
    g_registeredRenderingPlugin = true;
}
}