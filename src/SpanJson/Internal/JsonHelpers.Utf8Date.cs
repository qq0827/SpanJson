﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// borrowed from https://github.com/dotnet/corefx/blob/8135319caa7e457ed61053ca1418313b88057b51/src/System.Text.Json/src/System/Text/Json/JsonHelpers.Date.cs#L12

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SpanJson.Internal
{
    internal static partial class JsonHelpers
    {
        public static bool TryParseAsISO(ReadOnlySpan<byte> source, out DateTime value, out int bytesConsumed)
        {
            if (!TryParseDateTimeOffset(source, out DateTimeOffset dateTimeOffset, out bytesConsumed, out DateTimeKind kind))
            {
                value = default;
                bytesConsumed = 0;
                return false;
            }

            switch (kind)
            {
                case DateTimeKind.Local:
                    value = dateTimeOffset.LocalDateTime;
                    break;
                case DateTimeKind.Utc:
                    value = dateTimeOffset.UtcDateTime;
                    break;
                default:
                    Debug.Assert(kind == DateTimeKind.Unspecified);
                    value = dateTimeOffset.DateTime;
                    break;
            }

            return true;
        }

        public static bool TryParseAsISO(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed)
        {
            return TryParseDateTimeOffset(source, out value, out bytesConsumed, out _);
        }

        //
        // Flexible ISO 8601 format. One of
        //
        // ---------------------------------
        // YYYY-MM-DD (eg 1997-07-16)
        // YYYY-MM-DDThh:mm (eg 1997-07-16T19:20)
        // YYYY-MM-DDThh:mm:ss (eg 1997-07-16T19:20:30)
        // YYYY-MM-DDThh:mm:ss.s (eg 1997-07-16T19:20:30.45)
        // YYYY-MM-DDThh:mmTZD (eg 1997-07-16T19:20+01:00)
        // YYYY-MM-DDThh:mm:ssTZD (eg 1997-07-16T19:20:30+01:00)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45Z)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45+01:00)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45-01:00)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45+0100)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45-0100)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45+01)
        // YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45-01)
        private static bool TryParseDateTimeOffset(ReadOnlySpan<byte> source, out DateTimeOffset value, out int bytesConsumed, out DateTimeKind kind)
        {
            // Source does not have enough characters for YYYY-MM-DD
            if ((uint)source.Length < 10u)
            {
                goto ReturnFalse;
            }

            int year;
            {
                uint digit1 = source[0] - (uint)'0';
                uint digit2 = source[1] - (uint)'0';
                uint digit3 = source[2] - (uint)'0';
                uint digit4 = source[3] - (uint)'0';

                if (digit1 > 9u || digit2 > 9u || digit3 > 9u || digit4 > 9u)
                {
                    goto ReturnFalse;
                }

                year = (int)(digit1 * 1000u + digit2 * 100u + digit3 * 10u + digit4);
            }

            if (source[4] != JsonUtf8Constant.Hyphen)
            {
                goto ReturnFalse;
            }

            int month;
            if (!TryGetNextTwoDigits(source.Slice(start: 5, length: 2), out month))
            {
                goto ReturnFalse;
            }

            if (source[7] != JsonUtf8Constant.Hyphen)
            {
                goto ReturnFalse;
            }

            int day;
            if (!TryGetNextTwoDigits(source.Slice(start: 8, length: 2), out day))
            {
                goto ReturnFalse;
            }

            // We now have YYYY-MM-DD
            bytesConsumed = 10;

            int hour = 0;
            int minute = 0;
            int second = 0;
            int fraction = 0; // This value should never be greater than 9_999_999.
            int offsetHours = 0;
            int offsetMinutes = 0;
            byte offsetToken = default;

            if ((uint)source.Length < 11u)
            {
                goto FinishedParsing;
            }

            byte curByte = source[10];

            if (curByte == JsonUtf8Constant.UtcOffsetToken || curByte == JsonUtf8Constant.Plus || curByte == JsonUtf8Constant.Hyphen)
            {
                goto ReturnFalse;
            }
            else if (curByte != JsonUtf8Constant.TimePrefix)
            {
                goto FinishedParsing;
            }

            // Source does not have enough characters for YYYY-MM-DDThh:mm
            if ((uint)source.Length < 16u)
            {
                goto ReturnFalse;
            }

            if (!TryGetNextTwoDigits(source.Slice(start: 11, length: 2), out hour))
            {
                goto ReturnFalse;
            }

            if (source[13] != JsonUtf8Constant.Colon)
            {
                goto ReturnFalse;
            }

            if (!TryGetNextTwoDigits(source.Slice(start: 14, length: 2), out minute))
            {
                goto ReturnFalse;
            }

            // We now have YYYY-MM-DDThh:mm
            bytesConsumed = 16;

            if ((uint)source.Length < 17u)
            {
                goto FinishedParsing;
            }

            curByte = source[16];

            int sourceIndex = 16;

            if (curByte == JsonUtf8Constant.UtcOffsetToken)
            {
                bytesConsumed++;
                offsetToken = JsonUtf8Constant.UtcOffsetToken;
                goto FinishedParsing;
            }
            else if (curByte == JsonUtf8Constant.Plus || curByte == JsonUtf8Constant.Hyphen)
            {
                offsetToken = curByte;
                sourceIndex++;
                goto ParseOffset;
            }
            else if (curByte != JsonUtf8Constant.Colon)
            {
                goto FinishedParsing;
            }

            if (!TryGetNextTwoDigits(source.Slice(start: 17, length: 2), out second))
            {
                goto ReturnFalse;
            }

            // We now have YYYY-MM-DDThh:mm:ss
            bytesConsumed = 19;

            if ((uint)source.Length < 20u)
            {
                goto FinishedParsing;
            }

            curByte = source[19];
            sourceIndex = 19;

            if (curByte == JsonUtf8Constant.UtcOffsetToken)
            {
                bytesConsumed++;
                offsetToken = JsonUtf8Constant.UtcOffsetToken;
                goto FinishedParsing;
            }
            else if (curByte == JsonUtf8Constant.Plus || curByte == JsonUtf8Constant.Hyphen)
            {
                offsetToken = curByte;
                sourceIndex++;
                goto ParseOffset;
            }
            else if (curByte != JsonUtf8Constant.Period)
            {
                goto FinishedParsing;
            }

            // Source does not have enough characters for YYYY-MM-DDThh:mm:ss.s
            if ((uint)source.Length < 21u)
            {
                goto ReturnFalse;
            }

            sourceIndex = 20;

            // Parse fraction. This value should never be greater than 9_999_999
            {
                int numDigitsRead = 0;
                int fractionEnd = Math.Min(sourceIndex + JsonSharedConstant.DateTimeParseNumFractionDigits, source.Length);

                while (sourceIndex < fractionEnd && IsDigit(curByte = source[sourceIndex]))
                {
                    if (numDigitsRead < JsonSharedConstant.DateTimeNumFractionDigits)
                    {
                        fraction = (fraction * 10) + (int)(curByte - (uint)'0');
                        numDigitsRead++;
                    }

                    sourceIndex++;
                }

                if (fraction != 0)
                {
                    while (numDigitsRead < JsonSharedConstant.DateTimeNumFractionDigits)
                    {
                        fraction *= 10;
                        numDigitsRead++;
                    }
                }
            }

            // We now have YYYY-MM-DDThh:mm:ss.s
            bytesConsumed = sourceIndex;

            if (sourceIndex == source.Length)
            {
                goto FinishedParsing;
            }

            curByte = source[sourceIndex];

            if (curByte == JsonUtf8Constant.UtcOffsetToken)
            {
                bytesConsumed++;
                offsetToken = JsonUtf8Constant.UtcOffsetToken;
                goto FinishedParsing;
            }
            else if (curByte == JsonUtf8Constant.Plus || curByte == JsonUtf8Constant.Hyphen)
            {
                offsetToken = source[sourceIndex++];
                goto ParseOffset;
            }
            else if (IsDigit(curByte))
            {
                goto ReturnFalse;
            }

            goto FinishedParsing;

        ParseOffset:
            // Source does not have enough characters for YYYY-MM-DDThh:mm:ss.s+|-hh
            if (source.Length - sourceIndex < 2)
            {
                goto ReturnFalse;
            }

            if (!TryGetNextTwoDigits(source.Slice(start: sourceIndex, length: 2), out offsetHours))
            {
                goto ReturnFalse;
            }
            sourceIndex += 2;

            // We now have YYYY-MM-DDThh:mm:ss.s+|-hh
            bytesConsumed = sourceIndex;

            // Source does not have enough characters for YYYY-MM-DDThh:mm:ss.s+|-hhmm
            if (source.Length - sourceIndex < 2)
            {
                goto FinishedParsing;
            }

            // Source should be of format YYYY-MM-DDThh:mm:ss.s+|-hh:mm
            if (source[sourceIndex] == JsonUtf8Constant.Colon)
            {
                sourceIndex++;

                // Source does not have enough characters for YYYY-MM-DDThh:mm:ss.s+|-hh:mm
                if (source.Length - sourceIndex < 2)
                {
                    goto ReturnFalse;
                }
            }

            if (!TryGetNextTwoDigits(source.Slice(start: sourceIndex, length: 2), out offsetMinutes))
            {
                goto ReturnFalse;
            }
            sourceIndex += 2;

            // We now have YYYY-MM-DDThh:mm:ss.s+|-hh[:]mm
            bytesConsumed = sourceIndex;

        FinishedParsing:
            if ((offsetToken != JsonUtf8Constant.UtcOffsetToken) && (offsetToken != JsonUtf8Constant.Plus) && (offsetToken != JsonUtf8Constant.Hyphen))
            {
                if (!TryCreateDateTimeOffsetInterpretingDataAsLocalTime(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, out value))
                {
                    goto ReturnFalse;
                }

                kind = DateTimeKind.Unspecified;
                return true;
            }

            if (offsetToken == JsonUtf8Constant.UtcOffsetToken)
            {
                // Same as specifying an offset of "+00:00", except that DateTime's Kind gets set to UTC rather than Local
                if (!TryCreateDateTimeOffset(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, offsetNegative: false, offsetHours: 0, offsetMinutes: 0, out value))
                {
                    goto ReturnFalse;
                }

                kind = DateTimeKind.Utc;
                return true;
            }

            Debug.Assert(offsetToken == JsonUtf8Constant.Plus || offsetToken == JsonUtf8Constant.Hyphen);

            if (!TryCreateDateTimeOffset(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, offsetNegative: offsetToken == JsonUtf8Constant.Hyphen, offsetHours: offsetHours, offsetMinutes: offsetMinutes, out value))
            {
                goto ReturnFalse;
            }

            kind = DateTimeKind.Local;
            return true;

        ReturnFalse:
            value = default;
            bytesConsumed = 0;
            kind = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetNextTwoDigits(ReadOnlySpan<byte> source, out int value)
        {
            Debug.Assert(source.Length == 2);

            uint digit1 = source[0] - (uint)'0';
            uint digit2 = source[1] - (uint)'0';

            if (digit1 > 9u || digit2 > 9u)
            {
                value = default;
                return false;
            }

            value = (int)(digit1 * 10u + digit2);
            return true;
        }

        // The following methods are borrowed verbatim from src/Common/src/CoreLib/System/Buffers/Text/Utf8Parser/Utf8Parser.Date.Helpers.cs

        /// <summary>
        /// Overflow-safe DateTimeOffset factory.
        /// </summary>
        private static bool TryCreateDateTimeOffset(DateTime dateTime, bool offsetNegative, int offsetHours, int offsetMinutes, out DateTimeOffset value)
        {
            if (((uint)offsetHours) > JsonSharedConstant.MaxDateTimeUtcOffsetHours)
            {
                value = default;
                return false;
            }

            if (((uint)offsetMinutes) > 59u)
            {
                value = default;
                return false;
            }

            if (offsetHours == JsonSharedConstant.MaxDateTimeUtcOffsetHours && offsetMinutes != 0)
            {
                value = default;
                return false;
            }

            long offsetTicks = (((long)offsetHours) * 3600L + ((long)offsetMinutes) * 60L) * TimeSpan.TicksPerSecond;
            if (offsetNegative)
            {
                offsetTicks = -offsetTicks;
            }

            try
            {
                value = new DateTimeOffset(ticks: dateTime.Ticks, offset: new TimeSpan(ticks: offsetTicks));
            }
            catch (ArgumentOutOfRangeException)
            {
                // If we got here, the combination of the DateTime + UTC offset strayed outside the 1..9999 year range. This case seems rare enough
                // that it's better to catch the exception rather than replicate DateTime's range checking (which it's going to do anyway.)
                value = default;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Overflow-safe DateTimeOffset factory.
        /// </summary>
        private static bool TryCreateDateTimeOffset(int year, int month, int day, int hour, int minute, int second, int fraction, bool offsetNegative, int offsetHours, int offsetMinutes, out DateTimeOffset value)
        {
            if (!TryCreateDateTime(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, kind: DateTimeKind.Unspecified, out DateTime dateTime))
            {
                value = default;
                return false;
            }

            if (!TryCreateDateTimeOffset(dateTime: dateTime, offsetNegative: offsetNegative, offsetHours: offsetHours, offsetMinutes: offsetMinutes, out value))
            {
                value = default;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Overflow-safe DateTimeOffset/Local time conversion factory.
        /// </summary>
        private static bool TryCreateDateTimeOffsetInterpretingDataAsLocalTime(int year, int month, int day, int hour, int minute, int second, int fraction, out DateTimeOffset value)
        {
            if (!TryCreateDateTime(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, DateTimeKind.Local, out DateTime dateTime))
            {
                value = default;
                return false;
            }

            try
            {
                value = new DateTimeOffset(dateTime);
            }
            catch (ArgumentOutOfRangeException)
            {
                // If we got here, the combination of the DateTime + UTC offset strayed outside the 1..9999 year range. This case seems rare enough
                // that it's better to catch the exception rather than replicate DateTime's range checking (which it's going to do anyway.)
                value = default;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Overflow-safe DateTime factory.
        /// </summary>
        private static bool TryCreateDateTime(int year, int month, int day, int hour, int minute, int second, int fraction, DateTimeKind kind, out DateTime value)
        {
            if (0u >= (uint)year)
            {
                value = default;
                return false;
            }

            Debug.Assert(year <= 9999); // All of our callers to date parse the year from fixed 4-digit fields so this value is trusted.

            if ((((uint)month) - 1u) >= 12u)
            {
                value = default;
                return false;
            }

            uint dayMinusOne = ((uint)day) - 1u;
            if (dayMinusOne >= 28u && dayMinusOne >= (uint)DateTime.DaysInMonth(year, month))
            {
                value = default;
                return false;
            }

            if (((uint)hour) > 23u)
            {
                value = default;
                return false;
            }

            if (((uint)minute) > 59u)
            {
                value = default;
                return false;
            }

            if (((uint)second) > 59u)
            {
                value = default;
                return false;
            }

            Debug.Assert(fraction >= 0 && fraction <= JsonSharedConstant.MaxDateTimeFraction); // All of our callers to date parse the fraction from fixed 7-digit fields so this value is trusted.

            int[] days = DateTime.IsLeapYear(year) ? s_daysToMonth366 : s_daysToMonth365;
            int yearMinusOne = year - 1;
            int totalDays = (yearMinusOne * 365) + (yearMinusOne / 4) - (yearMinusOne / 100) + (yearMinusOne / 400) + days[month - 1] + day - 1;
            long ticks = totalDays * TimeSpan.TicksPerDay;
            int totalSeconds = (hour * 3600) + (minute * 60) + second;
            ticks += totalSeconds * TimeSpan.TicksPerSecond;
            ticks += fraction;
            value = new DateTime(ticks: ticks, kind: kind);
            return true;
        }

        private static readonly int[] s_daysToMonth365 = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
        private static readonly int[] s_daysToMonth366 = { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };
    }
}
