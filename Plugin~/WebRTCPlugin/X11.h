#pragma once

extern "C"
{
#include <X11/Xlib.h>

// note: X11 headers defines common words, so it is easy to conflicts name of variables.
// Please add names defined in X11 into the list if you need.
#undef CurrentTime    // Defined by X11/X.h
}