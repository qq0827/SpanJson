using System;
using System.Text;
using System.Threading;
#if NET40
using System.Reflection;
using CuteAnt.Reflection;
#endif

namespace System.IO
{
  /// <summary>增强的数据流写入器</summary>
  /// <remarks>StreamWriter太恶心了，自动把流给关闭了，还没有地方设置。</remarks>
  internal class StreamWriterX : StreamWriter
  {
    #region -- Properties --

    // For UTF-8, the values of 1K for the default buffer size and 4K for the
    // file stream buffer size are reasonable & give very reasonable
    // performance for in terms of construction time for the StreamWriter and
    // write perf.  Note that for UTF-8, we end up allocating a 4K byte buffer,
    // which means we take advantage of adaptive buffering code.
    // The performance using UnicodeEncoding is acceptable.  
    internal const int DefaultBufferSize = 1024;   // char[]

    // The high level goal is to be tolerant of encoding errors when we read and very strict 
    // when we write. Hence, default StreamWriter encoding will throw on encoding error.   
    // Note: when StreamWriter throws on invalid encoding chars (for ex, high surrogate character 
    // D800-DBFF without a following low surrogate character DC00-DFFF), it will cause the 
    // internal StreamWriter's state to be irrecoverable as it would have buffered the 
    // illegal chars and any subsequent call to Flush() would hit the encoding error again. 
    // Even Close() will hit the exception as it would try to flush the unwritten data. 
    // Maybe we can add a DiscardBufferedData() method to get out of such situation (like 
    // StreamReader though for different reason). Either way, the buffered data will be lost!
    private static volatile Encoding _UTF8NoBOM;

    internal static Encoding UTF8NoBOM
    {
      get
      {
        if (_UTF8NoBOM == null)
        {
          // No need for double lock - we just want to avoid extra
          // allocations in the common case.
          UTF8Encoding noBOM = new UTF8Encoding(false, true);
          Thread.MemoryBarrier();
          _UTF8NoBOM = noBOM;
        }
        return _UTF8NoBOM;
      }
    }

#if NET40
    private static readonly FieldInfo s_closable = typeof(StreamWriter).GetField("closable", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MemberGetter<StreamWriter> s_closableGetter = s_closable.GetValueGetter<StreamWriter>();
    private static readonly MemberSetter<StreamWriter> s_closableSetter = s_closable.GetValueSetter<StreamWriter>();

    /// <summary>是否在最后关闭流</summary>
    public bool LeaveOpenInternal { get { return !(bool)s_closableGetter(this); } set { s_closableSetter(this, !value); } }
#endif

    #endregion

    #region -- Consturctors --

    /// <summary>用 UTF-8 编码及默认缓冲区大小，为指定的流初始化 <see cref="T:System.IO.StreamWriter" /> 类的一个新实例。</summary>
    /// <param name="stream">要写入的流。</param>
    /// <exception cref="T:System.ArgumentException"><paramref name="stream" /> 不可写。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 为 null。</exception>
    public StreamWriterX(Stream stream)
      : this(stream, UTF8NoBOM, DefaultBufferSize, true) { }

    /// <summary>用 UTF-8 编码及默认缓冲区大小，为指定的流初始化 <see cref="T:System.IO.StreamWriter" /> 类的一个新实例。</summary>
    /// <param name="stream">要写入的流。</param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="T:System.ArgumentException"><paramref name="stream" /> 不可写。</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 为 null。</exception>
    public StreamWriterX(Stream stream, bool leaveOpen)
      : this(stream, UTF8NoBOM, DefaultBufferSize, leaveOpen) { }

    /// <summary>用指定的编码及默认缓冲区大小，为指定的流初始化 <see cref="T:System.IO.StreamWriter" /> 类的新实例。</summary>
    /// <param name="stream">要写入的流。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 或 <paramref name="encoding" /> 为 null。</exception>
    /// <exception cref="T:System.ArgumentException"><paramref name="stream" /> 不可写。</exception>
    public StreamWriterX(Stream stream, Encoding encoding)
      : this(stream, encoding, DefaultBufferSize, true) { }

    /// <summary>用指定的编码及缓冲区大小，为指定的流初始化 <see cref="T:System.IO.StreamWriter" /> 类的新实例。</summary>
    /// <param name="stream">要写入的流。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <param name="leaveOpen"></param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 或 <paramref name="encoding" /> 为 null。</exception>
    /// <exception cref="T:System.ArgumentException"><paramref name="stream" /> 不可写。</exception>
    public StreamWriterX(Stream stream, Encoding encoding, bool leaveOpen)
      : this(stream, encoding, DefaultBufferSize, leaveOpen) { }

    /// <summary>用指定的编码及缓冲区大小，为指定的流初始化 <see cref="T:System.IO.StreamWriter" /> 类的新实例。</summary>
    /// <param name="stream">要写入的流。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <param name="bufferSize">缓冲区大小（以字节为单位）。</param>
    /// <param name="leaveOpen">是否关闭数据流。</param>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> 或 <paramref name="encoding" /> 为 null。</exception>
    /// <exception cref="T:System.ArgumentException"><paramref name="stream" /> 不可写。</exception>
    public StreamWriterX(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
#if NET40
      : base(stream, encoding, bufferSize) { LeaveOpenInternal = leaveOpen; }
#else
      : base(stream, encoding, bufferSize, leaveOpen) { }
#endif

    #endregion
  }
}
