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

        public static string[] AsStringArray(this IntPtr ptr, int length)
        {
            IntPtr[] array = ptr.AsArray<IntPtr>(length);
            return Array.ConvertAll(array, AsAnsiStringWithFreeMem);
        }

        public static T[] AsArray<T>(this IntPtr ptr, int length, bool freePtr = true)
        {
            T[] ret = null;

            if (typeof(T) == typeof(byte))
            {
                byte[] _array = new byte[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = _array as T[];
            }
            else if (typeof(T) == typeof(uint))
            {
                int[] _array = new int[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = Array.ConvertAll(_array, Convert.ToUInt32) as T[];
            }
            else if (typeof(T) == typeof(int))
            {
                int[] _array = new int[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = _array as T[];
            }
            else if (typeof(T) == typeof(long))
            {
                long[] _array = new long[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = _array as T[];
            }
            else if (typeof(T) == typeof(ulong))
            {
                long[] _array = new long[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = Array.ConvertAll(_array, Convert.ToUInt64) as T[];
            }
            else if (typeof(T) == typeof(float))
            {
                float[] _array = new float[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = _array as T[];
            }
            else if (typeof(T) == typeof(double))
            {
                double[] _array = new double[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = _array as T[];
            }
            else if (typeof(T) == typeof(bool))
            {
                byte[] _array = new byte[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = Array.ConvertAll(_array, Convert.ToBoolean) as T[];
            }
            else if (typeof(T) == typeof(IntPtr))
            {
                IntPtr[] _array = new IntPtr[length];
                Marshal.Copy(ptr, _array, 0, length);
                ret = _array as T[];
            }
            else if (typeof(T) == typeof(string))
            {
                IntPtr[] array = ptr.AsArray<IntPtr>(length);
                ret = Array.ConvertAll(array, AsAnsiStringWithFreeMem) as T[];
            }
            else
            {
                ret = new T[length];
                IntPtr iterator = ptr;
                int size = Marshal.SizeOf(typeof(T));
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = (T)Marshal.PtrToStructure(iterator, typeof(T));
                    iterator = IntPtr.Add(iterator, size);
                }
            }
            if (freePtr)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            return ret;
        }

        public static IntPtr ToPtr<T>(T[] array)
        {
            int size = Marshal.SizeOf(typeof(T));
            int length = size * array.Length;
            IntPtr ptr = Marshal.AllocCoTaskMem(length);
            IntPtr iterator = ptr;
            for (var i = 0; i < array.Length; i++)
            {
                Marshal.StructureToPtr(array[i], iterator, false);
                iterator = IntPtr.Add(iterator, size);
            }
            return ptr;
        }
    }
}
