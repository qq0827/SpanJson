namespace SpanJson.Internal
{
    using System;
    using System.Runtime.CompilerServices;

    // for string key property name write optimization.

    internal static class UnsafeMemory
    {
        public static readonly bool Is64BitProcess = IntPtr.Size >= 8;

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
        internal static void WriteRawBytes(ref byte destination, ref byte source, int sourceBytesToCopy, ref int idx)
        {
            //if (0u >= (uint)sourceBytesToCopy) { return; }

            BinaryUtil.CopyMemory(ref source, ref Unsafe.AddByteOffset(ref destination, (IntPtr)idx), sourceBytesToCopy);
            idx += sourceBytesToCopy;
        }
    }

    internal static partial class UnsafeMemory32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw1(ref byte destination, ref byte source, ref int idx)
        {
            Unsafe.AddByteOffset(ref destination, (IntPtr)idx) = source;

            idx += 1;
        }
    }

    internal static partial class UnsafeMemory64
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw1(ref byte destination, ref byte source, ref int idx)
        {
            Unsafe.AddByteOffset(ref destination, (IntPtr)idx) = source;

            idx += 1;
        }
    }
}
