#include "AndroidCodecFactoryHelper.h"

#include <memory>
#include <pthread.h>

#include "rtc_base/checks.h"
#include "rtc_base/ignore_wundef.h"
#include "rtc_base/logging.h"
#include "sdk/android/native_api/base/init.h"
#include "sdk/android/native_api/codecs/wrapper.h"
#include "sdk/android/native_api/jni/class_loader.h"
#include "sdk/android/native_api/jni/jvm.h"
#include "sdk/android/native_api/jni/scoped_java_ref.h"

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

    std::unique_ptr<VideoEncoderFactory> CreateAndroidEncoderFactory()
    {
        JNIEnv* env = AttachCurrentThreadIfNeeded();
        RTC_DCHECK(env);
        ScopedJavaLocalRef<jclass> factory_class = GetClass(env, "org/webrtc/HardwareVideoEncoderFactory");
        jmethodID factory_constructor =
            env->GetMethodID(factory_class.obj(), "<init>", "(Lorg/webrtc/EglBase$Context;ZZ)V");
        ScopedJavaLocalRef<jobject> factory_object(
            env,
            env->NewObject(
                factory_class.obj(),
                factory_constructor,
                nullptr /* shared_context */,
                false /* enable_intel_vp8_encoder */,
                true /* enable_h264_high_profile */));
        return JavaToNativeVideoEncoderFactory(env, factory_object.obj());
    }

    std::unique_ptr<VideoDecoderFactory> CreateAndroidDecoderFactory()
    {
        JNIEnv* env = AttachCurrentThreadIfNeeded();
        RTC_DCHECK(env);
        ScopedJavaLocalRef<jclass> factory_class = GetClass(env, "org/webrtc/HardwareVideoDecoderFactory");
        jmethodID factory_constructor =
            env->GetMethodID(factory_class.obj(), "<init>", "(Lorg/webrtc/EglBase$Context;)V");
        ScopedJavaLocalRef<jobject> factory_object(
            env, env->NewObject(factory_class.obj(), factory_constructor, nullptr /* shared_context */));
        return JavaToNativeVideoDecoderFactory(env, factory_object.obj());
    }

} // namespace webrtc
} // namespace unity