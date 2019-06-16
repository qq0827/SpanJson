
namespace SpanJson.Internal
{
    using System.Runtime.CompilerServices;
    using System.Text;

    public static partial class TextEncodings
    {
        // Reject any invalid UTF-8 data rather than silently replacing.
        public static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        // Encoding Helpers
        const char HighSurrogateStart = '\ud800';
        const char HighSurrogateEnd = '\udbff';
        const char LowSurrogateStart = '\udc00';
        const char LowSurrogateEnd = '\udfff';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static int PtrDiff(char* a, char* b)
        {
            return (int)(((uint)((byte*)a - (byte*)b)) >> 1);
        }

        // byte* flavor just for parity
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static int PtrDiff(byte* a, byte* b)
        {
            return (int)(a - b);
        }

        /// <summary>Returns <see langword="true"/> iff <paramref name="value"/> is between
        /// <paramref name="lowerBound"/> and <paramref name="upperBound"/>, inclusive.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsInRangeInclusive(int value, int lowerBound, int upperBound)
            => (uint)(value - lowerBound) <= (uint)(upperBound - lowerBound);
    }
}
