// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Internal;

namespace SpanJson.Document
{
    public sealed partial class JsonDocument
    {
        private const int UnseekableStreamInitialRentSize = 4096;

        public static JsonDocument Parse(byte[] utf8Json, JsonDocumentOptions options = default, bool useArrayPools = true)
        {
            if (utf8Json is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }
            return ParseCore(new ReadOnlyMemory<byte>(utf8Json), options.GetReaderOptions(), null, useArrayPools);
        }

        public static JsonDocument Parse(in ReadOnlySpan<byte> utf8Json, JsonDocumentOptions options = default, bool useArrayPools = true)
        {
            return ParseCore(new ReadOnlyMemory<byte>(utf8Json.ToArray()), options.GetReaderOptions(), null, useArrayPools);
        }

        /// <summary>
        ///   Parse memory as UTF-8-encoded text representing a single JSON value into a JsonDocument.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The <see cref="ReadOnlyMemory{T}"/> value will be used for the entire lifetime of the
        ///     JsonDocument object, and the caller must ensure that the data therein does not change during
        ///     the object lifetime.
        ///   </para>
        ///
        ///   <para>
        ///     Because the input is considered to be text, a UTF-8 Byte-Order-Mark (BOM) must not be present.
        ///   </para>
        /// </remarks>
        /// <param name="utf8Json">JSON text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <param name="useArrayPools"></param>
        /// <returns>
        ///   A JsonDocument representation of the JSON value.
        /// </returns>
        /// <exception cref="JsonException">
        ///   <paramref name="utf8Json"/> does not represent a valid single JSON value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static JsonDocument Parse(in ReadOnlyMemory<byte> utf8Json, JsonDocumentOptions options = default, bool useArrayPools = true)
        {
            return ParseCore(utf8Json, options.GetReaderOptions(), null, useArrayPools);
        }

        /// <summary>
        ///   Parse a sequence as UTF-8-encoded text representing a single JSON value into a JsonDocument.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The <see cref="ReadOnlySequence{T}"/> may be used for the entire lifetime of the
        ///     JsonDocument object, and the caller must ensure that the data therein does not change during
        ///     the object lifetime.
        ///   </para>
        ///
        ///   <para>
        ///     Because the input is considered to be text, a UTF-8 Byte-Order-Mark (BOM) must not be present.
        ///   </para>
        /// </remarks>
        /// <param name="utf8Json">JSON text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <param name="useArrayPools"></param>
        /// <returns>
        ///   A JsonDocument representation of the JSON value.
        /// </returns>
        /// <exception cref="JsonException">
        ///   <paramref name="utf8Json"/> does not represent a valid single JSON value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static JsonDocument Parse(in ReadOnlySequence<byte> utf8Json, JsonDocumentOptions options = default, bool useArrayPools = true)
        {
            JsonReaderOptions readerOptions = options.GetReaderOptions();

            if (utf8Json.IsSingleSegment)
            {
                return ParseCore(utf8Json.First, readerOptions, null, useArrayPools);
            }

            int length = checked((int)utf8Json.Length);
            byte[] utf8Bytes;
            Span<byte> utf8Span = useArrayPools
                ? (utf8Bytes = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length)
                : (utf8Bytes = new byte[length]);

            try
            {
                utf8Json.CopyTo(utf8Span);
                return ParseCore(new ReadOnlyMemory<byte>(utf8Bytes, 0, length),
                    readerOptions: readerOptions,
                    extraRentedBytes: utf8Bytes,
                    isDisposable: useArrayPools);
            }
            catch
            {
                if (useArrayPools)
                {
                    // Holds document content, clear it before returning it.
                    utf8Span.Clear();
                    ArrayPool<byte>.Shared.Return(utf8Bytes);
                }
                throw;
            }
        }

