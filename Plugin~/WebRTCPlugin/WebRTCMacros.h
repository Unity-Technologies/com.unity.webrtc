#pragma once

#define SAFE_RELEASE(p)     { if( (p) )         { (p)->Release(); (p) = 0; } }
#define SAFE_DELETE(a)      { if( (a) != NULL ) { delete     (a); (a) = nullptr; } }  
#define SAFE_DELETE_ARR(a)  { if( (a) != NULL ) { delete[]   (a); (a) = nullptr; } }
