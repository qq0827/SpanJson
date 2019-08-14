using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Internal;

namespace SpanJson
{
    partial class JsonSerializer
    {
        partial class NonGeneric
        {
            public static class Inner<TSymbol, TResolver> where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
            {
                internal static readonly ConcurrentDictionary<Type, Invoker> Invokers = new ConcurrentDictionary<Type, Invoker>();

                internal static readonly Func<Type, Invoker> InvokerFactory = x => BuildInvoker(x);

                #region -- Utf16 Serialize --

                public static string InnerSerializeToString(object input)
                {
                    if (input is null) { return Generic.Utf16.Serialize<object>(null); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(input.GetType(), InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.ToStringSerializer(input);
                }

                public static char[] InnerSerializeToCharArray(object input)
                {
                    if (input is null) { return Generic.Utf16.SerializeToCharArray<object>(null); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(input.GetType(), InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.ToCharArraySerializer(input);
                }

                public static ArraySegment<char> InnerSerializeToCharArrayPool(object input)
                {
                    if (input is null) { return Generic.Utf16.SerializeToArrayPool<object>(null); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(input.GetType(), InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.ToCharArrayPoolSerializer(input);
                }

                public static ValueTask InnerSerializeAsync(object input, TextWriter writer, CancellationToken cancellationToken = default)
                {
                    if (input is null) { return Generic.Utf16.SerializeAsync<object>(null, writer, cancellationToken); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(input.GetType(), InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.ToTextWriterSerializerAsync(input, writer, cancellationToken);
                }

                #endregion

                #region -- Utf8 Serialize --


                public static byte[] InnerSerializeToByteArray(object input)
                {
                    if (input is null) { return Generic.Utf8.Serialize<object>(null); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(input.GetType(), InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.ToByteArraySerializer(input);
                }

                public static ArraySegment<byte> InnerSerializeToByteArrayPool(object input)
                {
                    if (input is null) { return Generic.Utf8.SerializeToArrayPool<object>(null); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(input.GetType(), InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.ToByteArrayPoolSerializer(input);
                }

                public static ValueTask InnerSerializeAsync(object input, Stream stream, CancellationToken cancellationToken = default)
                {
                    if (input is null) { return Generic.Utf8.SerializeAsync<object>(null, stream, cancellationToken); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(input.GetType(), InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.ToStreamSerializerAsync(input, stream, cancellationToken);
                }

                #endregion

                #region -- Common Deserialize --

                public static object InnerDeserialize(TSymbol[] input, Type type)
                {
                    if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(type, InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.FromByteArrayDeserializer(input);
                }

                public static object InnerDeserialize(in ArraySegment<TSymbol> input, Type type)
                {
                    if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(type, InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.FromBufferDeserializer(input);
                }

                public static object InnerDeserialize(in ReadOnlyMemory<TSymbol> input, Type type)
                {
                    if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(type, InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.FromMemoryDeserializer(input);
                }

                public static object InnerDeserialize(in ReadOnlySpan<TSymbol> input, Type type)
                {
                    if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(type, InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.Deserializer(input);
                }

                #endregion

                #region -- Utf16 Deserialize --

                public static ValueTask<object> InnerDeserializeAsync(TextReader reader, Type type, CancellationToken cancellationToken = default)
                {
                    if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(type, InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.FromTextReaderDeserializerAsync(reader, cancellationToken);
                }

                #endregion

                #region -- Utf8 Deserialize --

                public static ValueTask<object> InnerDeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
                {
                    if (type is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

                    // ReSharper disable ConvertClosureToMethodGroup
                    var invoker = Invokers.GetOrAdd(type, InvokerFactory);
                    // ReSharper restore ConvertClosureToMethodGroup
                    return invoker.FromStreamDeserializerAsync(stream, cancellationToken);
                }

                #endregion

                #region ** BuildInvoker **

                /// <summary>Build only the delegates which are actually required</summary>
                private static Invoker BuildInvoker(Type type)
                {
                    if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
                    {
                        return new Invoker(
                            BuildToByteArraySerializer(type),
                            BuildToByteArrayPoolSerializer(type),
                            BuildAsyncStreamSerializer(type),

                            BuildDeserializer(type),
                            BuildFromByteArrayDeserializer(type),
                            BuildFromBufferDeserializer(type),
                            BuildFromMemoryDeserializer(type),

                            BuildAsyncStreamDeserializer(type));
                    }

                    if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
                    {
                        return new Invoker(
                            BuildToStringSerializer(type),
                            BuildToCharArraySerializer(type),
                            BuildToCharArrayPoolSerializer(type),
                            BuildAsyncTextWriterSerializer(type),

                            BuildDeserializer(type),
                            BuildFromByteArrayDeserializer(type),
                            BuildFromBufferDeserializer(type),
                            BuildFromMemoryDeserializer(type),

                            BuildAsyncTextReaderDeserializer(type));
                    }

                    throw ThrowHelper.GetNotSupportedException();
                }

                #endregion

                #region ** Utf8 Builder **

                private static SerializeToByteArrayDelegate BuildToByteArraySerializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(object), "input");
                    var typedInputParam = Expression.Convert(inputParam, type);
                    var lambdaExpression =
                        Expression.Lambda<SerializeToByteArrayDelegate>(
                            Expression.Call(typeof(Generic.Utf8), nameof(Generic.Utf8.Serialize),
                                new[] { type, typeof(TResolver) }, typedInputParam),
                            inputParam);
                    return lambdaExpression.Compile();
                }

                private static SerializeToByteArrayPoolDelegate BuildToByteArrayPoolSerializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(object), "input");
                    var typedInputParam = Expression.Convert(inputParam, type);
                    var lambdaExpression =
                        Expression.Lambda<SerializeToByteArrayPoolDelegate>(
                            Expression.Call(typeof(Generic.Utf8), nameof(Generic.Utf8.SerializeToArrayPool),
                                new[] { type, typeof(TResolver) }, typedInputParam),
                            inputParam);
                    return lambdaExpression.Compile();
                }

                private static SerializeToStreamDelegateAsync BuildAsyncStreamSerializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(object), "input");
                    var typedInputParam = Expression.Convert(inputParam, type);
                    var textWriterParam = Expression.Parameter(typeof(Stream), "stream");
                    var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
                    var lambdaExpression =
                        Expression.Lambda<SerializeToStreamDelegateAsync>(
                            Expression.Call(typeof(Generic.Utf8), nameof(Generic.Utf8.SerializeAsync),
                                new[] { type, typeof(TResolver) }, typedInputParam, textWriterParam, cancellationTokenParam),
                            inputParam, textWriterParam, cancellationTokenParam);
                    return lambdaExpression.Compile();
                }

                private static DeserializeFromStreamDelegateAsync BuildAsyncStreamDeserializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(Stream), "stream");
                    var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
                    Expression genericCall = Expression.Call(typeof(Utf8), nameof(Utf8.GenericStreamObjectWrapper),
                        new[] { type, typeof(TResolver) }, inputParam, cancellationTokenParam);
                    var lambdaExpression = Expression.Lambda<DeserializeFromStreamDelegateAsync>(genericCall, inputParam, cancellationTokenParam);
                    return lambdaExpression.Compile();
                }

                #endregion

                #region ** Utf16 Builder **

                private static SerializeToStringDelegate BuildToStringSerializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(object), "input");
                    var typedInputParam = Expression.Convert(inputParam, type);
                    var lambdaExpression =
                        Expression.Lambda<SerializeToStringDelegate>(
                            Expression.Call(typeof(Generic.Utf16), nameof(Generic.Utf16.Serialize),
                                new[] { type, typeof(TResolver) }, typedInputParam),
                            inputParam);
                    return lambdaExpression.Compile();
                }

                private static SerializeToCharArrayDelegate BuildToCharArraySerializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(object), "input");
                    var typedInputParam = Expression.Convert(inputParam, type);
                    var lambdaExpression =
                        Expression.Lambda<SerializeToCharArrayDelegate>(
                            Expression.Call(typeof(Generic.Utf16), nameof(Generic.Utf16.SerializeToCharArray),
                                new[] { type, typeof(TResolver) }, typedInputParam),
                            inputParam);
                    return lambdaExpression.Compile();
                }

                private static SerializeToCharArrayPoolDelegate BuildToCharArrayPoolSerializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(object), "input");
                    var typedInputParam = Expression.Convert(inputParam, type);
                    var lambdaExpression =
                        Expression.Lambda<SerializeToCharArrayPoolDelegate>(
                            Expression.Call(typeof(Generic.Utf16), nameof(Generic.Utf16.SerializeToArrayPool),
                                new[] { type, typeof(TResolver) }, typedInputParam),
                            inputParam);
                    return lambdaExpression.Compile();
                }

                private static SerializeToTextWriterDelegateAsync BuildAsyncTextWriterSerializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(object), "input");
                    var typedInputParam = Expression.Convert(inputParam, type);
                    var textWriterParam = Expression.Parameter(typeof(TextWriter), "tw");
                    var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
                    var lambdaExpression =
                        Expression.Lambda<SerializeToTextWriterDelegateAsync>(
                            Expression.Call(typeof(Generic.Utf16), nameof(Generic.Utf16.SerializeAsync),
                                new[] { type, typeof(TResolver) }, typedInputParam, textWriterParam, cancellationTokenParam),
                            inputParam, textWriterParam, cancellationTokenParam);
                    return lambdaExpression.Compile();
                }

                private static DeserializeFromTextReaderDelegateAsync BuildAsyncTextReaderDeserializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(TextReader), "tr");
                    var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
                    Expression genericCall = Expression.Call(typeof(Utf16), nameof(Utf16.GenericTextReaderObjectWrapper),
                        new[] { type, typeof(TResolver) }, inputParam, cancellationTokenParam);
                    var lambdaExpression = Expression.Lambda<DeserializeFromTextReaderDelegateAsync>(genericCall, inputParam, cancellationTokenParam);
                    return lambdaExpression.Compile();
                }

                #endregion

                #region ** Common Builder **

                private static DeserializeDelegate BuildDeserializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(ReadOnlySpan<TSymbol>).MakeByRefType(), "input");
                    Expression genericCall = Expression.Call(typeof(Generic), nameof(Generic.DeserializeInternal),
                        new[] { type, typeof(TSymbol), typeof(TResolver) }, inputParam);
                    if (type.IsValueType)
                    {
                        genericCall = Expression.Convert(genericCall, typeof(object));
                    }

                    var lambdaExpression = Expression.Lambda<DeserializeDelegate>(genericCall, inputParam);
                    return lambdaExpression.Compile();
                }

