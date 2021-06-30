using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct OptionalInt
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool hasValue;
        public int value;

        public static implicit operator int?(OptionalInt a)
        {
            return a.hasValue ? a.value : (int?)null;
        }
        public static implicit operator OptionalInt(int? a)
        {
            return new OptionalInt { hasValue = a.HasValue, value = a.GetValueOrDefault() };
        }

        public static OptionalInt FromEnum<T>(T? a) where T : struct
        {
            Type type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();

            return new OptionalInt {hasValue = a.HasValue, value = Convert.ToInt32(a.GetValueOrDefault()) };
        }

        public T? AsEnum<T>() where T : struct
        {
            if(!hasValue)
                return null;
            Type type = typeof(T);
            if (!type.IsEnum)
                return null;
            return (T)Enum.ToObject(typeof(T), value);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct OptionalUlong
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool hasValue;
        public ulong value;

        public static implicit operator ulong?(OptionalUlong a)
        {
            return a.hasValue ? a.value : (ulong?)null;
        }
        public static implicit operator OptionalUlong(ulong? a)
        {
            return new OptionalUlong { hasValue = a.HasValue, value = a.GetValueOrDefault() };
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct OptionalUshort
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool hasValue;
        public ushort value;

        public static implicit operator ushort?(OptionalUshort a)
        {
            return a.hasValue ? a.value : (ushort?)null;
        }
        public static implicit operator OptionalUshort(ushort? a)
        {
            return new OptionalUshort { hasValue = a.HasValue, value = a.GetValueOrDefault() };
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct OptionalShort
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool hasValue;
        public short value;

        public static implicit operator short?(OptionalShort a)
        {
            return a.hasValue ? a.value : (short?)null;
        }
        public static implicit operator OptionalShort(short? a)
        {
            return new OptionalShort { hasValue = a.HasValue, value = a.GetValueOrDefault() };
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct OptionalUint
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool hasValue;
        public uint value;

        public static implicit operator uint?(OptionalUint a)
        {
            return a.hasValue ? a.value : (uint?)null;
        }
        public static implicit operator OptionalUint(uint? a)
        {
            return new OptionalUint { hasValue = a.HasValue, value = a.GetValueOrDefault() };
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct OptionalDouble
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool hasValue;
        public double value;

        public static implicit operator double?(OptionalDouble a)
        {
            return a.hasValue ? a.value : (double?)null;
        }
        public static implicit operator OptionalDouble(double? a)
        {
            return new OptionalDouble { hasValue = a.HasValue, value = a.GetValueOrDefault() };
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct OptionalBool
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool hasValue;
        [MarshalAs(UnmanagedType.U1)]
        public bool value;

        public static implicit operator bool?(OptionalBool a)
        {
            return a.hasValue ? a.value : (bool?)null;
        }
        public static implicit operator OptionalBool(bool? a)
        {
            return new OptionalBool { hasValue = a.HasValue, value = a.GetValueOrDefault() };
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct MarshallingArray<T> where T : struct
    {
        public int length;
        public IntPtr ptr;

        public T[] ToArray()
        {
            var array = ptr.AsArray<T>(length);
            ptr = IntPtr.Zero;
            return array;
        }

        public static implicit operator MarshallingArray<T>(T[] src)
        {
            MarshallingArray<T> dst = default;
            dst.length = src.Length;
            dst.ptr = src.ToPtr();
            return dst;
        }

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(ptr);
            ptr = IntPtr.Zero;
            length = 0;
        }
    }
}
