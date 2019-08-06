namespace SpanJson.Internal
{
    using System;
    using System.Text;

    public static partial class TextEncodings
    {
        // Reject any invalid UTF-8 data rather than silently replacing.
        public static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    }
}
