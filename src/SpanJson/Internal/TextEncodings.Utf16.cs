namespace SpanJson.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static partial class TextEncodings
    {
        public static class Utf16
        {
            static readonly AsymmetricKeyHashTable<string> s_stringCache = new AsymmetricKeyHashTable<string>(StringReadOnlySpanByteAscymmetricEqualityComparer.Instance);

            /// <summary>For short strings use only.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static string GetStringWithCache(in ReadOnlySpan<byte> utf16Source)
            {
                if (utf16Source.IsEmpty) { return string.Empty; }
                if (!s_stringCache.TryGetValue(utf16Source, out var value))
                {
                    GetStringWithCacheSlow(utf16Source, out value);
                }
                return value;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void GetStringWithCacheSlow(in ReadOnlySpan<byte> utf16Source, out string value)
            {
                if (utf16Source.IsEmpty)
                {
                    value = string.Empty;
                    s_stringCache.TryAdd(JsonHelpers.Empty<byte>(), value);
                }
                else
                {
                    var buffer = utf16Source.ToArray();
                    value = Encoding.Unicode.GetString(buffer);
                    s_stringCache.TryAdd(buffer, value);
                }
            }
        }
    }
}
