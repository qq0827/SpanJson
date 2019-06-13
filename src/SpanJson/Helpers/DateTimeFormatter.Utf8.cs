using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanJson.Helpers
{
    public static partial class DateTimeFormatter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFormat(DateTimeOffset value, Span<byte> output, out int bytesWritten)
        {
            if ((uint)output.Length < 33u)
            {
                bytesWritten = default;
                return false;
            }

            ref var b = ref MemoryMarshal.GetReference(output);
            WriteDateAndTime(value.DateTime, ref b, out bytesWritten);

            if (value.Offset == TimeSpan.Zero)
            {
                Unsafe.AddByteOffset(ref b, (IntPtr)bytesWritten++) = (byte)'Z';
            }
            else
            {
                WriteTimeZone(value.Offset, ref b, ref bytesWritten);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFormat(DateTime value, Span<byte> output, out int bytesWritten)
        {
            if ((uint)output.Length < 33u)
            {
                bytesWritten = default;
                return false;
            }

            ref var b = ref MemoryMarshal.GetReference(output);
            WriteDateAndTime(value, ref b, out bytesWritten);

            if (value.Kind == DateTimeKind.Local)
            {
                WriteTimeZone(TimeZoneInfo.Local.GetUtcOffset(value), ref b, ref bytesWritten);
            }
            else if (value.Kind == DateTimeKind.Utc)
            {
                Unsafe.AddByteOffset(ref b, (IntPtr)bytesWritten++) = (byte)'Z';
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteDateAndTime(DateTime value, ref byte b, out int bytesWritten)
        {
            IntPtr offset = (IntPtr)0;
            WriteFourDigits((uint)value.Year, ref b, 0);
            Unsafe.AddByteOffset(ref b, offset + 4) = (byte)'-';
            WriteTwoDigits(value.Month, ref b, 5);
            Unsafe.AddByteOffset(ref b, offset + 7) = (byte)'-';
            WriteTwoDigits(value.Day, ref b, 8);
            Unsafe.AddByteOffset(ref b, offset + 10) = (byte)'T';
            WriteTwoDigits(value.Hour, ref b, 11);
            Unsafe.AddByteOffset(ref b, offset + 13) = (byte)':';
            WriteTwoDigits(value.Minute, ref b, 14);
            Unsafe.AddByteOffset(ref b, offset + 16) = (byte)':';
            WriteTwoDigits(value.Second, ref b, 17);
            bytesWritten = 19;
            var fraction = (uint)((ulong)value.Ticks % TimeSpan.TicksPerSecond);
            if (fraction > 0)
            {
                Unsafe.AddByteOffset(ref b, offset + 19) = (byte)'.';
                WriteDigits(fraction, ref b, 20);
                bytesWritten = 27;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteTimeZone(TimeSpan offset, ref byte b, ref int bytesWritten)
        {
            byte sign;
            if (offset < default(TimeSpan))
            {
                sign = (byte)'-';
                offset = TimeSpan.FromTicks(-offset.Ticks);
            }
            else
            {
                sign = (byte)'+';
            }

            Unsafe.Add(ref b, bytesWritten) = sign;
            WriteTwoDigits(offset.Hours, ref b, bytesWritten + 1);
            Unsafe.Add(ref b, bytesWritten + 3) = (byte)':';
            WriteTwoDigits(offset.Minutes, ref b, bytesWritten + 4);
            bytesWritten += 6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteFourDigits(uint value, ref byte b, int startIndex)
        {
            IntPtr offset = (IntPtr)startIndex;
            var temp = '0' + value;
            value /= 10u;
            Unsafe.AddByteOffset(ref b, offset + 3) = (byte)(temp - value * 10u);

            temp = '0' + value;
            value /= 10u;
            Unsafe.AddByteOffset(ref b, offset + 2) = (byte)(temp - value * 10u);

            temp = '0' + value;
            value /= 10u;
            Unsafe.AddByteOffset(ref b, offset + 1) = (byte)(temp - value * 10u);

            Unsafe.AddByteOffset(ref b, offset) = (byte)('0' + value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteTwoDigits(int value, ref byte b, int startIndex)
        {
            var temp = '0' + value;
            value /= 10;
            IntPtr offset = (IntPtr)startIndex;
            Unsafe.AddByteOffset(ref b, offset + 1) = (byte)(temp - value * 10);
            Unsafe.AddByteOffset(ref b, offset) = (byte)('0' + value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteDigits(uint value, ref byte b, int pos)
        {
            IntPtr offset = (IntPtr)pos;
            for (var i = 6; i >= 0; i--)
            {
                ulong temp = '0' + value;
                value /= 10u;
                Unsafe.AddByteOffset(ref b, offset + i) = (byte)(temp - value * 10u);
            }
        }
    }
}