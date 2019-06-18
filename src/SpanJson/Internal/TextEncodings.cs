namespace SpanJson.Internal
{
    using System;
    using System.Text;

    public static partial class TextEncodings
    {
        // Reject any invalid UTF-8 data rather than silently replacing.
        public static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        public static int GetUtf8ByteCount(ReadOnlySpan<char> text)
        {
            try
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                return UTF8NoBOM.GetByteCount(text);
#else
                if (text.IsEmpty) { return 0; }

                unsafe
                {
                    fixed (char* charPtr = text)
                    {
                        return UTF8NoBOM.GetByteCount(charPtr, text.Length);
                    }
                }
#endif
            }
            catch (EncoderFallbackException ex)
            {
                // We want to be consistent with the exception being thrown
                // so the user only has to catch a single exception.
                // Since we already throw ArgumentException when validating other arguments,
                // using that exception for failure to encode invalid UTF-16 chars as well.
                // Therefore, wrapping the EncoderFallbackException around an ArgumentException.
                throw ThrowHelper.GetArgumentException_ReadInvalidUTF16(ex);
            }
        }

        public static int GetUtf8FromText(ReadOnlySpan<char> text, Span<byte> dest)
        {
            try
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                return UTF8NoBOM.GetBytes(text, dest);
#else
                if (text.IsEmpty) { return 0; }

                unsafe
                {
                    fixed (char* charPtr = text)
                    fixed (byte* destPtr = dest)
                    {
                        return UTF8NoBOM.GetBytes(charPtr, text.Length, destPtr, dest.Length);
                    }
                }
#endif
            }
            catch (EncoderFallbackException ex)
            {
                // We want to be consistent with the exception being thrown
                // so the user only has to catch a single exception.
                // Since we already throw ArgumentException when validating other arguments,
                // using that exception for failure to encode invalid UTF-16 chars as well.
                // Therefore, wrapping the EncoderFallbackException around an ArgumentException.
                throw ThrowHelper.GetArgumentException_ReadInvalidUTF16(ex);
            }
        }

        public static string GetTextFromUtf8(ReadOnlySpan<byte> utf8Text)
        {
#if NET451
            return Utf8.ToString(utf8Text);
#else
            try
            {
#if NETCOREAPP || NETSTANDARD_2_0_GREATER
                return UTF8NoBOM.GetString(utf8Text);
#else
                if (utf8Text.IsEmpty) { return string.Empty; }

                unsafe
                {
                    fixed (byte* bytePtr = utf8Text)
                    {
                        return UTF8NoBOM.GetString(bytePtr, utf8Text.Length);
                    }
                }
#endif
            }
            catch (DecoderFallbackException ex)
            {
                // We want to be consistent with the exception being thrown
                // so the user only has to catch a single exception.
                // Since we already throw InvalidOperationException for mismatch token type,
                // and while unescaping, using that exception for failure to decode invalid UTF-8 bytes as well.
                // Therefore, wrapping the DecoderFallbackException around an InvalidOperationException.
                throw ThrowHelper.GetInvalidOperationException_ReadInvalidUTF8(ex);
            }
#endif
        }
    }
}
