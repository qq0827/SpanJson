// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Internal;

namespace SpanJson
{
    partial struct Utf8JsonWriter
    {
        private void ValidateWritingValue()
        {
            if (!_options.SkipValidation)
            {
                if (_inObject)
                {
                    if (_tokenType != JsonTokenType.PropertyName)
                    {
                        Debug.Assert(_tokenType != JsonTokenType.None && _tokenType != JsonTokenType.BeginArray);
                        SysJsonThrowHelper.ThrowInvalidOperationException(ExceptionResource.CannotWriteValueWithinObject, currentDepth: default, token: default, _tokenType);
                    }
                }
                else
                {
                    Debug.Assert(_tokenType != JsonTokenType.PropertyName);

                    // It is more likely for CurrentDepth to not equal 0 when writing valid JSON, so check that first to rely on short-circuiting and return quickly.
                    if (0u >= (uint)CurrentDepth && _tokenType != JsonTokenType.None)
                    {
                        SysJsonThrowHelper.ThrowInvalidOperationException(ExceptionResource.CannotWriteValueAfterPrimitiveOrClose, currentDepth: default, token: default, _tokenType);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Base64EncodeAndWrite(in ReadOnlySpan<byte> bytes, ref byte output, int encodingLength, ref int pos)
        {
            byte[] outputText = null;

            Span<byte> encodedBytes = (uint)encodingLength <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[encodingLength] :
                (outputText = ArrayPool<byte>.Shared.Rent(encodingLength));

            OperationStatus status = Base64.EncodeToUtf8(bytes, encodedBytes, out int consumed, out int written);
            Debug.Assert(status == OperationStatus.Done);
            Debug.Assert(consumed == bytes.Length);

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(encodedBytes), ref Unsafe.Add(ref output, pos), encodingLength);
            pos += encodingLength;

            if (outputText != null)
            {
                ArrayPool<byte>.Shared.Return(outputText);
            }
        }
    }
}
