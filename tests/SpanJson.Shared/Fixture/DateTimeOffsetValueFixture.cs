using System;

namespace SpanJson.Shared.Fixture
{
    public class DateTimeOffsetValueFixture : IValueFixture
    {
        private long _lastValue;
        public Type Type { get; } = typeof(DateTimeOffset);

        public object Generate()
        {
            _lastValue += TimeSpan.TicksPerMillisecond * 500;
#if NET451
            return FromUnixTimeMilliseconds(_lastValue);
#else
            return DateTimeOffset.FromUnixTimeMilliseconds(_lastValue);
#endif
        }

#if NET451

        // Number of 100ns ticks per time unit
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of milliseconds per time unit
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60;
        private const int MillisPerHour = MillisPerMinute * 60;
        private const int MillisPerDay = MillisPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;          // 584388
        // Number of days from 1/1/0001 to 12/30/1899
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        // Number of days from 1/1/0001 to 12/31/1969
        internal const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162
        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;  // 3652059

        private const long MinTicks = 0;
        private const long MaxTicks = DaysTo10000 * TicksPerDay - 1;
        private const long MaxMillis = (long)DaysTo10000 * MillisPerDay;

        private const long UnixEpochTicks = DaysTo1970 * TicksPerDay;

        private const long UnixEpochMilliseconds = /*DateTime.*/UnixEpochTicks / TimeSpan.TicksPerMillisecond; // 62,135,596,800,000

        public static DateTimeOffset FromUnixTimeMilliseconds(long milliseconds)
        {
            const long MinMilliseconds = /*DateTime.*/MinTicks / TimeSpan.TicksPerMillisecond - UnixEpochMilliseconds;
            const long MaxMilliseconds = /*DateTime.*/MaxTicks / TimeSpan.TicksPerMillisecond - UnixEpochMilliseconds;

            if (milliseconds < MinMilliseconds || milliseconds > MaxMilliseconds)
            {
                throw new ArgumentOutOfRangeException(nameof(milliseconds),
                    "ArgumentOutOfRange_Range");
            }

            long ticks = milliseconds * TimeSpan.TicksPerMillisecond + /*DateTime.*/UnixEpochTicks;
            return new DateTimeOffset(ticks, TimeSpan.Zero);
        }

#endif
    }
}