        /// <summary>
        ///   Parse a <see cref="Stream"/> as UTF-8-encoded data representing a single JSON value into a
        ///   JsonDocument.  The Stream will be read to completion.
        /// </summary>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <param name="useArrayPools"></param>
        /// <returns>
        ///   A JsonDocument representation of the JSON value.
        /// </returns>
        /// <exception cref="JsonException">
        ///   <paramref name="utf8Json"/> does not represent a valid single JSON value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static JsonDocument Parse(Stream utf8Json, JsonDocumentOptions options = default, bool useArrayPools = true)
        {
            if (utf8Json is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            ArraySegment<byte> drained = ReadToEnd(utf8Json, useArrayPools);

            try
            {
                return ParseCore(new ReadOnlyMemory<byte>(drained.Array, drained.Offset, drained.Count),
                    readerOptions: options.GetReaderOptions(),
                    extraRentedBytes: useArrayPools ? drained.Array : null,
                    isDisposable: useArrayPools);
            }
            catch
            {
                if (useArrayPools)
                {
                    // Holds document content, clear it before returning it.
                    drained.AsSpan().Clear();
                    ArrayPool<byte>.Shared.Return(drained.Array);
                }
                throw;
            }
        }

        /// <summary>
        ///   Parse a <see cref="Stream"/> as UTF-8-encoded data representing a single JSON value into a
        ///   JsonDocument.  The Stream will be read to completion.
        /// </summary>
        /// <param name="utf8Json">JSON data to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <param name="useArrayPools"></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        ///   A Task to produce a JsonDocument representation of the JSON value.
        /// </returns>
        /// <exception cref="JsonException">
        ///   <paramref name="utf8Json"/> does not represent a valid single JSON value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static Task<JsonDocument> ParseAsync(
            Stream utf8Json,
            JsonDocumentOptions options = default,
            bool useArrayPools = true,
            CancellationToken cancellationToken = default)
        {
            if (utf8Json is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            return ParseAsyncCore(utf8Json, options, useArrayPools, cancellationToken);
        }

        private static async Task<JsonDocument> ParseAsyncCore(
            Stream utf8Json,
            JsonDocumentOptions options = default,
            bool useArrayPools = true,
            CancellationToken cancellationToken = default)
        {
            ArraySegment<byte> drained = await ReadToEndAsync(utf8Json, useArrayPools, cancellationToken).ConfigureAwait(false);

            try
            {
                return ParseCore(new ReadOnlyMemory<byte>(drained.Array, drained.Offset, drained.Count),
                    readerOptions: options.GetReaderOptions(),
                    extraRentedBytes: useArrayPools ? drained.Array : null,
                    isDisposable: useArrayPools);
            }
            catch
            {
                if (useArrayPools)
                {
                    // Holds document content, clear it before returning it.
                    drained.AsSpan().Clear();
                    ArrayPool<byte>.Shared.Return(drained.Array);
                }
                throw;
            }
        }

        /// <summary>
        ///   Parse text representing a single JSON value into a JsonDocument.
        /// </summary>
        /// <param name="json">JSON text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <param name="useArrayPools"></param>
        /// <returns>
        ///   A JsonDocument representation of the JSON value.
        /// </returns>
        /// <exception cref="JsonException">
        ///   <paramref name="json"/> does not represent a valid single JSON value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static JsonDocument Parse(string json, JsonDocumentOptions options = default, bool useArrayPools = true)
        {
            if (json is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.json); }

            return Parse(json.AsSpan(), options, useArrayPools);
        }

        /// <summary>
        ///   Parse text representing a single JSON value into a JsonDocument.
        /// </summary>
        /// <remarks>
        ///   The <see cref="ReadOnlyMemory{T}"/> value may be used for the entire lifetime of the
        ///   JsonDocument object, and the caller must ensure that the data therein does not change during
        ///   the object lifetime.
        /// </remarks>
        /// <param name="json">JSON text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <param name="useArrayPools"></param>
        /// <returns>
        ///   A JsonDocument representation of the JSON value.
        /// </returns>
        /// <exception cref="JsonException">
        ///   <paramref name="json"/> does not represent a valid single JSON value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static JsonDocument Parse(in ReadOnlyMemory<char> json, JsonDocumentOptions options = default, bool useArrayPools = true)
        {
            return Parse(json.Span, options, useArrayPools);
        }

