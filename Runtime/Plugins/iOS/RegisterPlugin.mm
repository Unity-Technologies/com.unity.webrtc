#import "UnityAppController.h"
#include "Unity/IUnityGraphics.h"

bool g_registeredRenderingPlugin = false;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload();
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RegisterRenderingPlugin()
{
    if(g_registeredRenderingPlugin)
        return;
    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
    g_registeredRenderingPlugin = true;
}
