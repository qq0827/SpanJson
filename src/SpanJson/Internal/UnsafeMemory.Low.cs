namespace SpanJson.Internal
{
    using System;
    using System.Runtime.CompilerServices;

    // for string key property name write optimization.

    internal static class UnsafeMemory
    {
        public static readonly bool Is64BitProcess = IntPtr.Size >= 8;

        // 直接引用 byte[], 节省 byte[] => ReadOnlySpan<byte> 的转换
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            if (Is64BitProcess)
            {
                UnsafeMemory64.WriteRaw(ref writer, source, ref idx);
            }
            else
            {
                UnsafeMemory32.WriteRaw(ref writer, source, ref idx);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw(ref byte destination, ref byte source, int sourceBytesToCopy, ref int idx)
        {
            if (Is64BitProcess)
            {
                UnsafeMemory64.WriteRaw(ref destination, ref source, sourceBytesToCopy, ref idx);
            }
            else
            {
                UnsafeMemory32.WriteRaw(ref destination, ref source, sourceBytesToCopy, ref idx);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteRawBytes<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            var count = source.Length;
            writer.Ensure(idx, count);
            BinaryUtil.CopyMemory(source, 0, writer._utf8Buffer, idx, count);
            idx += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteRawBytes(ref byte destination, ref byte source, int sourceBytesToCopy, ref int idx)
        {
            //if (0u >= (uint)sourceBytesToCopy) { return; }

            BinaryUtil.CopyMemory(ref source, ref Unsafe.AddByteOffset(ref destination, (IntPtr)idx), sourceBytesToCopy);
            idx += sourceBytesToCopy;
        }
    }

    internal static partial class UnsafeMemory32
    {
        // 直接引用 byte[], 节省 byte[] => ReadOnlySpan<byte> 的转换
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw1<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 1);

            Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(uint)idx) = source[0];

            idx += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw1(ref byte destination, ref byte source, ref int idx)
        {
            Unsafe.AddByteOffset(ref destination, (IntPtr)idx) = source;

            idx += 1;
        }
    }

    internal static partial class UnsafeMemory64
    {
        // 直接引用 byte[], 节省 byte[] => ReadOnlySpan<byte> 的转换
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw1<TSymbol>(ref JsonWriter<TSymbol> writer, byte[] source, ref int idx) where TSymbol : struct
        {
            writer.Ensure(idx, 1);

            Unsafe.Add(ref writer.PinnableUtf8Address, (IntPtr)(ulong)idx) = source[0];

            idx += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw1(ref byte destination, ref byte source, ref int idx)
        {
            Unsafe.AddByteOffset(ref destination, (IntPtr)idx) = source;

            idx += 1;
        }
    }
}
