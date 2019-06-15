namespace SpanJson
{
    using System.Runtime.CompilerServices;

    partial struct JsonWriter<TSymbol>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Boolean(bool value)
        {
            if (value)
            {
                WriteUtf8Verbatim(0x65757274); // True
            }
            else
            {
                WriteUtf8Verbatim(0x736C6166, 0x65); // False
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Null()
        {
            WriteUtf8Verbatim(0x6C6C756E);
        }
    }
}
