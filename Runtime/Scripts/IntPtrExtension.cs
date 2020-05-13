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

        public static void ToArray<T>(this IntPtr ptr, T[] array)
        {
            IntPtr iter = ptr;
            int size = Marshal.SizeOf(typeof(T));
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (T)Marshal.PtrToStructure(iter, typeof(T));
                iter = IntPtr.Add(iter, size);
            }
        }

        public static IntPtr ToPtr<T>(T[] array)
        {
            int size = Marshal.SizeOf(typeof(T));
            int length = size * array.Length;
            IntPtr ptr = Marshal.AllocCoTaskMem(length);
            IntPtr iter = ptr;
            for (var i = 0; i < array.Length; i++)
            {
                Marshal.StructureToPtr(array[i], iter, false);
                iter = IntPtr.Add(iter, size);
            }
            return ptr;

        }
    }
}
