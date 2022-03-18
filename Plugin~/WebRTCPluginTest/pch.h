//
// pch.h
// Header for standard system include files.
//

#pragma once

#include "gmock/gmock.h"
#include "gtest/gtest.h"
#include "../WebRTCPlugin/pch.h"

#if defined(LEAK_SANITIZER)
#include <sanitizer/lsan_interface.h>
#endif

// workaround:
// Visual Studio Test Adapter treats Skip type as a error.
#define GTEST_SKIP_SUCCESS() \
    return GTEST_MESSAGE_("Skipped", ::testing::TestPartResult::kSuccess)