        /// <summary>
        ///   Parse text representing a single JSON value into a JsonDocument.
        /// </summary>
        /// <remarks>
        ///   The <see cref="ReadOnlyMemory{T}"/> value may be used for the entire lifetime of the
        ///   JsonDocument object, and the caller must ensure that the data therein does not change during
        ///   the object lifetime.
        /// </remarks>
        /// <param name="jsonChars">JSON text to parse.</param>
        /// <param name="options">Options to control the reader behavior during parsing.</param>
        /// <param name="useArrayPools"></param>
        /// <returns>
        ///   A JsonDocument representation of the JSON value.
        /// </returns>
        /// <exception cref="JsonException">
        ///   <paramref name="jsonChars"/> does not represent a valid single JSON value.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   <paramref name="options"/> contains unsupported options.
        /// </exception>
        public static JsonDocument Parse(in ReadOnlySpan<char> jsonChars, JsonDocumentOptions options = default, bool useArrayPools = true)
        {
            int expectedByteCount = JsonReaderHelper.GetUtf8ByteCount(jsonChars);
            byte[] utf8Bytes;
            Span<byte> utf8Span = utf8Bytes = useArrayPools ? ArrayPool<byte>.Shared.Rent(expectedByteCount) : new byte[expectedByteCount];

            try
            {
                int actualByteCount = JsonReaderHelper.GetUtf8FromText(jsonChars, utf8Span);
                Debug.Assert(expectedByteCount == actualByteCount);

                return ParseCore(new ReadOnlyMemory<byte>(utf8Bytes, 0, actualByteCount),
                    readerOptions: options.GetReaderOptions(),
                    extraRentedBytes: useArrayPools ? utf8Bytes : null,
                    isDisposable: useArrayPools);
            }
            catch
            {
                if (useArrayPools)
                {
                    // Holds document content, clear it before returning it.
                    utf8Span.Slice(0, expectedByteCount).Clear();
                    ArrayPool<byte>.Shared.Return(utf8Bytes);
                }
                throw;
            }
        }

        /// <summary>
        ///   Attempts to parse one JSON value (including objects or arrays) from the provided reader.
        /// </summary>
        /// <param name="reader">The reader to read.</param>
        /// <param name="document">Receives the parsed document.</param>
        /// <param name="useArrayPools"></param>
        /// <returns>
        ///   <see langword="true"/> if a value was read and parsed into a JsonDocument,
        ///   <see langword="false"/> if the reader ran out of data while parsing.
        ///   All other situations result in an exception being thrown.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     If the <see cref="Utf8JsonReader.TokenType"/> property of <paramref name="reader"/>
        ///     is <see cref="JsonTokenType.PropertyName"/> or <see cref="JsonTokenType.None"/>, the
        ///     reader will be advanced by one call to <see cref="Utf8JsonReader.Read"/> to determine
        ///     the start of the value.
        ///   </para>
        ///
        ///   <para>
        ///     Upon completion of this method <paramref name="reader"/> will be positioned at the
        ///     final token in the JSON value.  If an exception is thrown, or <see langword="false"/>
        ///     is returned, the reader is reset to the state it was in when the method was called.
        ///   </para>
        ///
        ///   <para>
        ///     This method makes a copy of the data the reader acted on, so there is no caller
        ///     requirement to maintain data integrity beyond the return of this method.
        ///   </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   The current <paramref name="reader"/> token does not start or represent a value.
        /// </exception>
        /// <exception cref="JsonException">
        ///   A value could not be read from the reader.
        /// </exception>
        public static bool TryParseValue(ref Utf8JsonReader reader, out JsonDocument document, bool useArrayPools = true)
        {
            return TryParseValue(ref reader, out document, shouldThrow: false, useArrayPools: useArrayPools);
        }

