using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NUnit.Framework; 
using UnityEngine;


namespace Unity.WebRTC.RuntimeTest
{

    public class IntPtrTest
    {
        [Test]
        public void AsAnsiStringWithFreeMemThrowException()
        {
            IntPtr ptr = IntPtr.Zero;
            Assert.Throws<ArgumentException>(() => { ptr.AsAnsiStringWithFreeMem();  });
        }

        [Test]
        public void AsAnsiStringWithoutFreeMemThrowException()
        {
            IntPtr ptr = IntPtr.Zero;
            Assert.Throws<ArgumentException>(() => { ptr.AsAnsiStringWithoutFreeMem(); });
        }

        [Test]
        public void AsAnsiStringWithoutFreeMemReturnsString()
        {
            string source = "test";
            IntPtr ptr = Marshal.StringToCoTaskMemAnsi(source);
            string dest = ptr.AsAnsiStringWithoutFreeMem();
            Assert.IsNotEmpty(dest);
            Assert.AreEqual(source, dest);
            Marshal.FreeCoTaskMem(ptr);
        }

        [Test]
        public void AsAnsiStringWithFreeMemReturnsString()
        {
            string source = "test";
            IntPtr ptr = Marshal.StringToCoTaskMemAnsi(source);
            string dest = ptr.AsAnsiStringWithFreeMem();
            Assert.IsNotEmpty(dest);
            Assert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsByteArray()
        {
            byte[] source = { 1, 2, 3 };
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            byte[] dest = ptr.AsArray<byte>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsIntArray()
        {
            int[] source = {1, 2, 3};
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            int[] dest = ptr.AsArray<int>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsUintArray()
        {
            uint[] source = { 1, 2, 3 };
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            uint[] dest = ptr.AsArray<uint>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsLongArray()
        {
            long[] source = { 1, 2, 3 };
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            long[] dest = ptr.AsArray<long>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsUlongArray()
        {
            ulong[] source = { 1, 2, 3 };
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            ulong[] dest = ptr.AsArray<ulong>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsFloatArray()
        {
            double[] source = { 0.1, 0.2, 0.3 };
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            double[] dest = ptr.AsArray<double>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsDoubleArray()
        {
            float[] source = { 0.1f, 0.2f, 0.3f };
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            float[] dest = ptr.AsArray<float>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsBoolArray()
        {
            bool[] source = { true, false, true };
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            bool[] dest = ptr.AsArray<bool>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }

        [Test]
        public void AsArrayReturnsStringArray()
        {
            string[] source = { "red", "blue", "yellow" };
            IntPtr ptr = IntPtrExtension.ToPtr(source);
            string[] dest = ptr.AsArray<string>(source.Length);
            Assert.IsNotEmpty(dest);
            CollectionAssert.AreEqual(source, dest);
        }
    }
}
