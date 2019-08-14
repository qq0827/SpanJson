using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SpanJson.Dynamic
{
    public sealed class SpanJsonDynamicObject : DynamicObject
    {
        private readonly Dictionary<string, object> _dictionary;
        private object _rawJson;
        private readonly bool _isUtf16;

        internal SpanJsonDynamicObject(Dictionary<string, object> dictionary)
        {
            _dictionary = dictionary;
        }

        internal SpanJsonDynamicObject(Dictionary<string, object> dictionary, object rawJson, bool isUtf16)
        {
            _dictionary = dictionary;
            _rawJson = rawJson;
            _isUtf16 = isUtf16;
        }

        [IgnoreDataMember]
        internal bool HasRaw => _rawJson is object;
        [IgnoreDataMember]
        internal bool IsUtf16 => _isUtf16;
        [IgnoreDataMember]
        internal ArraySegment<char> Utf16Raw => (ArraySegment<char>)_rawJson;
        [IgnoreDataMember]
        internal ArraySegment<byte> Utf8Raw => (ArraySegment<byte>)_rawJson;

        /// <summary>Gets or sets the <see cref="object"/> with the specified name.</summary>
        /// <value>The <see cref="object"/>.</value>
        /// <param name="name">The name.</param>
        /// <returns>Value from the property.</returns>
        public object this[string name]
        {
            get
            {
                if (_dictionary.TryGetValue(name, out object result))
                {
                    return result;
                }

                return null;
            }
        }

        public override string ToString()
        {
            return $"{{{string.Join(",", _dictionary.Select(a => $"\"{a.Key}\":{a.Value.ToJsonValue()}"))}}}";
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _dictionary.TryGetValue(binder.Name, out result);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(binder.ReturnType))
            {
                result = _dictionary;
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dictionary.Keys;
        }
    }
}