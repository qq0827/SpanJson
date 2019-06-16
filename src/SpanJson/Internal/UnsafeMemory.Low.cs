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
        public static void WriteRaw(byte[] destination, byte[] source, ref int idx)
        {
            if (Is64BitProcess)
            {
                UnsafeMemory64.WriteRaw(destination, source, ref idx);
            }
            else
            {
                UnsafeMemory32.WriteRaw(destination, source, ref idx);
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
        internal static void WriteRawBytes(byte[] destination, byte[] source, ref int idx)
        {
            var count = source.Length;
            BinaryUtil.CopyMemory(source, 0, destination, idx, count);
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
        public static void WriteRaw1(byte[] destination, byte[] source, ref int idx)
        {
            Unsafe.Add(ref destination[0], (IntPtr)(uint)idx) = source[0];

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
        public static void WriteRaw1(byte[] destination, byte[] source, ref int idx)
        {
            Unsafe.Add(ref destination[0], (IntPtr)(ulong)idx) = source[0];

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