        /// <summary>
        ///   Parses one JSON value (including objects or arrays) from the provided reader.
        /// </summary>
        /// <param name="reader">The reader to read.</param>
        /// <param name="useArrayPools"></param>
        /// <returns>
        ///   A JsonDocument representing the value (and nested values) read from the reader.
        /// </returns>
        /// <remarks>
        ///   <para>
        ///     If the <see cref="Utf8JsonReader.TokenType"/> property of <paramref name="reader"/>
        ///     is <see cref="JsonTokenType.PropertyName"/> or <see cref="JsonTokenType.None"/>, the
        ///     reader will be advanced by one call to <see cref="Utf8JsonReader.Read"/> to determine
        ///     the start of the value.
        ///   </para>
        /// 
        ///   <para>
        ///     Upon completion of this method <paramref name="reader"/> will be positioned at the
        ///     final token in the JSON value.  If an exception is thrown the reader is reset to
        ///     the state it was in when the method was called.
        ///   </para>
        ///
        ///   <para>
        ///     This method makes a copy of the data the reader acted on, so there is no caller
        ///     requirement to maintain data integrity beyond the return of this method.
        ///   </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///   <paramref name="reader"/> is using unsupported options.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   The current <paramref name="reader"/> token does not start or represent a value.
        /// </exception>
        /// <exception cref="JsonException">
        ///   A value could not be read from the reader.
        /// </exception>
        public static JsonDocument ParseValue(ref Utf8JsonReader reader, bool useArrayPools = true)
        {
            bool ret = TryParseValue(ref reader, out JsonDocument document, shouldThrow: true, useArrayPools: useArrayPools);
            Debug.Assert(ret, "TryParseValue returned false with shouldThrow: true.");
            return document;
        }

