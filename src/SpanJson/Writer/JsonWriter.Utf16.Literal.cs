namespace SpanJson
{
    using System.Runtime.CompilerServices;

    partial struct JsonWriter<TSymbol>
    {
        public void WriteUtf16Boolean(bool value)
        {
            const int trueLength = 4;
            const int falseLength = 5;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, falseLength);

            ref char pinnableAddr = ref Utf16PinnableAddress;
            if (value)
            {
                Unsafe.Add(ref pinnableAddr, pos + 3) = 'e';
                Unsafe.Add(ref pinnableAddr, pos + 2) = 'u';
                Unsafe.Add(ref pinnableAddr, pos + 1) = 'r';
                Unsafe.Add(ref pinnableAddr, pos) = JsonUtf16Constant.True;
                pos += trueLength;
            }
            else
            {
                Unsafe.Add(ref pinnableAddr, pos + 4) = 'e';
                Unsafe.Add(ref pinnableAddr, pos + 3) = 's';
                Unsafe.Add(ref pinnableAddr, pos + 2) = 'l';
                Unsafe.Add(ref pinnableAddr, pos + 1) = 'a';
                Unsafe.Add(ref pinnableAddr, pos) = JsonUtf16Constant.False;
                pos += falseLength;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Null()
        {
            const int nullLength = 4;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, nullLength);

            ref char pinnableAddr = ref Utf16PinnableAddress;
            Unsafe.Add(ref pinnableAddr, pos + 3) = 'l';
            Unsafe.Add(ref pinnableAddr, pos + 2) = 'l';
            Unsafe.Add(ref pinnableAddr, pos + 1) = 'u';
            Unsafe.Add(ref pinnableAddr, pos) = JsonUtf16Constant.Null;
            pos += nullLength;
        }
    }
}
