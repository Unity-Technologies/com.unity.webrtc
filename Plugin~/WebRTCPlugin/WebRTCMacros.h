#pragma once

#define SAFE_DELETE(a)      { if( (a) != NULL ) { delete     (a); (a) = nullptr; } }  
#define SAFE_DELETE_ARR(a)  { if( (a) != NULL ) { delete[]   (a); (a) = nullptr; } }
