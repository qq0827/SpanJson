// Licensed to the .NET Foundation under one or more agreements.
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
        public static bool TryParseAsISO(ReadOnlySpan<char> source, out DateTime value, out int bytesConsumed)
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

        public static bool TryParseAsISO(ReadOnlySpan<char> source, out DateTimeOffset value, out int bytesConsumed)
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
        private static bool TryParseDateTimeOffset(ReadOnlySpan<char> source, out DateTimeOffset value, out int bytesConsumed, out DateTimeKind kind)
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

                year = (int)(digit1 * 1000 + digit2 * 100 + digit3 * 10 + digit4);
            }

            if (source[4] != JsonConstants.HyphenChar)
            {
                goto ReturnFalse;
            }

            if (!TryGetNextTwoDigits(source.Slice(start: 5, length: 2), out int month))
            {
                goto ReturnFalse;
            }

            if (source[7] != JsonConstants.HyphenChar)
            {
                goto ReturnFalse;
            }

            if (!TryGetNextTwoDigits(source.Slice(start: 8, length: 2), out int day))
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
            char offsetToken = default;

            if ((uint)source.Length < 11u)
            {
                goto FinishedParsing;
            }

            char curChar = source[10];

            if (curChar == JsonConstants.UtcOffsetTokenChar || curChar == JsonConstants.PlusChar || curChar == JsonConstants.HyphenChar)
            {
                goto ReturnFalse;
            }
            else if (curChar != JsonConstants.TimePrefixChar)
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

            if (source[13] != JsonConstants.ColonChar)
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

            curChar = source[16];

            int sourceIndex = 16;

            if (curChar == JsonConstants.UtcOffsetTokenChar)
            {
                bytesConsumed++;
                offsetToken = JsonConstants.UtcOffsetTokenChar;
                goto FinishedParsing;
            }
            else if (curChar == JsonConstants.PlusChar || curChar == JsonConstants.HyphenChar)
            {
                offsetToken = curChar;
                sourceIndex++;
                goto ParseOffset;
            }
            else if (curChar != JsonConstants.ColonChar)
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

            curChar = source[19];
            sourceIndex = 19;

            if (curChar == JsonConstants.UtcOffsetTokenChar)
            {
                bytesConsumed++;
                offsetToken = JsonConstants.UtcOffsetTokenChar;
                goto FinishedParsing;
            }
            else if (curChar == JsonConstants.PlusChar || curChar == JsonConstants.HyphenChar)
            {
                offsetToken = curChar;
                sourceIndex++;
                goto ParseOffset;
            }
            else if (curChar != JsonConstants.PeriodChar)
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
                int fractionEnd = Math.Min(sourceIndex + JsonConstants.DateTimeParseNumFractionDigits, source.Length);

                while (sourceIndex < fractionEnd && IsDigit(curChar = source[sourceIndex]))
                {
                    if (numDigitsRead < JsonConstants.DateTimeNumFractionDigits)
                    {
                        fraction = (fraction * 10) + (int)(curChar - (uint)'0');
                        numDigitsRead++;
                    }

                    sourceIndex++;
                }

                if (fraction != 0)
                {
                    while (numDigitsRead < JsonConstants.DateTimeNumFractionDigits)
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

            curChar = source[sourceIndex];

            if (curChar == JsonConstants.UtcOffsetTokenChar)
            {
                bytesConsumed++;
                offsetToken = JsonConstants.UtcOffsetTokenChar;
                goto FinishedParsing;
            }
            else if (curChar == JsonConstants.PlusChar || curChar == JsonConstants.HyphenChar)
            {
                offsetToken = source[sourceIndex++];
                goto ParseOffset;
            }
            else if (IsDigit(curChar))
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
            if (source[sourceIndex] == JsonConstants.ColonChar)
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
            if ((offsetToken != JsonConstants.UtcOffsetTokenChar) && (offsetToken != JsonConstants.PlusChar) && (offsetToken != JsonConstants.HyphenChar))
            {
                if (!TryCreateDateTimeOffsetInterpretingDataAsLocalTime(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, out value))
                {
                    goto ReturnFalse;
                }

                kind = DateTimeKind.Unspecified;
                return true;
            }

            if (offsetToken == JsonConstants.UtcOffsetTokenChar)
            {
                // Same as specifying an offset of "+00:00", except that DateTime's Kind gets set to UTC rather than Local
                if (!TryCreateDateTimeOffset(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, offsetNegative: false, offsetHours: 0, offsetMinutes: 0, out value))
                {
                    goto ReturnFalse;
                }

                kind = DateTimeKind.Utc;
                return true;
            }

            Debug.Assert(offsetToken == JsonConstants.Plus || offsetToken == JsonConstants.HyphenChar);

            if (!TryCreateDateTimeOffset(year: year, month: month, day: day, hour: hour, minute: minute, second: second, fraction: fraction, offsetNegative: offsetToken == JsonConstants.HyphenChar, offsetHours: offsetHours, offsetMinutes: offsetMinutes, out value))
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
        private static bool TryGetNextTwoDigits(ReadOnlySpan<char> source, out int value)
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
    }
}
