using System;
using System.Text;
#if NET40
using System.Reflection;
using CuteAnt.Reflection;
#endif

namespace System.IO
{
  /// <summary>增强的数据流读取器</summary>
  /// <remarks>StreamReader太恶心了，自动把流给关闭了，还没有地方设置。</remarks>
  internal class StreamReaderX : StreamReader
  {
    #region -- Properties --

    internal const int DefaultBufferSize = 1024;   // char[]

#if NET40
    private static readonly FieldInfo s_closable = typeof(StreamReader).GetField("_closable", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MemberGetter<StreamReader> s_closableGetter = s_closable.GetValueGetter<StreamReader>();
    private static readonly MemberSetter<StreamReader> s_closableSetter = s_closable.GetValueSetter<StreamReader>();

    private static readonly FieldInfo s_charPosition = typeof(StreamReader).GetField("charPos", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MemberGetter<StreamReader> s_charPositionGetter = s_charPosition.GetValueGetter<StreamReader>();
    private static readonly MemberSetter<StreamReader> s_charPositionSetter = s_charPosition.GetValueSetter<StreamReader>();
    /// <summary>是否在最后关闭流</summary>
    public bool LeaveOpenInternal { get { return !(bool)s_closableGetter(this); } set { s_closableSetter(this, !value); } }

    /// <summary>字符位置。因为涉及字符编码，所以跟流位置可能不同。对于ASCII编码没有问题。</summary>
    public int CharPosition { get { return (int)s_charPositionGetter(this); } set { s_charPositionSetter(this, value); } }
#endif

    #endregion

    #region -- Constructors --

    /// <summary>为指定的流初始化 <see cref="T:System.IO.StreamReader" /> 类的新实例。</summary>
    /// <param name="stream">要读取的流。</param>
    /// <exception cref="T:System.ArgumentException"><paramref name="stream" /> 不支持读取。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 为 null。</exception>
    public StreamReaderX(Stream stream)
      : this(stream, Encoding.UTF8, true, DefaultBufferSize, true) { }

    /// <summary>为指定的流初始化 <see cref="T:System.IO.StreamReader" /> 类的新实例。</summary>
    /// <param name="stream">要读取的流。</param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="T:System.ArgumentException"><paramref name="stream" /> 不支持读取。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 为 null。</exception>
    public StreamReaderX(Stream stream, bool leaveOpen)
      : this(stream, Encoding.UTF8, true, DefaultBufferSize, leaveOpen) { }

    /// <summary>用指定的字符编码为指定的流初始化 <see cref="T:System.IO.StreamReader" /> 类的一个新实例。</summary>
    /// <param name="stream">要读取的流。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <exception cref="T:System.ArgumentException"><paramref name="stream" /> 不支持读取。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 或 <paramref name="encoding" /> 为 null。</exception>
    public StreamReaderX(Stream stream, Encoding encoding)
      : this(stream, encoding, true, DefaultBufferSize, true) { }

    /// <summary>为指定的流初始化 <see cref="T:System.IO.StreamReader" /> 类的新实例，带有指定的字符编码、字节顺序标记检测选项和缓冲区大小。</summary>
    /// <param name="stream">要读取的流。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <param name="leaveOpen">是否关闭数据流。</param>
    /// <exception cref="T:System.ArgumentException">流不支持读取。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 或 <paramref name="encoding" /> 为 null。</exception>
    public StreamReaderX(Stream stream, Encoding encoding, bool leaveOpen)
      : this(stream, encoding, true, DefaultBufferSize, leaveOpen) { }

    /// <summary>为指定的流初始化 <see cref="T:System.IO.StreamReader" /> 类的新实例，带有指定的字符编码、字节顺序标记检测选项和缓冲区大小。</summary>
    /// <param name="stream">要读取的流。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <param name="bufferSize">最小缓冲区大小</param>
    /// <exception cref="T:System.ArgumentException">流不支持读取。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 或 <paramref name="encoding" /> 为 null。</exception>
    public StreamReaderX(Stream stream, Encoding encoding, int bufferSize)
      : this(stream, encoding, true, bufferSize, true) { }

    /// <summary>为指定的流初始化 <see cref="T:System.IO.StreamReader" /> 类的新实例，带有指定的字符编码、字节顺序标记检测选项和缓冲区大小。</summary>
    /// <param name="stream">要读取的流。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <param name="bufferSize">最小缓冲区大小</param>
    /// <param name="leaveOpen">是否关闭数据流。</param>
    /// <exception cref="T:System.ArgumentException">流不支持读取。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 或 <paramref name="encoding" /> 为 null。</exception>
    public StreamReaderX(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
      : this(stream, encoding, true, bufferSize, leaveOpen) { }

    /// <summary>为指定的流初始化 <see cref="T:System.IO.StreamReader" /> 类的新实例，带有指定的字符编码、字节顺序标记检测选项和缓冲区大小。</summary>
    /// <param name="stream">要读取的流。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <param name="detectEncodingFromByteOrderMarks">是否要在文件开头查找字节顺序标记</param>
    /// <param name="bufferSize">最小缓冲区大小</param>
    /// <param name="leaveOpen">是否关闭数据流。</param>
    /// <exception cref="T:System.ArgumentException">流不支持读取。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 或 <paramref name="encoding" /> 为 null。</exception>
    public StreamReaderX(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
#if NET40
      : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize) { LeaveOpenInternal = leaveOpen; }
#else
      : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen) { }
#endif

    #endregion
  }
}
