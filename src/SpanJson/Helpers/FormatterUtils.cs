using System;
using System.Buffers;
using System.Diagnostics;

namespace SpanJson.Helpers
{
    public static class FormatterUtils
    {
        public static int CountDigits(ulong value)
        {
            var digits = 1;
            uint part;
            if (value >= 10000000ul)
            {
                if (value >= 100000000000000ul)
                {
                    part = (uint)(value / 100000000000000ul);
                    digits += 14;
                }
                else
                {
                    part = (uint)(value / 10000000ul);
                    digits += 7;
                }
            }
            else
            {
                part = (uint)value;
            }

            if (part < 10u)
            {
                // no-op
            }
            else if (part < 100u)
            {
                digits += 1;
            }
            else if (part < 1000u)
            {
                digits += 2;
            }
            else if (part < 10000u)
            {
                digits += 3;
            }
            else if (part < 100000u)
            {
                digits += 4;
            }
            else if (part < 1000000u)
            {
                digits += 5;
            }
            else
            {
                Debug.Assert(part < 10000000);
                digits += 6;
            }

            return digits;
        }

        public static void GrowArray<T>(ref T[] array)
        {
            var backup = array;
            array = ArrayPool<T>.Shared.Rent(backup.Length * 2);
            backup.AsSpan().CopyTo(array);
            ArrayPool<T>.Shared.Return(backup);
        }

        public static T[] CopyArray<T>(T[] array, int count)
        {
            var result = new T[count];
            array.AsSpan(0, count).CopyTo(result);
            return result;
        }
    }
}