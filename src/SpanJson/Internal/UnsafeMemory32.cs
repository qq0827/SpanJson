
namespace SpanJson.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    partial class UnsafeMemory32
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteRaw<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            var nCount = (uint)source.Length;
            switch (nCount)
            {
                case 0u: return;
                case 1u: WriteRaw1(ref writer, source, ref idx); return;
                case 2u: WriteRaw2(ref writer, source, ref idx); return;
                case 3u: WriteRaw3(ref writer, source, ref idx); return;
                case 4u: WriteRaw4(ref writer, source, ref idx); return;
                case 5u: WriteRaw5(ref writer, source, ref idx); return;
                case 6u: WriteRaw6(ref writer, source, ref idx); return;
                case 7u: WriteRaw7(ref writer, source, ref idx); return;
                case 8u: WriteRaw8(ref writer, source, ref idx); return;
                case 9u: WriteRaw9(ref writer, source, ref idx); return;
                case 10u: WriteRaw10(ref writer, source, ref idx); return;
                case 11u: WriteRaw11(ref writer, source, ref idx); return;
                case 12u: WriteRaw12(ref writer, source, ref idx); return;
                case 13u: WriteRaw13(ref writer, source, ref idx); return;
                case 14u: WriteRaw14(ref writer, source, ref idx); return;
                case 15u: WriteRaw15(ref writer, source, ref idx); return;
                case 16u: WriteRaw16(ref writer, source, ref idx); return;
                case 17u: WriteRaw17(ref writer, source, ref idx); return;
                case 18u: WriteRaw18(ref writer, source, ref idx); return;
                case 19u: WriteRaw19(ref writer, source, ref idx); return;
                case 20u: WriteRaw20(ref writer, source, ref idx); return;
                case 21u: WriteRaw21(ref writer, source, ref idx); return;
                case 22u: WriteRaw22(ref writer, source, ref idx); return;
                case 23u: WriteRaw23(ref writer, source, ref idx); return;
                case 24u: WriteRaw24(ref writer, source, ref idx); return;
                case 25u: WriteRaw25(ref writer, source, ref idx); return;
                case 26u: WriteRaw26(ref writer, source, ref idx); return;
                case 27u: WriteRaw27(ref writer, source, ref idx); return;
                case 28u: WriteRaw28(ref writer, source, ref idx); return;
                case 29u: WriteRaw29(ref writer, source, ref idx); return;
                case 30u: WriteRaw30(ref writer, source, ref idx); return;
                case 31u: WriteRaw31(ref writer, source, ref idx); return;
                case 32u: WriteRaw32(ref writer, source, ref idx); return;
                default: UnsafeMemory.WriteRawBytes(ref writer, source, ref idx); return;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteRawUnsafe(ref byte destination, ref byte source, int sourceBytesToCopy, ref int idx)
        {
            var nCount = (uint)sourceBytesToCopy;
            switch (nCount)
            {
                case 0u: return;
                case 1u: WriteRawUnsafe1(ref destination, ref source, ref idx); return;
                case 2u: WriteRawUnsafe2(ref destination, ref source, ref idx); return;
                case 3u: WriteRawUnsafe3(ref destination, ref source, ref idx); return;
                case 4u: WriteRawUnsafe4(ref destination, ref source, ref idx); return;
                case 5u: WriteRawUnsafe5(ref destination, ref source, ref idx); return;
                case 6u: WriteRawUnsafe6(ref destination, ref source, ref idx); return;
                case 7u: WriteRawUnsafe7(ref destination, ref source, ref idx); return;
                case 8u: WriteRawUnsafe8(ref destination, ref source, ref idx); return;
                case 9u: WriteRawUnsafe9(ref destination, ref source, ref idx); return;
                case 10u: WriteRawUnsafe10(ref destination, ref source, ref idx); return;
                case 11u: WriteRawUnsafe11(ref destination, ref source, ref idx); return;
                case 12u: WriteRawUnsafe12(ref destination, ref source, ref idx); return;
                case 13u: WriteRawUnsafe13(ref destination, ref source, ref idx); return;
                case 14u: WriteRawUnsafe14(ref destination, ref source, ref idx); return;
                case 15u: WriteRawUnsafe15(ref destination, ref source, ref idx); return;
                case 16u: WriteRawUnsafe16(ref destination, ref source, ref idx); return;
                case 17u: WriteRawUnsafe17(ref destination, ref source, ref idx); return;
                case 18u: WriteRawUnsafe18(ref destination, ref source, ref idx); return;
                case 19u: WriteRawUnsafe19(ref destination, ref source, ref idx); return;
                case 20u: WriteRawUnsafe20(ref destination, ref source, ref idx); return;
                case 21u: WriteRawUnsafe21(ref destination, ref source, ref idx); return;
                case 22u: WriteRawUnsafe22(ref destination, ref source, ref idx); return;
                case 23u: WriteRawUnsafe23(ref destination, ref source, ref idx); return;
                case 24u: WriteRawUnsafe24(ref destination, ref source, ref idx); return;
                case 25u: WriteRawUnsafe25(ref destination, ref source, ref idx); return;
                case 26u: WriteRawUnsafe26(ref destination, ref source, ref idx); return;
                case 27u: WriteRawUnsafe27(ref destination, ref source, ref idx); return;
                case 28u: WriteRawUnsafe28(ref destination, ref source, ref idx); return;
                case 29u: WriteRawUnsafe29(ref destination, ref source, ref idx); return;
                case 30u: WriteRawUnsafe30(ref destination, ref source, ref idx); return;
                case 31u: WriteRawUnsafe31(ref destination, ref source, ref idx); return;
                case 32u: WriteRawUnsafe32(ref destination, ref source, ref idx); return;
                default: UnsafeMemory.WriteRawBytesUnsafe(ref destination, ref source, sourceBytesToCopy, ref idx); return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw2<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 2);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, short>(ref dest) = Unsafe.As<byte, short>(ref src);

            idx += 2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw3<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 3);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, short>(ref dest) = Unsafe.As<byte, short>(ref src);
            Unsafe.AddByteOffset(ref dest, (IntPtr)2) = Unsafe.AddByteOffset(ref src, (IntPtr)2);

            idx += 3;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw4<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 4);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);

            idx += 4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw5<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 5);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.AddByteOffset(ref dest, (IntPtr)4) = Unsafe.AddByteOffset(ref src, (IntPtr)4);

            idx += 5;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw6<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 6);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 4));

            idx += 6;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw7<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 7);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 4));
            Unsafe.AddByteOffset(ref dest, (IntPtr)6) = Unsafe.AddByteOffset(ref src, (IntPtr)6);

            idx += 7;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw8<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 8);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));

            idx += 8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw9<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 9);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.AddByteOffset(ref dest, (IntPtr)8) = Unsafe.AddByteOffset(ref src, (IntPtr)8);

            idx += 9;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw10<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 10);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 8));

            idx += 10;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw11<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 11);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 8));
            Unsafe.AddByteOffset(ref dest, (IntPtr)10) = Unsafe.AddByteOffset(ref src, (IntPtr)10);

            idx += 11;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw12<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 12);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));

            idx += 12;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw13<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 13);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.AddByteOffset(ref dest, (IntPtr)12) = Unsafe.AddByteOffset(ref src, (IntPtr)12);

            idx += 13;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw14<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 14);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 12));

            idx += 14;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw15<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 15);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 12));
            Unsafe.AddByteOffset(ref dest, (IntPtr)14) = Unsafe.AddByteOffset(ref src, (IntPtr)14);

            idx += 15;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw16<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 16);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));

            idx += 16;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw17<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 17);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.AddByteOffset(ref dest, (IntPtr)16) = Unsafe.AddByteOffset(ref src, (IntPtr)16);

            idx += 17;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw18<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 18);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 16));

            idx += 18;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw19<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 19);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 16));
            Unsafe.AddByteOffset(ref dest, (IntPtr)18) = Unsafe.AddByteOffset(ref src, (IntPtr)18);

            idx += 19;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw20<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 20);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));

            idx += 20;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw21<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 21);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.AddByteOffset(ref dest, (IntPtr)20) = Unsafe.AddByteOffset(ref src, (IntPtr)20);

            idx += 21;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw22<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 22);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 20));

            idx += 22;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw23<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 23);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 20));
            Unsafe.AddByteOffset(ref dest, (IntPtr)22) = Unsafe.AddByteOffset(ref src, (IntPtr)22);

            idx += 23;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw24<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 24);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));

            idx += 24;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw25<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 25);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.AddByteOffset(ref dest, (IntPtr)24) = Unsafe.AddByteOffset(ref src, (IntPtr)24);

            idx += 25;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw26<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 26);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 24));

            idx += 26;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw27<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 27);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 24));
            Unsafe.AddByteOffset(ref dest, (IntPtr)26) = Unsafe.AddByteOffset(ref src, (IntPtr)26);

            idx += 27;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw28<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 28);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));

            idx += 28;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw29<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 29);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.AddByteOffset(ref dest, (IntPtr)28) = Unsafe.AddByteOffset(ref src, (IntPtr)28);

            idx += 29;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw30<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 30);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 28)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 28));

            idx += 30;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw31<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 31);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 28)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 28));
            Unsafe.AddByteOffset(ref dest, (IntPtr)30) = Unsafe.AddByteOffset(ref src, (IntPtr)30);

            idx += 31;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw32<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 32);

            ref byte dest = ref Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx);
            ref byte src = ref source[0];

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 28)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 28));

            idx += 32;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe2(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, short>(ref dest) = Unsafe.As<byte, short>(ref src);

            idx += 2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe3(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, short>(ref dest) = Unsafe.As<byte, short>(ref src);
            Unsafe.AddByteOffset(ref dest, (IntPtr)2) = Unsafe.AddByteOffset(ref src, (IntPtr)2);

            idx += 3;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe4(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);

            idx += 4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe5(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.AddByteOffset(ref dest, (IntPtr)4) = Unsafe.AddByteOffset(ref src, (IntPtr)4);

            idx += 5;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe6(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 4));

            idx += 6;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe7(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 4));
            Unsafe.AddByteOffset(ref dest, (IntPtr)6) = Unsafe.AddByteOffset(ref src, (IntPtr)6);

            idx += 7;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe8(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));

            idx += 8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe9(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.AddByteOffset(ref dest, (IntPtr)8) = Unsafe.AddByteOffset(ref src, (IntPtr)8);

            idx += 9;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe10(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 8));

            idx += 10;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe11(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 8));
            Unsafe.AddByteOffset(ref dest, (IntPtr)10) = Unsafe.AddByteOffset(ref src, (IntPtr)10);

            idx += 11;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe12(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));

            idx += 12;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe13(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.AddByteOffset(ref dest, (IntPtr)12) = Unsafe.AddByteOffset(ref src, (IntPtr)12);

            idx += 13;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe14(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 12));

            idx += 14;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe15(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 12));
            Unsafe.AddByteOffset(ref dest, (IntPtr)14) = Unsafe.AddByteOffset(ref src, (IntPtr)14);

            idx += 15;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe16(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));

            idx += 16;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe17(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.AddByteOffset(ref dest, (IntPtr)16) = Unsafe.AddByteOffset(ref src, (IntPtr)16);

            idx += 17;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe18(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 16));

            idx += 18;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe19(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 16));
            Unsafe.AddByteOffset(ref dest, (IntPtr)18) = Unsafe.AddByteOffset(ref src, (IntPtr)18);

            idx += 19;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe20(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));

            idx += 20;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe21(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.AddByteOffset(ref dest, (IntPtr)20) = Unsafe.AddByteOffset(ref src, (IntPtr)20);

            idx += 21;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe22(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 20));

            idx += 22;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe23(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 20));
            Unsafe.AddByteOffset(ref dest, (IntPtr)22) = Unsafe.AddByteOffset(ref src, (IntPtr)22);

            idx += 23;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe24(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));

            idx += 24;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe25(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.AddByteOffset(ref dest, (IntPtr)24) = Unsafe.AddByteOffset(ref src, (IntPtr)24);

            idx += 25;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe26(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 24));

            idx += 26;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe27(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 24));
            Unsafe.AddByteOffset(ref dest, (IntPtr)26) = Unsafe.AddByteOffset(ref src, (IntPtr)26);

            idx += 27;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe28(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));

            idx += 28;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe29(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.AddByteOffset(ref dest, (IntPtr)28) = Unsafe.AddByteOffset(ref src, (IntPtr)28);

            idx += 29;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe30(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 28)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 28));

            idx += 30;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe31(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, short>(ref Unsafe.Add(ref dest, 28)) = Unsafe.As<byte, short>(ref Unsafe.Add(ref src, 28));
            Unsafe.AddByteOffset(ref dest, (IntPtr)30) = Unsafe.AddByteOffset(ref src, (IntPtr)30);

            idx += 31;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRawUnsafe32(ref byte destination, ref byte src, ref int idx)
        {
            ref byte dest = ref Unsafe.Add(ref destination, (IntPtr)(uint)idx);

            Unsafe.As<byte, int>(ref dest) = Unsafe.As<byte, int>(ref src);
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 4)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 4));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 8)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 8));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 12)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 12));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 16)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 16));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 20)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 20));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 24)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 24));
            Unsafe.As<byte, int>(ref Unsafe.Add(ref dest, 28)) = Unsafe.As<byte, int>(ref Unsafe.Add(ref src, 28));

            idx += 32;
        }
    }
}
