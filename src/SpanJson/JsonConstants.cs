using System;

namespace SpanJson
{
    public static class JsonSharedConstant
    {
        public const int MaxNumberBufferSize = 32;
        public const int MaximumFormatDoubleLength = 128;  // default (i.e. 'G'), using 128 (rather than say 32) to be future-proof.
        public const int MaximumFormatSingleLength = 128;  // default (i.e. 'G'), using 128 (rather than say 32) to be future-proof.
        public const int MaximumFormatDecimalLength = 31; // default (i.e. 'G')
        public const int MaxVersionLength = 45; // 4 * int + 3 . + 2 double quote
        public const int NestingLimit = 256;

        public const uint TooBigOrNegative = int.MaxValue;

        public const uint ByteSize = sizeof(byte);
        public const uint CharSize = sizeof(char);
    }

    public static class JsonUtf8Constant
    {
        public const byte BeginArray = (byte) '[';
        public const byte BeginObject = (byte) '{';
        public const byte DoubleQuote = (byte) '"';
        public const byte EndArray = (byte) ']';
        public const byte EndObject = (byte) '}';
        public const byte False = (byte) 'f';
        public const byte NameSeparator = (byte) ':';
        public const byte Null = (byte) 'n';
        public const byte ReverseSolidus = (byte) '\\';
        public const byte Solidus = (byte) '/';
        public const byte String = (byte) '"';
        public const byte True = (byte) 't';
        public const byte ValueSeparator = (byte) ',';
        public const byte Space = (byte)' ';
        public const byte Plus = (byte)'+';
        public const byte Hyphen = (byte)'-';
        public const byte UtcOffsetToken = (byte)'Z';
        public const byte TimePrefix = (byte)'T';

        public const byte CarriageReturn = (byte)'\r';
        public const byte LineFeed = (byte)'\n';
        public const byte Tab = (byte)'\t';

        public static ReadOnlySpan<byte> NewLine => new[] {(byte) '\r', (byte) '\n'};
        public static ReadOnlySpan<byte> NullTerminator => new byte[] {0};
    }

    public static class JsonUtf16Constant
    {
        public const char BeginArray = '[';
        public const char BeginObject = '{';
        public const char DoubleQuote = '"';
        public const char EndArray = ']';
        public const char EndObject = '}';
        public const char False = 'f';
        public const char NameSeparator = ':';
        public const char Null = 'n';
        public const char ReverseSolidus = '\\';
        public const char Solidus = '/';
        public const char String = '"';
        public const char True = 't';
        public const char ValueSeparator = ',';
        public const char Space = ' ';
        public const char Plus = '+';
        public const char Hyphen = '-';
        public const char UtcOffsetToken = 'Z';
        public const char TimePrefix = 'T';

        public const char CarriageReturn = '\r';
        public const char LineFeed = '\n';
        public const char Tab = '\t';

        public static ReadOnlySpan<char> NewLine => new[] {'\r', '\n'};
        public static ReadOnlySpan<char> NullTerminator => new[] {'\0'};
    }
}