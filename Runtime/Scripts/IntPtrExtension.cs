using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{

    public static class IntPtrExtension
    {
        public static string AsAnsiStringWithFreeMem(this IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentException("ptr is nullptr");
            }
            string str = Marshal.PtrToStringAnsi(ptr);
            Marshal.FreeCoTaskMem(ptr);
            return str;
        }
        public static string AsAnsiStringWithoutFreeMem(this IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                throw new ArgumentException("ptr is nullptr");
            }
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}
