// Unity Native Plugin API copyright © 2015 Unity Technologies ApS
//
// Licensed under the Unity Companion License for Unity - dependent projects--see[Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.Please review the license for details on these and other terms and conditions.

#pragma once
#include "IUnityInterface.h"

#include <stdint.h>
#include <stddef.h>

typedef struct PluginAllocator UnityAllocator;

UNITY_DECLARE_INTERFACE(IUnityMemoryManager)
{
    UnityAllocator* (UNITY_INTERFACE_API * CreateAllocator)(const char* areaName, const char* objectName);
    void(UNITY_INTERFACE_API * DestroyAllocator)(UnityAllocator * allocator);

    void* (UNITY_INTERFACE_API * Allocate)(UnityAllocator * allocator, size_t size, size_t align, const char* file, int32_t line);
    void(UNITY_INTERFACE_API * Deallocate)(UnityAllocator * allocator, void* ptr, const char* file, int32_t line);
    void* (UNITY_INTERFACE_API * Reallocate)(UnityAllocator * allocator, void* ptr, size_t size, size_t align, const char* file, int32_t line);
};
UNITY_REGISTER_INTERFACE_GUID(0xBAF9E57C61A811ECULL, 0xC5A7CC7861A811ECULL, IUnityMemoryManager)
