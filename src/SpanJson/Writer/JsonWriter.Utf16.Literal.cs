namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using SpanJson.Internal;

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

        public void WriteUtf16Base64String(in ReadOnlySpan<byte> bytes)
        {
            ref var pos = ref _pos;
            int charLengthRequired = Base64Helper.ToBase64_CalculateAndValidateOutputLength(bytes.Length, false);
            EnsureUnsafe(pos, charLengthRequired + 3);

            ref char pinnableAddr = ref Utf16PinnableAddress;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);

            if (!bytes.IsEmpty)
            {
                unsafe
                {
                    fixed (char* outChars = &Unsafe.Add(ref pinnableAddr, pos))
                    fixed (byte* inData = &MemoryMarshal.GetReference(bytes))
                    {
                        var written = Base64Helper.ConvertToBase64Array(outChars, inData, 0, bytes.Length, false);
                        pos += written;
                    }
                }
            }

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }
    }
}
