using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    internal static class IntPtrExtension
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
                int[] _array = new int[length];
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
                IntPtr[] _array = ptr.AsArray<IntPtr>(length, false);
                Converter<IntPtr, string> converter =
                    freePtr ? new Converter<IntPtr, string>(AsAnsiStringWithFreeMem)
                        : new Converter<IntPtr, string>(AsAnsiStringWithoutFreeMem);
                ret = Array.ConvertAll(_array, converter) as T[];
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

        public static Dictionary<string, T> AsMap<T>(this IntPtr ptr, IntPtr valuesPtr, int length, bool freePtr = true)
        {
            Dictionary<string, T> ret = new Dictionary<string, T>();

            string[] keys = ptr.AsArray<string>(length, freePtr);
            T[] values = valuesPtr.AsArray<T>(length, freePtr);

            for (int i = 0; i < length; i++)
            {
                ret[keys[i]] = values[i];
            }
            return ret;
        }

        public static IntPtr ToPtrAnsi(this string str)
        {
            return Marshal.StringToCoTaskMemAnsi(str);
        }

        public static IntPtr ToPtr(this string[] array)
        {
            int size = Marshal.SizeOf(typeof(IntPtr));
            int length = size * array.Length;

            IntPtr[] ptrArray = new IntPtr[array.Length];
            for (var i = 0; i < array.Length; i++)
            {
                ptrArray[i] = Marshal.StringToCoTaskMemAnsi(array[i]);
            }
            IntPtr dst = Marshal.AllocCoTaskMem(length);
            Marshal.Copy(ptrArray, 0, dst, array.Length);
            return dst;
        }

        public static IntPtr ToPtr<T>(this T[] array)
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