        private static bool TryParseValue(ref Utf8JsonReader reader, out JsonDocument document,
            bool shouldThrow, bool useArrayPools)
        {
            JsonReaderState state = reader.CurrentState;
            CheckSupportedOptions(state.Options, ExceptionArgument.reader);

            // Value copy to overwrite the ref on an exception and undo the destructive reads.
            Utf8JsonReader restore = reader;

            ReadOnlySpan<byte> valueSpan = default;
            ReadOnlySequence<byte> valueSequence = default;

            try
            {
                switch (reader.TokenType)
                {
                    // A new reader was created and has never been read,
                    // so we need to move to the first token.
                    // (or a reader has terminated and we're about to throw)
                    case JsonTokenType.None:
                    // Using a reader loop the caller has identified a property they wish to
                    // hydrate into a JsonDocument. Move to the value first.
                    case JsonTokenType.PropertyName:
                        {
                            if (!reader.Read())
                            {
                                if (shouldThrow)
                                {
                                    SysJsonThrowHelper.ThrowJsonReaderException(
                                        ref reader,
                                        ExceptionResource.ExpectedJsonTokens);
                                }

                                reader = restore;
                                document = null;
                                return false;
                            }
                            break;
                        }
                }

                switch (reader.TokenType)
                {
                    // Any of the "value start" states are acceptable.
                    case JsonTokenType.BeginObject:
                    case JsonTokenType.BeginArray:
                        {
                            long startingOffset = reader.TokenStartIndex;

                            // Placeholder until reader.Skip() is written (#33295)
                            {
                                int depth = reader.CurrentDepth;

                                // CurrentDepth rises late and falls fast,
                                // a payload of "[ 1, 2, 3, 4 ]" will report post-Read()
                                // CurrentDepth values of { 0, 1, 1, 1, 1, 0 },
                                // Since we're logically at 0 ([), Read() once and keep
                                // reading until we've come back down to 0 (]).
                                do
                                {
                                    if (!reader.Read())
                                    {
                                        if (shouldThrow)
                                        {
                                            SysJsonThrowHelper.ThrowJsonReaderException(
                                                ref reader,
                                                ExceptionResource.ExpectedJsonTokens);
                                        }

                                        reader = restore;
                                        document = null;
                                        return false;
                                    }
                                } while (reader.CurrentDepth > depth);
                            }

                            long totalLength = reader.BytesConsumed - startingOffset;
                            ReadOnlySequence<byte> sequence = reader.OriginalSequence;

                            if (sequence.IsEmpty)
                            {
                                valueSpan = reader.OriginalSpan.Slice(
                                    checked((int)startingOffset),
                                    checked((int)totalLength));
                            }
                            else
                            {
                                valueSequence = sequence.Slice(startingOffset, totalLength);
                            }

                            Debug.Assert(
                                reader.TokenType == JsonTokenType.EndObject ||
                                reader.TokenType == JsonTokenType.EndArray);

                            break;
                        }

                    // Single-token values
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                        {
                            if (reader.HasValueSequence)
                            {
                                valueSequence = reader.ValueSequence;
                            }
                            else
                            {
                                valueSpan = reader.ValueSpan;
                            }

                            break;
                        }
                    // String's ValueSequence/ValueSpan omits the quotes, we need them back.
                    case JsonTokenType.String:
                        {
                            ReadOnlySequence<byte> sequence = reader.OriginalSequence;

                            if (sequence.IsEmpty)
                            {
                                // Since the quoted string fit in a ReadOnlySpan originally
                                // the contents length plus the two quotes can't overflow.
                                int payloadLength = reader.ValueSpan.Length + 2;
                                Debug.Assert(payloadLength > 1);

                                ReadOnlySpan<byte> readerSpan = reader.OriginalSpan;

                                Debug.Assert(
                                    readerSpan[(int)reader.TokenStartIndex] == (byte)'"',
                                    $"Calculated span starts with {readerSpan[(int)reader.TokenStartIndex]}");

                                Debug.Assert(
                                    readerSpan[(int)reader.TokenStartIndex + payloadLength - 1] == (byte)'"',
                                    $"Calculated span ends with {readerSpan[(int)reader.TokenStartIndex + payloadLength - 1]}");

                                valueSpan = readerSpan.Slice((int)reader.TokenStartIndex, payloadLength);
                            }
                            else
                            {
                                long payloadLength = 2;

                                if (reader.HasValueSequence)
                                {
                                    payloadLength += reader.ValueSequence.Length;
                                }
                                else
                                {
                                    payloadLength += reader.ValueSpan.Length;
                                }

                                valueSequence = sequence.Slice(reader.TokenStartIndex, payloadLength);
                                Debug.Assert(
                                    valueSequence.First.Span[0] == (byte)'"',
                                    $"Calculated sequence starts with {valueSequence.First.Span[0]}");

                                Debug.Assert(
                                    valueSequence.ToArray()[payloadLength - 1] == (byte)'"',
                                    $"Calculated sequence ends with {valueSequence.ToArray()[payloadLength - 1]}");
                            }

                            break;
                        }
                    default:
                        {
                            if (shouldThrow)
                            {
                                // Default case would only hit if TokenType equals JsonTokenType.EndObject or JsonTokenType.EndArray in which case it would never be sequence
                                Debug.Assert(!reader.HasValueSequence);
                                byte displayByte = reader.ValueSpan[0];

                                SysJsonThrowHelper.ThrowJsonReaderException(
                                    ref reader,
                                    ExceptionResource.ExpectedStartOfValueNotFound,
                                    displayByte);
                            }

                            reader = restore;
                            document = null;
                            return false;
                        }
                }
            }
            catch
            {
                reader = restore;
                throw;
            }

            int length = valueSpan.IsEmpty ? checked((int)valueSequence.Length) : valueSpan.Length;
            byte[] rented;
            Span<byte> rentedSpan = useArrayPools
                ? (rented = ArrayPool<byte>.Shared.Rent(length)).AsSpan(0, length)
                : (rented = new byte[length]);

            try
            {
                if (valueSpan.IsEmpty)
                {
                    valueSequence.CopyTo(rentedSpan);
                }
                else
                {
                    valueSpan.CopyTo(rentedSpan);
                }

                document = ParseCore(new ReadOnlyMemory<byte>(rented, 0, length),
                    readerOptions: state.Options,
                    extraRentedBytes: useArrayPools ? rented : null,
                    isDisposable: useArrayPools);
                return true;
            }
            catch
            {
                if (useArrayPools)
                {
                    // This really shouldn't happen since the document was already checked
                    // for consistency by Skip.  But if data mutations happened just after
                    // the calls to Read then the copy may not be valid.
                    rentedSpan.Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }
                throw;
            }
        }

