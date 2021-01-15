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