                private static DeserializeFromByteArrayDelegate BuildFromByteArrayDeserializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(TSymbol[]), "input");
                    Expression genericCall = Expression.Call(typeof(Generic), nameof(Generic.DeserializeFromByteArrayInternal),
                        new[] { type, typeof(TSymbol), typeof(TResolver) }, inputParam);
                    if (type.IsValueType)
                    {
                        genericCall = Expression.Convert(genericCall, typeof(object));
                    }

                    var lambdaExpression = Expression.Lambda<DeserializeFromByteArrayDelegate>(genericCall, inputParam);
                    return lambdaExpression.Compile();
                }

                private static DeserializeFromBufferDelegate BuildFromBufferDeserializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(ArraySegment<TSymbol>).MakeByRefType(), "input");
                    Expression genericCall = Expression.Call(typeof(Generic), nameof(Generic.DeserializeFromBufferInternal),
                        new[] { type, typeof(TSymbol), typeof(TResolver) }, inputParam);
                    if (type.IsValueType)
                    {
                        genericCall = Expression.Convert(genericCall, typeof(object));
                    }

                    var lambdaExpression = Expression.Lambda<DeserializeFromBufferDelegate>(genericCall, inputParam);
                    return lambdaExpression.Compile();
                }

                private static DeserializeFromMemoryDelegate BuildFromMemoryDeserializer(Type type)
                {
                    var inputParam = Expression.Parameter(typeof(ReadOnlyMemory<TSymbol>).MakeByRefType(), "input");
                    Expression genericCall = Expression.Call(typeof(Generic), nameof(Generic.DeserializeFromMemoryInternal),
                        new[] { type, typeof(TSymbol), typeof(TResolver) }, inputParam);
                    if (type.IsValueType)
                    {
                        genericCall = Expression.Convert(genericCall, typeof(object));
                    }

                    var lambdaExpression = Expression.Lambda<DeserializeFromMemoryDelegate>(genericCall, inputParam);
                    return lambdaExpression.Compile();
                }

                #endregion

                #region -- class Invoker --

                internal sealed class Invoker
                {
                    public Invoker(
                        SerializeToByteArrayDelegate toByteArraySerializer,
                        SerializeToByteArrayPoolDelegate toByteArrayPoolSerializer,
                        SerializeToStreamDelegateAsync toStreamSerializerAsync,
                        DeserializeDelegate deserializer,
                        DeserializeFromByteArrayDelegate fromByteArrayDeserializer,
                        DeserializeFromBufferDelegate fromBufferDeserializer,
                        DeserializeFromMemoryDelegate fromMemoryDeserializer,
                        DeserializeFromStreamDelegateAsync fromStreamDeserializerAsync)
                    {
                        ToByteArraySerializer = toByteArraySerializer;
                        ToByteArrayPoolSerializer = toByteArrayPoolSerializer;
                        ToStreamSerializerAsync = toStreamSerializerAsync;

                        Deserializer = deserializer;
                        FromByteArrayDeserializer = fromByteArrayDeserializer;
                        FromBufferDeserializer = fromBufferDeserializer;
                        FromMemoryDeserializer = fromMemoryDeserializer;

                        FromStreamDeserializerAsync = fromStreamDeserializerAsync;
                    }

                    public Invoker(
                        SerializeToStringDelegate toStringSerializer,
                        SerializeToCharArrayDelegate toCharArraySerializer,
                        SerializeToCharArrayPoolDelegate toCharArrayPoolSerializer,
                        SerializeToTextWriterDelegateAsync serializeToTextWriterDelegateAsync,
                        DeserializeDelegate deserializer,
                        DeserializeFromByteArrayDelegate fromByteArrayDeserializer,
                        DeserializeFromBufferDelegate fromBufferDeserializer,
                        DeserializeFromMemoryDelegate fromMemoryDeserializer,
                        DeserializeFromTextReaderDelegateAsync deserializeFromTextReaderDelegateAsync)
                    {
                        ToStringSerializer = toStringSerializer;
                        ToCharArraySerializer = toCharArraySerializer;
                        ToCharArrayPoolSerializer = toCharArrayPoolSerializer;
                        ToTextWriterSerializerAsync = serializeToTextWriterDelegateAsync;

                        Deserializer = deserializer;
                        FromByteArrayDeserializer = fromByteArrayDeserializer;
                        FromBufferDeserializer = fromBufferDeserializer;
                        FromMemoryDeserializer = fromMemoryDeserializer;

                        FromTextReaderDeserializerAsync = deserializeFromTextReaderDelegateAsync;
                    }

                    internal readonly DeserializeDelegate Deserializer;
                    internal readonly DeserializeFromByteArrayDelegate FromByteArrayDeserializer;
                    internal readonly DeserializeFromBufferDelegate FromBufferDeserializer;
                    internal readonly DeserializeFromMemoryDelegate FromMemoryDeserializer;

                    internal readonly SerializeToByteArrayDelegate ToByteArraySerializer;
                    internal readonly SerializeToByteArrayPoolDelegate ToByteArrayPoolSerializer;
                    internal readonly SerializeToStreamDelegateAsync ToStreamSerializerAsync;

                    internal readonly DeserializeFromStreamDelegateAsync FromStreamDeserializerAsync;

                    internal readonly SerializeToStringDelegate ToStringSerializer;
                    internal readonly SerializeToCharArrayDelegate ToCharArraySerializer;
                    internal readonly SerializeToCharArrayPoolDelegate ToCharArrayPoolSerializer;
                    internal readonly SerializeToTextWriterDelegateAsync ToTextWriterSerializerAsync;

                    internal readonly DeserializeFromTextReaderDelegateAsync FromTextReaderDeserializerAsync;
                }

                #endregion

                #region -- Delegates --

                internal delegate object DeserializeFromByteArrayDelegate(TSymbol[] input);

                internal delegate object DeserializeFromBufferDelegate(in ArraySegment<TSymbol> input);

                internal delegate object DeserializeFromMemoryDelegate(in ReadOnlyMemory<TSymbol> input);

                internal delegate object DeserializeDelegate(in ReadOnlySpan<TSymbol> input);




                internal delegate byte[] SerializeToByteArrayDelegate(object input);

                internal delegate ArraySegment<byte> SerializeToByteArrayPoolDelegate(object input);

                internal delegate ValueTask SerializeToStreamDelegateAsync(object input, Stream stream, CancellationToken cancellationToken = default);

                internal delegate ValueTask<object> DeserializeFromStreamDelegateAsync(Stream stream, CancellationToken cancellationToken = default);




                internal delegate string SerializeToStringDelegate(object input);

                internal delegate char[] SerializeToCharArrayDelegate(object input);

                internal delegate ArraySegment<char> SerializeToCharArrayPoolDelegate(object input);

                internal delegate ValueTask SerializeToTextWriterDelegateAsync(object input, TextWriter writer, CancellationToken cancellationToken = default);

                internal delegate ValueTask<object> DeserializeFromTextReaderDelegateAsync(TextReader textReader, CancellationToken cancellationToken = default);

                #endregion
            }
        }
    }
}