        private static JsonDocument ParseCore(in ReadOnlyMemory<byte> utf8Json,
            JsonReaderOptions readerOptions, byte[] extraRentedBytes, bool isDisposable)
        {
            ReadOnlySpan<byte> utf8JsonSpan = utf8Json.Span;
            Utf8JsonReader reader = new Utf8JsonReader(
                utf8JsonSpan,
                isFinalBlock: true,
                new JsonReaderState(options: readerOptions));

            var database = new MetadataDb(utf8Json.Length, isDisposable);
            var stack = new StackRowStack(JsonDocumentOptions.DefaultMaxDepth * StackRow.Size);

            try
            {
                ParseCore(utf8JsonSpan, reader, ref database, ref stack);
            }
            catch
            {
                database.Dispose();
                throw;
            }
            finally
            {
                stack.Dispose();
            }

            return new JsonDocument(utf8Json, database, extraRentedBytes, isDisposable);
        }

        private static ArraySegment<byte> ReadToEnd(Stream stream, bool useArrayPools)
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
                    rented = useArrayPools ? ArrayPool<byte>.Shared.Rent(checked((int)expectedLength)) : new byte[checked((int)expectedLength)];
                }
                else
                {
                    rented = useArrayPools ? ArrayPool<byte>.Shared.Rent(UnseekableStreamInitialRentSize) : new byte[UnseekableStreamInitialRentSize];
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
                        rented = useArrayPools ? ArrayPool<byte>.Shared.Rent(checked(toReturn.Length * 2)) : new byte[checked(toReturn.Length * 2)];
                        BinaryUtil.CopyMemory(toReturn, 0, rented, 0, toReturn.Length);
                        if (useArrayPools)
                        {
                            // Holds document content, clear it.
                            ArrayPool<byte>.Shared.Return(toReturn, clearArray: true);
                        }
                    }

                    lastRead = stream.Read(rented, written, rented.Length - written);
                    written += lastRead;
                } while (lastRead > 0);

                return new ArraySegment<byte>(rented, 0, written);
            }
            catch
            {
                if (useArrayPools && rented is object)
                {
                    // Holds document content, clear it before returning it.
                    rented.AsSpan(0, written).Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }

                throw;
            }
        }

        private static async ValueTask<ArraySegment<byte>> ReadToEndAsync(Stream stream, bool useArrayPools, CancellationToken cancellationToken)
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
                    rented = useArrayPools ? ArrayPool<byte>.Shared.Rent(checked((int)expectedLength)) : new byte[checked((int)expectedLength)];
                }
                else
                {
                    rented = useArrayPools ? ArrayPool<byte>.Shared.Rent(UnseekableStreamInitialRentSize) : new byte[UnseekableStreamInitialRentSize];
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
                        rented = useArrayPools ? ArrayPool<byte>.Shared.Rent(toReturn.Length * 2) : new byte[toReturn.Length * 2];
                        BinaryUtil.CopyMemory(toReturn, 0, rented, 0, toReturn.Length);
                        if (useArrayPools)
                        {
                            // Holds document content, clear it.
                            ArrayPool<byte>.Shared.Return(toReturn, clearArray: true);
                        }
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
                if (useArrayPools && rented is object)
                {
                    // Holds document content, clear it before returning it.
                    rented.AsSpan(0, written).Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }

                throw;
            }
        }
    }
}
