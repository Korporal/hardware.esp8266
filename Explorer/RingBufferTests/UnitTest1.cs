using Microsoft.VisualStudio.TestTools.UnitTesting;
using Steadsoft.IO;
using System;

namespace Steadsoft.UnitTests
{
    [TestClass]
    public class RingBufferUnitTests
    {
        [TestMethod]
        public void TestMethod01()
        {
            RingBuffer buffer = new RingBuffer(32);

            buffer.Write(0x00);

            Assert.AreEqual(1, buffer.UsedBytes);
        }
        [TestMethod]
        public void TestMethod02()
        {
            RingBuffer buffer = new RingBuffer(32);

            buffer.Write(0x00);
            buffer.Read(1);

            Assert.AreEqual(0, buffer.UsedBytes);
        }
        [TestMethod]
        public void TestMethod03()
        {
            RingBuffer buffer = new RingBuffer(32);

            buffer.Write(0x00);
            var bytes = buffer.Read(1);

            Assert.AreEqual(1, bytes.Length);
        }
        [TestMethod]
        public void TestMethod04()
        {
            RingBuffer buffer = new RingBuffer(32);

            buffer.Write(0x01, 0x02, 0x03, 0x04, 0x05);
            buffer.Read(1);

            Assert.AreEqual(4, buffer.UsedBytes);
        }
        [TestMethod]
        public void TestMethod05()
        {
            RingBuffer buffer = new RingBuffer(32);

            buffer.Write(0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F);

            buffer.Read(4);

            bool flag = buffer.TryFindBytes(out var length, 0x06, 0x07, 0x08);

            Assert.AreEqual(4, length);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestMethod07()
        {
            RingBuffer buffer = new RingBuffer(32);

            buffer.Write(0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F);

            buffer.Read(0); // illegal

            bool flag = buffer.TryFindBytes(out var length, 0x06, 0x07, 0x08);

            Assert.AreEqual(8, length);
        }

        [TestMethod]
        public void TestMethod06()
        {
            RingBuffer buffer = new RingBuffer(32);

            buffer.Write(0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F);

            buffer.Read(4);

            bool flag = buffer.TryFindBytes(out var length, 0x06, 0x07, 0x08);

            Assert.AreEqual(true, flag);
        }

        [TestMethod]
        public void TestMethod08()
        {
            RingBuffer buffer = new RingBuffer(32);

            for (byte X = 0; X < 32; X++)
            {
                buffer.Write(X);
            }

            buffer.Read(10);

            for (byte X = 0; X < 8; X++)
            {
                buffer.Write(X);
            }

            buffer.Read(3);

            bool flag = buffer.TryFindBytes(out var length, 0x0F, 0x10, 0x11);

            Assert.AreEqual(true, flag);
        }

        [TestMethod]
        public void TestMethod09()
        {
            RingBuffer buffer = new RingBuffer(32);

            for (byte X = 0; X < 32; X++)
            {
                buffer.Write(X);
            }

            buffer.Read(10);

            for (byte X = 0; X < 8; X++)
            {
                buffer.Write(X);
            }

            buffer.Read(3);

            bool flag = buffer.TryFindBytes(out var length, 0x0F, 0x10, 0x11);

            Assert.AreEqual(5, length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestMethod10()
        {
            RingBuffer buffer = new RingBuffer(32);

            for (byte X = 0; X < 64; X++)
            {
                buffer.Write(X);
            }
        }

        [TestMethod]
        public void TestMethod11()
        {
            RingBuffer buffer = new RingBuffer(32);

            for (byte X = 0; X < 22; X++)
            {
                buffer.Write(X);
            }

            buffer.Read(4);

            Assert.AreEqual(10, buffer.ContiguousFreeSpace);
        }

        [TestMethod]
        public void TestMethod12()
        {
            RingBuffer buffer = new RingBuffer(32);

            for (byte X = 0; X < 22; X++)
            {
                buffer.Write(X);
            }

            buffer.Read(4);

            Assert.AreEqual(14, buffer.FreeBytes);
        }

        [TestMethod]
        public void TestMethod13()
        {
            RingBuffer buffer = new RingBuffer(64);

            byte[] test = GenerateRandomLengthBlock(0xFF, buffer.ContiguousFreeSpace);

            // Copy the test data into the ring buffer's buffer.

            Array.Copy(test, buffer.Buffer, test.Length);

            buffer.FinishFastRecv(test.Length);

            Assert.AreEqual(buffer.Capacity - test.Length, buffer.FreeBytes);
        }

        [TestMethod]
        public void TestMethod14()
        {
            RingBuffer buffer = new RingBuffer(64);

            byte[] test = GenerateByteBlock(0xFF, buffer.ContiguousFreeSpace);

            Array.Copy(test, buffer.Buffer, test.Length);

            buffer.FinishFastRecv(test.Length);

            Assert.AreEqual(0, buffer.FreeBytes);
        }

        [TestMethod]
        public void TestMethod15()
        {
            RingBuffer buffer = new RingBuffer(64);

            byte[] test = GenerateByteBlock(0xFF, buffer.ContiguousFreeSpace);

            Array.Copy(test, buffer.Buffer, test.Length);

            buffer.FinishFastRecv(test.Length);

            buffer.Read(22);

            Assert.AreEqual(22, buffer.FreeBytes);
        }

        [TestMethod]
        public void TestMethod16()
        {
            RingBuffer buffer = new RingBuffer(64);

            byte[] test_1 = GenerateByteBlock(0xFF, buffer.ContiguousFreeSpace);

            Array.Copy(test_1, buffer.Buffer, test_1.Length);

            buffer.FinishFastRecv(test_1.Length);

            buffer.Read(22);

            byte[] test_2 = GenerateByteBlock(0x11, buffer.ContiguousFreeSpace);

            Array.Copy(test_2, buffer.Buffer, test_2.Length);

            buffer.FinishFastRecv(test_2.Length);
        }

        [TestMethod]
        public void TestMethod17()
        {
            const int CAPACITY = 4;

            RingBuffer buffer = new RingBuffer(CAPACITY);

            byte[] test_1 = GenerateByteBlock(0xFF, CAPACITY);

            Array.Copy(test_1, buffer.Buffer, test_1.Length);

            buffer.FinishFastRecv(test_1.Length);

            Assert.AreEqual(0, buffer.FreeBytes);

            buffer.Read(1);

            Assert.AreEqual(1, buffer.FreeBytes);

            buffer.Read(3);

            Assert.AreEqual(CAPACITY, buffer.FreeBytes);

        }

        [TestMethod]
        public void TestMethod18()
        {
            // This is an important test and exposes
            // issues that may be present with wrapping.

            RingBuffer buffer = new RingBuffer(16);

            byte[] test_1 = GenerateByteBlock(0xFF, 5);
            byte[] test_2 = GenerateByteBlock(0xFF, 3);
            byte[] test_3 = GenerateByteBlock(0xFF, 2);
            byte[] test_4 = GenerateByteBlock(0xFF, 6);

            buffer.Write(test_1);  // W 5
            buffer.Read(4);        // R 4

            Assert.AreEqual(1, buffer.UsedBytes);
            Assert.AreEqual(15, buffer.FreeBytes);

            buffer.Write(test_2); // W 3
            buffer.Read(4);       // R 4

            Assert.AreEqual(0, buffer.UsedBytes);
            Assert.AreEqual(16, buffer.FreeBytes);

            buffer.Write(test_3);  // W 2
            buffer.Read(1);        // R 1

            Assert.AreEqual(1, buffer.UsedBytes);
            Assert.AreEqual(15, buffer.FreeBytes);

            buffer.Write(test_4);  // W 6
            buffer.Read(4);        // R 4

            Assert.AreEqual(3, buffer.UsedBytes);
            Assert.AreEqual(13, buffer.FreeBytes);

            buffer.Write(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });  // W 6

            int num = 0;

            foreach (byte B in buffer.Content())
            {
                num++;

                if (num > 3)
                    Assert.AreEqual(num - 3, B);
            }

            Assert.AreEqual(9, num);

            bool found = buffer.Contains(0x03, 0x04);

            Assert.AreEqual(true, found);

            bool begins = buffer.BeginsWith(0x02, 0x03);

            Assert.AreEqual(false, begins);

            buffer.Read(2);

            begins = buffer.BeginsWith(0xFF, 0x01, 0x02, 0x03);

            Assert.AreEqual(true, begins);
        }



        private byte[] GenerateByteBlock(byte Value, int Length)
        {
            byte[] bytes = new byte[Length];

            for (int I = 0; I < bytes.Length; I++)
            {
                bytes[I] = Value;
            }

            return bytes;
        }

        private byte[] GenerateRandomLengthBlock(byte Value, int MaxLength)
        {
            var size = (new Random().Next(1, MaxLength));

            return GenerateByteBlock(Value, size);
        }


    }
}