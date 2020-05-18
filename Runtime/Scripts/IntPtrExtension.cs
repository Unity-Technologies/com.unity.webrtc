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

        public static bool[] AsBoolArray(this IntPtr ptr, int length)
        {
            byte[] array = new byte[length];
            Marshal.Copy(ptr, array, 0, length);
            Marshal.FreeCoTaskMem(ptr);
            return Array.ConvertAll(array, Convert.ToBoolean);
        }

        public static int[] AsIntArray(this IntPtr ptr, int length)
        {
            int[] array = new int[length];
            Marshal.Copy(ptr, array, 0, length);
            Marshal.FreeCoTaskMem(ptr);
            return array;
        }

        public static uint[] AsUnsignedIntArray(this IntPtr ptr, int length)
        {
            int[] array = AsIntArray(ptr, length);
            return Array.ConvertAll(array, Convert.ToUInt32);
        }

        public static long[] AsLongArray(this IntPtr ptr, int length)
        {
            long[] array = new long[length];
            Marshal.Copy(ptr, array, 0, length);
            Marshal.FreeCoTaskMem(ptr);
            return array;
        }

        public static ulong[] AsUnsignedLongArray(this IntPtr ptr, int length)
        {
            long[] array = AsLongArray(ptr, length);
            return Array.ConvertAll(array, Convert.ToUInt64);
        }

        public static double[] AsDoubleArray(this IntPtr ptr, int length)
        {
            double[] array = new double[length];
            Marshal.Copy(ptr, array, 0, length);
            Marshal.FreeCoTaskMem(ptr);
            return array;
        }

        public static IntPtr[] AsIntPtrArray(this IntPtr ptr, int length)
        {
            IntPtr[] array = new IntPtr[length];
            Marshal.Copy(ptr, array, 0, length);
            Marshal.FreeCoTaskMem(ptr);
            return array;
        }

        public static string[] AsStringArray(this IntPtr ptr, int length)
        {
            IntPtr[] array = ptr.AsIntPtrArray(length);
            return Array.ConvertAll(array, AsAnsiStringWithFreeMem);
        }
    }
}
