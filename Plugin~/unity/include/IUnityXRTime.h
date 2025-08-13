// Unity Native Plugin API copyright © 2022 Unity Technologies ApS
//
// Licensed under the Unity Companion License for Unity - dependent projects--see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.Please review the license for details on these and other terms and conditions.

#pragma once
#include "IUnityInterface.h"

/// Interface to allow providers to set predicted time.
UNITY_DECLARE_INTERFACE(IUnityXRTime)
{
    /// Time in seconds for when pixels end up on screen.  Epoch must be consistent frame-to-frame.
    /// Should be called in UnityXRDisplayGraphicsThreadProvider::PopulateNextFrameDesc or UnityXRDisplayGraphicsThreadProvider::SubmitCurrentFrame.
    /// If provided, will be used to calculate time derivatives for simulation + rendering.
    /// If set to zero, unity will calculate time derivatives from system time.
    void(UNITY_INTERFACE_API * SetPredictedRenderTime)(double time);
};

UNITY_REGISTER_INTERFACE_GUID(0x650C79F9649240B7ULL, 0x9F89DF9F47DDD42FULL, IUnityXRTime);
