using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanJson
{
    partial struct JsonReader<TSymbol>
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowJsonParserException(JsonParserException.ParserError error, JsonParserException.ValueType type, int position)
        {
            throw GetJsonParserException(error, type, position);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static JsonParserException GetJsonParserException(JsonParserException.ParserError error, JsonParserException.ValueType type, int position)
        {
            return new JsonParserException(error, type, position);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowJsonParserException(JsonParserException.ParserError error, int position)
        {
            throw GetJsonParserException(error, position);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static JsonParserException GetJsonParserException(JsonParserException.ParserError error, int position)
        {
            return new JsonParserException(error, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNumericSymbol(uint nValue)
        {
            switch (nValue)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '+':
                case '-':
                case '.':
                case 'E':
                case 'e':
                    return true;
                default:
                    return false;
            }
        }
    }
}