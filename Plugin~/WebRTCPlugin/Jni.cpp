#include "pch.h"
#include <jni.h>
#undef JNIEXPORT
#define JNIEXPORT __attribute__((visibility("default")))

//#include "examples/unityplugin/class_reference_holder.h"
#include "rtc_base/ssl_adapter.h"
#include "sdk/android/src/jni/class_reference_holder.h"
#include "sdk/android/src/jni/jni_helpers.h"
#include "sdk/android/native_api/base/init.h"

using namespace ::webrtc;


namespace unity {
namespace webrtc {

extern "C" jint JNIEXPORT JNICALL JNI_OnLoad(JavaVM* jvm, void* reserved) {
//  RTC_CHECK(rtc::InitializeSSL()) << "Failed to InitializeSSL()";
//  LoadGlobalClassReferenceHolder();
//  unity_plugin::LoadGlobalClassReferenceHolder();
  InitAndroid(jvm);
  return 1;
}

extern "C" void JNIEXPORT JNICALL JNI_OnUnLoad(JavaVM* jvm, void* reserved) {
//  FreeGlobalClassReferenceHolder();
//  unity_plugin::FreeGlobalClassReferenceHolder();
//  RTC_CHECK(rtc::CleanupSSL()) << "Failed to CleanupSSL()";
}

}
}