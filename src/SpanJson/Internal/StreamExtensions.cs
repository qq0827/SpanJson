using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SpanJson.Internal
{
    public static class StreamExtensions
    {
        private const int UnseekableStreamInitialRentSize = 4096;

        public static ArraySegment<byte> ReadToEnd(this Stream stream)
        {
            int written = 0;
            byte[] rented = null;

            ReadOnlySpan<byte> utf8Bom = JsonUtf8Constant.Utf8Bom;

            try
            {
                if (stream.CanSeek)
                {
                    // Ask for 1 more than the length to avoid resizing later,
                    // which is unnecessary in the common case where the stream length doesn't change.
                    long expectedLength = Math.Max(utf8Bom.Length, stream.Length - stream.Position) + 1;
                    rented = ArrayPool<byte>.Shared.Rent(checked((int)expectedLength));
                }
                else
                {
                    rented = ArrayPool<byte>.Shared.Rent(UnseekableStreamInitialRentSize);
                }

                int lastRead;

                // Read up to 3 bytes to see if it's the UTF-8 BOM
                do
                {
                    // No need for checking for growth, the minimal rent sizes both guarantee it'll fit.
                    Debug.Assert(rented.Length >= utf8Bom.Length);

                    lastRead = stream.Read(
                        rented,
                        written,
                        utf8Bom.Length - written);

                    written += lastRead;
                } while (lastRead > 0 && written < utf8Bom.Length);

                // If we have 3 bytes, and they're the BOM, reset the write position to 0.
                if (written == utf8Bom.Length &&
                    utf8Bom.SequenceEqual(rented.AsSpan(0, utf8Bom.Length)))
                {
                    written = 0;
                }

                do
                {
                    if (rented.Length == written)
                    {
                        byte[] toReturn = rented;
                        rented = ArrayPool<byte>.Shared.Rent(checked(toReturn.Length * 2));
                        BinaryUtil.CopyMemory(toReturn, 0, rented, 0, toReturn.Length);
                        // Holds document content, clear it.
                        ArrayPool<byte>.Shared.Return(toReturn, clearArray: true);
                    }

                    lastRead = stream.Read(rented, written, rented.Length - written);
                    written += lastRead;
                } while (lastRead > 0);

                return new ArraySegment<byte>(rented, 0, written);
            }
            catch
            {
                if (rented is object)
                {
                    // Holds document content, clear it before returning it.
                    //rented.AsSpan(0, written).Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }

                throw;
            }
        }

        public static async ValueTask<ArraySegment<byte>> ReadToEndAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            int written = 0;
            byte[] rented = null;

            try
            {
                // Save the length to a local to be reused across awaits.
                int utf8BomLength = JsonUtf8Constant.Utf8Bom.Length;

                if (stream.CanSeek)
                {
                    // Ask for 1 more than the length to avoid resizing later,
                    // which is unnecessary in the common case where the stream length doesn't change.
                    long expectedLength = Math.Max(utf8BomLength, stream.Length - stream.Position) + 1;
                    rented = ArrayPool<byte>.Shared.Rent(checked((int)expectedLength));
                }
                else
                {
                    rented = ArrayPool<byte>.Shared.Rent(UnseekableStreamInitialRentSize);
                }

                int lastRead;

                // Read up to 3 bytes to see if it's the UTF-8 BOM
                do
                {
                    // No need for checking for growth, the minimal rent sizes both guarantee it'll fit.
                    Debug.Assert(rented.Length >= JsonUtf8Constant.Utf8Bom.Length);

                    lastRead = await stream.ReadAsync(
                        rented,
                        written,
                        utf8BomLength - written,
                        cancellationToken).ConfigureAwait(false);

                    written += lastRead;
                } while (lastRead > 0 && written < utf8BomLength);

                // If we have 3 bytes, and they're the BOM, reset the write position to 0.
                if (written == utf8BomLength &&
                    JsonUtf8Constant.Utf8Bom.SequenceEqual(rented.AsSpan(0, utf8BomLength)))
                {
                    written = 0;
                }

                do
                {
                    if (rented.Length == written)
                    {
                        byte[] toReturn = rented;
                        rented = ArrayPool<byte>.Shared.Rent(toReturn.Length * 2);
                        BinaryUtil.CopyMemory(toReturn, 0, rented, 0, toReturn.Length);
                        // Holds document content, clear it.
                        ArrayPool<byte>.Shared.Return(toReturn, clearArray: true);
                    }

                    lastRead = await stream.ReadAsync(
                        rented,
                        written,
                        rented.Length - written,
                        cancellationToken).ConfigureAwait(false);

                    written += lastRead;

                } while (lastRead > 0);

                return new ArraySegment<byte>(rented, 0, written);
            }
            catch
            {
                if (rented is object)
                {
                    // Holds document content, clear it before returning it.
                    //rented.AsSpan(0, written).Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }

                throw;
            }
        }
    }
}
