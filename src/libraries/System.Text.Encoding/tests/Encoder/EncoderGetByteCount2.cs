// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Text.Tests
{
    // GetByteCount(System.Char[],System.Int32,System.Int32,System.Boolean)
    public class EncoderGetByteCount2
    {
        private readonly RandomDataGenerator _generator = new RandomDataGenerator();

        #region Private Fields
        private const int c_SIZE_OF_ARRAY = 256;
        private const char HIGH_SURROGATE_START = '\ud800';
        private const char HIGH_SURROGATE_END = '\udbff';
        #endregion

        #region Positive Test Cases
        // PosTest1: Call GetByteCount to get byte count of an arbitrary character array by using ASCII encoder
        [Fact]
        public void PosTest1()
        {
            char[] chars = new char[c_SIZE_OF_ARRAY];
            Encoder encoder = Encoding.UTF8.GetEncoder();

            for (int i = 0; i < chars.Length; ++i)
            {
                chars[i] = _generator.GetChar(-55);
            }

            int ret1 = encoder.GetByteCount(chars, 0, chars.Length, true);
            int ret2 = encoder.GetByteCount(chars, 0, chars.Length, false);

            if (IsHighSurrogate(chars[chars.Length - 1]))
            {
                ret2 += 3;
                encoder.GetByteCount(chars, 0, chars.Length, true);
            }
            Assert.Equal(ret2, ret1);
            ret1 = encoder.GetByteCount(chars, 0, 0, true);
            ret2 = encoder.GetByteCount(chars, 0, 0, false);

            Assert.Equal(ret2, ret1);
            Assert.Equal(0, ret1);
        }

        // PosTest2: Call GetByteCount to get byte count of an arbitrary character array by using ASCII encoder
        [Fact]
        public void PosTest2()
        {
            char[] chars = new char[c_SIZE_OF_ARRAY];
            Encoder encoder = Encoding.Unicode.GetEncoder();

            for (int i = 0; i < chars.Length; ++i)
            {
                chars[i] = _generator.GetChar(-55);
            }

            int ret1 = encoder.GetByteCount(chars, 0, chars.Length, true);
            int ret2 = encoder.GetByteCount(chars, 0, chars.Length, false);
            // If the last character is a surrogate and the encoder is flushing its state, it will return a fallback character. 
            // When the encoder isn't flushing its state then it holds on to the remnant bytes between calls so that if the next
            // bytes passed in form a valid character you'd get that char as a result
            if (IsHighSurrogate(chars[chars.Length - 1]))
            {
                ret2 += 2;
            }

            Assert.Equal(ret2, ret1);

            ret1 = encoder.GetByteCount(chars, 0, 0, true);
            ret2 = encoder.GetByteCount(chars, 0, 0, false);

            Assert.Equal(ret2, ret1);
            Assert.Equal(0, ret1);
        }

        // PosTest3: Call GetByteCount to get byte count of an ASCII character array by using ASCII encoder
        [Fact]
        public void PosTest3()
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()_+-=\\|/?<>  ,.`~".ToCharArray();
            Encoder encoder = Encoding.UTF8.GetEncoder();

            VerificationHelper(encoder, chars, 0, chars.Length, true, chars.Length, "003.1");
            VerificationHelper(encoder, chars, 0, chars.Length, false, chars.Length, "003.2");

            VerificationHelper(encoder, chars, 1, chars.Length - 1, true, chars.Length - 1, "003.3");
            VerificationHelper(encoder, chars, 1, chars.Length - 1, false, chars.Length - 1, "003.4");
        }

        // PosTest4: Call GetByteCount to get byte count of an ASCII character array by using unicode encoder
        [Fact]
        public void PosTest4()
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyz1234567890!@#$%^&*()_+-=\\|/?<>  ,.`~".ToCharArray();
            Encoder encoder = Encoding.Unicode.GetEncoder();

            VerificationHelper(encoder, chars, 0, chars.Length, true, chars.Length * 2, "004.1");
            VerificationHelper(encoder, chars, 0, chars.Length, false, chars.Length * 2, "004.2");

            VerificationHelper(encoder, chars, 1, chars.Length - 1, true, (chars.Length - 1) * 2, "004.3");
            VerificationHelper(encoder, chars, chars.Length - 1, 1, false, 2, "004.4");
        }

        // PosTest5: Call GetByteCount to get byte count of an unicode character array by using unicode encoder
        [Fact]
        public void PosTest5()
        {
            char[] chars = "\u8FD9\u662F\u4E00\u4E2AABC\u6D4B\u8BD5".ToCharArray();
            Encoder encoder = Encoding.Unicode.GetEncoder();
            VerificationHelper(encoder, chars, 0, chars.Length, true, chars.Length * 2, "005.1");
            VerificationHelper(encoder, chars, 0, chars.Length, false, chars.Length * 2, "005.2");

            VerificationHelper(encoder, chars, 1, chars.Length - 1, true, (chars.Length - 1) * 2, "005.3");
            VerificationHelper(encoder, chars, chars.Length - 1, 1, false, 2, "005.4");
        }
        #endregion

        #region Nagetive Test Cases
        // NegTest1: ArgumentNullException should be thrown when chars is a null reference
        [Fact]
        public void NegTest1()
        {
            VerificationHelper<ArgumentNullException>(Encoding.UTF8.GetEncoder(), null, 0, 0, true, typeof(ArgumentNullException), "101.1");
        }

        // NegTest2: ArgumentOutOfRangeException should be thrown when index or count is less than zero.
        [Fact]
        public void NegTest2()
        {
            VerificationHelper<ArgumentOutOfRangeException>(Encoding.UTF8.GetEncoder(), new char[1], 0, -1, true, typeof(ArgumentOutOfRangeException), "102.1");
            VerificationHelper<ArgumentOutOfRangeException>(Encoding.UTF8.GetEncoder(), new char[1], -1, 0, true, typeof(ArgumentOutOfRangeException), "102.2");
        }

        // NegTest3: ArgumentOutOfRangeException should be thrown when index and count do not denote a valid range in char
        [Fact]
        public void NegTest3()
        {
            VerificationHelper<ArgumentOutOfRangeException>(Encoding.UTF8.GetEncoder(), new char[1], 0, 2, true, typeof(ArgumentOutOfRangeException), "103.1");
            VerificationHelper<ArgumentOutOfRangeException>(Encoding.UTF8.GetEncoder(), new char[1], 1, 1, true, typeof(ArgumentOutOfRangeException), "103.2");
        }
        #endregion

        private void VerificationHelper(Encoder encoder, char[] chars, int index, int count, bool flush, int expected, string errorno)
        {
            int ret = encoder.GetByteCount(chars, index, count, flush);
            Assert.Equal(expected, ret);
        }

        private void VerificationHelper<T>(Encoder encoder, char[] chars, int index, int count, bool flush, Type expected, string errorno) where T : Exception
        {
            string str = new string(chars);

            Assert.Throws<T>(() =>
            {
                int ret = encoder.GetByteCount(chars, index, count, flush);
            });
        }

        private bool IsHighSurrogate(char c)
        {
            return ((c >= HIGH_SURROGATE_START) && (c <= HIGH_SURROGATE_END));
        }
    }
}
