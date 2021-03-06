﻿// Largely based on https://github.com/neuecc/Utf8Json/blob/master/src/Utf8Json/Internal/GuidBits.cs

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanJson.Internal
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct GuidBits
    {
        [FieldOffset(0)]
        public readonly Guid Value;

        [FieldOffset(0)]
        public readonly byte Byte0;
        [FieldOffset(1)]
        public readonly byte Byte1;
        [FieldOffset(2)]
        public readonly byte Byte2;
        [FieldOffset(3)]
        public readonly byte Byte3;
        [FieldOffset(4)]
        public readonly byte Byte4;
        [FieldOffset(5)]
        public readonly byte Byte5;
        [FieldOffset(6)]
        public readonly byte Byte6;
        [FieldOffset(7)]
        public readonly byte Byte7;
        [FieldOffset(8)]
        public readonly byte Byte8;
        [FieldOffset(9)]
        public readonly byte Byte9;
        [FieldOffset(10)]
        public readonly byte Byte10;
        [FieldOffset(11)]
        public readonly byte Byte11;
        [FieldOffset(12)]
        public readonly byte Byte12;
        [FieldOffset(13)]
        public readonly byte Byte13;
        [FieldOffset(14)]
        public readonly byte Byte14;
        [FieldOffset(15)]
        public readonly byte Byte15;

        public GuidBits(ref Guid value)
        {
            this = default(GuidBits);
            this.Value = value;
        }

        // 4-pattern, lower/upper and '-' or no
        public GuidBits(ref ArraySegment<byte> utf8string)
        {
            this = default(GuidBits);

            var array = utf8string.Array;
            var offset = utf8string.Offset;

            // 32
            if (utf8string.Count == 32)
            {
                if (BitConverter.IsLittleEndian)
                {
                    this.Byte0 = Parse(array, offset + 6);
                    this.Byte1 = Parse(array, offset + 4);
                    this.Byte2 = Parse(array, offset + 2);
                    this.Byte3 = Parse(array, offset + 0);

                    this.Byte4 = Parse(array, offset + 10);
                    this.Byte5 = Parse(array, offset + 8);

                    this.Byte6 = Parse(array, offset + 14);
                    this.Byte7 = Parse(array, offset + 12);
                }
                else
                {
                    this.Byte0 = Parse(array, offset + 0);
                    this.Byte1 = Parse(array, offset + 2);
                    this.Byte2 = Parse(array, offset + 4);
                    this.Byte3 = Parse(array, offset + 6);

                    this.Byte4 = Parse(array, offset + 8);
                    this.Byte5 = Parse(array, offset + 10);

                    this.Byte6 = Parse(array, offset + 12);
                    this.Byte7 = Parse(array, offset + 14);
                }
                this.Byte8 = Parse(array, offset + 16);
                this.Byte9 = Parse(array, offset + 18);

                this.Byte10 = Parse(array, offset + 20);
                this.Byte11 = Parse(array, offset + 22);
                this.Byte12 = Parse(array, offset + 24);
                this.Byte13 = Parse(array, offset + 26);
                this.Byte14 = Parse(array, offset + 28);
                this.Byte15 = Parse(array, offset + 30);
                return;
            }
            else if (utf8string.Count == 36)
            {
                // '-' => 45
                if (BitConverter.IsLittleEndian)
                {
                    this.Byte0 = Parse(array, offset + 6);
                    this.Byte1 = Parse(array, offset + 4);
                    this.Byte2 = Parse(array, offset + 2);
                    this.Byte3 = Parse(array, offset + 0);

                    if (array[offset + 8] != '-') goto ERROR;

                    this.Byte4 = Parse(array, offset + 11);
                    this.Byte5 = Parse(array, offset + 9);

                    if (array[offset + 13] != '-') goto ERROR;

                    this.Byte6 = Parse(array, offset + 16);
                    this.Byte7 = Parse(array, offset + 14);
                }
                else
                {
                    this.Byte0 = Parse(array, offset + 0);
                    this.Byte1 = Parse(array, offset + 2);
                    this.Byte2 = Parse(array, offset + 4);
                    this.Byte3 = Parse(array, offset + 6);

                    if (array[offset + 8] != '-') goto ERROR;

                    this.Byte4 = Parse(array, offset + 9);
                    this.Byte5 = Parse(array, offset + 11);

                    if (array[offset + 13] != '-') goto ERROR;

                    this.Byte6 = Parse(array, offset + 14);
                    this.Byte7 = Parse(array, offset + 16);
                }

                if (array[offset + 18] != '-') goto ERROR;

                this.Byte8 = Parse(array, offset + 19);
                this.Byte9 = Parse(array, offset + 21);

                if (array[offset + 23] != '-') goto ERROR;

                this.Byte10 = Parse(array, offset + 24);
                this.Byte11 = Parse(array, offset + 26);
                this.Byte12 = Parse(array, offset + 28);
                this.Byte13 = Parse(array, offset + 30);
                this.Byte14 = Parse(array, offset + 32);
                this.Byte15 = Parse(array, offset + 34);
                return;
            }

            ERROR:
            ThrowHelper.ThrowArgumentException_Guid_Pattern();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte Parse(byte[] bytes, int highOffset)
        {
            return unchecked((byte)(SwitchParse(bytes[highOffset]) * 16 + SwitchParse(bytes[highOffset + 1])));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte SwitchParse(byte b)
        {
            // '0'(48) ~ '9'(57) => -48
            // 'A'(65) ~ 'F'(70) => -55
            // 'a'(97) ~ 'f'(102) => -87
            switch (b)
            {
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                    return unchecked((byte)((b - 48)));
                case 65:
                case 66:
                case 67:
                case 68:
                case 69:
                case 70:
                    return unchecked((byte)((b - 55)));
                case 97:
                case 98:
                case 99:
                case 100:
                case 101:
                case 102:
                    return unchecked((byte)((b - 87)));
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 37:
                case 38:
                case 39:
                case 40:
                case 41:
                case 42:
                case 43:
                case 44:
                case 45:
                case 46:
                case 47:
                case 58:
                case 59:
                case 60:
                case 61:
                case 62:
                case 63:
                case 64:
                case 71:
                case 72:
                case 73:
                case 74:
                case 75:
                case 76:
                case 77:
                case 78:
                case 79:
                case 80:
                case 81:
                case 82:
                case 83:
                case 84:
                case 85:
                case 86:
                case 87:
                case 88:
                case 89:
                case 90:
                case 91:
                case 92:
                case 93:
                case 94:
                case 95:
                case 96:
                default:
                    throw ThrowHelper.GetArgumentException_Guid_Pattern();
            }
        }

        // 4(x2) - 2(x2) - 2(x2) - 2(x2) - 6(x2)
        public void Write(ref byte destination, ref int pos)
        {
            var offset = (IntPtr)pos;
            if (BitConverter.IsLittleEndian)
            {
                // int(_a)
                Unsafe.AddByteOffset(ref destination, offset + 6) = JsonHelpers.ByteToHexStringHigh[Byte0];
                Unsafe.AddByteOffset(ref destination, offset + 7) = JsonHelpers.ByteToHexStringLow[Byte0];
                Unsafe.AddByteOffset(ref destination, offset + 4) = JsonHelpers.ByteToHexStringHigh[Byte1];
                Unsafe.AddByteOffset(ref destination, offset + 5) = JsonHelpers.ByteToHexStringLow[Byte1];
                Unsafe.AddByteOffset(ref destination, offset + 2) = JsonHelpers.ByteToHexStringHigh[Byte2];
                Unsafe.AddByteOffset(ref destination, offset + 3) = JsonHelpers.ByteToHexStringLow[Byte2];
                Unsafe.AddByteOffset(ref destination, offset) = JsonHelpers.ByteToHexStringHigh[Byte3];
                Unsafe.AddByteOffset(ref destination, offset + 1) = JsonHelpers.ByteToHexStringLow[Byte3];

                Unsafe.AddByteOffset(ref destination, offset + 8) = (byte)'-';

                // short(_b)
                Unsafe.AddByteOffset(ref destination, offset + 11) = JsonHelpers.ByteToHexStringHigh[Byte4];
                Unsafe.AddByteOffset(ref destination, offset + 12) = JsonHelpers.ByteToHexStringLow[Byte4];
                Unsafe.AddByteOffset(ref destination, offset + 9) = JsonHelpers.ByteToHexStringHigh[Byte5];
                Unsafe.AddByteOffset(ref destination, offset + 10) = JsonHelpers.ByteToHexStringLow[Byte5];

                Unsafe.AddByteOffset(ref destination, offset + 13) = (byte)'-';

                // short(_c)
                Unsafe.AddByteOffset(ref destination, offset + 16) = JsonHelpers.ByteToHexStringHigh[Byte6];
                Unsafe.AddByteOffset(ref destination, offset + 17) = JsonHelpers.ByteToHexStringLow[Byte6];
                Unsafe.AddByteOffset(ref destination, offset + 14) = JsonHelpers.ByteToHexStringHigh[Byte7];
                Unsafe.AddByteOffset(ref destination, offset + 15) = JsonHelpers.ByteToHexStringLow[Byte7];
            }
            else
            {
                Unsafe.AddByteOffset(ref destination, offset) = JsonHelpers.ByteToHexStringHigh[Byte0];
                Unsafe.AddByteOffset(ref destination, offset + 1) = JsonHelpers.ByteToHexStringLow[Byte0];
                Unsafe.AddByteOffset(ref destination, offset + 2) = JsonHelpers.ByteToHexStringHigh[Byte1];
                Unsafe.AddByteOffset(ref destination, offset + 3) = JsonHelpers.ByteToHexStringLow[Byte1];
                Unsafe.AddByteOffset(ref destination, offset + 4) = JsonHelpers.ByteToHexStringHigh[Byte2];
                Unsafe.AddByteOffset(ref destination, offset + 5) = JsonHelpers.ByteToHexStringLow[Byte2];
                Unsafe.AddByteOffset(ref destination, offset + 6) = JsonHelpers.ByteToHexStringHigh[Byte3];
                Unsafe.AddByteOffset(ref destination, offset + 7) = JsonHelpers.ByteToHexStringLow[Byte3];

                Unsafe.AddByteOffset(ref destination, offset + 8) = (byte)'-';

                Unsafe.AddByteOffset(ref destination, offset + 9) = JsonHelpers.ByteToHexStringHigh[Byte4];
                Unsafe.AddByteOffset(ref destination, offset + 10) = JsonHelpers.ByteToHexStringLow[Byte4];
                Unsafe.AddByteOffset(ref destination, offset + 11) = JsonHelpers.ByteToHexStringHigh[Byte5];
                Unsafe.AddByteOffset(ref destination, offset + 12) = JsonHelpers.ByteToHexStringLow[Byte5];

                Unsafe.AddByteOffset(ref destination, offset + 13) = (byte)'-';

                Unsafe.AddByteOffset(ref destination, offset + 14) = JsonHelpers.ByteToHexStringHigh[Byte6];
                Unsafe.AddByteOffset(ref destination, offset + 15) = JsonHelpers.ByteToHexStringLow[Byte6];
                Unsafe.AddByteOffset(ref destination, offset + 16) = JsonHelpers.ByteToHexStringHigh[Byte7];
                Unsafe.AddByteOffset(ref destination, offset + 17) = JsonHelpers.ByteToHexStringLow[Byte7];
            }

            Unsafe.AddByteOffset(ref destination, offset + 18) = (byte)'-';

            Unsafe.AddByteOffset(ref destination, offset + 19) = JsonHelpers.ByteToHexStringHigh[Byte8];
            Unsafe.AddByteOffset(ref destination, offset + 20) = JsonHelpers.ByteToHexStringLow[Byte8];
            Unsafe.AddByteOffset(ref destination, offset + 21) = JsonHelpers.ByteToHexStringHigh[Byte9];
            Unsafe.AddByteOffset(ref destination, offset + 22) = JsonHelpers.ByteToHexStringLow[Byte9];

            Unsafe.AddByteOffset(ref destination, offset + 23) = (byte)'-';

            Unsafe.AddByteOffset(ref destination, offset + 24) = JsonHelpers.ByteToHexStringHigh[Byte10];
            Unsafe.AddByteOffset(ref destination, offset + 25) = JsonHelpers.ByteToHexStringLow[Byte10];
            Unsafe.AddByteOffset(ref destination, offset + 26) = JsonHelpers.ByteToHexStringHigh[Byte11];
            Unsafe.AddByteOffset(ref destination, offset + 27) = JsonHelpers.ByteToHexStringLow[Byte11];
            Unsafe.AddByteOffset(ref destination, offset + 28) = JsonHelpers.ByteToHexStringHigh[Byte12];
            Unsafe.AddByteOffset(ref destination, offset + 29) = JsonHelpers.ByteToHexStringLow[Byte12];
            Unsafe.AddByteOffset(ref destination, offset + 30) = JsonHelpers.ByteToHexStringHigh[Byte13];
            Unsafe.AddByteOffset(ref destination, offset + 31) = JsonHelpers.ByteToHexStringLow[Byte13];
            Unsafe.AddByteOffset(ref destination, offset + 32) = JsonHelpers.ByteToHexStringHigh[Byte14];
            Unsafe.AddByteOffset(ref destination, offset + 33) = JsonHelpers.ByteToHexStringLow[Byte14];
            Unsafe.AddByteOffset(ref destination, offset + 34) = JsonHelpers.ByteToHexStringHigh[Byte15];
            Unsafe.AddByteOffset(ref destination, offset + 35) = JsonHelpers.ByteToHexStringLow[Byte15];

            pos += 36;
        }

        public void Write(ref char destination, ref int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                // int(_a)
                Unsafe.Add(ref destination, offset + 6) = JsonHelpers.CharToHexStringHigh[Byte0];
                Unsafe.Add(ref destination, offset + 7) = JsonHelpers.CharToHexStringLow[Byte0];
                Unsafe.Add(ref destination, offset + 4) = JsonHelpers.CharToHexStringHigh[Byte1];
                Unsafe.Add(ref destination, offset + 5) = JsonHelpers.CharToHexStringLow[Byte1];
                Unsafe.Add(ref destination, offset + 2) = JsonHelpers.CharToHexStringHigh[Byte2];
                Unsafe.Add(ref destination, offset + 3) = JsonHelpers.CharToHexStringLow[Byte2];
                Unsafe.Add(ref destination, offset) = JsonHelpers.CharToHexStringHigh[Byte3];
                Unsafe.Add(ref destination, offset + 1) = JsonHelpers.CharToHexStringLow[Byte3];

                Unsafe.Add(ref destination, offset + 8) = '-';

                // short(_b)
                Unsafe.Add(ref destination, offset + 11) = JsonHelpers.CharToHexStringHigh[Byte4];
                Unsafe.Add(ref destination, offset + 12) = JsonHelpers.CharToHexStringLow[Byte4];
                Unsafe.Add(ref destination, offset + 9) = JsonHelpers.CharToHexStringHigh[Byte5];
                Unsafe.Add(ref destination, offset + 10) = JsonHelpers.CharToHexStringLow[Byte5];

                Unsafe.Add(ref destination, offset + 13) = '-';

                // short(_c)
                Unsafe.Add(ref destination, offset + 16) = JsonHelpers.CharToHexStringHigh[Byte6];
                Unsafe.Add(ref destination, offset + 17) = JsonHelpers.CharToHexStringLow[Byte6];
                Unsafe.Add(ref destination, offset + 14) = JsonHelpers.CharToHexStringHigh[Byte7];
                Unsafe.Add(ref destination, offset + 15) = JsonHelpers.CharToHexStringLow[Byte7];
            }
            else
            {
                Unsafe.Add(ref destination, offset) = JsonHelpers.CharToHexStringHigh[Byte0];
                Unsafe.Add(ref destination, offset + 1) = JsonHelpers.CharToHexStringLow[Byte0];
                Unsafe.Add(ref destination, offset + 2) = JsonHelpers.CharToHexStringHigh[Byte1];
                Unsafe.Add(ref destination, offset + 3) = JsonHelpers.CharToHexStringLow[Byte1];
                Unsafe.Add(ref destination, offset + 4) = JsonHelpers.CharToHexStringHigh[Byte2];
                Unsafe.Add(ref destination, offset + 5) = JsonHelpers.CharToHexStringLow[Byte2];
                Unsafe.Add(ref destination, offset + 6) = JsonHelpers.CharToHexStringHigh[Byte3];
                Unsafe.Add(ref destination, offset + 7) = JsonHelpers.CharToHexStringLow[Byte3];

                Unsafe.Add(ref destination, offset + 8) = '-';

                Unsafe.Add(ref destination, offset + 9) = JsonHelpers.CharToHexStringHigh[Byte4];
                Unsafe.Add(ref destination, offset + 10) = JsonHelpers.CharToHexStringLow[Byte4];
                Unsafe.Add(ref destination, offset + 11) = JsonHelpers.CharToHexStringHigh[Byte5];
                Unsafe.Add(ref destination, offset + 12) = JsonHelpers.CharToHexStringLow[Byte5];

                Unsafe.Add(ref destination, offset + 13) = '-';

                Unsafe.Add(ref destination, offset + 14) = JsonHelpers.CharToHexStringHigh[Byte6];
                Unsafe.Add(ref destination, offset + 15) = JsonHelpers.CharToHexStringLow[Byte6];
                Unsafe.Add(ref destination, offset + 16) = JsonHelpers.CharToHexStringHigh[Byte7];
                Unsafe.Add(ref destination, offset + 17) = JsonHelpers.CharToHexStringLow[Byte7];
            }

            Unsafe.Add(ref destination, offset + 18) = '-';

            Unsafe.Add(ref destination, offset + 19) = JsonHelpers.CharToHexStringHigh[Byte8];
            Unsafe.Add(ref destination, offset + 20) = JsonHelpers.CharToHexStringLow[Byte8];
            Unsafe.Add(ref destination, offset + 21) = JsonHelpers.CharToHexStringHigh[Byte9];
            Unsafe.Add(ref destination, offset + 22) = JsonHelpers.CharToHexStringLow[Byte9];

            Unsafe.Add(ref destination, offset + 23) = '-';

            Unsafe.Add(ref destination, offset + 24) = JsonHelpers.CharToHexStringHigh[Byte10];
            Unsafe.Add(ref destination, offset + 25) = JsonHelpers.CharToHexStringLow[Byte10];
            Unsafe.Add(ref destination, offset + 26) = JsonHelpers.CharToHexStringHigh[Byte11];
            Unsafe.Add(ref destination, offset + 27) = JsonHelpers.CharToHexStringLow[Byte11];
            Unsafe.Add(ref destination, offset + 28) = JsonHelpers.CharToHexStringHigh[Byte12];
            Unsafe.Add(ref destination, offset + 29) = JsonHelpers.CharToHexStringLow[Byte12];
            Unsafe.Add(ref destination, offset + 30) = JsonHelpers.CharToHexStringHigh[Byte13];
            Unsafe.Add(ref destination, offset + 31) = JsonHelpers.CharToHexStringLow[Byte13];
            Unsafe.Add(ref destination, offset + 32) = JsonHelpers.CharToHexStringHigh[Byte14];
            Unsafe.Add(ref destination, offset + 33) = JsonHelpers.CharToHexStringLow[Byte14];
            Unsafe.Add(ref destination, offset + 34) = JsonHelpers.CharToHexStringHigh[Byte15];
            Unsafe.Add(ref destination, offset + 35) = JsonHelpers.CharToHexStringLow[Byte15];

            offset += 36;
        }
    }
}