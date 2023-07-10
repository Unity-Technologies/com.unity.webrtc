using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;


namespace Unity.WebRTC.RuntimeTest
{

    class IntPtrTest
    {
        [Test]
        public void AsAnsiStringWithFreeMemThrowException()
        {
            IntPtr ptr = IntPtr.Zero;
            Assert.That(() => { ptr.AsAnsiStringWithFreeMem(); }, Throws.TypeOf<ArgumentException>());

        }

        [Test]
        public void AsAnsiStringWithoutFreeMemThrowException()
        {
            IntPtr ptr = IntPtr.Zero;
            Assert.That(() => { ptr.AsAnsiStringWithoutFreeMem(); }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void AsAnsiStringWithoutFreeMemReturnsString()
        {
            string source = "test";
            IntPtr ptr = Marshal.StringToCoTaskMemAnsi(source);
            string dest = ptr.AsAnsiStringWithoutFreeMem();
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
            Marshal.FreeCoTaskMem(ptr);
        }

        [Test]
        public void AsAnsiStringWithFreeMemReturnsString()
        {
            string source = "test";
            IntPtr ptr = Marshal.StringToCoTaskMemAnsi(source);
            string dest = ptr.AsAnsiStringWithFreeMem();
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsByteArray()
        {
            byte[] source = { 1, 2, 3 };
            IntPtr ptr = source.ToPtr();
            byte[] dest = ptr.AsArray<byte>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsIntArray()
        {
            int[] source = { 1, 2, 3 };
            IntPtr ptr = source.ToPtr();
            int[] dest = ptr.AsArray<int>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsUintArray()
        {
            uint[] source = { 1, 2, 3 };
            IntPtr ptr = source.ToPtr();
            uint[] dest = ptr.AsArray<uint>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsLongArray()
        {
            long[] source = { 1, 2, 3 };
            IntPtr ptr = source.ToPtr();
            long[] dest = ptr.AsArray<long>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsUlongArray()
        {
            ulong[] source = { 1, 2, 3 };
            IntPtr ptr = source.ToPtr();
            ulong[] dest = ptr.AsArray<ulong>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsFloatArray()
        {
            double[] source = { 0.1, 0.2, 0.3 };
            IntPtr ptr = source.ToPtr();
            double[] dest = ptr.AsArray<double>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsDoubleArray()
        {
            float[] source = { 0.1f, 0.2f, 0.3f };
            IntPtr ptr = source.ToPtr();
            float[] dest = ptr.AsArray<float>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsBoolArray()
        {
            bool[] source = { true, false, true };
            IntPtr ptr = source.ToPtr();
            bool[] dest = ptr.AsArray<bool>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsArrayReturnsStringArray()
        {
            string[] source = { "red", "blue", "yellow" };
            IntPtr ptr = source.ToPtr();
            string[] dest = ptr.AsArray<string>(source.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(source, Is.EqualTo(dest));
        }

        [Test]
        public void AsMapReturnsDictionary()
        {
            string[] keys = { "red", "blue", "yellow" };
            IntPtr ptr1 = keys.ToPtr();
            ulong[] values = { 1, 2, 3 };
            IntPtr ptr2 = values.ToPtr();
            Dictionary<string, ulong> dest = ptr1.AsMap<ulong>(ptr2, values.Length);
            Assert.That(dest, Is.Not.Null);
            Assert.That(dest.Keys, Is.EqualTo(keys));
            Assert.That(dest.Values, Is.EqualTo(values));
        }
    }
}
