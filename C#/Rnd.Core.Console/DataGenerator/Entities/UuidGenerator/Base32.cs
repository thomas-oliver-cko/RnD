/*
 * Derived from https://github.com/google/google-authenticator-android/blob/master/AuthenticatorApp/src/main/java/com/google/android/apps/authenticator/Base32String.java
 * 
 * Copyright (C) 2016 BravoTango86
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Rnd.Core.ConsoleApp.DataGenerator.Entities.UuidGenerator
{
    public static class Base32
    {
        const string Separator = "-";

        static readonly char[] Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToLower().ToCharArray();
        static readonly int Mask = Digits.Length - 1;
        static readonly int Shift = NumberOfTrailingZeros(Digits.Length);
        static readonly Dictionary<char, int> CharMap = new Dictionary<char, int>();

        static Base32()
        {
            for (var i = 0; i < Digits.Length; i++)
            {
                CharMap[Digits[i]] = i;
            }
        }

        public static byte[] Decode(string encoded)
        {
            // Remove whitespace and separators
            encoded = encoded.Trim()
                .ToLower()
                .Replace(Separator, "");

            // Remove padding. Note: the padding is used as hint to determine how many
            // bits to decode from the last incomplete chunk (which is commented out
            // below, so this may have been wrong to start with).
            encoded = Regex.Replace(encoded, "[=]*$", "");

            // Canonicalize to all upper case
            if (encoded.Length == 0)
                return new byte[0];

            var encodedLength = encoded.Length;
            var outLength = encodedLength * Shift / 8;
            var result = new byte[outLength];
            var buffer = 0;
            var next = 0;
            var bitsLeft = 0;
            foreach (var c in encoded)
            {
                if (!CharMap.ContainsKey(c))
                    throw new DecodingException("Illegal character: " + c);

                buffer <<= Shift;
                buffer |= CharMap[c] & Mask;
                bitsLeft += Shift;

                if (bitsLeft < 8) continue;

                result[next++] = (byte) (buffer >> (bitsLeft - 8));
                bitsLeft -= 8;
            }

            var remainingBuffer = buffer & ((1 << bitsLeft) - 1);

            // Remaining non-zero buffer as well as left bits amount being at least the shift size lead to 
            // ambiguity, since different input strings lead to the same result when simply ignoring those.
            if (remainingBuffer != 0)
                throw new DecodingException("Remaining buffer: " + remainingBuffer);

            if (next != outLength || bitsLeft >= Shift)
                throw new DecodingException("Bits left: " + bitsLeft);

            return result;
        }


        public static string Encode(byte[] data, bool padOutput = false)
        {
            if (data.Length == 0)
                return "";

            // SHIFT is the number of bits per output character, so the length of the
            // output is the length of the input multiplied by 8/SHIFT, rounded up.
            if (data.Length >= 1 << 28)
            {
                // The computation below will fail, so don't do it.
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            var outputLength = (data.Length * 8 + Shift - 1) / Shift;
            var result = new StringBuilder(outputLength);

            int buffer = data[0];
            var next = 1;
            var bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < Shift)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xff;
                        bitsLeft += 8;
                    }
                    else
                    {
                        var pad = Shift - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }

                var index = Mask & (buffer >> (bitsLeft - Shift));
                bitsLeft -= Shift;
                result.Append(Digits[index]);
            }

            if (!padOutput) return result.ToString();

            var padding = 8 - result.Length % 8;
            if (padding > 0) result.Append(new string('=', padding == 8 ? 0 : padding));

            return result.ToString();
        }

        static int NumberOfTrailingZeros(int i)
        {
            if (i == 0) return 32;
            var n = 31;
            var y = i << 16;

            if (y != 0)
            {
                n -= 16;
                i = y;
            }

            y = i << 8;
            if (y != 0)
            {
                n -= 8;
                i = y;
            }

            y = i << 4;
            if (y != 0)
            {
                n -= 4;
                i = y;
            }

            y = i << 2;
            if (y != 0)
            {
                n -= 2;
                i = y;
            }

            return n - (int)((uint)(i << 1) >> 31);
        }
    }
}